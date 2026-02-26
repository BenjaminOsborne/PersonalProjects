var builder = DistributedApplication.CreateBuilder(args);

// ── PostgreSQL ───────────────────────────────────────────────────────────────
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("aspire-demo-pgdata");   // persist data between runs

var catalogDb = postgres.AddDatabase("catalogdb");

// ── Services ─────────────────────────────────────────────────────────────────
// CatalogService owns the database and seeds it on first run.
var catalogService = builder.AddProject<Projects.AspireDemo_CatalogService>("catalogservice")
    .WithReference(catalogDb)
    .WaitFor(catalogDb)
    .WithHttpHealthCheck("/health");

// ApiService is a gateway — it calls CatalogService for product data.
var apiService = builder.AddProject<Projects.AspireDemo_ApiService>("apiservice")
    .WithReference(catalogService)
    .WaitFor(catalogService)
    .WithHttpHealthCheck("/health");

// Blazor Web frontend talks exclusively to ApiService.
builder.AddProject<Projects.AspireDemo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithHttpHealthCheck("/health");

builder.Build().Run();
