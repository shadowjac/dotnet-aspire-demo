using Globant.AspireDemo.Worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;

var builder = Host.CreateApplicationBuilder(args);// .CreateDefaultBuilder(args);

builder.AddRabbitMQClient("rabbitMQ");
builder.AddMongoDBClient(connectionName: "todos-db");

builder.AddServiceDefaults();

builder.Services.AddSingleton<MetricsService>();


builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.AddConsumer<OnTodoCreated>();
    busConfigurator.AddConsumer<OnTodoUpdated>();

    busConfigurator.UsingRabbitMq((context, busFactoryConfigurator) =>
        {
            var cs = builder.Configuration.GetConnectionString("rabbitMQ");
            busFactoryConfigurator.Host(cs);
            Console.WriteLine($"*** ConnString: {cs} ***");
            busFactoryConfigurator.ConfigureEndpoints(context);
        });
});

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics => metrics
        .AddMeter(MetricsService.Meter.Name));


var app = builder.Build();

await app.RunAsync();