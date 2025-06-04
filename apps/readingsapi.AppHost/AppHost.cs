var builder = DistributedApplication.CreateBuilder(args);

// TODO: Get passwords from a vault or secure store
var postgressPassword = builder.AddParameter("PostgresPassword", secret: true);
var postgres = builder.AddPostgres("postgres", password: postgressPassword)
    .WithDataVolume("postgres_data");

var postgresdb = postgres.AddDatabase("postgresdb");

builder.AddProject<Projects.readingsapi>("readingsapi")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb)
    .WithEnvironment("DB_CONNECTION_TYPE", "postgresdb");

builder.Build().Run();
