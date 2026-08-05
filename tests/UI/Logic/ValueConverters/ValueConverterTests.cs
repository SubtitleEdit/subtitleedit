using Avalonia.Controls.Documents;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Globalization;

namespace UITests.Logic.ValueConverters;

/// <summary>
/// Covers the behaviour the grid-cell converters have to keep: which characters end up in which
/// color, which formatting a run carries, and the sentinel/edge cases. The converters run per
/// visible row on every repaint, so they get rewritten for speed now and then - these tests are
/// what says the output did not move.
/// </summary>
public class ValueConverterTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private static InlineCollection Highlight(string text, SubtitleGridFormattingTypes mode, bool singleLine = false)
    {
        var previousMode = Se.Settings.Appearance.SubtitleGridFormattingType;
        var previousSingleLine = Se.Settings.Appearance.SubtitleGridTextSingleLine;
        var previousSpellCheck = Se.Settings.Appearance.SubtitleGridLiveSpellCheck;
        try
        {
            Se.Settings.Appearance.SubtitleGridFormattingType = (int)mode;
            Se.Settings.Appearance.SubtitleGridTextSingleLine = singleLine;
            Se.Settings.Appearance.SubtitleGridLiveSpellCheck = false;
            var converter = new TextWithSubtitleSyntaxHighlightingConverter();
            return (InlineCollection)converter.Convert(text, typeof(InlineCollection), null, Culture)!;
        }
        finally
        {
            Se.Settings.Appearance.SubtitleGridFormattingType = previousMode;
            Se.Settings.Appearance.SubtitleGridTextSingleLine = previousSingleLine;
            Se.Settings.Appearance.SubtitleGridLiveSpellCheck = previousSpellCheck;
        }
    }

    /// <summary>All rendered characters, with a line break rendering as "\n".</summary>
    private static string FlatText(InlineCollection inlines)
    {
        var text = string.Empty;
        foreach (var inline in inlines)
        {
            text += inline switch
            {
                LineBreak => "\n",
                Run run => run.Text ?? string.Empty,
                _ => string.Empty,
            };
        }

        return text;
    }

    /// <summary>The characters that carry <paramref name="color"/> as their foreground.</summary>
    private static string TextWithColor(InlineCollection inlines, Color color)
    {
        var text = string.Empty;
        foreach (var inline in inlines)
        {
            if (inline is Run run && run.Foreground is ISolidColorBrush brush && brush.Color == color)
            {
                text += run.Text;
            }
        }

        return text;
    }

    private static Run SingleRun(InlineCollection inlines)
    {
        var run = Assert.IsType<Run>(Assert.Single(inlines));
        return run;
    }

    [AvaloniaFact]
    public void NoFormatting_KeepsMarkupVerbatimAndBreaksLines()
    {
        var inlines = Highlight("<i>one</i>\r\ntwo", SubtitleGridFormattingTypes.NoFormatting);

        Assert.Equal("<i>one</i>\ntwo", FlatText(inlines));
        Assert.Contains(inlines, i => i is LineBreak);
    }

    [AvaloniaFact]
    public void NoFormatting_SingleLineModeUsesTheSeparatorInsteadOfALineBreak()
    {
        var previous = Se.Settings.Appearance.SubtitleGridTextSingleLineSeparator;
        try
        {
            Se.Settings.Appearance.SubtitleGridTextSingleLineSeparator = " | ";
            var inlines = Highlight("one\r\ntwo", SubtitleGridFormattingTypes.NoFormatting, singleLine: true);

            Assert.Equal("one | two", FlatText(inlines));
            Assert.DoesNotContain(inlines, i => i is LineBreak);
        }
        finally
        {
            Se.Settings.Appearance.SubtitleGridTextSingleLineSeparator = previous;
        }
    }

    [AvaloniaTheory]
    [InlineData("")]
    [InlineData("one")]
    [InlineData("one\r\ntwo")]
    [InlineData("one\ntwo")]
    [InlineData("one\rtwo")]
    [InlineData("one\r\n\r\ntwo")]
    [InlineData("\r\none")]
    [InlineData("one\r\n")]
    public void NoFormatting_RendersEveryLineOfPlainText(string text)
    {
        var expected = text.Replace("\r\n", "\n").Replace('\r', '\n');

        Assert.Equal(expected, FlatText(Highlight(text, SubtitleGridFormattingTypes.NoFormatting)));
    }

    [AvaloniaFact]
    public void ShowFormatting_HidesTheMarkupAndAppliesIt()
    {
        var inlines = Highlight("<i><b><u>styled</u></b></i>", SubtitleGridFormattingTypes.ShowFormatting);

        var run = SingleRun(inlines);
        Assert.Equal("styled", run.Text);
        Assert.Equal(FontStyle.Italic, run.FontStyle);
        Assert.Equal(FontWeight.Bold, run.FontWeight);
        Assert.NotNull(run.TextDecorations);
        Assert.NotEmpty(run.TextDecorations!);
    }

    [AvaloniaFact]
    public void ShowFormatting_AppliesFontTagAttributes()
    {
        var inlines = Highlight("<font color=\"#ff8800\" face=\"Arial\" size=\"20\">x</font>",
            SubtitleGridFormattingTypes.ShowFormatting);

        var run = SingleRun(inlines);
        Assert.Equal("x", run.Text);
        Assert.Equal(Color.FromArgb(255, 0xff, 0x88, 0x00), Assert.IsAssignableFrom<ISolidColorBrush>(run.Foreground).Color);
        Assert.Equal("Arial", run.FontFamily.Name);
        Assert.Equal(16, run.FontSize); // 20 * 0.8, clamped to 8..24
    }

    [AvaloniaFact]
    public void ShowFormatting_ClosingFontTagResetsColorFaceAndSize()
    {
        var inlines = Highlight("<font color=\"#ff8800\" face=\"Arial\" size=\"20\">a</font>b",
            SubtitleGridFormattingTypes.ShowFormatting);

        var last = Assert.IsType<Run>(inlines[^1]);
        Assert.Equal("b", last.Text);
        Assert.False(last.IsSet(TextElement.ForegroundProperty));
        Assert.False(last.IsSet(TextElement.FontFamilyProperty));
        Assert.False(last.IsSet(TextElement.FontSizeProperty));
    }

    [AvaloniaTheory]
    [InlineData("{\\i1}x", true)]
    [InlineData("{\\i0}x", false)]
    [InlineData("{\\i1\\r}x", false)]
    public void ShowFormatting_AppliesAssaItalic(string text, bool italic)
    {
        var run = SingleRun(Highlight(text, SubtitleGridFormattingTypes.ShowFormatting));

        Assert.Equal(italic ? FontStyle.Italic : FontStyle.Normal, run.FontStyle);
    }

    [AvaloniaTheory]
    [InlineData("{\\b1}x", true)]
    [InlineData("{\\b0}x", false)]
    [InlineData("{\\b700}x", true)]
    [InlineData("{\\b400}x", false)]
    public void ShowFormatting_AppliesAssaBoldAndWeights(string text, bool bold)
    {
        var run = SingleRun(Highlight(text, SubtitleGridFormattingTypes.ShowFormatting));

        Assert.Equal(bold ? FontWeight.Bold : FontWeight.Normal, run.FontWeight);
    }

    [AvaloniaTheory]
    // ASSA colors are BGR with an inverted alpha: &HBBGGRR& and &HAABBGGRR&.
    [InlineData("{\\c&H00FF00&}x", 255, 0, 255, 0)]
    [InlineData("{\\c&HFF0000&}x", 255, 0, 0, 255)]
    [InlineData("{\\1c&H0000FF&}x", 255, 255, 0, 0)]
    [InlineData("{\\c&H8000FF00&}x", 127, 0, 255, 0)]
    public void ShowFormatting_ParsesAssaColors(string text, int a, int r, int g, int b)
    {
        var run = SingleRun(Highlight(text, SubtitleGridFormattingTypes.ShowFormatting));

        Assert.Equal(Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b),
            Assert.IsAssignableFrom<ISolidColorBrush>(run.Foreground).Color);
    }

    [AvaloniaTheory]
    [InlineData("{\\c}x")]              // reset
    [InlineData("{\\c&HFFF&}x")]        // too short
    [InlineData("{\\c&HZZZZZZ&}x")]     // not hex
    [InlineData("{\\c&H0011223344&}x")] // too long
    [InlineData("{\\2c&H00FF00&}x")]    // secondary color is not the text color
    public void ShowFormatting_LeavesForegroundUnsetForUnusableAssaColors(string text)
    {
        var run = SingleRun(Highlight(text, SubtitleGridFormattingTypes.ShowFormatting));

        Assert.False(run.IsSet(TextElement.ForegroundProperty));
    }

    [AvaloniaFact]
    public void ShowFormatting_AppliesAssaFontNameAndSize()
    {
        var run = SingleRun(Highlight("{\\fnArial\\fs20}x", SubtitleGridFormattingTypes.ShowFormatting));

        Assert.Equal("Arial", run.FontFamily.Name);
        Assert.Equal(16, run.FontSize);
    }

    [AvaloniaFact]
    public void ShowTags_ShowsTheMarkupItself()
    {
        var text = "<font color=\"#00ff00\">hi</font>";

        Assert.Equal(text, FlatText(Highlight(text, SubtitleGridFormattingTypes.ShowTags)));
    }

    [AvaloniaFact]
    public void ShowTags_ColorsElementNamesAndAttributeNames()
    {
        var inlines = Highlight("<font color=\"#00ff00\">hi</font>", SubtitleGridFormattingTypes.ShowTags);

        Assert.Equal("fontfont", TextWithColor(inlines, SubtitleSyntaxTokenizer.ElementColor));
        Assert.Equal("color", TextWithColor(inlines, SubtitleSyntaxTokenizer.AttributeColor));
    }

    [AvaloniaFact]
    public void ShowTags_ShowsAColorAttributeValueInItsOwnColor()
    {
        var inlines = Highlight("<font color=\"#00ff00\">hi</font>", SubtitleGridFormattingTypes.ShowTags);

        Assert.Equal("#00ff00", TextWithColor(inlines, Color.FromRgb(0, 0xff, 0)));
    }

    [AvaloniaFact]
    public void ShowTags_ShowsAnAssaColorValueInItsOwnColor()
    {
        var inlines = Highlight("{\\c&H00FF00&}hi", SubtitleGridFormattingTypes.ShowTags);

        Assert.Equal("{\\c&H00FF00&}hi", FlatText(inlines));
        Assert.Equal("&H00FF00&", TextWithColor(inlines, Color.FromRgb(0, 0xff, 0)));
        Assert.Equal("c", TextWithColor(inlines, SubtitleSyntaxTokenizer.ElementColor));
    }

    [AvaloniaFact]
    public void ShowTags_ColorsAStyleAttributeValueDifferently()
    {
        var inlines = Highlight("<span style=\"font-weight:bold\">hi</span>", SubtitleGridFormattingTypes.ShowTags);

        Assert.Equal("font-weight:bold", TextWithColor(inlines, SubtitleSyntaxTokenizer.StyleColor));
    }

    [AvaloniaFact]
    public void ShowTags_ColorsHtmlComments()
    {
        var inlines = Highlight("<!-- note -->x", SubtitleGridFormattingTypes.ShowTags);

        Assert.Equal("<!-- note -->", TextWithColor(inlines, SubtitleSyntaxTokenizer.CommentColor));
    }

    [AvaloniaTheory]
    [InlineData("<b/>x")]
    [InlineData("<b />x")]
    public void ShowTags_NormalizesSelfClosingTags(string text)
    {
        Assert.Equal("<b />x", FlatText(Highlight(text, SubtitleGridFormattingTypes.ShowTags)));
    }

    [AvaloniaTheory]
    [InlineData("a < b")]
    [InlineData("unclosed <tag")]
    [InlineData("{\\unclosed")]
    [InlineData("{ not a tag }")]
    [InlineData("<>")]
    public void ShowTags_LeavesMalformedMarkupIntact(string text)
    {
        Assert.Equal(text, FlatText(Highlight(text, SubtitleGridFormattingTypes.ShowTags)));
    }

    [AvaloniaFact]
    public void Highlight_TruncatesVeryLongText()
    {
        var inlines = Highlight(new string('a', 400), SubtitleGridFormattingTypes.NoFormatting);

        Assert.Equal(new string('a', 197) + "...", FlatText(inlines));
    }

    [AvaloniaFact]
    public void Highlight_EmptyTextGivesAnEmptyCollection()
    {
        Assert.Empty(Highlight(string.Empty, SubtitleGridFormattingTypes.ShowFormatting));
    }

    [Fact]
    public void MiddleEllipsis_KeepsTheStartAndTheEnd()
    {
        var converter = new MiddleEllipsisConverter();

        Assert.Equal("0123...6789", converter.Convert("0123456789ABCDEF6789", typeof(string), "11", Culture));
    }

    [Fact]
    public void MiddleEllipsis_LeavesShortValuesAlone()
    {
        var converter = new MiddleEllipsisConverter();

        Assert.Equal("short", converter.Convert("short", typeof(string), "50", Culture));
    }

    [Fact]
    public void TextOneLineShort_FlattensLineBreaks()
    {
        var converter = new TextOneLineShortConverter();

        // Each of \r and \n becomes a space, so a CRLF leaves two.
        Assert.Equal("one  two", converter.Convert("one\r\ntwo", typeof(string), "30", Culture));
        Assert.Equal("one two", converter.Convert("one\ntwo", typeof(string), "30", Culture));
    }

    [Fact]
    public void TextOneLineShort_TruncatesAtAWordBoundary()
    {
        var converter = new TextOneLineShortConverter();

        Assert.Equal("one two...", converter.Convert("one two three four", typeof(string), "10", Culture));
    }

    [Fact]
    public void TextOneLineShort_HardCutsWhenThereIsNoSpaceToCutAt()
    {
        var converter = new TextOneLineShortConverter();

        Assert.Equal("0123456789...", converter.Convert("0123456789ABCDEF", typeof(string), "10", Culture));
    }

    [Fact]
    public void DoubleToNoDecimalHideMax_HidesTheSentinelsIncludingNaN()
    {
        var converter = new DoubleToNoDecimalHideMaxConverter();

        Assert.Equal(string.Empty, converter.Convert(double.MaxValue, typeof(string), null, Culture));
        Assert.Equal(string.Empty, converter.Convert(double.NaN, typeof(string), null, Culture));
        Assert.Equal("42", converter.Convert(41.6, typeof(string), null, Culture));
    }

    [Fact]
    public void BooleanAnd_IsTrueOnlyWhenEveryValueIsTrue()
    {
        var converter = BooleanAndConverter.Instance;

        Assert.Equal(true, converter.Convert(new object?[] { true, true }, typeof(bool), null, Culture));
        Assert.Equal(false, converter.Convert(new object?[] { true, false }, typeof(bool), null, Culture));
        Assert.Equal(false, converter.Convert(new object?[] { true, null }, typeof(bool), null, Culture));
        Assert.Equal(false, converter.Convert(new object?[] { true, "yes" }, typeof(bool), null, Culture));
        Assert.Equal(false, converter.Convert(Array.Empty<object?>(), typeof(bool), null, Culture));
    }

    [Fact]
    public void SmallConverters_ReturnSharedBoxesSoBindingDoesNotAllocatePerRow()
    {
        var inverse = new InverseBooleanConverter();
        var notNull = new NotNullConverter();

        Assert.Same(
            inverse.Convert(false, typeof(bool), null, Culture),
            notNull.Convert("anything", typeof(bool), null, Culture));
        Assert.Same(
            inverse.Convert(true, typeof(bool), null, Culture),
            notNull.Convert(null, typeof(bool), null, Culture));
    }

    [AvaloniaFact]
    public void DurationToBackground_ReusesTheSameBrushInstances()
    {
        var converter = DurationToBackgroundConverter.Instance;
        var previousShort = Se.Settings.General.ColorDurationTooShort;
        var previousMinimum = Se.Settings.General.SubtitleMinimumDisplayMilliseconds;
        try
        {
            Se.Settings.General.ColorDurationTooShort = true;
            Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;

            var tooShortA = converter.Convert(TimeSpan.FromMilliseconds(100), typeof(object), null, Culture);
            var tooShortB = converter.Convert(TimeSpan.FromMilliseconds(200), typeof(object), null, Culture);
            var okA = converter.Convert(TimeSpan.FromMilliseconds(5000), typeof(object), null, Culture);
            var okB = converter.Convert(TimeSpan.FromMilliseconds(6000), typeof(object), null, Culture);

            Assert.Same(tooShortA, tooShortB);
            Assert.Same(okA, okB);
            Assert.NotSame(tooShortA, okA);
            Assert.Equal(Colors.Transparent, Assert.IsAssignableFrom<ISolidColorBrush>(okA).Color);
        }
        finally
        {
            Se.Settings.General.ColorDurationTooShort = previousShort;
            Se.Settings.General.SubtitleMinimumDisplayMilliseconds = previousMinimum;
        }
    }

    [AvaloniaFact]
    public void BatchConvertStatusColor_StillFollowsTheErrorTextAfterItChanges()
    {
        var converter = new BatchConvertStatusColorConverter();
        var previous = Se.Language.General.ErrorX;
        try
        {
            Se.Language.General.ErrorX = "Error: {0}";
            var first = converter.Convert("Error: nope", typeof(object), null, Culture);
            Assert.NotNull(first);

            // A loaded translation swaps the string - the cached prefix has to follow.
            Se.Language.General.ErrorX = "Fejl: {0}";
            Assert.Null(converter.Convert("Error: nope", typeof(object), null, Culture) as ISolidColorBrush);
            Assert.NotNull(converter.Convert("Fejl: nix", typeof(object), null, Culture) as ISolidColorBrush);
        }
        finally
        {
            Se.Language.General.ErrorX = previous;
        }
    }
}
