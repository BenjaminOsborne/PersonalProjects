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
        _RebuildModel();
        return Task.CompletedTask;
    }

    private void ClearMatch(ModelMatchItem row) =>
        _PerformActionWithRebuildModel(() => _modelService.DeleteMatchItem(row));

    private void _PerformActionWithRebuildModel(Func<bool> fnPerform)
    {
        var didChange = fnPerform();
        if (didChange)
        {
            _RebuildModel();
        }
    }

    private void _RebuildModel() =>
        Items = _modelService.GetAllModelMatches();
}