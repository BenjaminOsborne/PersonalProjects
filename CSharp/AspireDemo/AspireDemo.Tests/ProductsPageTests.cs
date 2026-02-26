using AspireDemo.Web;
using AspireDemo.Web.Components.Pages;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;

namespace AspireDemo.Tests;

public class ProductsPageTests : MudBlazorTestContext
{
    // ── Shared test data ─────────────────────────────────────────────────────

    private static readonly ProductSummary Mug = new(
        Id: 1, Name: "Coffee Mug", Description: "16 oz mug",
        Price: 15.00m, Category: "Kitchen", StockQuantity: 200,
        IsFeatured: false, StockStatus: "In stock");

    private static readonly ProductSummary Kettle = new(
        Id: 2, Name: "Pour-Over Kettle", Description: "Gooseneck kettle",
        Price: 89.99m, Category: "Kitchen", StockQuantity: 30,
        IsFeatured: false, StockStatus: "In stock");

    private static readonly ProductSummary Keyboard = new(
        Id: 3, Name: "Mechanical Keyboard", Description: "TKL keyboard",
        Price: 129.99m, Category: "Electronics", StockQuantity: 0,
        IsFeatured: true, StockStatus: "Out of stock");

    private static readonly ProductSummary Mouse = new(
        Id: 4, Name: "Wireless Mouse", Description: "Ergonomic mouse",
        Price: 39.99m, Category: "Electronics", StockQuantity: 5,
        IsFeatured: false, StockStatus: "Low stock");

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Registers a mock that immediately returns the given products.</summary>
    private ICatalogApiClient MockClientReturning(params ProductSummary[] products)
    {
        var client = Substitute.For<ICatalogApiClient>();
        client.GetProductsAsync(Arg.Any<CancellationToken>())
              .Returns(Task.FromResult(products));
        Services.AddSingleton(client);
        return client;
    }

    /// <summary>
    /// Registers a mock backed by a TaskCompletionSource so we can control
    /// exactly when the async result arrives — useful for testing loading state.
    /// </summary>
    private (ICatalogApiClient client, TaskCompletionSource<ProductSummary[]> tcs) MockClientPending()
    {
        var tcs = new TaskCompletionSource<ProductSummary[]>();
        var client = Substitute.For<ICatalogApiClient>();
        client.GetProductsAsync(Arg.Any<CancellationToken>()).Returns(tcs.Task);
        Services.AddSingleton(client);
        return (client, tcs);
    }

    // ── Loading state ─────────────────────────────────────────────────────────

    [Fact]
    public void ShowsProgressBar_WhileDataIsLoading()
    {
        var (_, tcs) = MockClientPending();

        var cut = Render<Products>();

        // Data hasn't arrived yet — progress bar must be visible
        Assert.NotEmpty(cut.FindAll(".mud-progress-linear"));

        // Cleanup so the pending task doesn't leak across tests
        tcs.SetCanceled();
    }

    [Fact]
    public async Task HidesProgressBar_OnceDataLoads()
    {
        var (_, tcs) = MockClientPending();
        var cut = Render<Products>();

        tcs.SetResult([Mug]);

        await cut.WaitForStateAsync(() => !cut.FindAll(".mud-progress-linear").Any());

        Assert.Empty(cut.FindAll(".mud-progress-linear"));
    }

    // ── Empty state ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ShowsAlert_WhenApiReturnsNoProducts()
    {
        MockClientReturning(); // empty array

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-alert").Any());

        Assert.Contains("No products found", cut.Markup);
        Assert.Empty(cut.FindAll(".mud-card"));
    }

    // ── Product cards ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RendersOneCard_PerProduct()
    {
        MockClientReturning(Mug, Kettle, Keyboard, Mouse);

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Count == 4);

        Assert.Equal(4, cut.FindAll(".mud-card").Count);
    }

    [Fact]
    public async Task EachCard_ShowsProductNameAndDescription()
    {
        MockClientReturning(Keyboard);

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Any());

        Assert.Contains("Mechanical Keyboard", cut.Markup);
        Assert.Contains("TKL keyboard", cut.Markup);
    }

    [Fact]
    public async Task EachCard_ShowsFormattedPrice()
    {
        MockClientReturning(Keyboard); // Price = 129.99

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Any());

        Assert.Contains("$129.99", cut.Markup);
    }

    // ── Featured chip ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ShowsFeaturedChip_OnlyForFeaturedProducts()
    {
        MockClientReturning(Keyboard, Mouse); // Keyboard is featured, Mouse is not

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Count == 2);

        var featuredChips = cut.FindAll(".mud-chip-content")
                               .Where(c => c.TextContent.Contains("Featured"))
                               .ToList();

        Assert.Single(featuredChips);
    }

    [Fact]
    public async Task DoesNotShowFeaturedChip_WhenNoProductIsFeatured()
    {
        MockClientReturning(Mug, Mouse); // neither is featured

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Count == 2);

        Assert.DoesNotContain("Featured", cut.Markup);
    }

    // ── Stock status badge ────────────────────────────────────────────────────

    [Theory]
    [InlineData("In stock")]
    [InlineData("Low stock")]
    [InlineData("Out of stock")]
    public async Task ShowsStockStatusBadge_ForEachStatus(string expectedStatus)
    {
        var product = Mug with { StockStatus = expectedStatus };
        MockClientReturning(product);

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Any());

        Assert.Contains(expectedStatus, cut.Markup);
    }

    // ── Category grouping and ordering ────────────────────────────────────────

    [Fact]
    public async Task GroupsProducts_UnderCategoryHeadings()
    {
        MockClientReturning(Mug, Keyboard); // Kitchen and Electronics

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Count == 2);

        Assert.Contains("Kitchen", cut.Markup);
        Assert.Contains("Electronics", cut.Markup);
    }

    [Fact]
    public async Task OrdersCategories_Alphabetically()
    {
        MockClientReturning(Mug, Keyboard); // Kitchen and Electronics

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Count == 2);

        var electronicsPos = cut.Markup.IndexOf("Electronics", StringComparison.Ordinal);
        var kitchenPos = cut.Markup.IndexOf("Kitchen", StringComparison.Ordinal);

        Assert.True(electronicsPos < kitchenPos, "Electronics should appear before Kitchen");
    }

    [Fact]
    public async Task OrdersProductsWithinCategory_Alphabetically()
    {
        // Kettle (K) should appear after Coffee Mug (C) — same Kitchen category
        MockClientReturning(Kettle, Mug);

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Count == 2);

        var mugPos    = cut.Markup.IndexOf("Coffee Mug",      StringComparison.Ordinal);
        var kettlePos = cut.Markup.IndexOf("Pour-Over Kettle", StringComparison.Ordinal);

        Assert.True(mugPos < kettlePos, "Coffee Mug should appear before Pour-Over Kettle");
    }

    // ── API interaction ───────────────────────────────────────────────────────

    [Fact]
    public async Task CallsApiExactlyOnce_OnInitialisation()
    {
        var client = MockClientReturning(Mug);

        var cut = Render<Products>();

        await cut.WaitForStateAsync(() => cut.FindAll(".mud-card").Any());

        await client.Received(1).GetProductsAsync(Arg.Any<CancellationToken>());
    }
}
