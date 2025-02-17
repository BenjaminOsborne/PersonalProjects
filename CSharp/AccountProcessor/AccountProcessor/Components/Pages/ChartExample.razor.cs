using System.Collections.Immutable;
using PSC.Blazor.Components.Chartjs.Models.Bar;
using PSC.Blazor.Components.Chartjs;
using PSC.Blazor.Components.Chartjs.Models.Common;
using PSC.Blazor.Components.Chartjs.Models.Line;

namespace AccountProcessor.Components.Pages;

public partial class ChartExample
{
    private BarChartConfig _barConfig;
    private Chart? _barChart;

    private LineChartConfig _lineConfig;
    private Chart? _lineChart;

    protected override Task OnInitializedAsync()
    {
        _barConfig = _InitialiseBarChart();

        _lineConfig = _CreateLineChartConfig();
        return Task.CompletedTask;
    }

    /// <summary> https://github.com/erossini/BlazorChartjs/blob/main/ChartjsDemo/Pages/BarSimple.razor </summary>
    private static BarChartConfig _InitialiseBarChart() =>
        new()
        {
            Options = new Options
            {
                Plugins = new Plugins
                {
                    Legend = new Legend
                    {
                        Align = Align.Center,
                        Display = false,
                        Position = LegendPosition.Right
                    }
                },
                Scales = new Dictionary<string, Axis>
                {
                    {
                        Scales.XAxisId, new Axis
                        {
                            Stacked = true,
                            Ticks = new Ticks
                            {
                                MaxRotation = 0,
                                MinRotation = 0
                            }
                        }
                    },
                    {
                        Scales.YAxisId, new Axis
                        {
                            Stacked = true
                        }
                    }
                }
            },
            Data =
            {
                Labels = BarDataExamples.SimpleBarText.ToList(),
                Datasets =
                [
                    new BarDataset
                    {
                        Label = "Value",
                        Data = BarDataExamples.SimpleBar.Select(l => l.Value).ToList(),
                        BackgroundColor = SampleColors.Palette1.ToList(),
                        BorderColor = SampleColors.PaletteBorder1.ToList(),
                        BorderWidth = 1
                    }
                ]
            }
        };

    /// <summary> https://github.com/erossini/BlazorChartjs/blob/main/ChartjsDemo/Pages/LineSimple.razor </summary>
    private static LineChartConfig _CreateLineChartConfig()
    {
        var config = new LineChartConfig();
        config.Options = new Options
        {
            Responsive = true,
            MaintainAspectRatio = false,
            //Interaction = new Interaction { Mode = InteractionMode.Dataset },
            //Animation = true,
            Plugins = new Plugins
            {
                Zoom = new Zoom
                {
                    Enabled = true,
                    Mode = "x",
                    ZoomOptions = new ZoomOptions
                    {
                        Wheel = new Wheel {Enabled = true}, //ModifierKey = "shift"
                        Pinch = new Pinch {Enabled = true},
                        //Drag = new Drag { Enabled = true, ModifierKey = "alt" }
                    }
                }
            }
        };

        config.Data.Labels = LineDataExamples.SimpleLineText;

        config.Data.Datasets = [
            new LineDataset
            {
                Label = "My First Dataset",
                Data = LineDataExamples.SimpleLine.ToList(),
                BorderColor = SampleColors.PaletteBorder1.FirstOrDefault(),
                Tension = 0.1M,
                Fill = true,
                PointRadius = 15,
                PointStyle = PointStyle.Cross
            }
        ];
        config.Data.Datasets.Add(new LineDataset()
        {
            Label = "My Second Dataset",
            Data = LineDataExamples.SimpleLine2.ToList(),
            BackgroundColor = "rgba(75,192,192,0.2)",
            BorderColor = "rgba(75,192,192,1)",
            Fill = true
        });

        return config;
    }

    private void _AddLineChartValue() =>
        _lineChart?.AddData(["August"], 0, [new Random().Next(0, 200)]);
}

public static class BarDataExamples
{
    public class DataItem
    {
        public string? Name { get; init; }
        public decimal? Value { get; init; }
    }

    public static readonly ImmutableList<string> SimpleBarText = ["January", "February", "March", "April", "May", "June", "July"];
    public static readonly ImmutableList<DataItem> SimpleBar =
    [
        new() {Name = "January", Value = 65},
        new() {Name = "February", Value = 59},
        new() {Name = "March", Value = 80},
        new() {Name = "April", Value = 81},
        new() {Name = "May", Value = 56},
        new() {Name = "June", Value = 55},
        new() {Name = "July", Value = 40}
    ];

    public static readonly ImmutableList<string> GroupedLabels = ["1900", "1950", "1999", "2050"];
    public static readonly ImmutableList<decimal?> Grouped1 = [133, 221, 783, 2478];
    public static readonly ImmutableList<decimal?> Grouped2 = [408, 547, 675, 734];

    public static readonly ImmutableList<string> CallbackLabels = ["Q1", "Q2", "Q3", "Q4"];
    public static readonly ImmutableList<decimal?> CallbackValues = [50000, 60000, 70000, 1800000];
}

public static class SampleColors
{
    public static readonly ImmutableList<string> Palette1 =
    [
        "rgba(255, 99, 132, 0.2)",
        "rgba(255, 159, 64, 0.2)",
        "rgba(255, 205, 86, 0.2)",
        "rgba(75, 192, 192, 0.2)",
        "rgba(54, 162, 235, 0.2)",
        "rgba(153, 102, 255, 0.2)",
        "rgba(201, 203, 207, 0.2)"
    ];

    public static readonly ImmutableList<string> PaletteBorder1 =
    [
        "rgb(255, 99, 132)",
        "rgb(255, 159, 64)",
        "rgb(255, 205, 86)",
        "rgb(75, 192, 192)",
        "rgb(54, 162, 235)",
        "rgb(153, 102, 255)",
        "rgb(201, 203, 207)"
    ];
}

public static class LineDataExamples
{
    public static List<string> SimpleLineText = new List<string>() { "January", "February", "March", "April", "May", "June", "July" };
    public static List<decimal?> SimpleLine = new List<decimal?>() { 65, 59, 80, 81, 86, 55, 40 };
    public static List<decimal?> SimpleLine2 = new List<decimal?>() { 33, 25, 35, 51, 54, 76, 60 };
    public static List<decimal?> SimpleLine3 = new List<decimal?>() { 53, 91, 39, 61, 39, 87, 23 };

    // stepped line
    public static List<string> StepLineText = new List<string>() { "Day 1", "Day 2", "Day 3", "Day 4", "Day 5", "Day 6" };
    public static List<decimal?> StepLine = new List<decimal?>() { 65, 59, 80, 81, 86, 55, 40 };

    // custom code
    public static List<string> CustomLineText = new List<string>() { "January", "February", "March", "April", "May", "June" };
    public static List<decimal?> CustomLine = new List<decimal?>() { 60, 80, 81, 56, 55, 40 };

    // multi axes
    public static List<string> MultiAxesLineText = new List<string>() {
        "January;2015", "February;2015;Y", "March;2015",
        "January;2016", "February;2016;Y", "March;2016" };
    public static List<decimal?> MultiAxesLine = new List<decimal?>() { 12, 19, 3, 5, 2, 3 };

    public static List<decimal?> BreakLine = new List<decimal?>() { 0, 20, 20, 60, 60, 120, null, 180, 120, 125, 105, 110, 170 };
    public static List<string> BreakLineText = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" };
}