using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System.Text;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// <see cref="NOcrDbOptimizer"/> drops lines that don't tell an entry apart from its lookalikes;
/// what it keeps must still read the trained text and still reject the other characters.
/// </summary>
public class NOcrDbOptimizerTests
{
    private const float FontSize = 50f;
    private const string Characters = "HeloWrd123ilj!";

    private static string TestFontName => SKTypeface.Default.FamilyName;

    private static NOcrDb Train()
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        var settings = new NOcrTrainerSettings
        {
            FontNames = { TestFontName },
            FontSize = FontSize,
            CharactersToTrain = Characters,
            NumberOfLineSegments = 100,
        };
        Assert.True(new NOcrTrainer().Train(settings, db) > 0);
        return db;
    }

    private static NOcrDbOptimizerSettings Settings(int minLines) => new()
    {
        FontNames = { TestFontName },
        FontSizes = { FontSize },
        MinLines = minLines,
    };

    private static string Recognize(NOcrDb db, string text)
    {
        using var bmp = NOcrTrainer.RenderCharacterImage(text, TestFontName, FontSize, false, false);
        Assert.NotNull(bmp);
        var parent = new NikseBitmap2(bmp!);
        parent.MakeTwoColor(200);
        parent.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, 12, false, false, 25, false);
        var sb = new StringBuilder();
        for (var i = 0; i < letters.Count; i++)
        {
            var item = letters[i];
            if (item.NikseBitmap == null)
            {
                sb.Append(item.SpecialCharacter);
                continue;
            }

            var match = db.GetMatch(parent, letters, item, item.Top, true, 25);
            if (match is { ExpandCount: > 0 })
            {
                i += match.ExpandCount - 1;
            }

            sb.Append(match?.Text ?? "*");
        }

        return sb.ToString().Trim();
    }

    private static int LineCount(NOcrChar oc) => oc.LinesForeground.Count + oc.LinesBackground.Count;

    [Fact]
    public void RemovesLinesAndStillReadsTrainedText()
    {
        var db = Train();
        var before = db.OcrCharacters.ToDictionary(oc => oc, LineCount);
        Assert.Equal("Hello World 123", Recognize(db, "Hello World 123"));

        var result = NOcrDbOptimizer.Optimize(db, Settings(minLines: 10));

        Assert.NotNull(result);
        Assert.True(result!.LinesAfter < result.LinesBefore, $"{result.LinesBefore} -> {result.LinesAfter}");
        Assert.Equal(before.Values.Sum(), result.LinesBefore);
        foreach (var oc in db.OcrCharacters)
        {
            Assert.InRange(LineCount(oc), Math.Min(before[oc], 10), before[oc]);
        }

        Assert.Equal("Hello World 123", Recognize(db, "Hello World 123"));
        Assert.Equal("ill", Recognize(db, "ill"));
    }

    [Fact]
    public void LookalikesKeepAtLeastOneWrongPoint()
    {
        var db = Train();

        NOcrDbOptimizer.Optimize(db, Settings(minLines: 1));

        // The exact-size check accepts the first entry with zero wrong points, so every
        // other trained glyph must still have at least one against each entry.
        foreach (var text in new[] { "i", "l", "j", "1", "!" })
        {
            using var bmp = NOcrTrainer.RenderCharacterImage("H   " + text, TestFontName, FontSize, false, false);
            var nb = new NikseBitmap2(bmp!);
            nb.MakeTwoColor(200);
            nb.CropTop(0, new SKColor(0, 0, 0, 0));
            var glyph = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(nb, 10, false, false, 25, false)[2].NikseBitmap!;
            foreach (var oc in db.OcrCharacters.Where(oc => oc.Text != text && !(oc.Text is "I" or "l" && text is "I" or "l")))
            {
                Assert.False(NOcrDb.IsMatch(glyph, oc, 0), $"'{oc.Text}' matches '{text}' with no wrong points");
            }
        }
    }

    [Fact]
    public void EntriesAtOrBelowMinLinesAreUnchanged()
    {
        var db = Train();
        var max = db.OcrCharacters.Max(LineCount);
        var before = db.OcrCharacters.Select(oc => oc.LinesForeground.Concat(oc.LinesBackground).ToList()).ToList();

        var result = NOcrDbOptimizer.Optimize(db, Settings(minLines: max));

        Assert.Equal(result!.LinesBefore, result.LinesAfter);
        for (var i = 0; i < before.Count; i++)
        {
            Assert.Equal(before[i], db.OcrCharacters[i].LinesForeground.Concat(db.OcrCharacters[i].LinesBackground));
        }
    }

    [Fact]
    public void AbortLeavesDatabaseUnchanged()
    {
        var db = Train();
        var before = db.OcrCharacters.Sum(LineCount);

        var result = NOcrDbOptimizer.Optimize(db, Settings(minLines: 10), abortRequested: () => true);

        Assert.Null(result);
        Assert.Equal(before, db.OcrCharacters.Sum(LineCount));
    }

    [Fact]
    public void GetFreeBackupFileName_NeverReturnsAnExistingBackup()
    {
        var folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(folder);
        try
        {
            var fileName = Path.Combine(folder, "Latin.nocr");
            Assert.Equal(fileName + ".bak", NOcrDbOptimizer.GetFreeBackupFileName(fileName));

            File.WriteAllText(fileName + ".bak", "original");
            Assert.Equal(fileName + ".2.bak", NOcrDbOptimizer.GetFreeBackupFileName(fileName));

            File.WriteAllText(fileName + ".2.bak", "second");
            Assert.Equal(fileName + ".3.bak", NOcrDbOptimizer.GetFreeBackupFileName(fileName));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
