using Globant.AspireDemo.Api.Contexts;
using Globant.AspireDemo.Api.UseCases;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddSqlServerDbContext<TodoContext>("sqldata");
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.AddRedisDistributedCache("cache");
builder.AddRabbitMQClient("rabbitmq");

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConnection = builder.Configuration.GetConnectionString("rabbitmq");
        cfg.Host(rabbitMqConnection);
    });
});

builder.AddServiceDefaults();

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    if ((await context.Database.GetPendingMigrationsAsync()).Any())
    {
        await context.Database.MigrateAsync();
    }

    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Todo API V1");
    });

    app.UseReDoc(options =>
    {
        options.DocumentTitle = "Todo API V1";
        options.SpecUrl = "/openapi/v1.json";
    });
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapGet("/todos", async ([FromServices] ISender sender) => await sender.Send(new GetAllToDoItemsQuery()));

app.MapGet("/todos/{id}", async (int id, [FromServices] ISender sender) => Results.Ok(await sender.Send(new GetTodoByIdQuery(id))));

app.MapPost("/todos", async (CreateTodoCommand todo, [FromServices] ISender sender) =>
{
    var result = await sender.Send(todo);
    return Results.Created($"/todos/{result.Id}", todo);
});

app.MapPut("/todos/{id}", async (int id, UpdateTodoCommand todo, [FromServices] ISender sender) =>
{
    var todoToUpdate = todo with { Id = id };
    var result = await sender.Send(todoToUpdate);
    if (result is null) return Results.NotFound();
    return Results.NoContent();
});

app.MapDelete("/todos/{id}", async (int id, [FromServices] ISender sender) =>
{
    var result = await sender.Send(new DeleteTodoCommand(id));
    if (!result) return Results.NotFound();
    return Results.NoContent();
});

await app.RunAsync();