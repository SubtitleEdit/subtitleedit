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

    [Theory]
    [InlineData("é", true)]
    [InlineData("ø", true)]
    [InlineData("\"", true)]
    [InlineData("%", true)]
    [InlineData("«", true)]
    [InlineData("»", true)]
    [InlineData("‹", true)]
    [InlineData("›", true)]
    [InlineData("“", true)]
    [InlineData("”", true)]
    [InlineData("„", true)]
    [InlineData("…", true)]
    [InlineData("‼", true)]
    [InlineData("ы", true)]
    [InlineData("Ы", true)]
    [InlineData("ь", false)]
    [InlineData("f", false)]
    [InlineData("m", false)]
    [InlineData("2", false)]
    [InlineData("(", false)]
    public void CanBeMultiPart_OnlyForGlyphsDrawnInPieces(string text, bool expected)
    {
        // A plain letter falling apart is a thin font losing hairlines at the threshold; stored
        // as an expanded "f" it later claimed "t." as one character.
        Assert.Equal(expected, NOcrTrainer.CanBeMultiPart(text));
    }

    // « and » are left out: how far apart their two chevrons render depends on the
    // platform's default font (they fail on the Linux CI font), CanBeMultiPart covers them.
    [Theory]
    [InlineData("„")]
    [InlineData("“")]
    [InlineData("ы")]
    public void Trainer_LearnsMultiPartGlyphs(string text)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        var settings = new NOcrTrainerSettings
        {
            FontNames = { TestFontName },
            FontSize = FontSize,
            CharactersToTrain = text,
        };

        new NOcrTrainer().Train(settings, db);

        Assert.Contains(db.OcrCharactersExpanded, c => c.Text == text);
    }

    [Fact]
    public void AddDiscriminativeLines_SeparatesThinFeatureFromLookalike()
    {
        // "ø" vs "o": a ring with and without a one pixel slash through the hole. The random
        // generator puts no lines on a stroke that thin, so the entry matched both.
        var own = Ring(withSlash: true);
        var lookalike = Ring(withSlash: false);
        var nOcrChar = new NOcrChar("ø") { Width = own.Width, Height = own.Height };
        nOcrChar.LinesForeground.Add(new NOcrLine(new OcrPoint(1, 2), new OcrPoint(1, 17)));
        Assert.True(NOcrDb.IsMatch(lookalike, nOcrChar, 0));

        NOcrTrainer.AddDiscriminativeLines(nOcrChar, own, lookalike);

        Assert.True(NOcrDb.IsMatch(own, nOcrChar, 0));
        Assert.False(NOcrDb.IsMatch(lookalike, nOcrChar, 3));
    }

    private static NikseBitmap2 Ring(bool withSlash)
    {
        using var bitmap = new SKBitmap(20, 20);
        bitmap.Erase(SKColors.Transparent);
        for (var y = 0; y < 20; y++)
        {
            for (var x = 0; x < 20; x++)
            {
                var inHole = x >= 5 && x < 15 && y >= 5 && y < 15;
                var onSlash = withSlash && inHole && x == y;
                if (!inHole || onSlash)
                {
                    bitmap.SetPixel(x, y, SKColors.White);
                }
            }
        }

        return new NikseBitmap2(bitmap);
    }

    [Fact]
    public void TrainedDb_RecognizesTextAtTwiceTheTrainedSize()
    {
        // Top margins, aspect gates and error budgets used to be absolute pixels at the trained
        // size, so "." and "i" (tall, thin, or small) never matched at another size.
        var db = Train("This.");

        var result = RecognizeAtSize(db, "This is his.", FontSize * 2);

        Assert.Equal("This is his.", result);
    }

    private static string RecognizeAtSize(NOcrDb db, string text, float fontSize)
    {
        using var bmp = NOcrTrainer.RenderCharacterImage(text, TestFontName, fontSize, false, false);
        Assert.NotNull(bmp);
        var parent = new NikseBitmap2(bmp!);
        parent.MakeTwoColor(200);
        parent.CropTop(0, new SKColor(0, 0, 0, 0));
        var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, PixelsAreSpace * 2, false, false, 25, false);
        var sb = new StringBuilder();
        foreach (var item in letters)
        {
            sb.Append(item.NikseBitmap == null
                ? item.SpecialCharacter
                : db.GetMatch(parent, letters, item, item.Top, true, MaxWrongPixels)?.Text ?? "*");
        }

        return sb.ToString().Trim();
    }
}
