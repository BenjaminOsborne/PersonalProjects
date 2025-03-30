using System.Collections.Immutable;
using AccountProcessor.Client.ClientServices;
using AccountProcessor.Core;
using AccountProcessor.Core.Services;
using Microsoft.AspNetCore.Components;

namespace AccountProcessor.Client.Pages;

public partial class ModelEditor
{
    private record SectionViewModel(
        CategoryHeader Category,
        SectionHeader Section,
        int MatchItemCount,
        string? MonthOnlyDisplay)
    {
        public bool MatchesSearch(string searchString) =>
            SearchHelper.MatchesSearch(
                lowerSearch: searchString.ToLower(),
                Category.Name,
                Section.Name,
                MonthOnlyDisplay,
                MatchItemCount.ToString()
                );
    }

    [Inject] private IClientMatchModelService _modelService { get; init; } = null!;

    private IReadOnlyList<ModelMatchItem>? MatchItems;
    private IReadOnlyList<SectionViewModel>? SectionItems;
    
    private string? _searchMatches;
    private string? _searchSections;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await _RebuildModelAsync();
    }

    private Task ClearMatchAsync(ModelMatchItem row) =>
        _PerformActionWithRebuildModel(() => _modelService.DeleteMatchItemAsync(row));

    private Task DeleteSectionAsync(SectionViewModel section) =>
        _PerformActionWithRebuildModel(() => _modelService.DeleteSectionAsync(section.Section));

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
            var sm = result.Result!.Sections;
            SectionItems = sm.Select(_ToViewModel).ToImmutableArray();
            MatchItems = sm.SelectMany(x => x.MatchItems).ToImmutableArray();
        }
    }

    private static SectionViewModel _ToViewModel(SectionAndMatches sm) =>
        new (
            sm.Section.Parent,
            sm.Section,
            sm.MatchItems.Length,
            MonthOnlyDisplay: sm.Section.Month?.ToString("yyyy/MM"));

    private bool _ApplyMatchSearchFilter(ModelMatchItem arg) =>
        _searchMatches.IsNullOrWhiteSpace() ||
        arg.MatchesSearch(_searchMatches!);

    private bool _ApplySectionSearchFilter(SectionViewModel arg) =>
        _searchSections.IsNullOrWhiteSpace() ||
        arg.MatchesSearch(_searchSections!);
}