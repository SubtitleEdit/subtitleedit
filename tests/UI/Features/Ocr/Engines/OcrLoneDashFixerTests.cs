using System.Collections.Generic;
using Nikse.SubtitleEdit.Features.Ocr.Engines;

namespace UITests.Features.Ocr.Engines;

// Google Lens returns the dialog dash of a subtitle line as its own text line, in varying
// positions: alternating with the text, grouped before it, or trailing after it (#12988).
public class OcrLoneDashFixerTests
{
    [Fact]
    public void AlternatingDashes_AreJoinedWithTheirLines()
    {
        var lines = new List<string> { "-", "YOU CAN'T DO THIS.", "-", "DO WHAT?" };

        var result = OcrLoneDashFixer.FixLoneDashes(lines);

        Assert.Equal(new List<string> { "- YOU CAN'T DO THIS.", "- DO WHAT?" }, result);
    }

    [Fact]
    public void GroupedDashesBeforeText_AreDistributed()
    {
        var lines = new List<string> { "-", "-", "MOM?", "MM-HMM?" };

        var result = OcrLoneDashFixer.FixLoneDashes(lines);

        Assert.Equal(new List<string> { "- MOM?", "- MM-HMM?" }, result);
    }

    [Fact]
    public void TrailingDash_IsAttachedToTheLineMissingItsDash()
    {
        var lines = new List<string> { "YOU CAN'T DO THIS.", "- DO WHAT?", "-" };

        var result = OcrLoneDashFixer.FixLoneDashes(lines);

        Assert.Equal(new List<string> { "- YOU CAN'T DO THIS.", "- DO WHAT?" }, result);
    }

    [Fact]
    public void SingleDashBeforeSingleLine_IsJoined()
    {
        var lines = new List<string> { "-", "DADDY." };

        var result = OcrLoneDashFixer.FixLoneDashes(lines);

        Assert.Equal(new List<string> { "- DADDY." }, result);
    }

    [Fact]
    public void DashBetweenLines_JoinsWithFollowingLine()
    {
        var lines = new List<string> { "- YEAH, I THINK THAT I DO.", "-", "DADDY." };

        var result = OcrLoneDashFixer.FixLoneDashes(lines);

        Assert.Equal(new List<string> { "- YEAH, I THINK THAT I DO.", "- DADDY." }, result);
    }

    [Fact]
    public void NoLoneDashes_LeavesLinesUntouched()
    {
        var lines = new List<string> { "- YOU CAN'T DO THIS.", "- DO WHAT?" };

        var result = OcrLoneDashFixer.FixLoneDashes(lines);

        Assert.Equal(lines, result);
    }

    [Fact]
    public void OnlyDashes_AreLeftUntouched()
    {
        var lines = new List<string> { "-", "-" };

        var result = OcrLoneDashFixer.FixLoneDashes(lines);

        Assert.Equal(lines, result);
    }

    [Fact]
    public void MoreDashesThanLinesMissingOne_FallsBackToJoiningWithNextLine()
    {
        var lines = new List<string> { "-", "-", "HELLO." };

        var result = OcrLoneDashFixer.FixLoneDashes(lines);

        Assert.Equal(new List<string> { "-", "- HELLO." }, result);
    }

    [Fact]
    public void StringOverload_UsesNewLineSeparators()
    {
        var text = "-\nYOU CAN'T DO THIS.\n-\nDO WHAT?";

        var result = OcrLoneDashFixer.FixLoneDashes(text);

        Assert.Equal("- YOU CAN'T DO THIS." + System.Environment.NewLine + "- DO WHAT?", result);
    }

    [Fact]
    public void StringOverload_WithoutDashes_ReturnsSameText()
    {
        var text = "HELLO THERE.";

        Assert.Equal(text, OcrLoneDashFixer.FixLoneDashes(text));
    }
}
