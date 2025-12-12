using BibleApp.Client.ClientServices;
using BibleApp.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BibleApp.Client.Pages;

public partial class Home
{
    private record BookViewModel(BookId Book, IReadOnlyList<ChapterViewModel> Chapters);

    private record ChapterViewModel(Chapter Chapter)
    {
        public ElementReference? Ref { get; set; }
        public string InstanceKey { get; } = Guid.NewGuid().ToString();
    }

    [Inject] private IBibleService BibleService { get; init; } = null!;
    [Inject] private IJSRuntime JS { get; init; } = null!;

    private bool _isLoading = true;
    private IReadOnlyList<BookStructure> _books = [];

    private TranslationId? _translation;
    private ChapterStructure _selectedChapter;
    private BookViewModel? _selectedBook;

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

    private async Task _SelectChapterAsync(BookStructure book, ChapterStructure chapter, bool isSelected)
    {
        if (!isSelected)
        {
            return;
        }
        _selectedChapter = chapter;
        await _SelectBookAsync(book);

        var found = _selectedBook?.Chapters.FirstOrDefault(x => x.Chapter.Id.ChapterNumber == chapter.ChapterNumber);
        if(found != null)
        {
            var element = found.Ref;
            if (element != null)
            {
                await JS.InvokeVoidAsync("scrollToElement", element);
            }
        }
    }

    private async Task _SelectBookAsync(BookStructure book, bool isSelected = true)
    {
        if (!isSelected)
        {
            return; //Do nothing for now...
        }
        
        var bookId = new BookId(BookName: book.BookName);
        if (_selectedBook != null && _selectedBook.Book.EqualsSafe(bookId))
        {
            return;
        }

        var fetched = await BibleService.GetBookAsync(_translation!, bookId);
        if (fetched.IsFail)
        {
            return;
        }

        _selectedBook = new BookViewModel(
            Book: bookId,
            Chapters: fetched.Value!.Chapters
                .MaterialiseMap(c => new ChapterViewModel(c)));
    }
}