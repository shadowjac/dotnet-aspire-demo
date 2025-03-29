using Globant.AspireDemo.Api.Contexts;
using Globant.AspireDemo.Api.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using TStatus = Globant.AspireDemo.Api.Entities.TaskStatus;
using Globant.AspireDemo.Api.Infrastructure;
using MassTransit;
using Globant.AspireDemo.Contracts;

namespace Globant.AspireDemo.Api.UseCases;

// create a record based on the Todo entity
public record CreateTodoCommand(string Title, string Description, TStatus Status, DateTime? DueDate) : IRequest<Todo>;

public class CreateTodoHandler : IRequestHandler<CreateTodoCommand, Todo>
{
    private readonly TodoContext _context;
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateTodoHandler> _logger;
    public CreateTodoHandler(TodoContext context, IDistributedCache cache, IPublishEndpoint publishEndpoint, ILogger<CreateTodoHandler> logger)
    {
        _context = context;
        _cache = cache;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Todo> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new Todo
        {
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            DueDate = request.DueDate
        };

        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Todo created with id {Id}", todo.Id);

        await _cache.SetAsync($"todo-{todo.Id}", todo);
        _logger.LogInformation("Todo cached with id {Key}", $"todo-{todo.Id}");

        await _publishEndpoint.Publish(new TodoCreated(todo.Id, todo.Title, todo.Description, todo.DueDate, todo.Status.ToString()), cancellationToken);
        _logger.LogInformation("Todo created event published with id {Id} and Title {Title}", todo.Id, todo.Title);

        return todo;
    }
}
