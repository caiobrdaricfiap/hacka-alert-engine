using System.Text.Json.Serialization;

public class AlertMessage
{
    [JsonPropertyName("service")]
    public string Service { get; set; } = default!;

    [JsonPropertyName("level")]
    public string Level { get; set; } = default!;

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    [JsonPropertyName("durationms")]
    public int DurationMs { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
}
