using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WebApiIntegrationDemo.Tests;

public class WeatherForecastIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsOk()
    {
        var response = await _client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFiveForecasts()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecastDto[]>("/weatherforecast");

        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Length);
    }

    [Fact]
    public async Task GetWeatherForecast_EachForecastHasADate()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecastDto[]>("/weatherforecast");

        Assert.NotNull(forecasts);
        Assert.All(forecasts, f => Assert.NotEqual(default, f.Date));
    }

    private record WeatherForecastDto(DateOnly Date, int TemperatureC, int TemperatureF, string? Summary);
}
