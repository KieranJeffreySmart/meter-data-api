var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var postgresdb = postgres.AddDatabase("postgresdb");

builder.AddProject<Projects.readingsapi>("readingsapi")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb);

builder.Build().Run();
