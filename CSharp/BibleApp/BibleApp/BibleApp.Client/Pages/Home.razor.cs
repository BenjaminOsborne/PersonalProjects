using BibleApp.Client.ClientServices;
using BibleApp.Core;
using Microsoft.AspNetCore.Components;

namespace BibleApp.Client.Pages;

public partial class Home
{
    [Inject] private IBibleService BibleService { get; init; } = null!;

    private bool _isLoading = true;
    private IReadOnlyList<BookStructure> _books = [];

    private TranslationId? _translation;
    private ChapterStructure _selectedChapter;
    private Book? _selectedBook;

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
        
        _translation = translations.Value!.FirstOrDefault();
        if (_translation is null)
        {
            return;
        }

        var bible = await BibleService.GetBibleAsync(_translation);
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

    private async Task _SelectChapterAsync(BookStructure book, ChapterStructure chapter)
    {
        _selectedChapter = chapter;
        await _SelectBookAsync(book);
    }

    private async Task _SelectBookAsync(BookStructure book, bool isSelected = true)
    {
        if (!isSelected)
        {
            return; //Do nothing for now...
        }
        var fetched = await BibleService.GetBookAsync(_translation!, new(BookName: book.BookName));
        if (fetched.IsFail)
        {
            return;
        }
        _selectedBook = fetched.Value;
    }
}