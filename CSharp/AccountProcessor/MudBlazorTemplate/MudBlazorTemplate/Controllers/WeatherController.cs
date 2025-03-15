using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using MudBlazorTemplate.Client.DTOs;

namespace MudBlazorTemplate.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    [HttpGet("getall")]
    public IReadOnlyList<WeatherForecast> GetAll()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
        return Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast(
                Date: startDate.AddDays(index),
                TemperatureC: Random.Shared.Next(-20, 55),
                Summary: summaries[Random.Shared.Next(summaries.Length)]))
            .ToImmutableList();
    }
}