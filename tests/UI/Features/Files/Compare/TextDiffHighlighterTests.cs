using Nikse.SubtitleEdit.Features.Files.Compare;
using System.Collections.Generic;
using System.Reflection;

namespace UITests.Features.Files.Compare;

public class TextDiffHighlighterTests
{
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
