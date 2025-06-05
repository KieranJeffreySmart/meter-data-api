var builder = DistributedApplication.CreateBuilder(args);

// TODO: Get passwords from a vault or secure store
// var postgressPassword = builder.AddParameter("PostgresPassword", secret: true);
// var postgres = builder.AddPostgres("postgres", password: postgressPassword)
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var postgresdb = postgres.AddDatabase("readingsdb");

builder.AddProject<Projects.readingsapi>("readingsApi")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb)
    .WithEnvironment("DB_CONNECTION_TYPE", "postgres");

builder.Build().Run();
