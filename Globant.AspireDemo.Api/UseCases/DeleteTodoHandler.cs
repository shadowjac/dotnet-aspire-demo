using Globant.AspireDemo.Api.Contexts;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Globant.AspireDemo.Api.UseCases;

// Generate a record for the delete todo command
public record DeleteTodoCommand(int Id) : IRequest<bool>;

public class DeleteTodoHandler : IRequestHandler<DeleteTodoCommand, bool>
{
    private readonly TodoContext _context;
    private readonly IDistributedCache _cache;

    public DeleteTodoHandler(TodoContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await _context.Todos.FindAsync(request.Id);
        if (todo == null)
        {
            throw new KeyNotFoundException();
        }
        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync($"todo-{todo.Id}");
        return true;
    }
}
