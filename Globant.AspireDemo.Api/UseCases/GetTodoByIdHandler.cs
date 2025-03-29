using Globant.AspireDemo.Api.Contexts;
using Globant.AspireDemo.Api.Entities;
using Globant.AspireDemo.Api.Infrastructure;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace Globant.AspireDemo.Api.UseCases;

// generate record query by id
public record GetTodoByIdQuery(int Id) : IRequest<Todo?>;

public class GetTodoByIdHandler : IRequestHandler<GetTodoByIdQuery, Todo?>
{
    private readonly TodoContext _context;
    private readonly IDistributedCache _cache;

    public GetTodoByIdHandler(TodoContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Todo?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await _cache.GetOrSetAsync($"todo-{request.Id}", async () => await _context.Todos.FindAsync(request.Id));
        return todo;
    }
}
