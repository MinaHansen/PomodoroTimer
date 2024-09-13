using System.Text.Json;

namespace PomodoroTimer;

public class PomodoroConfiguration {

	public float PomodoroTime { get; init; } = 25;
	public float ShortBrakeTime { get; init; } = 5;
	public float LongBrakeTime { get; init; } = 15;
	public int LongBrakeInterval { get; init; } = 4;

	public static PomodoroConfiguration LoadOrStartWizard() {
		string configPath = Path.Combine(AppContext.BaseDirectory, "Config.json");

		if (File.Exists(configPath)) {
			
			return JsonSerializer.Deserialize(File.ReadAllText(configPath), PomodoroConfigurationJsonContext.Default.PomodoroConfiguration) ?? throw new Exception("Invalid config.");
		}
		
		// Wizard
		Console.WriteLine("First time setup.\n");
		
		Console.WriteLine("Default values:");
		Console.WriteLine("	PomodoroTime: 25min");
		Console.WriteLine("	ShortBrakeTime: 5min");
		Console.WriteLine("	LongBrakeTime: 15");
		Console.WriteLine("	LongBrakeInterval: 4");
		if (PromptYesNo("Use default configuration? [y]/n", true)) {
			var defaultConfig = new PomodoroConfiguration();
			WriteConfig(configPath, defaultConfig);
			return defaultConfig;
		}
		
		float pomodoroTime = PromptFloat("How long do you want to work before a brake?\nDefault is 25min", 25f, f => f > 0);
		float shortBrakeTime = PromptFloat("How long should short brakes last?\nDefault is 5min", 5, f => f > 0);
		float longBrakeTime = PromptFloat("How long should long brakes last?\nDefault is 15min", 15, f => f > 0);
		int longBrakeInterval = PromptInt("How many short brakes before each long brake?\nDefault is 4", 4, i => i > 0);

		PomodoroConfiguration configuration = new PomodoroConfiguration() {
			PomodoroTime = pomodoroTime,
			ShortBrakeTime = shortBrakeTime,
			LongBrakeTime = longBrakeTime,
			LongBrakeInterval = longBrakeInterval
		};

		WriteConfig(configPath, configuration);
		return configuration;
	}
	
	private static void WriteConfig(string configPath, PomodoroConfiguration configuration) {
		using StreamWriter writer = File.CreateText(configPath);
		writer.WriteLine(JsonSerializer.Serialize(configuration, typeof(PomodoroConfiguration), PomodoroConfigurationJsonContext.Default));
		writer.Flush();
		writer.Close();
	}

	private static bool PromptYesNo(string message, bool defaultValue) {
		while (true) {
			Console.WriteLine(message);
			string input = Console.ReadLine() ?? throw new Exception("Could not read console input");
			if (input.Trim() == string.Empty) {
				return defaultValue;
			}

			switch (input.Trim().ToLower()) {
				case "y":
					return true;
				case "yes":
					return true;
				case "n":
					return false;
				case "no":
					return false;
			}
			
			Console.WriteLine("Invalid input try again.");
		}
	}
	
	private static float PromptFloat(string message, float defaultValue, Func<float, bool> validate) {
		while (true) {
			Console.WriteLine(message);
			string input = Console.ReadLine() ?? throw new Exception("Could not read console input");
			if (input.Trim() == string.Empty) {
				return defaultValue;
			}
			
			if (float.TryParse(input, out float result)) {
				if (validate.Invoke(result)) {
					return result;
				}
			}
			else {
				Console.WriteLine("Not a valid floating point integer try again.");
				continue;
			}
			
			Console.WriteLine("Invalid input try again.");
		}
	}

	private static int PromptInt(string message, int defaultValue, Func<int, bool> validate) {
		while (true) {
			Console.WriteLine(message);
			string input = Console.ReadLine() ?? throw new Exception("Could not read console input");
			if (input.Trim() == string.Empty) {
				return defaultValue;
			}
			
			if (int.TryParse(input, out int result)) {
				if (validate.Invoke(result)) {
					return result;
				}
			}
			else {
				Console.WriteLine("Not a valid integer try again.");
				continue;
			}
			
			Console.WriteLine("Invalid input try again.");
		}
	}
}