using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace LibUiLogicTests.Ocr;

public class NOcrDbErrorFinderTests
{
    private readonly Dictionary<string, NOcrChar> _entries = new();

    private NOcrChar Entry(string text)
    {
        if (!_entries.TryGetValue(text, out var entry))
        {
            entry = new NOcrChar { Text = text };
            _entries[text] = entry;
        }

        return entry;
    }

    /// <summary>One glyph per character; "|" joins characters read by one (expanded) entry, "*" is an unknown glyph.</summary>
    private List<NOcrGlyphRead> Read(params string[] glyphTexts) =>
        glyphTexts.Select(t => t switch
        {
            " " => new NOcrGlyphRead { Text = " " },
            "*" => new NOcrGlyphRead { Text = "*" },
            _ => new NOcrGlyphRead { Text = t, Entry = Entry(t) },
        }).ToList();

    [Fact]
    public void WrongEntry_IsBlamedWithWhatTheControlsRead()
    {
        var finder = new NOcrDbErrorFinder();
        finder.AddLine(0, Read("t", "n", "e", " ", "c", "a", "r"), new[] { "the car", "tne car" });
        finder.AddLine(1, Read("t", "n", "e", " ", "c", "a", "r"), new[] { "the car", "the car" });

        var suspect = Assert.Single(finder.GetSuspects());
        Assert.Same(_entries["n"], suspect.Entry);
        Assert.Equal(1, suspect.Errors);
        Assert.Equal(1, suspect.Uses);
        Assert.Equal("h", suspect.SuggestedText);
        Assert.Equal(2, finder.LinesChecked);
        Assert.Equal(1, finder.LinesCompared); // controls disagreed on line 0
    }

    [Fact]
    public void Spaces_Tags_QuoteStyles_And_CaseTwins_AreNotErrors()
    {
        var finder = new NOcrDbErrorFinder();
        finder.AddLine(0, Read("O", "h", ",", " ", "I", "'", "m", " ", "o", "k", "."), new[] { "<i>oh, I’m  ok.</i>" });

        Assert.Empty(finder.GetSuspects());
    }

    [Fact]
    public void MostlyRightEntry_HasNoSuggestedText()
    {
        var finder = new NOcrDbErrorFinder();
        finder.AddLine(0, Read("W", "e"), new[] { "We" });
        finder.AddLine(1, Read("W", "e"), new[] { "We" });
        finder.AddLine(2, Read("R", "E", "W", "I", "N", "G"), new[] { "REVVING" });

        var suspect = Assert.Single(finder.GetSuspects());
        Assert.Equal(1, suspect.Errors);
        Assert.Equal(3, suspect.Uses);
        Assert.Equal("VV", suspect.MostCommonExpected);
        Assert.Null(suspect.SuggestedText);
    }

    [Fact]
    public void BarLetters_AreNotErrors()
    {
        var finder = new NOcrDbErrorFinder();
        finder.AddLine(0, Read("l", "t", "'", "s", " ", "I", "i", "f", "e"), new[] { "It's life" });

        Assert.Empty(finder.GetSuspects());
    }

    [Fact]
    public void MergedLetters_ExpectedIncludesTheMissingCharacter()
    {
        var finder = new NOcrDbErrorFinder();
        finder.AddLine(0, Read("t", "u", "m", "s"), new[] { "turns" });

        var suspect = Assert.Single(finder.GetSuspects());
        Assert.Same(_entries["m"], suspect.Entry);
        Assert.Equal("rn", suspect.MostCommonExpected);
    }

    [Fact]
    public void ExtraGlyph_IsBlamedWithNothingExpected()
    {
        var finder = new NOcrDbErrorFinder();
        finder.AddLine(0, Read("H", "i", "'", "!"), new[] { "Hi!" });

        var suspect = Assert.Single(finder.GetSuspects());
        Assert.Same(_entries["'"], suspect.Entry);
        Assert.Equal(string.Empty, suspect.MostCommonExpected);
    }

    [Fact]
    public void UnknownGlyph_BlamesNoEntry()
    {
        var finder = new NOcrDbErrorFinder();
        finder.AddLine(0, Read("H", "*", "l", "l", "o"), new[] { "Hello" });

        Assert.Empty(finder.GetSuspects());
        Assert.Equal(1, finder.LinesCompared);
    }

    [Fact]
    public void LinesWithoutControlText_AreSkipped()
    {
        var finder = new NOcrDbErrorFinder();
        finder.AddLine(0, Read("I"), new string?[] { null });
        finder.AddLine(1, Read("I"), new[] { string.Empty });

        Assert.Empty(finder.GetSuspects());
        Assert.Equal(0, finder.LinesCompared);
    }
}
