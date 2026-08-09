using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Linq;
using Xunit;

namespace UITests.Logic;

public class SubtitleGridCopyPasteHelperTests
{
    [Fact]
    public void AssaCopyPayload_ContainsOnlyEventLines_AndRoundTrips()
    {
        var sub = new Subtitle();
        sub.Header = AdvancedSubStationAlpha.DefaultHeader;
        sub.Paragraphs.Add(new Paragraph("Line one", 1000, 2000));
        sub.Paragraphs.Add(new Paragraph("Line two", 3000, 4000) { IsComment = true });

        // The clipboard payload must contain only Dialogue/Comment lines (no [Script Info] /
        // [V4+ Styles] file headers), because Aegisub's paste turns any other line into a fake
        // subtitle line (#10476). SE's own paste parses the bare event lines back correctly.
        var payload = SubtitleGridCopyPasteHelper.GetClipboardText(new AdvancedSubStationAlpha(), sub);
        var lines = payload.SplitToLines();
        Assert.All(lines, l => Assert.True(
            l.TrimStart().StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase) ||
            l.TrimStart().StartsWith("Comment:", StringComparison.OrdinalIgnoreCase)));

        var pasted = Subtitle.Parse(lines, "ass");
        Assert.NotNull(pasted);
        Assert.Equal(2, pasted.Paragraphs.Count);
        Assert.Equal("Line one", pasted.Paragraphs[0].Text);
        Assert.Equal("Line two", pasted.Paragraphs[1].Text);
        Assert.True(pasted.Paragraphs[1].IsComment);
    }

    // Plain SubStationAlpha (.ssa) has the same [Script Info]/[V4 Styles] headers and the
    // same Aegisub paste problem, so its clipboard payload gets the same treatment.
    [Fact]
    public void SsaCopyPayload_ContainsOnlyEventLines_AndRoundTrips()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Line one", 1000, 2000));
        sub.Paragraphs.Add(new Paragraph("Line two", 3000, 4000));

        var payload = SubtitleGridCopyPasteHelper.GetClipboardText(new SubStationAlpha(), sub);
        var lines = payload.SplitToLines();
        Assert.All(lines, l => Assert.True(
            l.TrimStart().StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase) ||
            l.TrimStart().StartsWith("Comment:", StringComparison.OrdinalIgnoreCase)));

        var pasted = Subtitle.Parse(lines, "ssa");
        Assert.NotNull(pasted);
        Assert.Equal(2, pasted.Paragraphs.Count);
        Assert.Equal("Line one", pasted.Paragraphs[0].Text);
        Assert.Equal("Line two", pasted.Paragraphs[1].Text);
    }

    // The event section used to start only at "Dialogue:", so a payload whose first event was
    // a "Comment:" lost that line (or, when every line was commented, failed to parse at all
    // and got pasted as raw text).
    [Theory]
    [InlineData("ass")]
    [InlineData("ssa")]
    public void CopyPayload_LeadingCommentLine_RoundTrips(string extension)
    {
        var format = extension == "ass" ? (SubtitleFormat)new AdvancedSubStationAlpha() : new SubStationAlpha();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Commented first", 1000, 2000) { IsComment = true });
        sub.Paragraphs.Add(new Paragraph("Normal second", 3000, 4000));

        var lines = SubtitleGridCopyPasteHelper.GetClipboardText(format, sub).SplitToLines();
        var pasted = Subtitle.Parse(lines, extension);

        Assert.NotNull(pasted);
        Assert.Equal(2, pasted.Paragraphs.Count);
        Assert.Equal("Commented first", pasted.Paragraphs[0].Text);
        Assert.Equal("Normal second", pasted.Paragraphs[1].Text);
    }

    [Theory]
    [InlineData("ass")]
    [InlineData("ssa")]
    public void CopyPayload_AllCommentLines_RoundTrips(string extension)
    {
        var format = extension == "ass" ? (SubtitleFormat)new AdvancedSubStationAlpha() : new SubStationAlpha();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Only comment", 1000, 2000) { IsComment = true });

        var lines = SubtitleGridCopyPasteHelper.GetClipboardText(format, sub).SplitToLines();
        var pasted = Subtitle.Parse(lines, extension);

        Assert.NotNull(pasted);
        Assert.Single(pasted.Paragraphs);
        Assert.Equal("Only comment", pasted.Paragraphs[0].Text);
    }

    // ToText appends the subtitle footer after the event lines, so stripping only the header
    // still left [Fonts]/[Graphics] (including embedded font payloads) on the clipboard - the
    // same fake-subtitle-lines problem #10476 is about, just from the other end of the file.
    [Theory]
    [InlineData("ass")]
    [InlineData("ssa")]
    public void CopyPayload_DoesNotIncludeFooterSections(string extension)
    {
        var format = extension == "ass" ? (SubtitleFormat)new AdvancedSubStationAlpha() : new SubStationAlpha();
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Line one", 1000, 2000));
        sub.Footer = "[Fonts]" + Environment.NewLine + "fontname: Arial_0.ttf" + Environment.NewLine + "!!0000";

        var payload = SubtitleGridCopyPasteHelper.GetClipboardText(format, sub);

        Assert.DoesNotContain("[Fonts]", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fontname:", payload, StringComparison.OrdinalIgnoreCase);
        Assert.All(payload.SplitToLines(), l => Assert.True(
            l.TrimStart().StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase) ||
            l.TrimStart().StartsWith("Comment:", StringComparison.OrdinalIgnoreCase)));
    }
}
