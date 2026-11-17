using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The dots of a bold "ï" are wider than its stem: the splitter's path slid between the left
/// dot and the stem, and "chouïa" was read as "chou'ía" (#16993).
/// </summary>
public class SplitterDiaeresisTests
{
    private static List<ImageSplitterItem2> Split(Action<SKCanvas, SKPaint> draw)
    {
        var bmp = new SKBitmap(120, 50);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            draw(canvas, paint);
        }

        return NikseBitmapImageSplitter2.SplitBitmapToLettersNew(new NikseBitmap2(bmp), 12, false, true, 25, true)
            .Where(i => i.NikseBitmap != null)
            .ToList();
    }

    [Fact]
    public void BoldDiaeresisWiderThanStem_StaysOneGlyph()
    {
        // The pixels of the "ï" in the issue's image: the left dot shares one column with the stem.
        var glyphs = Split((canvas, paint) =>
        {
            canvas.DrawRect(0, 8, 20, 21, paint);  // "u"
            canvas.DrawRect(31, 0, 5, 5, paint);   // left dot
            canvas.DrawRect(40, 0, 5, 5, paint);   // right dot
            canvas.DrawRect(35, 8, 6, 21, paint);  // stem
            canvas.DrawRect(56, 8, 19, 21, paint); // "a"
        });

        Assert.Equal(3, glyphs.Count);
        var i = glyphs[1];
        Assert.Equal(31, i.X);
        Assert.Equal(0, i.Y);
        Assert.Equal(14, i.NikseBitmap!.Width);
        Assert.Equal(29, i.NikseBitmap.Height);
    }

    [Fact]
    public void RightDotCutOff_StaysOneGlyph()
    {
        // The mirror image: the right dot shares a column with the stem, the left one doesn't.
        var glyphs = Split((canvas, paint) =>
        {
            canvas.DrawRect(0, 8, 20, 21, paint);  // "u"
            canvas.DrawRect(30, 0, 5, 5, paint);   // left dot
            canvas.DrawRect(39, 0, 5, 5, paint);   // right dot
            canvas.DrawRect(34, 8, 6, 21, paint);  // stem
            canvas.DrawRect(56, 8, 19, 21, paint); // "a"
        });

        Assert.Equal(3, glyphs.Count);
        Assert.Equal(30, glyphs[1].X);
        Assert.Equal(14, glyphs[1].NikseBitmap!.Width);
        Assert.Equal(29, glyphs[1].NikseBitmap.Height);
    }

    [Fact]
    public void CapitalDiaeresisWiderThanStem_StaysOneGlyph()
    {
        // "Ï": a cap-high stem under the dots.
        var glyphs = Split((canvas, paint) =>
        {
            canvas.DrawRect(31, 0, 5, 5, paint);
            canvas.DrawRect(40, 0, 5, 5, paint);
            canvas.DrawRect(35, 8, 6, 30, paint);
            canvas.DrawRect(56, 8, 22, 30, paint); // "N"
        });

        Assert.Equal(2, glyphs.Count);
        Assert.Equal(14, glyphs[0].NikseBitmap!.Width);
        Assert.Equal(38, glyphs[0].NikseBitmap.Height);
    }

    [Fact]
    public void DottedLetterWithOneDot_IsLeftAlone()
    {
        // "u" "i" "a": the dot of "i" is as wide as its stem, nothing to merge.
        var glyphs = Split((canvas, paint) =>
        {
            canvas.DrawRect(0, 8, 20, 21, paint);
            canvas.DrawRect(30, 0, 6, 5, paint);
            canvas.DrawRect(30, 8, 6, 21, paint);
            canvas.DrawRect(46, 8, 19, 21, paint);
        });

        Assert.Equal(3, glyphs.Count);
        Assert.Equal(6, glyphs[1].NikseBitmap!.Width);
    }

    [Fact]
    public void QuoteBeforeDottedI_StaysApart()
    {
        // "\"i": the quote's strokes are taller than the dot, and a column apart from the "i".
        var glyphs = Split((canvas, paint) =>
        {
            canvas.DrawRect(20, 0, 4, 11, paint);
            canvas.DrawRect(27, 0, 4, 11, paint);
            canvas.DrawRect(33, 0, 6, 5, paint);
            canvas.DrawRect(33, 8, 6, 33, paint);
        });

        Assert.Equal(41, glyphs[^1].NikseBitmap!.Height);
        Assert.Equal(6, glyphs[^1].NikseBitmap!.Width);
        Assert.All(glyphs.Take(glyphs.Count - 1), g => Assert.Equal(11, g.NikseBitmap!.Height));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TrainedDiaeresisAndLigature_RoundTrip(bool bold)
    {
        // Trained and read through the same splitter, in the test's default font.
        var fontName = SKTypeface.Default.FamilyName;
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        var learned = new NOcrTrainer().Train(new NOcrTrainerSettings
        {
            FontNames = { fontName },
            FontSize = 50,
            IncludeBold = bold,
            CharactersToTrain = "chouaïnveœrsÏ",
            RandomSeed = 16993,
        }, db);
        Assert.True(learned > 0);

        foreach (var text in new[] { "chouïa", "naïve", "cœur", "sœur", "Ïa" })
        {
            using var bmp = NOcrTrainer.RenderCharacterImage(text, fontName, 50, bold, false);
            var parent = new NikseBitmap2(bmp!);
            parent.MakeTwoColor(200);
            parent.CropTop(0, new SKColor(0, 0, 0, 0));
            var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, 12, false, true, 25, false);
            var sb = new System.Text.StringBuilder();
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

            Assert.Equal(text, sb.ToString().Trim());
        }
    }

    [Fact]
    public void BoldDiaeresisWiderThanStem_ShippedLatinDbReadsIt()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "Ocr", "Latin.nocr")))
        {
            dir = dir.Parent;
        }

        Assert.SkipWhen(dir is null, "Ocr/Latin.nocr not found");
        var db = new NOcrDb(Path.Combine(dir!.FullName, "Ocr", "Latin.nocr"));

        // The "ï" of "chouïa" from the issue's image, as the two-color pass leaves it.
        var bmp = new SKBitmap(40, 29);
        using (var canvas = new SKCanvas(bmp))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(0, 0, 5, 5, paint);
            canvas.DrawRect(9, 0, 5, 5, paint);
            canvas.DrawRect(4, 8, 6, 21, paint);
        }

        var parent = new NikseBitmap2(bmp);
        var letters = NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, 12, false, true, 25, true);
        var glyph = Assert.Single(letters, l => l.NikseBitmap != null);

        Assert.Equal("ï", db.GetMatch(parent, letters, glyph, glyph.Top, true, 25, lastDitch: true)?.Text);
    }

    [Fact]
    public void ApostropheTouchingMarkedLetter_StaysApart()
    {
        // An apostrophe is much taller than the mark on the letter next to it, so it's no half
        // of that mark.
        var glyphs = Split((canvas, paint) =>
        {
            canvas.DrawRect(31, 0, 5, 11, paint);  // apostrophe
            canvas.DrawRect(40, 0, 5, 5, paint);   // mark
            canvas.DrawRect(35, 14, 6, 33, paint); // stem
        });

        Assert.Equal(2, glyphs.Count);
        Assert.Equal(11, glyphs[0].NikseBitmap!.Height);
        Assert.Equal(47, glyphs[1].NikseBitmap!.Height);
    }

    [Fact]
    public void ApostropheTouchingLetterWithoutMark_StaysApart()
    {
        // "d'a" in italic: the apostrophe's box touches the "a", which has no mark of its own.
        var glyphs = Split((canvas, paint) =>
        {
            canvas.DrawRect(20, 0, 5, 6, paint);   // short apostrophe
            canvas.DrawRect(24, 10, 18, 22, paint); // "a"
        });

        Assert.Equal(2, glyphs.Count);
    }
}
