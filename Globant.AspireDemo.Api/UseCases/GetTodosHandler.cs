using Globant.AspireDemo.Api.Contexts;
using Globant.AspireDemo.Api.Entities;
using Globant.AspireDemo.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Globant.AspireDemo.Api.UseCases;

public record GetAllToDoItemsQuery : IRequest<IEnumerable<Todo>>;

public class GetTodosHandler : IRequestHandler<GetAllToDoItemsQuery, IEnumerable<Todo>>
{
    private readonly TodoContext _context;
    private readonly IDistributedCache _cache;

    public GetTodosHandler(TodoContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<Todo>> Handle(GetAllToDoItemsQuery request, CancellationToken cancellationToken)
    {
        var result = await _cache.GetOrSetAsync("todos", async () =>
        {
            var r = await _context.Todos.ToListAsync(cancellationToken);
            return r.Count == 0 ? null : r;
        });
        return result ?? [];
    }
}
