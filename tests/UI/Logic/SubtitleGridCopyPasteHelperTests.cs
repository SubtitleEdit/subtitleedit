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
}
