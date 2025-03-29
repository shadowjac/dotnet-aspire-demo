using Globant.AspireDemo.Contracts;
using Globant.AspireDemo.Worker.Collections;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

namespace Globant.AspireDemo.Worker.Consumers;

internal class OnTodoCreated : IConsumer<TodoCreated>
{
    private readonly MetricsService _metricsService;
    private readonly IMongoClient _mongoClient;
    private readonly ILogger<OnTodoCreated> _logger;

    public OnTodoCreated(ILogger<OnTodoCreated> logger, IMongoClient mongoClient, MetricsService metricsService)
    {
        _logger = logger;
        _mongoClient = mongoClient;
        _metricsService = metricsService;
    }

    public async Task Consume(ConsumeContext<TodoCreated> context)
    {
        var stopwatch = Stopwatch.StartNew();
        var newId = ObjectId.GenerateNewId();

        await _mongoClient.GetDatabase("todos-db")
            .GetCollection<Todo>("todos")
            .InsertOneAsync(new Todo
            {
                Id = newId,
                RelatedId = context.Message.Id,
                Title = context.Message.Title,
                Description = context.Message.Description,
                IsCompleted = false
            });
        _logger.LogInformation("Todo created with id {Id} and text: {Text} - MongoId: {MongoId}", context.Message.Id, context.Message.Title, newId);

        stopwatch.Stop();

        _metricsService.IncrementOperation("insert");
        _metricsService.RecordDbResponseTime("insert", stopwatch.ElapsedMilliseconds);
    }
}
