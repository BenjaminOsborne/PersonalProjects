using AccountProcessor.Client.ClientServices;
using Microsoft.AspNetCore.Components;

namespace AccountProcessor.Client.Pages;

public partial class JsonEditor : IAsyncDisposable
{
    private readonly TaskTracker _taskTracker = new ();
    [Inject] private IClientMatchModelService _modelService { get; init; } = null!;

    private string? LoadedJson;
    
    private string? _search;

    public string? BindSearch
    {
        get => _search;
        set
        {
            if (value == _search)
            {
                return;
            }
            _search = value;
            _taskTracker.TrackTask(_OnSearchChangeAsync());
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await _OnSearchChangeAsync(); //will load for initial empty search
    }

    public ValueTask DisposeAsync() =>
        _taskTracker.DisposeAsync();

    private async Task _OnSearchChangeAsync()
    {
        var result = await _modelService.DisplayRawModelJsonSearchResultAsync(_search ?? string.Empty);
        if (result.IsSuccess)
        {
            LoadedJson = result.Result!.Json;
        }
        await InvokeAsync(StateHasChanged); //Required else refresh not triggered as property-binding is fire-and-forget
    }
}