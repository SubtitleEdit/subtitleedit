using Nikse.SubtitleEdit.Core.Common;
using System.Linq;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

public class SpeechToTextPostProcessorTests
{
    [Fact]
    public void IsNonStandardLineTerminationLanguage_WhisperJapanese_ReturnsTrue()
    {
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("ja"));
    }

    [Fact]
    public void IsNonStandardLineTerminationLanguage_WhisperChinese_ReturnsTrue()
    {
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("zh"));
    }

    [Fact]
    public void IsNonStandardLineTerminationLanguage_WhisperCantonese_ReturnsTrue()
    {
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("yue"));
    }

    [Fact]
    public void IsNonStandardLineTerminationLanguage_VoskCodes_ReturnsTrue()
    {
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("jp"));
        Assert.True(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("cn"));
    }

    [Fact]
    public void IsNonStandardLineTerminationLanguage_OtherLanguages_ReturnsFalse()
    {
        Assert.False(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("en"));
        Assert.False(SpeechToTextPostProcessor.IsNonStandardLineTerminationLanguage("da"));
    }

    // The merge step used to only know the Vosk codes, so Whisper/Crisp ASR
    // transcripts merged Japanese and Chinese up to the 86-char Latin cap
    // (issue #13548).
    [Theory]
    [InlineData("jp", 32)]
    [InlineData("ja", 32)]
    [InlineData("cn", 36)]
    [InlineData("zh", 36)]
    [InlineData("yue", 36)]
    [InlineData("en", 86)]
    public void MergeShortLines_UsesTheLineLengthCapForTheLanguage(string languageCode, int expectedMaxChars)
    {
        var postProcessor = new SpeechToTextPostProcessor(languageCode);

        postProcessor.MergeShortLines(new Subtitle(), languageCode);

        Assert.Equal(expectedMaxChars, postProcessor.ParagraphMaxChars);
    }
}

// Issue #13973: the post-processor should tell the user what went wrong
// (too short / too long / overlaps / non-speech / repeats) and optionally drop
// the non-speech and looping lines.
public class SpeechToTextQualityReportTests
{
    private static Subtitle Make(params (string text, double startMs, double endMs)[] lines)
    {
        var s = new Subtitle();
        foreach (var (text, start, end) in lines)
        {
            s.Paragraphs.Add(new Paragraph(text, start, end));
        }

        return s;
    }

    [Theory]
    [InlineData("[Music]", true)]
    [InlineData("(waves crashing)", true)]
    [InlineData("♪ ♪", true)]
    [InlineData("[Música] (risas)", true)]
    [InlineData("<i>[Applause]</i>", true)]
    [InlineData("[John] Hello there", false)]
    [InlineData("Hello (maybe)", false)]
    [InlineData("Hello", false)]
    [InlineData("", false)]
    public void IsNonSpeechLine(string text, bool expected)
    {
        Assert.Equal(expected, SpeechToTextQualityReport.IsNonSpeechLine(text));
    }

    [Theory]
    [InlineData("Thank you.", "thank you", true)]
    [InlineData("Thank you.", "Thank you!", true)]
    [InlineData("Thank you.", "Thanks.", false)]
    [InlineData("...", "...", false)]
    public void IsRepeatOf(string text, string previous, bool expected)
    {
        Assert.Equal(expected, SpeechToTextQualityReport.IsRepeatOf(text, previous));
    }

    [Fact]
    public void Analyze_FindsEachIssueType()
    {
        var subtitle = Make(
            ("Hi", 0, 200), // too short
            ("The boy ran up the hill and kept on running all the way", 1000, 1500), // too fast
            ("Yes", 3000, 20000), // too long (sparse)
            ("Overlaps next", 21000, 23000),
            ("Overlapped", 22000, 24000),
            ("[Music]", 25000, 27000),
            ("[Music]", 28000, 30000)); // repeat + non-speech

        var report = new SpeechToTextQualityReport();
        report.Analyze(subtitle, 1000, 8000, 25.0);

        Assert.Equal(7, report.TotalLines);
        Assert.Equal(2, report.Count(SpeechToTextQualityIssueType.TooShort));
        Assert.Equal(1, report.Count(SpeechToTextQualityIssueType.TooLong));
        Assert.Equal(1, report.Count(SpeechToTextQualityIssueType.Overlap));
        Assert.Equal(2, report.Count(SpeechToTextQualityIssueType.NonSpeech));
        Assert.Equal(0, report.Count(SpeechToTextQualityIssueType.Repeated)); // non-speech wins over repeat
        Assert.Equal(4, report.Issues.Single(p => p.Type == SpeechToTextQualityIssueType.Overlap).Number);
        Assert.True(report.HasIssues);
    }

    [Fact]
    public void Analyze_CleanSubtitle_HasNoIssues()
    {
        var subtitle = Make(("Hello there.", 0, 1500), ("How are you?", 1600, 3000));

        var report = new SpeechToTextQualityReport();
        report.Analyze(subtitle, 1000, 8000, 25.0);

        Assert.False(report.HasIssues);
        Assert.Contains("no issues", report.ToLogString());
    }

    [Fact]
    public void Fix_RemovesNonSpeechAndRepeatedLines_WhenEnabled()
    {
        var subtitle = Make(
            ("Hello there.", 0, 1500),
            ("[Music]", 2000, 3500),
            ("Thank you.", 4000, 5500),
            ("Thank you.", 6000, 7500),
            ("Thank you.", 8000, 9500),
            ("Bye.", 10000, 11500));

        var pp = new SpeechToTextPostProcessor("en") { RemoveNonSpeechLines = true, RemoveRepeatedLines = true };
        var result = pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, subtitle, true, false, false, false, false, false, false, Avalonia.Media.Colors.Red);

        Assert.Equal(new[] { "Hello there.", "Thank you.", "Bye." }, result.Paragraphs.Select(p => p.Text).ToArray());
        Assert.Equal(1, pp.QualityReport.RemovedCount(SpeechToTextQualityIssueType.NonSpeech));
        Assert.Equal(2, pp.QualityReport.RemovedCount(SpeechToTextQualityIssueType.Repeated));
        Assert.Equal(0, pp.QualityReport.Count(SpeechToTextQualityIssueType.Repeated));
    }

    [Fact]
    public void Fix_RepeatedDetail_NamesTheLineActuallyDuplicated()
    {
        // Line 2 ("[Music]") is removed first, so line 3 repeats line 1 - the detail must
        // say "= #1", not point at the removed line in between.
        var subtitle = Make(
            ("Hello there.", 0, 1500),
            ("[Music]", 2000, 3500),
            ("Hello there.", 4000, 5500));

        var pp = new SpeechToTextPostProcessor("en") { RemoveNonSpeechLines = true, RemoveRepeatedLines = true };
        pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, subtitle, true, false, false, false, false, false, false, Avalonia.Media.Colors.Red);

        var repeated = pp.QualityReport.Removed.Single(i => i.Type == SpeechToTextQualityIssueType.Repeated);
        Assert.Equal(3, repeated.Number);
        Assert.Equal("= #1", repeated.Detail);
    }

    [Fact]
    public void Fix_KeepsLinesButReportsThem_WhenDisabled()
    {
        var subtitle = Make(("Hello there.", 0, 1500), ("[Music]", 2000, 3500), ("Bye.", 4000, 5500), ("Bye.", 6000, 7500));

        var pp = new SpeechToTextPostProcessor("en");
        var result = pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, subtitle, true, false, false, false, false, false, false, Avalonia.Media.Colors.Red);

        Assert.Equal(4, result.Paragraphs.Count);
        Assert.Empty(pp.QualityReport.Removed);
        Assert.Equal(1, pp.QualityReport.Count(SpeechToTextQualityIssueType.NonSpeech));
        Assert.Equal(1, pp.QualityReport.Count(SpeechToTextQualityIssueType.Repeated));
    }

    [Fact]
    public void Fix_FixShortDuration_AlsoFixesOverlaps()
    {
        var subtitle = Make(("First line here.", 0, 3000), ("Second line here.", 2000, 5000));

        var pp = new SpeechToTextPostProcessor("en");
        var result = pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, subtitle, true, false, false, false, true, false, false, Avalonia.Media.Colors.Red);

        Assert.True(result.Paragraphs[0].EndTime.TotalMilliseconds <= result.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(0, pp.QualityReport.Count(SpeechToTextQualityIssueType.Overlap));
    }

    [Fact]
    public void Fix_ReportIsPopulated_EvenWithoutPostProcessing()
    {
        var subtitle = Make(("Hi", 0, 100), ("[Music]", 200, 1500));

        var pp = new SpeechToTextPostProcessor("en");
        pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, subtitle, false, false, false, false, false, false, false, Avalonia.Media.Colors.Red);

        Assert.Equal(2, pp.QualityReport.TotalLines);
        Assert.Equal(1, pp.QualityReport.Count(SpeechToTextQualityIssueType.TooShort));
        Assert.Equal(1, pp.QualityReport.Count(SpeechToTextQualityIssueType.NonSpeech));
    }

    // Splitting a long line divides its time; the short-duration fix must run after
    // split/merge so the halves it creates are fixed too (discussion #12929).
    [Fact]
    public void Fix_FixShortDuration_RunsAfterSplitLines()
    {
        var oldMin = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
        var oldMaxLen = Configuration.Settings.General.SubtitleLineMaximumLength;
        var oldMaxLines = Configuration.Settings.General.MaxNumberOfLines;
        var oldMinGap = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
        try
        {
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            Configuration.Settings.General.MaxNumberOfLines = 2;
            // The split spaces the halves by this gap and the short-duration fix keeps it; an
            // earlier test leaving the mirror at 0 made the halves touch and the first half
            // unfixable (order-dependent CI flake).
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = 24;

            // Long enough to be split into two lines, short enough that each half is < 1 s.
            var text = "This is the first sentence of the line. This is the second sentence of the line. And a third sentence too.";
            var subtitle = Make((text, 0, 1500), ("Far away.", 20000, 22000));

            var pp = new SpeechToTextPostProcessor("en");
            var result = pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, subtitle, true, false, false, false, true, true, false, Avalonia.Media.Colors.Red);

            Assert.True(result.Paragraphs.Count > 1, "expected the line to be split");
            foreach (var p in result.Paragraphs.Take(result.Paragraphs.Count - 1))
            {
                Assert.True(p.DurationTotalMilliseconds >= 1000 || p.EndTime.TotalMilliseconds >= result.Paragraphs[result.Paragraphs.IndexOf(p) + 1].StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines,
                    $"#{p.Number} '{p.Text}' {p.StartTime.TotalMilliseconds}-{p.EndTime.TotalMilliseconds} was left short");
            }
        }
        finally
        {
            Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds = oldMin;
            Configuration.Settings.General.SubtitleLineMaximumLength = oldMaxLen;
            Configuration.Settings.General.MaxNumberOfLines = oldMaxLines;
            Configuration.Settings.General.MinimumMillisecondsBetweenLines = oldMinGap;
        }
    }
}
