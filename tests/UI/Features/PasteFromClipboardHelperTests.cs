using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.ObjectModel;

namespace Tests.Features;

public class PasteFromClipboardHelperTests
{
    private static PasteFromClipboardHelper MakeHelper()
    {
        return new PasteFromClipboardHelper(new InsertService());
    }

    [Fact]
    public void PasteSrt_AnchorsFirstLineAtPastePosition()
    {
        var helper = MakeHelper();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>();
        var srt = "1\r\n00:00:10,000 --> 00:00:12,000\r\nHello\r\n\r\n2\r\n00:00:13,000 --> 00:00:15,000\r\nWorld\r\n\r\n";

        var inserted = helper.PasteFromClipboard(srt, 60_000.0, subtitles, new SubRip());

        Assert.Equal(2, inserted.Count);
        Assert.Equal(60_000.0, inserted[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(62_000.0, inserted[0].EndTime.TotalMilliseconds, 3);

        // The second line keeps its offset relative to the first.
        Assert.Equal(63_000.0, inserted[1].StartTime.TotalMilliseconds, 3);
        Assert.Equal(65_000.0, inserted[1].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void PasteSrt_CanAnchorEarlierThanOriginalTimes()
    {
        var helper = MakeHelper();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>();
        var srt = "1\r\n00:01:00,000 --> 00:01:02,000\r\nHello\r\n\r\n";

        var inserted = helper.PasteFromClipboard(srt, 5_000.0, subtitles, new SubRip());

        Assert.Single(inserted);
        Assert.Equal(5_000.0, inserted[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(7_000.0, inserted[0].EndTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void PasteCurrentFormat_RoundTripsWhenFormatIsNotSubRip()
    {
        var helper = MakeHelper();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>();
        var vtt = "WEBVTT\r\n\r\n00:00:10.000 --> 00:00:12.000\r\nHello\r\n\r\n";

        var inserted = helper.PasteFromClipboard(vtt, 20_000.0, subtitles, new WebVTT());

        Assert.Single(inserted);
        Assert.Equal("Hello", inserted[0].Text);
        Assert.Equal(20_000.0, inserted[0].StartTime.TotalMilliseconds, 3);
    }

    [Fact]
    public void PastePlainText_StartsAtPastePosition()
    {
        var helper = MakeHelper();
        var subtitles = new ObservableCollection<SubtitleLineViewModel>();

        var inserted = helper.PasteFromClipboard("Hello world", 30_000.0, subtitles, new SubRip());

        Assert.Single(inserted);
        Assert.Equal(30_000.0, inserted[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal("Hello world", inserted[0].Text);
    }
}
