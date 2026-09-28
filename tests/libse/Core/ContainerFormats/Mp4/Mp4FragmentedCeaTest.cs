using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System.IO;

namespace LibSETests.Core.ContainerFormats.Mp4;

public class Mp4FragmentedCeaTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    /// <summary>
    /// ffmpeg fate-suite Closedcaption_rollup.m2v as H.264 with "-a53cc 1", remuxed to fragmented
    /// MP4 (frag_keyframe+empty_moov). Each frame's SEI carries the CEA-608 pairs and the CEA-708
    /// packet bytes; the fragment reader kept only the first triplet per sample, so CEA-708 never
    /// came out of a fragmented file.
    /// </summary>
    [Fact]
    public void FragmentedMp4ReadsCea608AndCea708()
    {
        var parser = new MP4Parser(FilePath("sample_fragmented_mp4_cea608_708.mp4"));

        Assert.NotNull(parser.TrunCea608Subtitle);
        var cea608 = parser.TrunCea608Subtitle.Paragraphs;
        Assert.Equal(4, cea608.Count); // the last one is the caption still on screen when the stream ends
        Assert.Equal("(<i>inaudible radio chatter</i>)", cea608[0].Text);
        Assert.InRange(cea608[0].StartTime.TotalMilliseconds, 1034 - 1, 1034 + 1);

        Assert.NotNull(parser.TrunCea708Subtitle);
        var cea708 = parser.TrunCea708Subtitle.Paragraphs;
        Assert.Equal("(<i> inaudible radio chatter</i> )", cea708[0].Text);
        Assert.InRange(cea708[0].StartTime.TotalMilliseconds, 1034 - 1, 1034 + 1);
        Assert.Equal(">> Safety remains our number one", cea708[1].Text);
        Assert.InRange(cea708[1].StartTime.TotalMilliseconds, 3169 - 1, 3169 + 1);
    }
}
