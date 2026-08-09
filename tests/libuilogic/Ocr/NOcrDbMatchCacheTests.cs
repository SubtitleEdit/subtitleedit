using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Verifies that the match-result cache in <see cref="NOcrDb"/> is transparent: repeated calls
/// return the same results, and database mutations (Add/Remove) invalidate cached results.
/// </summary>
public class NOcrDbMatchCacheTests
{
    private static string TestFontName => SKTypeface.Default.FamilyName;

    private static (NOcrDb Db, NikseBitmap2 Parent, List<ImageSplitterItem2> Letters) SetUp(string trainChars, string renderText)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        var trainer = new NOcrTrainer();
        trainer.Train(new NOcrTrainerSettings
        {
            FontNames = { TestFontName },
            FontSize = 50,
            CharactersToTrain = trainChars,
        }, db);

        using var bmp = NOcrTrainer.RenderCharacterImage(renderText, TestFontName, 50, false, false);
        var parent = new NikseBitmap2(bmp!);
        parent.MakeTwoColor(200);
        parent.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, 10, false, false, 25, false);
        return (db, parent, letters);
    }

    [Fact]
    public void GetMatch_RepeatedCalls_ReturnSameResults()
    {
        var (db, parent, letters) = SetUp("abc", "abc abc abc");

        var first = new List<string?>();
        var second = new List<string?>();
        foreach (var round in new[] { first, second })
        {
            foreach (var item in letters.Where(l => l.NikseBitmap != null))
            {
                var match = db.GetMatch(parent, letters, item, item.Top, true, 25);
                round.Add(match?.Text);
            }
        }

        Assert.Equal(first, second);
        Assert.Contains("a", first);
        Assert.Contains("b", first);
        Assert.Contains("c", first);
    }

    [Fact]
    public void GetMatch_AddInvalidatesCachedMiss()
    {
        var (db, parent, letters) = SetUp("ab", "abz");

        var glyphs = letters.Where(l => l.NikseBitmap != null).ToList();
        var zItem = glyphs[^1];

        // Cache the miss for 'z'.
        var missing = db.GetMatch(parent, letters, zItem, zItem.Top, true, 25);
        Assert.Null(missing);

        // Now train 'z' into the same db - the cached miss must be discarded.
        var trainer = new NOcrTrainer();
        trainer.Train(new NOcrTrainerSettings
        {
            FontNames = { TestFontName },
            FontSize = 50,
            CharactersToTrain = "z",
        }, db);

        var found = db.GetMatch(parent, letters, zItem, zItem.Top, true, 25);
        Assert.NotNull(found);
        Assert.Equal("z", found!.Text);
    }

    [Fact]
    public void GetMatch_RemoveInvalidatesCachedHit()
    {
        var (db, parent, letters) = SetUp("ab", "ab");

        var glyphs = letters.Where(l => l.NikseBitmap != null).ToList();
        var aItem = glyphs[0];

        var match = db.GetMatch(parent, letters, aItem, aItem.Top, true, 25);
        Assert.NotNull(match);

        db.Remove(match!);

        var afterRemove = db.GetMatch(parent, letters, aItem, aItem.Top, true, 25);
        Assert.True(afterRemove == null || !ReferenceEquals(afterRemove, match));
    }

    [Fact]
    public void GetMatch_DifferentParameters_DoNotShareCacheEntries()
    {
        var (db, parent, letters) = SetUp("a", "a");

        var item = letters.First(l => l.NikseBitmap != null);

        // Prime the cache with a strict budget, then query with a generous one; the two
        // parameter sets must not alias each other.
        var strict = db.GetMatch(parent, letters, item, item.Top, false, 0);
        var generous = db.GetMatch(parent, letters, item, item.Top, true, 25);

        Assert.NotNull(generous);
        Assert.True(strict == null || strict.Text == generous!.Text);
    }
}
