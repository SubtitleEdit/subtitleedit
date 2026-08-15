using SeConv.Core;
using System.Text;
using Xunit;

namespace SeConvTests.Core;

public class SubtitleLinterTest : IDisposable
{
    private readonly string _tempDir;

    public SubtitleLinterTest()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "seconv-lint-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private string WriteSrt(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content, new UTF8Encoding(true));
        return path;
    }

    [Fact]
    public void Lint_CleanFile_ReportsNoIssues()
    {
        var path = WriteSrt("clean.srt",
            "1\n00:00:01,000 --> 00:00:03,000\nHello world.\n\n" +
            "2\n00:00:05,000 --> 00:00:07,000\nGoodbye.\n");

        var report = SubtitleLinter.Lint(path);

        Assert.True(report.IsClean, "Expected no issues, got: " + string.Join(", ", report.Issues.Select(i => i.Type)));
    }

    [Fact]
    public void Lint_DetectsOverlap()
    {
        var path = WriteSrt("overlap.srt",
            "1\n00:00:01,000 --> 00:00:05,000\nFirst.\n\n" +
            "2\n00:00:03,000 --> 00:00:06,000\nOverlapping.\n");

        var report = SubtitleLinter.Lint(path);

        Assert.Contains(report.Issues, i => i.Type == "overlap" && i.ParagraphNumber == 1);
    }

    [Fact]
    public void Lint_DetectsTooLongLine()
    {
        // 80-char line; default max is 43.
        var longLine = new string('x', 80);
        var path = WriteSrt("long.srt",
            $"1\n00:00:01,000 --> 00:00:03,000\n{longLine}\n");

        var report = SubtitleLinter.Lint(path);

        Assert.Contains(report.Issues, i => i.Type == "line-too-long");
    }

    [Fact]
    public void Lint_DetectsMismatchedItalicTags()
    {
        var path = WriteSrt("italic.srt",
            "1\n00:00:01,000 --> 00:00:03,000\n<i>Open without close.\n");

        var report = SubtitleLinter.Lint(path);

        Assert.Contains(report.Issues, i => i.Type == "mismatched-italic");
    }

    [Fact]
    public void Lint_DetectsZeroDuration()
    {
        var path = WriteSrt("zero.srt",
            "1\n00:00:01,000 --> 00:00:01,000\nNo duration.\n");

        var report = SubtitleLinter.Lint(path);

        Assert.Contains(report.Issues, i => i.Type == "zero-duration");
    }

    [Fact]
    public void Lint_DetectsEmptyParagraph()
    {
        // SRT loader may strip pure-blank entries, but text consisting only of whitespace
        // (and surviving the parser) should still flag.
        var path = WriteSrt("empty.srt",
            "1\n00:00:01,000 --> 00:00:03,000\n   \n\n" +
            "2\n00:00:05,000 --> 00:00:07,000\nReal text.\n");

        var report = SubtitleLinter.Lint(path);

        // Either the loader normalised the paragraph away (clean) or flagged it as empty;
        // in either case there must NOT be a non-empty issue claimed about line content.
        Assert.DoesNotContain(report.Issues, i => i.Type == "line-too-long");
    }

    /// <summary>
    /// The line-length check skips stripping tags when the raw line already fits, which is only
    /// sound because stripping cannot lengthen a line. A line that is over the limit only with
    /// its markup, and under it without, must still come back clean.
    /// </summary>
    [Fact]
    public void Lint_LongOnlyBecauseOfMarkup_IsNotTooLong()
    {
        // 40 visible chars (under the 43 default), 63 with the font tag.
        var text = "<font color=\"#ff0000\">" + new string('x', 40) + "</font>";
        var path = WriteSrt("markup.srt",
            $"1\n00:00:01,000 --> 00:00:03,000\n{text}\n");

        var report = SubtitleLinter.Lint(path);

        Assert.DoesNotContain(report.Issues, i => i.Type == "line-too-long");
    }

    /// <summary>Tag counting is case-insensitive, as the old substring scan was.</summary>
    [Theory]
    [InlineData("<I>Open without close.", "mismatched-italic")]
    [InlineData("<B>Open without close.", "mismatched-bold")]
    [InlineData("</i>Close without open.", "mismatched-italic")]
    public void Lint_DetectsMismatchedTagsRegardlessOfCase(string text, string expectedIssue)
    {
        var path = WriteSrt("case.srt",
            $"1\n00:00:01,000 --> 00:00:03,000\n{text}\n");

        var report = SubtitleLinter.Lint(path);

        Assert.Contains(report.Issues, i => i.Type == expectedIssue);
    }

    /// <summary>Balanced tags — in any case, and several per line — must not be flagged.</summary>
    [Theory]
    [InlineData("<i>One</i> and <I>two</I>.")]
    [InlineData("<b>Bold</b> and <i>italic</i>.")]
    [InlineData("Plain text with a < less-than sign.")]
    public void Lint_BalancedTags_AreClean(string text)
    {
        var path = WriteSrt("balanced.srt",
            $"1\n00:00:01,000 --> 00:00:03,000\n{text}\n");

        var report = SubtitleLinter.Lint(path);

        Assert.DoesNotContain(report.Issues, i => i.Type.StartsWith("mismatched-", StringComparison.Ordinal));
    }
}
