using Nikse.SubtitleEdit.Core.Common;
using System.Diagnostics;

namespace LibSETests.Common;

public class UnknownFormatImporterTest
{
    [Fact]
    public void AutoGuessImportParsesSubtitleWithNoLineBreaks()
    {
        var importer = new UnknownFormatImporter();
        var text = "1 00:00:01.502 --> 00:00:03.604 Hello there my good friend. " +
                   "2 00:00:04.000 --> 00:00:06.000 How are you doing today? " +
                   "3 00:00:07.000 --> 00:00:09.000 I am doing fine.";

        var subtitle = importer.AutoGuessImport([text], "test.txt");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1502, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(3604, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        // The winning importer does not consume the "2"/"3" sequence numbers, so they
        // trail the preceding paragraph's text - long-standing behavior, asserted here
        // so the O(n²)→O(n) rewrite of the no-line-break importers is provably neutral.
        Assert.Equal("Hello there my good friend. 2", subtitle.Paragraphs[0].Text);
        Assert.Equal("How are you doing today? 3", subtitle.Paragraphs[1].Text);
        Assert.Equal("I am doing fine.", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void AutoGuessImportParsesSubtitleWithNoLineBreaksWithExtraSpaces()
    {
        var importer = new UnknownFormatImporter();
        var text = "00: 00 : 01, 502 --> 00: 00 : 03, 604 Hello there my good friend. " +
                   "00: 00 : 04, 000 --> 00: 00 : 06, 000 How are you doing today? " +
                   "00: 00 : 07, 000 --> 00: 00 : 09, 000 I am doing fine.";

        var subtitle = importer.AutoGuessImport([text], "test.txt");

        Assert.Equal(3, subtitle.Paragraphs.Count);
        Assert.Equal(1502, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(3604, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
        Assert.Equal("Hello there my good friend.", subtitle.Paragraphs[0].Text);
        Assert.Equal("How are you doing today?", subtitle.Paragraphs[1].Text);
        Assert.Equal("I am doing fine.", subtitle.Paragraphs[2].Text);
    }

    [Fact]
    public void AutoGuessImportIsFastOnDigitHeavyBinaryInput()
    {
        // Regression for issue #12683: a raw PGS .sup decoded as text reached the
        // no-line-break importers, whose per-digit Substring(i) + greedy \d+ retry
        // at every position of a digit run made megabytes of digit-heavy binary
        // quadratic - the UI froze for hours. Linear behavior finishes in well
        // under a second; the generous bound only guards against the quadratic case
        // (2M digits was on the order of 10^12 operations before the fix).
        var importer = new UnknownFormatImporter();
        var text = new string('0', 2_000_000) + " --> ";

        var stopwatch = Stopwatch.StartNew();
        var subtitle = importer.AutoGuessImport([text], "test.sup");
        stopwatch.Stop();

        Assert.Empty(subtitle.Paragraphs);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(20),
            $"AutoGuessImport took {stopwatch.Elapsed} on digit-heavy input - quadratic behavior is back?");
    }
}
