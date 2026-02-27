using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(cfg =>
    cfg.WriteTo.Console());

builder.Services.AddHttpClient<ApmNotifier>();
builder.Services.AddSingleton<ApmNotifier>();
builder.Services.AddHostedService<RabbitWorker>();

var app = builder.Build();
app.Run();
