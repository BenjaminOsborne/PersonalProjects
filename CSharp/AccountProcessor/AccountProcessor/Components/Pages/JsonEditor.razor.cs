using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace AccountProcessor.Components.Pages;

public partial class JsonEditor
{
    [Inject]
    private IMatchModelService _modelService { get; init; }

    private string? _loadedJson;

    protected override Task OnInitializedAsync()
    {
        _loadedJson = _modelService.LoadRawModelJson();
        return Task.CompletedTask;
    }
}