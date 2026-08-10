using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System.Text;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// End-to-end verification of the nOCR engine: train a database from rendered glyphs
/// (<see cref="NOcrTrainer"/>), then run the same recognition pipeline the app uses
/// (two-color + crop + splitter + <see cref="NOcrDb.GetMatch"/>) over rendered sentences and
/// verify the text round-trips.
/// </summary>
public class NOcrTrainerTests
{
    private const float FontSize = 50f;
    private const int MaxWrongPixels = 25;
    // The app default (SeOcr.NOcrPixelsAreSpace / seconv PixelsAreSpaceDefault).
    private const int PixelsAreSpace = 12;

    private static string TestFontName => SKTypeface.Default.FamilyName;

    private static NOcrDb Train(string characters, bool italic = false)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        var trainer = new NOcrTrainer();
        var settings = new NOcrTrainerSettings
        {
            FontNames = { TestFontName },
            FontSize = FontSize,
            CharactersToTrain = characters,
            IncludeItalic = italic,
        };
        var learned = trainer.Train(settings, db);
        Assert.True(learned > 0, "training should learn at least one character");
        return db;
    }

    private static string Recognize(NOcrDb db, string text, bool italic = false)
    {
        using var bmp = NOcrTrainer.RenderCharacterImage(text, TestFontName, FontSize, false, italic);
        Assert.NotNull(bmp);
        var parent = new NikseBitmap2(bmp!);
        parent.MakeTwoColor(200);
        parent.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, PixelsAreSpace, false, false, 25, false);

        var sb = new StringBuilder();
        var i = 0;
        while (i < letters.Count)
        {
            var item = letters[i];
            if (item.NikseBitmap == null)
            {
                sb.Append(item.SpecialCharacter);
            }
            else
            {
                var match = db.GetMatch(parent, letters, item, item.Top, true, MaxWrongPixels);
                if (match is { ExpandCount: > 0 })
                {
                    i += match.ExpandCount - 1;
                }

                sb.Append(match?.Text ?? "*");
            }

            i++;
        }

        return sb.ToString().Trim();
    }

    [Fact]
    public void TrainedDb_RoundTripsRenderedSentence()
    {
        var db = Train("HeloWrd123");

        var result = Recognize(db, "Hello World 123");

        Assert.Equal("Hello World 123", result);
    }

    [Fact]
    public void TrainedDb_RoundTripsAfterSaveAndReload()
    {
        var db = Train("Subtiles");
        try
        {
            db.Save();
            var reloaded = new NOcrDb(db.FileName);
            Assert.Equal(db.TotalCharacterCount, reloaded.TotalCharacterCount);

            var result = Recognize(reloaded, "Subtitle test");
            Assert.Equal("Subtitle test", result);
        }
        finally
        {
            if (File.Exists(db.FileName))
            {
                File.Delete(db.FileName);
            }
        }
    }

    [Fact]
    public void Trainer_SkipsAlreadyKnownCharacters()
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        var trainer = new NOcrTrainer();
        var settings = new NOcrTrainerSettings
        {
            FontNames = { TestFontName },
            FontSize = FontSize,
            CharactersToTrain = "ABC",
        };

        var firstRun = trainer.Train(settings, db);
        var countAfterFirst = db.TotalCharacterCount;
        var secondRun = trainer.Train(settings, db);

        Assert.True(firstRun > 0);
        Assert.Equal(countAfterFirst, db.TotalCharacterCount);
        Assert.Equal(0, secondRun);
    }

    [Fact]
    public void Trainer_UnknownGlyph_RecognizesAsAsterisk()
    {
        var db = Train("Helo");

        var result = Recognize(db, "Hello Zebra");

        Assert.StartsWith("Hello ", result);
        Assert.Contains("*", result);
    }
}
