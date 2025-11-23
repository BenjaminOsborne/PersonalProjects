namespace BibleApp.Client.ClientServices;

public static class DependencyInjectionExtensions
{
    public static void ConfigureClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBibleService, BibleService>();

        services.AddSingleton(_ =>
        {
            var uri = configuration["BackendUrl"];
            return new HttpClient { BaseAddress = new Uri(uri!) };
        });
    }
}