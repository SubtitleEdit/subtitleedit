using Nikse.SubtitleEdit.Logic.Ocr;

namespace UITests.Logic.Ocr;

public class NOcrDbExpandedMatchTests
{
    [Fact]
    public void GetMatchExpanded_WhenMultipleExactCandidatesMatch_ReturnsFirstCandidateInDbOrder()
    {
        var db = CreateDb();
        var firstExact = CreateExpandedChar("first-exact", width: 5, height: 3);
        var secondExact = CreateExpandedChar("second-exact", width: 5, height: 3);
        db.OcrCharactersExpanded = new List<NOcrChar> { firstExact, secondExact };
        var letters = CreateLetters();

        var match = db.GetMatchExpanded(CreateParentBitmap(), letters[0], 0, letters);

        Assert.Same(firstExact, match);
    }

    [Fact]
    public void GetMatchExpanded_WhenExactAndRelaxedCandidatesMatch_PrefersExactPhase()
    {
        var db = CreateDb();
        var relaxed = CreateExpandedChar("relaxed", width: 10, height: 6);
        var exact = CreateExpandedChar("exact", width: 5, height: 3);
        db.OcrCharactersExpanded = new List<NOcrChar> { relaxed, exact };
        var letters = CreateLetters();

        var match = db.GetMatchExpanded(CreateParentBitmap(), letters[0], 0, letters);

        Assert.Same(exact, match);
    }

    [Fact]
    public void GetMatchExpanded_WhenOnlyRelaxedCandidatesMatch_ReturnsFirstCandidateInDbOrder()
    {
        var db = CreateDb();
        var firstRelaxed = CreateExpandedChar("first-relaxed", width: 10, height: 6);
        var secondRelaxed = CreateExpandedChar("second-relaxed", width: 10, height: 6);
        db.OcrCharactersExpanded = new List<NOcrChar> { firstRelaxed, secondRelaxed };
        var letters = CreateLetters();

        var match = db.GetMatchExpanded(CreateParentBitmap(), letters[0], 0, letters);

        Assert.Same(firstRelaxed, match);
    }

    private static NOcrDb CreateDb()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr");
        return new NOcrDb(fileName);
    }

    private static NOcrChar CreateExpandedChar(string text, int width, int height, IEnumerable<NOcrLine>? foregroundLines = null, IEnumerable<NOcrLine>? backgroundLines = null)
    {
        var item = new NOcrChar(text)
        {
            Width = width,
            Height = height,
            MarginTop = 0,
            ExpandCount = 2,
        };
        item.LinesForeground.AddRange(foregroundLines ?? new[]
        {
            new NOcrLine(new OcrPoint(0, 0), new OcrPoint(0, 0)),
        });
        item.LinesBackground.AddRange(backgroundLines ?? []);
        return item;
    }

    private static NikseBitmap2 CreateParentBitmap()
    {
        var bitmap = new NikseBitmap2(30, 20);
        bitmap.SetAlpha(5, 5, byte.MaxValue);
        return bitmap;
    }

    private static List<ImageSplitterItem2> CreateLetters()
    {
        return new List<ImageSplitterItem2>
        {
            new ImageSplitterItem2(5, 5, new NikseBitmap2(2, 3)),
            new ImageSplitterItem2(8, 5, new NikseBitmap2(2, 3)),
        };
    }
}
