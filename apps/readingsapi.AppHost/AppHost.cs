var builder = DistributedApplication.CreateBuilder(args);

// TODO: Get passwords from a vault or secure store
// var postgressPassword = builder.AddParameter("PostgresPassword", secret: true);
// var postgres = builder.AddPostgres("postgres", password: postgressPassword)
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();

var postgresdb = postgres.AddDatabase("readingsdb");

builder.AddProject<Projects.readingsapi>("readingsApi")
    .WithExternalHttpEndpoints()
    .WithReference(postgresdb);

// TOT: Get this working. Currently the readingsAPI port seems to have a conflict with this job.
// builder.AddProject<Projects.readingsapi>("readingsApiSeedDataJob")
//     .WithReference(postgresdb)
//     .WithEnvironment("TASK_NAME", "seed");

builder.Build().Run();