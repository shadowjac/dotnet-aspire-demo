using Globant.AspireDemo.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var sqlPwd = builder.AddParameter("sql-password", secret: true);

var sql = builder.AddSqlServer("sql", password: sqlPwd)
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithDataVolume()
                 .AddDatabase("sqldata");

var cache = builder.AddRedis("cache")
                   .WithLifetime(ContainerLifetime.Persistent)
                   .WithRedisInsight()
                   .WithDataVolume(isReadOnly: false);

var rabbitmq = builder.AddRabbitMQ("rabbitmq", port: 5672)
                        .WithLifetime(ContainerLifetime.Persistent)
                        .WithDataVolume(isReadOnly: false)
                        .WithManagementPlugin();

var mongo = builder.AddMongoDB("mongo")
                   .WithDataVolume() // Allow data to persist
                   .WithMongoExpress()
                   .WithLifetime(ContainerLifetime.Persistent)
                   .AddDatabase("todos-db");

var worker = builder.AddProject<Projects.Globant_AspireDemo_Worker>("globant-aspiredemo-worker")
                   .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
                   .WithReference(rabbitmq)
                   .WaitFor(rabbitmq)
                   .WithReference(mongo)
                   .WaitFor(mongo);

builder.AddProject<Projects.Globant_AspireDemo_Api>("globant-aspiredemo-api")
    .WithSwaggerUI()
    .WithReDoc()
    .WithScalar()
    .WithReference(sql)
    .WaitFor(sql)
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.Build().Run();
