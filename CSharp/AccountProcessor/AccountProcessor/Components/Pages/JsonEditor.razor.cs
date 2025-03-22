using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace AccountProcessor.Components.Pages;

public partial class JsonEditor
{
    [Inject] private IMatchModelService _modelService { get; init; } = null!;

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
            _OnSearchChange();
        }
    }

    protected override Task OnInitializedAsync()
    {
        _OnSearchChange(); //will load for initial empty search
        return Task.CompletedTask;
    }

    private void _OnSearchChange() =>
        LoadedJson = _modelService.DisplayRawModelJsonSearchResult(_search ?? string.Empty);
}