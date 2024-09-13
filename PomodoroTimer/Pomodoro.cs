using System.Text;

namespace PomodoroTimer;

public class Pomodoro(PomodoroConfiguration pomodoroConfiguration) : IDisposable {
	private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
	private readonly CoreLogic coreLogic = new CoreLogic(pomodoroConfiguration);
	
	public async Task RunAsync() {
		// Configure console
		Console.CursorVisible = false;
		
		// Run loops
		Task inputLoop = Task.Run(() => InputLoopAsync(cancellationTokenSource.Token));
		Task logicLoop = Task.Run(() => LogicLoopAsync(cancellationTokenSource.Token));
		Task renderLoop = Task.Run(() => RenderLoopAsync(cancellationTokenSource.Token));
		Task gcLoop = Task.Run(() => GcLoopAsync(cancellationTokenSource.Token));
		Task.WaitAll(inputLoop, logicLoop, renderLoop, gcLoop);
	}

	private async Task InputLoopAsync(CancellationToken cancellationToken) {
		while (!cancellationToken.IsCancellationRequested) {
			if (!Console.KeyAvailable) {
				await Task.Delay(250, cancellationToken);
				continue;
			}

			ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
			switch (consoleKeyInfo.Key) {
				case ConsoleKey.Q:
					await cancellationTokenSource.CancelAsync();
					Environment.Exit(0);
					break;
				
				case ConsoleKey.P:
					coreLogic.SetPaused(!coreLogic.IsPaused);
					break;
				
				case ConsoleKey.S:
					coreLogic.SkipPhase();
					break;
			}
		}
	}
	
	private async Task LogicLoopAsync(CancellationToken cancellationToken) {
		while (!cancellationToken.IsCancellationRequested) {
			coreLogic.Tick();
			await Task.Delay(125, cancellationToken);
		}
	}
	
	// Making it not readonly brakes one guideline, but I have good reason to brake that guideline.
	private struct RenderData : IEquatable<RenderData> {
		public bool Equals(RenderData other) {
			return CurrentPhase == other.CurrentPhase && Minutes == other.Minutes && Seconds == other.Seconds && IsPaused == other.IsPaused;
		}

		public override bool Equals(object? obj) {
			return obj is RenderData other && Equals(other);
		}

		public override int GetHashCode() {
			return HashCode.Combine((int)CurrentPhase, Minutes, Seconds, IsPaused);
		}

		public PomodoroPhase CurrentPhase;
		public int Minutes;
		public int Seconds;
		public bool IsPaused;

		public RenderData(PomodoroPhase currentPhase, int minutes, int seconds, bool isPaused) {
			CurrentPhase = currentPhase;
			Minutes = minutes;
			Seconds = seconds;
			IsPaused = isPaused;
		}

		public static bool operator ==(RenderData left, RenderData right) {
			return left.Seconds == right.Seconds && left.Minutes == right.Minutes && left.CurrentPhase == right.CurrentPhase && left.IsPaused == right.IsPaused;
		}

		public static bool operator !=(RenderData left, RenderData right) {
			return !(left == right);
		}
	}
	
	private async Task RenderLoopAsync(CancellationToken cancellationToken) {
		RenderData previousRenderData = new RenderData();
		RenderData currentRenderData = new RenderData(coreLogic.CurrentPhase, coreLogic.RemainingTime.Minutes, coreLogic.RemainingTime.Seconds, coreLogic.IsPaused);
		FullRender(currentRenderData);
		
		while (!cancellationToken.IsCancellationRequested) {
			currentRenderData.CurrentPhase = coreLogic.CurrentPhase;
			currentRenderData.Minutes = coreLogic.RemainingTime.Minutes;
			currentRenderData.Seconds = coreLogic.RemainingTime.Seconds;
			currentRenderData.IsPaused = coreLogic.IsPaused;
			
			if (currentRenderData != previousRenderData) {
				DiffRender(currentRenderData, previousRenderData);
				previousRenderData = currentRenderData;
			}
			await Task.Delay(125, cancellationToken); // Using Task.Delay(125, cancellationToken).GetAwaiter(); will bring the memory usage up to 161 MB.
													  // Use await Task.Delay(125, cancellationToken); instead it brought memory usage down to 9,12MB.
		}
	}

	private void FullRender(RenderData renderData) {
		const int secondDigits = 2;
		const int minuteDigits = 2;
		const int maxBoolStringValueLength = 5;
		
		Console.Clear();
		Console.WriteLine($"Phase: {Enum.GetName(renderData.CurrentPhase)}\n" +
		                  $"Time remaining: {renderData.Minutes.ToString().PadLeft(minuteDigits)} min {renderData.Seconds.ToString().PadLeft(secondDigits)} sec\n" +
		                  $"IsPaused: {renderData.IsPaused.ToString().PadRight(maxBoolStringValueLength)}\n" +
		                  "Q = Quit | P = Pause | S = Skip");
	}
	
	// Diff rendering reduces flickering in the terminal.
	private void DiffRender(RenderData renderData, RenderData previousRenderData) {
		if (renderData.CurrentPhase != previousRenderData.CurrentPhase) {
			Console.CursorTop = 0;
			Console.CursorLeft = 7;
			Console.Write(Enum.GetName(renderData.CurrentPhase)!.PadRight(Enum.GetName(previousRenderData.CurrentPhase)!.Length));
		}

		if (renderData.Minutes != previousRenderData.Minutes) {
			Console.CursorTop = 1;
			Console.CursorLeft = 16;

			const int minuteDigits = 2;
			Console.Write(renderData.Minutes.ToString("00").PadRight(minuteDigits));
		}

		if (renderData.Seconds != previousRenderData.Seconds) {
			Console.CursorTop = 1;
			Console.CursorLeft = 23;

			const int secondDigits = 2;
			Console.Write(renderData.Seconds.ToString("00").PadLeft(secondDigits));
		}

		if (renderData.IsPaused != previousRenderData.IsPaused) {
			Console.CursorTop = 2;
			Console.CursorLeft = 10;
			
			const int maxBoolStringValueLength = 5;
			Console.Write(renderData.IsPaused.ToString().PadRight(maxBoolStringValueLength));
		}

		// Return to neutral position
		Console.CursorTop = 5;
		Console.CursorLeft = 0;
	}
	
	// Helps fix the issue of memory growth over time due to strings.
	private async Task GcLoopAsync(CancellationToken cancellationToken) {
		const int gcInterval = 1000 * 60 * 5; // Every five minutes
		
		while (!cancellationToken.IsCancellationRequested) {
			await Task.Delay(gcInterval, cancellationToken);
			GC.Collect();
		}
	}
	
	public void Dispose() {
		GC.SuppressFinalize(this);
		cancellationTokenSource.Dispose();
	}
}