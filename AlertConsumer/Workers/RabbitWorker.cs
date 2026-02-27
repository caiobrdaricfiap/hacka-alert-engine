using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class RabbitWorker : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<RabbitWorker> _logger;
    private readonly ApmNotifier _notifier;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitWorker(IConfiguration config, ILogger<RabbitWorker> logger, ApmNotifier notifier)
    {
        _config = config;
        _logger = logger;
        _notifier = notifier;
        InitRabbit();
    }

    private void InitRabbit()
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"],
            UserName = _config["RabbitMQ:User"],
            Password = _config["RabbitMQ:Pass"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: _config["RabbitMQ:Queue"],
            durable: true,
            exclusive: false,
            autoDelete: false);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                _logger.LogInformation("JSON recebido: {Json}", json);// Mostra json no console

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var msg = JsonSerializer.Deserialize<AlertMessage>(json)!;
                await ProcessMessage(msg);
                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem");
                _channel!.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: _config["RabbitMQ:Queue"],
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    private async Task ProcessMessage(AlertMessage msg)
    {
        if (msg.Level == "alerta" || msg.DurationMs > 3000)
        {
            await _notifier.SendToGrafanaAsync(msg);
            await _notifier.SendToDatadogAsync(msg);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
