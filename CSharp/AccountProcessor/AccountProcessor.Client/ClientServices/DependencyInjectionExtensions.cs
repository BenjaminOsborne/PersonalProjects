namespace AccountProcessor.Client.ClientServices;

public static class DependencyInjectionExtensions
{
    public static void ConfigureClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IClientExcelFileService, ClientExcelFileService>();
        services.AddSingleton<IClientMatchModelService, ClientMatchModelService>();
        services.AddSingleton<IClientTransactionCategoriser, ClientTransactionCategoriser>();

        services.AddSingleton(_ =>
        {
            var uri = configuration["BackendUrl"];
            return new HttpClient { BaseAddress = new Uri(uri!) };
        });
    }
}