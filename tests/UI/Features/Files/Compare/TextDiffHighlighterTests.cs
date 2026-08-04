using Avalonia.Controls.Documents;
using Nikse.SubtitleEdit.Features.Files.Compare;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UITests.Features.Files.Compare;

public class TextDiffHighlighterTests
{
    private static string JoinRuns(InlineCollection? inlines)
        => string.Concat(inlines!.Cast<Run>().Select(r => r.Text));

    [Fact]
    public void CompareReplacement_CrLfBeforeVsLfAfter_NormalizesInsteadOfDiffingTheCr()
    {
        // Windows subtitle text carries \r\n while a regex replacement comes back \n-normalized
        // (RegexUtils.ReplaceNewLineSafe). Diffing the raw strings isolated the \r in its own
        // one-character run, and a \r/\n pair split across two runs renders as two line breaks -
        // a phantom empty line (plus a red mark for the invisible \r) in the Multiple Replace
        // preview's Before column (#12622).
        var (before, after) = TextDiffHighlighter.CompareReplacement(
            "Just don't move out\r\nof my school district, okay?",
            "Just don't move out\nof my school district, ok?");

        var beforeText = JoinRuns(before.Inlines);
        var afterText = JoinRuns(after.Inlines);

        Assert.Equal("Just don't move out\nof my school district, okay?", beforeText);
        Assert.Equal("Just don't move out\nof my school district, ok?", afterText);
        Assert.All(before.Inlines!.Cast<Run>(), r => Assert.DoesNotContain('\r', r.Text!));
    }

    [Fact]
    public void Compare_CrLfVsLf_TreatedAsIdenticalText()
    {
        // The compare view gets one text per file: a CRLF file against an LF file must not paint
        // every line break as a difference (same normalization as CompareReplacement, #12622).
        var (left, right) = TextDiffHighlighter.Compare("First line\r\nSecond line", "First line\nSecond line");

        var leftRun = Assert.IsType<Run>(Assert.Single(left.Inlines!));
        var rightRun = Assert.IsType<Run>(Assert.Single(right.Inlines!));

        // The identical-texts path adds a single uncolored run per side.
        Assert.Equal("First line\nSecond line", leftRun.Text);
        Assert.Equal("First line\nSecond line", rightRun.Text);
        Assert.Null(leftRun.Background);
        Assert.Null(rightRun.Background);
    }

    [Fact]
    public void FindCommonParts_WhenContentIsReordered_MiddleCommon2IsSortedByText2Positions()
    {
        // Regression test for: <i>[KITT] text</i> → [KITT] <i>text</i>
        // "<i>" (pos 0 in text1, pos 7 in text2) and "[KITT] " (pos 3 in text1,
        // pos 0 in text2) are the two common middle substrings. Before the fix,
        // middleCommon2 was sorted by text1 positions → [(7,3),(0,7)], causing
        // BuildDiffRuns to walk backwards and emit duplicate text.
        var method = typeof(TextDiffHighlighter).GetMethod(
            "FindCommonParts", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var result = method!.Invoke(null, [
            "<i>[KITT] Why is that, Michael?</i>",
            "[KITT] <i>Why is that, Michael?</i>"
        ])!;

        var middleCommon2 = (List<(int start, int length)>)result.GetType()
            .GetField("Item4")!.GetValue(result)!;

        Assert.True(middleCommon2.Count >= 2,
            "Expected at least 2 common middle substrings for this input");

        for (var i = 1; i < middleCommon2.Count; i++)
        {
            Assert.True(middleCommon2[i - 1].start <= middleCommon2[i].start,
                $"middleCommon2[{i - 1}].start ({middleCommon2[i - 1].start}) " +
                $"must be <= middleCommon2[{i}].start ({middleCommon2[i].start})");
        }
    }
}
