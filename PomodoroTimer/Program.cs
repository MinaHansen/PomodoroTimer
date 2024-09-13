namespace PomodoroTimer;

class Program {
	static void Main(string[] args) {
		try {
			PomodoroConfiguration pomodoroConfiguration = PomodoroConfiguration.LoadOrStartWizard();
			Pomodoro pomodoro = new Pomodoro(pomodoroConfiguration);
			pomodoro.RunAsync().Wait(); 
		}
		catch (Exception exception) {
			#if DEBUG
			throw;
			#endif
			
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(exception);
			Console.ForegroundColor = ConsoleColor.White;
			
			Console.WriteLine("Press enter to continue...");
			Console.ReadLine(); // Pause
		}
	}
}