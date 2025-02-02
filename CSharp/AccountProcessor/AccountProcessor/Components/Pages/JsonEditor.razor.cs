using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace AccountProcessor.Components.Pages;

public partial class JsonEditor
{
    [Inject]
    private IMatchModelService _modelService { get; init; }

    private string? LoadedJson;
    
    private string? Search;

    protected override Task OnInitializedAsync()
    {
        LoadedJson = _modelService.LoadRawModelJson();
        return Task.CompletedTask;
    }

    private void PerformSearch(string? search)
    {
        Search = search;
        LoadedJson = _modelService.DisplaySearchResult(Search ?? string.Empty);
    }
}