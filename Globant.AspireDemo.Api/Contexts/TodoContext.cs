using Globant.AspireDemo.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Globant.AspireDemo.Api.Contexts;

public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
        
    }
    public DbSet<Todo> Todos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoContext).Assembly);
    }
}