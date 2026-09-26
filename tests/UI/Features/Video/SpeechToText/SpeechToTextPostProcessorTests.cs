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

    // Issue #15295: a one-line profile (TikTok/Shorts) must not get its short cues merged
    // back into two-line cues - the merge cap was line length times two, whatever the
    // profile's number of lines.
    [Theory]
    [InlineData(14, 1, 14)]
    [InlineData(42, 2, 84)]
    [InlineData(20, 0, 20)]
    public void GetParagraphMaxChars_UsesLineLengthTimesNumberOfLines(int maxLength, int maxLines, int expected)
    {
        var oldMaxLen = Configuration.Settings.General.SubtitleLineMaximumLength;
        var oldMaxLines = Configuration.Settings.General.MaxNumberOfLines;
        try
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = maxLength;
            Configuration.Settings.General.MaxNumberOfLines = maxLines;

            Assert.Equal(expected, SpeechToTextPostProcessor.GetParagraphMaxChars());
        }
        finally
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = oldMaxLen;
            Configuration.Settings.General.MaxNumberOfLines = oldMaxLines;
        }
    }

    [Fact]
    public void Fix_OneLineProfile_MergeKeepsCuesOnOneLine()
    {
        var oldMaxLen = Configuration.Settings.General.SubtitleLineMaximumLength;
        var oldMaxLines = Configuration.Settings.General.MaxNumberOfLines;
        var oldMergeShorter = Configuration.Settings.General.MergeLinesShorterThan;
        try
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 14;
            Configuration.Settings.General.MaxNumberOfLines = 1;
            Configuration.Settings.General.MergeLinesShorterThan = 15; // one-line profile: break only above the line length

            // Word-sized cues back to back, no sentence endings - all qualify for merging.
            var subtitle = Make(
                ("so this is", 0, 600),
                ("how we", 600, 1000),
                ("make the", 1000, 1500),
                ("short", 1500, 1800),
                ("captions for", 1800, 2400),
                ("the video", 2400, 3000),
                ("today", 3000, 3400),
                ("and more", 3400, 4000));

            var pp = new SpeechToTextPostProcessor("en") { ParagraphMaxChars = SpeechToTextPostProcessor.GetParagraphMaxChars() };
            var result = pp.Fix(subtitle, true, false, true, false, false, true, SpeechToTextPostProcessor.Engine.Whisper);

            foreach (var p in result.Paragraphs)
            {
                Assert.DoesNotContain("\n", p.Text);
                Assert.True(p.Text.Length <= 14, $"'{p.Text}' is longer than the 14-char line");
            }
        }
        finally
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = oldMaxLen;
            Configuration.Settings.General.MaxNumberOfLines = oldMaxLines;
            Configuration.Settings.General.MergeLinesShorterThan = oldMergeShorter;
        }
    }

    // Real CrispASR Parakeet + Sortformer output (--max-len 50 --split-on-punct) for a
    // three-voice dialog: every sentence comes back as two labelled halves.
    private static Subtitle MakeDiarizedDialog() => Make(
        ("(speaker 0) Good morning, did you finish the report about the", 0, 2320),
        ("(speaker 0)  new subtitle project yesterday?", 2320, 4880),
        ("(speaker 1) Not yet, I still need the numbers from the", 4960, 7520),
        ("(speaker 1)  translation team before I can send it.", 7520, 10240),
        ("(speaker 0) Fine by me, I will bring the coffee and the", 24080, 26560),
        ("(speaker 0)  printed copies.", 26560, 27840),
        ("(speaker 0)  I", 27920, 28160),
        ("(speaker 2) will tell the other.", 28160, 29760));

    [Fact]
    public void Fix_DiarizedTranscript_KeepsEachLabelAtTheStartOfItsSpeakersLines()
    {
        var pp = new SpeechToTextPostProcessor("en");
        var result = pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, MakeDiarizedDialog(), true, true, true, true, true, true, false, Avalonia.Media.Colors.Red);

        Assert.All(result.Paragraphs, p => Assert.Matches(@"^\(speaker \d\) [^(]*$", p.Text.Replace(System.Environment.NewLine, " ")));
        Assert.Equal("(speaker 0) Good morning, did you finish the report about the new subtitle project yesterday?", Flatten(result.Paragraphs[0].Text));
        Assert.Equal("(speaker 1) Not yet, I still need the numbers from the translation team before I can send it.", Flatten(result.Paragraphs[1].Text));
    }

    [Fact]
    public void Fix_DiarizedTranscript_NeverMergesTwoSpeakers()
    {
        var pp = new SpeechToTextPostProcessor("en");
        var result = pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, MakeDiarizedDialog(), true, true, true, true, true, true, false, Avalonia.Media.Colors.Red);

        // "I" is speaker 0's according to the engine; merged with the next line it would hand
        // speaker 2's words to speaker 0.
        Assert.Equal("(speaker 2) will tell the other.", result.Paragraphs.Last().Text);
        Assert.StartsWith("(speaker 0) I", result.Paragraphs[^2].Text);
    }

    [Fact]
    public void Fix_DiarizedTranscript_DoesNotLengthenALineIntoTheNextSpeaker()
    {
        var pp = new SpeechToTextPostProcessor("en");
        var result = pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, MakeDiarizedDialog(), true, true, true, true, true, true, false, Avalonia.Media.Colors.Red);

        for (var i = 0; i < result.Paragraphs.Count - 1; i++)
        {
            Assert.True(result.Paragraphs[i].EndTime.TotalMilliseconds <= result.Paragraphs[i + 1].StartTime.TotalMilliseconds, $"line {i + 1} overlaps line {i + 2}");
        }
    }

    [Fact]
    public void Fix_DiarizedTranscript_PostProcessingOff_LeavesTheTextAlone()
    {
        var input = MakeDiarizedDialog();
        var expected = input.Paragraphs.Select(p => p.Text).ToArray();

        var result = new SpeechToTextPostProcessor("en").Fix(SpeechToTextPostProcessor.Engine.Whisper, input, false, true, true, true, true, true, false, Avalonia.Media.Colors.Red);

        Assert.Equal(expected, result.Paragraphs.Select(p => p.Text).ToArray());
    }

    private static string Flatten(string text) => text.Replace(System.Environment.NewLine, " ");
}
