namespace BibleApp.Test;

public class FileLoaderTests
{
    [Test]
    public async Task Load_bibles()
    {
        var bibles = await Source.FileLoader.LoadAllAsync();
        Assert.That(bibles.Count, Is.EqualTo(2));

        foreach (var bible in bibles)
        {
            Assert.That(bible.Books.Count, Is.EqualTo(66));
            foreach (var book in bible.Books)
            {
                Assert.That(book.Chapters.Count, Is.GreaterThanOrEqualTo(1));

                for (var cNx = 0; cNx < book.Chapters.Count; cNx++)
                {
                    var chapter = book.Chapters[cNx];
                    Assert.That(chapter.Number, Is.EqualTo(cNx + 1));
                    Assert.That(chapter.Verses.Count, Is.GreaterThanOrEqualTo(2));

                    for (var vNx = 0; vNx < chapter.Verses.Count; vNx++)
                    {
                        var verse = chapter.Verses[vNx];
                        Assert.That(verse.Number, Is.EqualTo(vNx+1));
                        Assert.That(verse.Text, Is.Not.Null);
                    }
                }
            }
        }
    }
}