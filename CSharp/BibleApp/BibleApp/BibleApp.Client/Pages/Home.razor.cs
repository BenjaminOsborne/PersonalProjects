using BibleApp.Client.ClientServices;
using Microsoft.AspNetCore.Components;

namespace BibleApp.Client.Pages;

public partial class Home
{
    [Inject] private IBibleService BibleService { get; init; } = null!;

    private IReadOnlyList<string> _books = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        var books = await BibleService.GetBooksAsync();
        if (books.IsSuccess)
        {
            _books = books.Value!;
        }
    }
}