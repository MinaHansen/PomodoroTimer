using System.Diagnostics;

namespace PomodoroTimer;

public class CoreLogic {
	private readonly Stopwatch stopwatch = new Stopwatch();
	private readonly PomodoroConfiguration configuration;
	private TimeSpan deltaTime;
	
	private int currentPomodoroCount = 0;
	public PomodoroPhase CurrentPhase { get; private set; }
	public TimeSpan RemainingTime { get; private set; }
	public bool IsPaused { get; private set; }

	public CoreLogic(PomodoroConfiguration configuration) {
		CurrentPhase = PomodoroPhase.Pomodoro;
		RemainingTime = TimeSpan.FromMinutes(configuration.PomodoroTime); 
		IsPaused = false;
		this.configuration = configuration;
		deltaTime = TimeSpan.Zero;
	}

	public void Tick() {
		deltaTime = stopwatch.Elapsed;
		stopwatch.Restart();

		if (IsPaused) return;
		
		RemainingTime -= deltaTime;
		if (RemainingTime > TimeSpan.Zero) return;
		
		if (CurrentPhase == PomodoroPhase.Pomodoro) {
			currentPomodoroCount++;
				
			switch (currentPomodoroCount % configuration.LongBrakeInterval) {
				case 0:
					SetPhaseAndUpdateRemainingTime(PomodoroPhase.LongBrake);
					break;
				default:
					SetPhaseAndUpdateRemainingTime(PomodoroPhase.ShortBrake);
					break;
			}
		}
		else {
			SetPhaseAndUpdateRemainingTime(PomodoroPhase.Pomodoro);
		}
	}
	
	public void SetPaused(bool paused) {
		IsPaused = paused;
	}

	private void SetPhaseAndUpdateRemainingTime(PomodoroPhase phase) {
		Console.Beep();
		
		CurrentPhase = phase;
		RemainingTime = CurrentPhase switch {
			PomodoroPhase.Pomodoro => TimeSpan.FromMinutes(configuration.PomodoroTime),
			PomodoroPhase.ShortBrake => TimeSpan.FromMinutes(configuration.ShortBrakeTime),
			PomodoroPhase.LongBrake => TimeSpan.FromMinutes(configuration.LongBrakeTime),
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	public void SkipPhase() {
		if (CurrentPhase == PomodoroPhase.Pomodoro) {
			currentPomodoroCount++;
				
			switch (currentPomodoroCount % configuration.LongBrakeInterval) {
				case 0:
					SetPhaseAndUpdateRemainingTime(PomodoroPhase.LongBrake);
					break;
				default:
					SetPhaseAndUpdateRemainingTime(PomodoroPhase.ShortBrake);
					break;
			}
		}
		else {
			SetPhaseAndUpdateRemainingTime(PomodoroPhase.Pomodoro);
		}
	}
}