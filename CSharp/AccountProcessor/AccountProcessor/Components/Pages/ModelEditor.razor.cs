using System.Collections.Immutable;
using AccountProcessor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace AccountProcessor.Components.Pages;

public partial class ModelEditor
{
    [Inject]
    private IMatchModelService _modelService { get; init; }

    private ImmutableArray<ModelMatchItem>? Items;

    protected override Task OnInitializedAsync()
    {
        Items = _modelService.GetAllModelMatches();
        return Task.CompletedTask;
    }

    private void ClearMatch(ModelMatchItem row)
    {
        //TODO
    }
}