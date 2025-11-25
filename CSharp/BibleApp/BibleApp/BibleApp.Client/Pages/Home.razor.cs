using BibleApp.Client.ClientServices;
using BibleApp.Core;
using Microsoft.AspNetCore.Components;

namespace BibleApp.Client.Pages;

public partial class Home
{
    [Inject] private IBibleService BibleService { get; init; } = null!;

    private IReadOnlyList<string> _books = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var bible = await BibleService.GetBiblesAsync();
        if (bible.IsSuccess)
        {
            _books = bible.Value!.MaterialiseMap(x => x.Translation);
        }
    }
}