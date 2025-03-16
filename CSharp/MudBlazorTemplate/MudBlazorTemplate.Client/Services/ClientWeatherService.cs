using System.Net.Http.Json;
using MudBlazorTemplate.Client.DTOs;

namespace MudBlazorTemplate.Client.Services;

public interface IClientWeatherService
{
    Task<IReadOnlyList<WeatherForecast>> GetAll();
}

public record ClientWeatherService(HttpClient Client, string Context) : IClientWeatherService
{
    public async Task<IReadOnlyList<WeatherForecast>> GetAll() =>
        await Client.GetFromJsonAsync<WeatherForecast[]>("/weather/getall") ?? throw new Exception("Could not load weather");
}