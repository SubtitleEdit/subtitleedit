using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;

namespace LibSETests.ContainerFormats;

/// <summary>
/// A Matroska USF track (codec S_TEXT/USF, what mkvmerge writes for a .usf input) stores the
/// subtitle's &lt;text&gt; element in each block. There was no branch for it, so the track fell
/// through to SubRip and every cue read as the raw XML markup - "&lt;text style="Default"&gt;Line
/// one plain&lt;/text&gt;" instead of "Line one plain".
/// </summary>
public class MatroskaUsfTrackTest
{
    [Fact]
    public void UsfTrackIsReadAsText()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_MKV_USF.mks");

        using var matroska = new MatroskaFile(path);
        Assert.True(matroska.IsValid);

        var track = Assert.Single(matroska.GetTracks(subtitleOnly: true));
        Assert.Equal("S_TEXT/USF", track.CodecId);

        var subtitle = new Subtitle();
        var format = Utilities.LoadMatroskaTextSubtitle(track, matroska, matroska.GetSubtitle(track.TrackNumber, null), subtitle);

        Assert.Equal(new UniversalSubtitleFormat().Name, format.Name);
        Assert.Equal(5, subtitle.Paragraphs.Count);
        Assert.Equal("Line one plain", subtitle.Paragraphs[0].Text);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(2500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 1);
        Assert.Equal("<i>Italic line</i>", subtitle.Paragraphs[1].Text);
        Assert.Equal("Accénts: café naïve" + Environment.NewLine + "second line", subtitle.Paragraphs[2].Text);
        Assert.Equal("<b>Bold</b> and <u>underline</u>", subtitle.Paragraphs[3].Text);
        Assert.Equal("日本語のテスト", subtitle.Paragraphs[4].Text);
    }

    [Theory]
    [InlineData("<text style=\"Default\">Hello</text>", "Hello")]
    [InlineData("<text><i>Hello</i></text>", "<i>Hello</i>")]
    [InlineData("<text>one<br/>two</text>", "one\ntwo")]
    [InlineData("<text><b>a</b> and <u>b</u></text>", "<b>a</b> and <u>b</u>")]
    [InlineData("<text><karaoke>sung</karaoke></text>", "sung")] // unknown markup: keep the text
    [InlineData("plain, no markup", "plain, no markup")]
    [InlineData("<text>unclosed", null)] // not parsable as XML - caller keeps the raw block
    public void GetTextFromMatroskaBlock_ReadsUsfMarkup(string block, string? expected)
    {
        var actual = UniversalSubtitleFormat.GetTextFromMatroskaBlock(block);
        Assert.Equal(expected?.Replace("\n", Environment.NewLine), actual);
    }
}
