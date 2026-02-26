namespace AspireDemo.Web;

public class CatalogApiClient(HttpClient httpClient)
{
    public async Task<ProductSummary[]> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await httpClient.GetFromJsonAsync<ProductSummary[]>("/products", cancellationToken);
        return products ?? [];
    }
}

public record ProductSummary(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    int StockQuantity,
    bool IsFeatured,
    string StockStatus);
