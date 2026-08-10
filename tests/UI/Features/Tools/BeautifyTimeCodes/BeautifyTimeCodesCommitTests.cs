using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes;

namespace UITests.Features.Tools.BeautifyTimeCodes;

/// <summary>
/// The beautify OK path used to rebuild its rows from Paragraphs, silently dropping
/// view-model-only state (OriginalText, Id, ASSA style via Extra). The shipped callers only
/// copied times back so nothing was lost in practice, but any consumer taking whole rows
/// would get degraded data. CommitBeautifiedTimes must adjust times in place instead.
/// </summary>
public class BeautifyTimeCodesCommitTests
{
    private static SubtitleLineViewModel MakeLine(string text, string originalText, string style, int startMs, int endMs)
    {
        var line = new SubtitleLineViewModel(
            new Paragraph(text, startMs, endMs) { Extra = style },
            new AdvancedSubStationAlpha());
        line.OriginalText = originalText;
        return line;
    }

    [Fact]
    public void CommitBeautifiedTimes_AdjustsTimesInPlaceAndKeepsRowState()
    {
        // Deliberately unsorted, with off-frame time codes.
        var line2 = MakeLine("Second", "Orig 2", "Big", 4007, 6499);
        var line1 = MakeLine("First", "Orig 1", "Default", 1013, 3987);
        var rows = new List<SubtitleLineViewModel> { line2, line1 };
        var ids = new[] { line1.Id, line2.Id };

        BeautifyTimeCodesViewModel.CommitBeautifiedTimes(rows, 25.0, new List<double>());

        // Same instances, sorted by start time - nothing rebuilt.
        Assert.Equal(2, rows.Count);
        Assert.Same(line1, rows[0]);
        Assert.Same(line2, rows[1]);
        Assert.Equal(ids[0], rows[0].Id);
        Assert.Equal(ids[1], rows[1].Id);

        // View-model-only state survives.
        Assert.Equal("Orig 1", rows[0].OriginalText);
        Assert.Equal("Orig 2", rows[1].OriginalText);
        Assert.Equal("Default", rows[0].Style);
        Assert.Equal("Big", rows[1].Style);

        // And the beautifier actually ran: all time codes are aligned to 25 fps frames.
        foreach (var row in rows)
        {
            foreach (var ms in new[] { row.StartTime.TotalMilliseconds, row.EndTime.TotalMilliseconds })
            {
                var frames = ms * 25.0 / 1000.0;
                Assert.True(Math.Abs(frames - Math.Round(frames)) < 0.1,
                    $"time code {ms} ms is not aligned to a 25 fps frame");
            }
        }
    }
}
