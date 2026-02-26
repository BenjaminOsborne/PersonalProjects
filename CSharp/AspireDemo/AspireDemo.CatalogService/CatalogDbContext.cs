using Microsoft.EntityFrameworkCore;

namespace AspireDemo.CatalogService;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}
