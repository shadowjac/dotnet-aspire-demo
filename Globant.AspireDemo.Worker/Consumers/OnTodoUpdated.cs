using Globant.AspireDemo.Contracts;
using Globant.AspireDemo.Worker.Collections;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Diagnostics;

namespace Globant.AspireDemo.Worker.Consumers;

internal class OnTodoUpdated : IConsumer<TodoUpdated>
{
    private readonly MetricsService _metricsService;
    private readonly IMongoClient _mongoClient;
    private readonly ILogger<OnTodoUpdated> _logger;

    public OnTodoUpdated(ILogger<OnTodoUpdated> logger, IMongoClient mongoClient, MetricsService metricsService)
    {
        _logger = logger;
        _mongoClient = mongoClient;
        _metricsService = metricsService;
    }

    public async Task Consume(ConsumeContext<TodoUpdated> context)
    {
        var stopwatch = Stopwatch.StartNew();

        var filter = Builders<Todo>.Filter.Eq(x => x.RelatedId, context.Message.Id);

        var update = Builders<Todo>.Update
            .Set(x => x.Title, context.Message.Title)
            .Set(x => x.Description, context.Message.Description)
            .Set(x => x.IsCompleted, context.Message.Status == "Completed");

        await _mongoClient.GetDatabase("todos-db")
            .GetCollection<Todo>("todos")
            .UpdateOneAsync(filter, update);

        _logger.LogInformation("Todo updated with id {Id} and text: {Text}", context.Message.Id, context.Message.Title);
        stopwatch.Stop();

        _metricsService.IncrementOperation("update");
        _metricsService.RecordDbResponseTime("update", stopwatch.ElapsedMilliseconds);
    }
}
