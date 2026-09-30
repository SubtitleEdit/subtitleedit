using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System;
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
        // the last one is the caption still on screen when the stream ends; the 134 ms between
        // its roll-up and the next row's first characters is part of it, not a cue of its own
        Assert.Equal(3, cea608.Count);
        Assert.Equal("(<i>inaudible radio chatter</i>)", cea608[0].Text);
        Assert.InRange(cea608[0].StartTime.TotalMilliseconds, 1034 - 1, 1034 + 1);

        Assert.NotNull(parser.TrunCea708Subtitle);
        var cea708 = parser.TrunCea708Subtitle.Paragraphs;
        Assert.Equal("(<i> inaudible radio chatter</i> )", cea708[0].Text);
        Assert.InRange(cea708[0].StartTime.TotalMilliseconds, 1034 - 1, 1034 + 1);
        Assert.Equal(">> Safety remains our number one", cea708[1].Text);
        Assert.InRange(cea708[1].StartTime.TotalMilliseconds, 3169 - 1, 3169 + 1);
    }

    /// <summary>
    /// A bare media segment cut without its init segment (no moov, e.g. a captured DASH/HLS
    /// segment): the track timescale is unknown, and the 1000 fallback put every cue ~90x too
    /// late. The 90 kHz MPEG clock is assumed instead; this 30 kHz sample then comes out at a
    /// third of its real time, which proves which clock was used.
    /// </summary>
    [Fact]
    public void FragmentedMp4WithoutMoovAssumes90KhzTimescale()
    {
        var bytes = File.ReadAllBytes(FilePath("sample_fragmented_mp4_cea608_708.mp4"));
        var tempFile = Path.Combine(Path.GetTempPath(), "se_no_moov_" + Guid.NewGuid().ToString("N") + ".mp4");
        try
        {
            File.WriteAllBytes(tempFile, RemoveTopLevelBox(bytes, "moov"));
            var parser = new MP4Parser(tempFile);

            Assert.Null(parser.Moov);
            Assert.NotNull(parser.TrunCea608Subtitle);
            var cea608 = parser.TrunCea608Subtitle.Paragraphs;
            Assert.Equal("(<i>inaudible radio chatter</i>)", cea608[0].Text);
            Assert.InRange(cea608[0].StartTime.TotalMilliseconds, 1034 / 3.0 - 1, 1034 / 3.0 + 1);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private static byte[] RemoveTopLevelBox(byte[] bytes, string boxName)
    {
        using var output = new MemoryStream();
        var offset = 0;
        while (offset + 8 <= bytes.Length)
        {
            var size = (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
            var name = System.Text.Encoding.ASCII.GetString(bytes, offset + 4, 4);
            if (name != boxName)
            {
                output.Write(bytes, offset, size);
            }

            offset += size;
        }

        return output.ToArray();
    }
}
