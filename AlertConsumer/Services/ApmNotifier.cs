using System.Text;
using System.Text.Json;

public class ApmNotifier
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<ApmNotifier> _logger;

    public ApmNotifier(HttpClient http, IConfiguration config, ILogger<ApmNotifier> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task SendToGrafanaAsync(AlertMessage msg)
    {
        var url = _config["Alerting:GrafanaWebhook"];

        var payload = new
        {
            title = $"ALERTA - {msg.Service}",
            message = msg.Message,
            level = msg.Level,
            duration = msg.DurationMs
        };

        var json = JsonSerializer.Serialize(payload);
        var response = await _http.PostAsync(url,
            new StringContent(json, Encoding.UTF8, "application/json"));

        _logger.LogInformation("Grafana response: {Status}", response.StatusCode);
    }

    public async Task SendToDatadogAsync(AlertMessage msg)
    {
        var apiKey = _config["Alerting:DatadogApiKey"];
        var url = $"https://api.datadoghq.com/api/v1/events?api_key={apiKey}";

        var payload = new
        {
            title = $"Alert - {msg.Service}",
            text = msg.Message,
            alert_type = msg.Level == "error" ? "error" : "warning",
            source_type_name = "rabbitmq-consumer"
        };

        var json = JsonSerializer.Serialize(payload);
        var response = await _http.PostAsync(url,
            new StringContent(json, Encoding.UTF8, "application/json"));

        _logger.LogInformation("Datadog response: {Status}", response.StatusCode);
    }
}
