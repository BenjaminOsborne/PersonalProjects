namespace BibleApp.Core;

public record BibleId(string Translation);

public record BookId(string BookName);

public record ChapterId(BookId BookId, int ChapterNumber);

public record VerseId(ChapterId ChapterId, int VerseNumber);

public record Bible(BibleId Id, IReadOnlyList<Book> Books);

public record Book(BibleId BibleId, BookId Id, IReadOnlyList<Chapter> Chapters);

public record Chapter(ChapterId Id, IReadOnlyList<Verse> Verses);

public record Verse(VerseId Id, string Text);

public record BibleStructure(string Translation, IReadOnlyList<BookStructure> Books);

public record BookStructure(string BookName, IReadOnlyList<ChapterStructure> Chapters);

public record ChapterStructure(int ChapterNumber, IReadOnlyList<int> Verses);

public static class BibleExtensions
{
    public static BibleStructure ToStructure(this Bible bible) =>
        new(bible.Id.Translation, bible.Books
            .MaterialiseMap(book =>
                new BookStructure(book.Id.BookName, book.Chapters.MaterialiseMap(chapter =>
                    new ChapterStructure(chapter.Id.ChapterNumber, chapter.Verses.MaterialiseMap(verse =>
                        verse.Id.VerseNumber
                    ))))));
}