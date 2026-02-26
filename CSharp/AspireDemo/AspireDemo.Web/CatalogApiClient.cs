namespace AspireDemo.Web;

public interface ICatalogApiClient
{
    Task<ProductSummary[]> GetProductsAsync(CancellationToken cancellationToken = default);
}

public class CatalogApiClient(HttpClient httpClient) : ICatalogApiClient
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
