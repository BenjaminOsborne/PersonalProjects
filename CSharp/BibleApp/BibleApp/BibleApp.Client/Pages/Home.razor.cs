using BibleApp.Client.ClientServices;
using BibleApp.Core;
using Microsoft.AspNetCore.Components;

namespace BibleApp.Client.Pages;

public partial class Home
{
    [Inject] private IBibleService BibleService { get; init; } = null!;

    private bool _isLoading = true;
    private IReadOnlyList<BookStructure> _books = [];
    
    private ChapterStructure _selectedChapter;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        await _InitialiseAsync();
    }

    private async Task _InitialiseAsync()
    {
        var translations = await BibleService.GetTranslationsAsync();
        if (translations.IsFail)
        {
            return;
        }


        var translation = translations.Value!.FirstOrDefault();
        if (translation is null)
        {
            return;
        }

        var bible = await BibleService.GetBibleAsync(translation);
        if (bible.IsFail)
        {
            return;
        }

        _books = bible.Value!.Books;
        if (_books.IsEmpty())
        {
            return;
        }

        _isLoading = false;

    }
}