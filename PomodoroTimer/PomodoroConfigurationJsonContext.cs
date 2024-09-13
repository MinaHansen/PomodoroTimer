using System.Text.Json.Serialization;

namespace PomodoroTimer;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PomodoroConfiguration))]
public partial class PomodoroConfigurationJsonContext : JsonSerializerContext {
	
}