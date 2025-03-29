using Globant.AspireDemo.Api.Contexts;
using Globant.AspireDemo.Api.Entities;
using Globant.AspireDemo.Api.Infrastructure;
using Globant.AspireDemo.Contracts;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using TStatus = Globant.AspireDemo.Api.Entities.TaskStatus;

namespace Globant.AspireDemo.Api.UseCases;

public record UpdateTodoCommand(int Id, string Title, string Description, TStatus Status, DateTime? DueDate) : IRequest<Todo>;

public class UpdateTodoHandler : IRequestHandler<UpdateTodoCommand, Todo>
{
    private readonly TodoContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UpdateTodoHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateTodoHandler(TodoContext context, IDistributedCache cache, ILogger<UpdateTodoHandler> logger, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Todo> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _context.Todos.FindAsync(request.Id);
        if (todo == null)
        {
            throw new KeyNotFoundException();
        }
        todo.Title = request.Title;
        todo.Description = request.Description;
        todo.Status = request.Status;
        todo.DueDate = request.DueDate;
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Todo updated with id {Id}, Title {Title}, Description {Description}, Status {Status}, DueDate {DueDate}", todo.Id, todo.Title, todo.Description, todo.Status, todo.DueDate);


        await _cache.SetAsync($"todo-{todo.Id}", todo);
        _logger.LogInformation("Todo cached with id {Key}", $"todo-{todo.Id}");

        await _publishEndpoint.Publish(new TodoUpdated(todo.Id, todo.Title, todo.Description, todo.DueDate, todo.Status.ToString()), cancellationToken);
        _logger.LogInformation("Todo updated event published with id {Id} and Title {Title}", todo.Id, todo.Title);

        return todo;
    }
}
