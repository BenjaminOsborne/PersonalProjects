using System.Collections.Immutable;

namespace AccountProcessor.Components.Services
{
    public interface IWeatherService
    {
        Task<ImmutableArray<WeatherForecast>> GetAll();
    }

    public class WeatherForecast
    {
        public DateOnly Date { get; init; }
        public int TemperatureC { get; init; }
        public string? Summary { get; init; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    public class WeatherService : IWeatherService
    {
        public async Task<ImmutableArray<WeatherForecast>> GetAll()
        {
            await Task.Delay(10); //async delay

            var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
            var startDate = DateOnly.FromDateTime(DateTime.Now);
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            }).ToImmutableArray();
        }
    }
}
