using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// HTTP client for talking to CatalogService.
// Aspire's service discovery resolves "catalogservice" to the correct URL.
builder.Services.AddHttpClient("catalogservice", client =>
{
    client.BaseAddress = new Uri("https+http://catalogservice");
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// ── Weather endpoint (from the starter template) ────────────────────────────
string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API Gateway is running. Try /weatherforecast or /products.");

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// ── Products endpoint — calls CatalogService ────────────────────────────────
app.MapGet("/products", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("catalogservice");
    var products = await client.GetFromJsonAsync<CatalogProduct[]>("/products");

    if (products is null) return Results.Problem("Catalog service returned no data.");

    // Enrich each product with a "featured" flag (business logic lives here)
    var enriched = products.Select(p => new ProductSummary(
        p.Id,
        p.Name,
        p.Description,
        p.Price,
        p.Category,
        p.StockQuantity,
        IsFeatured: p.Price >= 99.99m,
        StockStatus: p.StockQuantity switch
        {
            0       => "Out of stock",
            <= 10   => "Low stock",
            _       => "In stock"
        }
    ));

    return Results.Ok(enriched);
})
.WithName("GetProducts");

app.MapGet("/products/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("catalogservice");
    var response = await client.GetAsync($"/products/{id}");
    if (!response.IsSuccessStatusCode) return Results.NotFound();
    var product = await response.Content.ReadFromJsonAsync<CatalogProduct>();
    return product is null ? Results.NotFound() : Results.Ok(product);
})
.WithName("GetProductById");

app.MapDefaultEndpoints();

app.Run();

// ── Types ────────────────────────────────────────────────────────────────────

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record CatalogProduct(int Id, string Name, string Description, decimal Price, string Category, int StockQuantity);

record ProductSummary(int Id, string Name, string Description, decimal Price, string Category, int StockQuantity, bool IsFeatured, string StockStatus);
