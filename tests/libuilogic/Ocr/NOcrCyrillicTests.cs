using Nikse.SubtitleEdit.UiLogic.Ocr;
using SkiaSharp;
using System.Text;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// Cyrillic in nOCR: case twins ("о"/"О", "т"/"Т", ...) resolved by the matcher and the case
/// fixer, descender letters ("д", "у") cased by their height above the baseline, and "ы" read
/// as one letter although it splits into a soft sign and a bar.
/// </summary>
public class NOcrCyrillicTests
{
    private const float FontSize = 50f;
    private const int MaxWrongPixels = 25;
    private const int PixelsAreSpace = 12;

    private static string TestFontName => SKTypeface.Default.FamilyName;

    private static NOcrDb Train(string characters)
    {
        var db = new NOcrDb(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".nocr"));
        var learned = new NOcrTrainer().Train(new NOcrTrainerSettings
        {
            FontNames = { TestFontName },
            FontSize = FontSize,
            CharactersToTrain = characters,
        }, db);
        Assert.True(learned > 0, "training should learn at least one character");
        return db;
    }

    private static List<ImageSplitterItem2> Split(string text, out NikseBitmap2 parent)
    {
        using var bmp = NOcrTrainer.RenderCharacterImage(text, TestFontName, FontSize, false, false);
        Assert.NotNull(bmp);
        parent = new NikseBitmap2(bmp!);
        parent.MakeTwoColor(200);
        parent.CropTop(0, new SKColor(0, 0, 0, 0));
        return NikseBitmapImageSplitter2.SplitBitmapToLettersNew(parent, PixelsAreSpace, false, false, 25, false);
    }

    private static string Recognize(NOcrDb db, string text, INOcrCaseFixer? caseFixer = null)
    {
        var letters = Split(text, out var parent);
        var sb = new StringBuilder();
        for (var i = 0; i < letters.Count; i++)
        {
            var item = letters[i];
            if (item.NikseBitmap == null)
            {
                sb.Append(item.SpecialCharacter);
                continue;
            }

            var match = db.GetMatch(parent, letters, item, item.Top, true, MaxWrongPixels);
            if (match is { ExpandCount: > 0 })
            {
                i += match.ExpandCount - 1;
            }

            sb.Append(match == null ? "*" : caseFixer?.FixUppercaseLowercaseIssues(item, match) ?? match.Text);
        }

        return sb.ToString().Trim();
    }

    [Fact]
    public void Matcher_ReadsCyrillicCaseTwinsInTheirCase()
    {
        var db = Train("МмОоЯяТтЕеВвАаСсНнИиКк");

        Assert.Equal("Моя тетя", Recognize(db, "Моя тетя"));
        Assert.Equal("Вас Нет", Recognize(db, "Вас Нет"));
    }

    [Fact]
    public void SoftSignFollowedBySameHeightBar_IsReadAsYery()
    {
        // Only "ь" and "І" are trained - no two-part "ы" entry to match.
        var db = Train("ьЬІ");

        Assert.Equal("Іы", Recognize(db, "Іы"));
        Assert.Equal("ІЫ", Recognize(db, "ІЫ"));
    }

    [Fact]
    public void SoftSignFollowedByTallerBar_StaysTwoLetters()
    {
        var db = Train("ьЬІ");

        Assert.Equal("ьІ", Recognize(db, "ьІ"));
    }

    [Fact]
    public void Splitter_SetsLineBaseline_DescenderAboveBaselineIsXHeight()
    {
        var letters = Split("Ааау", out _).Where(p => p.NikseBitmap != null).ToList();

        Assert.Equal(4, letters.Count);
        Assert.All(letters, p => Assert.Equal(letters[0].Baseline, p.Baseline));
        var capital = letters[0];
        var a = letters[1];
        var u = letters[3];
        Assert.True(u.NikseBitmap!.Height > u.HeightAboveBaseline, "the descender is below the baseline");
        Assert.InRange(u.HeightAboveBaseline, a.HeightAboveBaseline - 2, a.HeightAboveBaseline + 2);
        Assert.True(capital.HeightAboveBaseline > a.HeightAboveBaseline + 5);
    }

    [Fact]
    public void Splitter_TwoGlyphLineWithDescender_BaselineIsTheLetterBottom()
    {
        // With two glyphs the upper median is the descender's bottom, which made "у" look cap high.
        var letters = Split("Ну", out _).Where(p => p.NikseBitmap != null).ToList();

        Assert.Equal(2, letters.Count);
        var capital = letters[0];
        var u = letters[1];
        Assert.Equal(capital.Top + capital.NikseBitmap!.Height, capital.Baseline);
        Assert.True(u.NikseBitmap!.Height > u.HeightAboveBaseline, "the descender is below the baseline");
    }

    private static ImageSplitterItem2 Glyph(int height, int top, int baseline = 40) =>
        new(0, top, new NikseBitmap2(20, height)) { Top = top, Baseline = baseline };

    private static NOcrCaseFixer WarmedUpFixer()
    {
        // x-height 20 ("а"/"е"), cap height 30 ("Б"/"Е"/"Р"), baseline at 40.
        var fixer = new NOcrCaseFixer();
        for (var i = 0; i < 3; i++)
        {
            fixer.FixUppercaseLowercaseIssues(Glyph(20, 20), new NOcrChar("а"));
            fixer.FixUppercaseLowercaseIssues(Glyph(20, 20), new NOcrChar("е"));
            fixer.FixUppercaseLowercaseIssues(Glyph(30, 10), new NOcrChar("Б"));
            fixer.FixUppercaseLowercaseIssues(Glyph(30, 10), new NOcrChar("Р"));
        }

        return fixer;
    }

    [Theory]
    [InlineData("О", 20, 20, "о")]
    [InlineData("о", 30, 10, "О")]
    [InlineData("Т", 20, 20, "т")]
    [InlineData("Д", 26, 20, "д")] // descender: whole glyph almost cap high, x-high above the baseline
    [InlineData("У", 27, 20, "у")]
    [InlineData("д", 36, 10, "Д")]
    [InlineData("ГУ", 27, 20, "гу")] // merged pair
    [InlineData("жд", 30, 10, "ЖД")]
    [InlineData("б", 30, 10, "б")] // no case twin
    public void CaseFixer_CyrillicTwin_CaseFromHeightAboveBaseline(string matched, int height, int top, string expected)
    {
        var fixer = WarmedUpFixer();

        Assert.Equal(expected, fixer.FixUppercaseLowercaseIssues(Glyph(height, top), new NOcrChar(matched) { Height = 20, MarginTop = 10 }));
    }

    [Fact]
    public void CaseFixer_LowercaseEntryTrainedCapHigh_StaysLowercase()
    {
        // The "∂"-like "д" of some fonts: no descender, as tall as a capital.
        var fixer = WarmedUpFixer();
        var tallLowercase = new NOcrChar("д") { Height = 30, MarginTop = 0 };

        Assert.Equal("д", fixer.FixUppercaseLowercaseIssues(Glyph(30, 10), tallLowercase));
    }
}
