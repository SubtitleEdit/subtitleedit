using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.IO;
using System.Linq;

namespace LibSETests.SubtitleFormats;

/// <summary>
/// MCC files built from the real caption stream of ffmpeg's fate-suite Closedcaption_rollup.m2v
/// (one 29.97 fps caption distribution packet per frame, 20 cc_data triplets, FA0000 padding) -
/// ffmpeg's own MCC demuxer + caption decoder read both as the two roll-up cues checked here.
/// </summary>
public class MacCaptionCea608Test
{
    private static Subtitle Load(string fileName)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", fileName);
        var lines = File.ReadAllLines(path).ToList();
        var format = new MacCaption10();
        Assert.True(format.IsMine(lines, path));
        var subtitle = new Subtitle();
        format.LoadSubtitle(subtitle, lines, path);
        return subtitle;
    }

    /// <summary>
    /// CEA-608 only (the CEA-708 part is padding): only cc_type 2 bytes were decoded, so the file
    /// read as empty and was not even recognized as MCC.
    /// </summary>
    [Fact]
    public void ReadsCea608Cc1WhenThereIsNoCea708Text()
    {
        var subtitle = Load("sample_mcc_cea608_only.mcc");

        Assert.Equal("(<i>inaudible radio chatter</i>)", subtitle.Paragraphs[0].Text);
        Assert.Equal("(<i>inaudible radio chatter</i>)" + System.Environment.NewLine + ">> Safety remains our number one", subtitle.Paragraphs[1].Text);
    }

    /// <summary>
    /// CEA-608 and CEA-708: the CEA-708 text is used, as before.
    /// </summary>
    [Fact]
    public void PrefersCea708TextWhenPresent()
    {
        var subtitle = Load("sample_mcc_cea608_708.mcc");

        Assert.Equal("(<i> inaudible radio chatter</i> )", subtitle.Paragraphs[0].Text);
        Assert.Equal(">> Safety remains our number one", subtitle.Paragraphs[1].Text);
    }
}
