using AspireDemo.CatalogService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Register EF Core with Aspire's Npgsql integration.
// "catalogdb" matches the database name defined in AppHost.
builder.AddNpgsqlDbContext<CatalogDbContext>("catalogdb");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// Migrate schema and seed data on startup
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.EnsureCreatedAsync();

    if (!await db.Products.AnyAsync())
    {
        db.Products.AddRange(
            new Product { Name = "Mechanical Keyboard",  Description = "Tactile 87-key TKL keyboard with RGB backlighting",     Price = 129.99m, Category = "Electronics", StockQuantity = 42 },
            new Product { Name = "USB-C Hub",            Description = "7-in-1 hub with 4K HDMI, PD charging, and SD card slot", Price =  45.00m, Category = "Electronics", StockQuantity = 120 },
            new Product { Name = "Wireless Mouse",       Description = "Ergonomic wireless mouse with 3-year battery life",       Price =  39.99m, Category = "Electronics", StockQuantity = 85 },
            new Product { Name = "Ergonomic Chair",      Description = "Lumbar-support mesh chair with adjustable armrests",     Price = 299.00m, Category = "Furniture",    StockQuantity = 18 },
            new Product { Name = "Standing Desk",        Description = "Electric height-adjustable desk, 160 × 80 cm",           Price = 499.00m, Category = "Furniture",    StockQuantity = 7  },
            new Product { Name = "Monitor Arm",          Description = "Single-arm VESA mount with cable management",            Price =  59.99m, Category = "Furniture",    StockQuantity = 34 },
            new Product { Name = "Coffee Mug",           Description = "16 oz insulated ceramic mug — keeps drinks hot 6 hrs",  Price =  15.00m, Category = "Kitchen",      StockQuantity = 200 },
            new Product { Name = "Pour-Over Kettle",     Description = "Gooseneck electric kettle with temperature control",     Price =  89.99m, Category = "Kitchen",      StockQuantity = 30 }
        );
        await db.SaveChangesAsync();
    }
}

app.MapGet("/", () => "Catalog Service is running. Navigate to /products.");

app.MapGet("/products", async (CatalogDbContext db) =>
    await db.Products.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync())
    .WithName("GetProducts");

app.MapGet("/products/{id:int}", async (int id, CatalogDbContext db) =>
    await db.Products.FindAsync(id) is Product p ? Results.Ok(p) : Results.NotFound())
    .WithName("GetProductById");

app.MapDefaultEndpoints();

app.Run();
