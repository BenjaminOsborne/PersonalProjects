using AccountProcessor.ClientServices;
using AccountProcessor.Services;
using Microsoft.AspNetCore.Components;

namespace AccountProcessor.Components.Pages;

public partial class ModelEditor
{
    [Inject] private IClientMatchModelService _modelService { get; init; } = null!;

    private IReadOnlyList<ModelMatchItem>? Items;
    private string? _searchString;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await _RebuildModelAsync();
    }

    private Task ClearMatchAsync(ModelMatchItem row) =>
        _PerformActionWithRebuildModel(() => _modelService.DeleteMatchItemAsync(row));

    private async Task _PerformActionWithRebuildModel(Func<Task<Result>> fnPerform)
    {
        var didChange = await fnPerform();
        if (didChange.IsSuccess)
        {
            await _RebuildModelAsync();
        }
    }

    private async Task _RebuildModelAsync()
    {
        var result = await _modelService.GetAllModelMatchesAsync();
        if (result.IsSuccess)
        {
            Items = result.Result;
        }
    }

    private bool _ApplySearchFilter(ModelMatchItem arg) =>
        _searchString.IsNullOrWhiteSpace() ||
        arg.MatchesSearch(_searchString!);
}