using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace UITests.Features.Main;

/// <summary>
/// The error scans (list errors, go to next/previous error, batch convert's error list) ask
/// <see cref="SubtitleLineViewModel.HasErrors"/> instead of building the message with
/// <see cref="SubtitleLineViewModel.GetErrors"/>, so the two must agree on every line.
/// </summary>
public class SubtitleLineViewModelHasErrorsTests
{
    private static SubtitleLineViewModel Line(string text, double startMs, double endMs)
        => new()
        {
            Text = text,
            StartTime = TimeSpan.FromMilliseconds(startMs),
            EndTime = TimeSpan.FromMilliseconds(endMs),
        };

    private static List<SubtitleLineViewModel> MakeLines()
        => new()
        {
            Line("Hello there.", 1000, 3000),                                   // clean
            Line("Hi.", 3100, 3200),                                            // duration too short
            Line("A very long single line that is well past forty-three characters.", 4000, 12000), // too long + too long duration
            Line("Way too much text to read in this little time, honestly.", 12100, 12900), // cps/wpm
            Line("Line one" + Environment.NewLine + "Line two" + Environment.NewLine + "Line three", 13000, 16000), // too many lines
            Line("<i>Overlapping.</i>", 15500, 17000),                          // overlaps the previous line
            Line("Barely a gap.", 17005, 19000),                                // gap too short
            Line(string.Empty, 20000, 22000),                                   // empty text
            Line("{\\an8}Somewhere in Denmark", 22100, 24000),                  // clean, ssa tag
        };

    /// <summary>Returns how many lines have errors, so a test can prove it is not vacuous.</summary>
    private static int AssertAgrees(List<SubtitleLineViewModel> lines)
    {
        var withErrors = 0;
        for (var i = 0; i < lines.Count; i++)
        {
            var prev = i > 0 ? lines[i - 1] : null;
            var next = i < lines.Count - 1 ? lines[i + 1] : null;
            var expected = !string.IsNullOrEmpty(lines[i].GetErrors(prev, next));

            Assert.True(
                expected == lines[i].HasErrors(prev, next),
                $"line {i} ('{lines[i].Text}'): GetErrors says {expected}, HasErrors says {!expected}");

            if (expected)
            {
                withErrors++;
            }
        }

        return withErrors;
    }

    [AvaloniaFact]
    public void HasErrors_MatchesGetErrors_WithAllRulesOn()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            var general = Se.Settings.General;
            general.ColorDurationTooShort = true;
            general.ColorDurationTooLong = true;
            general.ColorTextTooLong = true;
            general.ColorTextTooManyLines = true;
            general.ColorCharactersPerSecond = true;
            general.ColorWordsPerMinute = true;
            general.ColorTimeCodeOverlap = true;
            general.ColorGapTooShort = true;
            general.ColorTextTooWide = false; // font dependent, covered separately below

            var withErrors = AssertAgrees(MakeLines());
            Assert.True(withErrors >= 5, $"expected the battery to trip several rules, got {withErrors}");
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [AvaloniaFact]
    public void HasErrors_MatchesGetErrors_WithTextTooWideOn()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            var general = Se.Settings.General;
            general.ColorTextTooWide = true;
            general.ColorTextTooWidePixels = 200; // narrow enough that longer lines trip it
            general.ColorTextTooWideFontName = "Arial";
            general.ColorTextTooWideFontSize = 40;

            var withErrors = AssertAgrees(MakeLines());
            Assert.True(withErrors >= 1, "expected at least one line to be too wide");
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [AvaloniaFact]
    public void HasErrors_MatchesGetErrors_WithAllRulesOff()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            var general = Se.Settings.General;
            general.ColorDurationTooShort = false;
            general.ColorDurationTooLong = false;
            general.ColorTextTooLong = false;
            general.ColorTextTooWide = false;
            general.ColorTextTooManyLines = false;
            general.ColorCharactersPerSecond = false;
            general.ColorWordsPerMinute = false;
            general.ColorTimeCodeOverlap = false;
            general.ColorGapTooShort = false;

            var lines = MakeLines();
            Assert.Equal(0, AssertAgrees(lines));

            for (var i = 0; i < lines.Count; i++)
            {
                Assert.False(lines[i].HasErrors(i > 0 ? lines[i - 1] : null, i < lines.Count - 1 ? lines[i + 1] : null));
            }
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    /// <summary>
    /// GetErrors, the pixel width column and the text error verdict share one memo of the
    /// html-stripped lines, so an edited line must not keep answering for the old text.
    /// </summary>
    [AvaloniaFact]
    public void HasErrors_FollowsTextChange()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ColorTextTooLong = true;
            Se.Settings.General.SubtitleLineMaximumLength = 10;
            Se.Settings.General.ColorCharactersPerSecond = false;
            Se.Settings.General.ColorWordsPerMinute = false;
            Se.Settings.General.ColorDurationTooShort = false;
            Se.Settings.General.ColorDurationTooLong = false;

            var line = Line("Short", 1000, 3000);
            Assert.False(line.HasErrors(null, null));

            line.Text = "A line that is clearly longer than ten characters.";
            Assert.True(line.HasErrors(null, null));

            line.Text = "Short";
            Assert.False(line.HasErrors(null, null));
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    /// <summary>The pixel width column reads the same memo, so it must follow the text too.</summary>
    [AvaloniaFact]
    public void PixelWidth_FollowsTextChange()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.ShowColumnPixelWidth = true;
            Se.Settings.General.ColorTextTooWideFontName = "Arial";
            Se.Settings.General.ColorTextTooWideFontSize = 40;

            var line = Line("Hi", 1000, 3000);
            var narrow = line.PixelWidth;

            line.Text = "A considerably wider line of text than the first one.";
            var wide = line.PixelWidth;

            Assert.True(wide > narrow, $"expected the wider text to measure wider, got {wide} <= {narrow}");
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }
}
