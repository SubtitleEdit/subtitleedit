using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.ProgramStream;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibSETests.ContainerFormats;

/// <summary>
/// CEA-608 captions embedded as ATSC A/53 picture user data in an MPEG-2 program stream (#15405).
/// sample_mpg_mpeg2_no_captions.mpg is the video of sample_TS_cea608_mpeg2.ts (4.5 seconds at
/// 29.97 fps, with B-frames) with its caption user data removed by ffmpeg's filter_units, muxed as .mpg.
/// </summary>
public class ProgramStreamClosedCaptionWriterTest
{
    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph("Second caption", 3000, 4200));
        return subtitle;
    }

    private static byte[] Embed(byte[] input, List<SccBytePairs.TimedPair> field1, List<SccBytePairs.TimedPair> field2)
    {
        using var output = new MemoryStream();
        ProgramStreamClosedCaptionWriter.Write(new MemoryStream(input), output, field1, field2, null);
        return output.ToArray();
    }

    private static SortedDictionary<int, List<Paragraph>> ReadCaptions(byte[] programStream)
    {
        return ProgramStreamClosedCaptionReader.Read(new MemoryStream(programStream), ProgramStreamClosedCaptionReader.DefaultProbeMilliseconds, null);
    }

    [Fact]
    public void EmbedsCaptionsReadBackAtTheirTimes()
    {
        var input = File.ReadAllBytes(FilePath("sample_mpg_mpeg2_no_captions.mpg"));
        Assert.Empty(ReadCaptions(input));

        var output = Embed(input, SccBytePairs.FromSubtitle(MakeSubtitle()), new List<SccBytePairs.TimedPair>());

        var cc1 = Assert.Single(ReadCaptions(output)).Value;
        Assert.Equal(2, cc1.Count);
        Assert.Equal("Hello world", cc1[0].Text);
        Assert.InRange(cc1[0].StartTime.TotalMilliseconds, 960, 1040);
        Assert.InRange(cc1[0].EndTime.TotalMilliseconds, 1960, 2040);
        Assert.Equal("Second caption", cc1[1].Text);
        Assert.InRange(cc1[1].StartTime.TotalMilliseconds, 2960, 3040);
        Assert.InRange(cc1[1].EndTime.TotalMilliseconds, 4160, 4240);
    }

    /// <summary>
    /// Embedding again replaces the caption data instead of adding a second copy, and nothing
    /// but the caption data changes.
    /// </summary>
    [Fact]
    public void EmbeddingAgainReplacesCaptions()
    {
        var input = File.ReadAllBytes(FilePath("sample_mpg_mpeg2_no_captions.mpg"));
        var first = Embed(input, SccBytePairs.FromSubtitle(MakeSubtitle()), new List<SccBytePairs.TimedPair>());

        var other = new Subtitle();
        other.Paragraphs.Add(new Paragraph("Replaced", 1500, 3000));
        var second = Embed(first, SccBytePairs.FromSubtitle(other), new List<SccBytePairs.TimedPair>());

        Assert.Equal(first.Length, second.Length);
        var cue = Assert.Single(Assert.Single(ReadCaptions(second)).Value);
        Assert.Equal("Replaced", cue.Text);

        var removed = Embed(second, new List<SccBytePairs.TimedPair>(), new List<SccBytePairs.TimedPair>());
        Assert.Empty(ReadCaptions(removed));
    }

    /// <summary>
    /// Field 2 pairs with channel 1 codes are CC3.
    /// </summary>
    [Fact]
    public void Field2IsCc3()
    {
        var input = File.ReadAllBytes(FilePath("sample_mpg_mpeg2_no_captions.mpg"));
        var field2 = new Subtitle();
        field2.Paragraphs.Add(new Paragraph("Field two", 1000, 2000));

        var output = Embed(input, SccBytePairs.FromSubtitle(MakeSubtitle()), SccBytePairs.FromSubtitle(field2));

        var tracks = ReadCaptions(output);
        Assert.Equal(new[] { 1, 3 }, tracks.Keys.ToArray());
        Assert.Equal("Field two", Assert.Single(tracks[3]).Text);
    }

    /// <summary>
    /// An MCC file goes in with all its caption data - CEA-608 and CEA-708 - and reads back like
    /// the MCC itself decodes. sample_mcc_cea608_708.mcc has the captions of the same broadcast
    /// clip as the video (29.97 fps, one caption packet per frame).
    /// </summary>
    [Theory]
    [InlineData("sample_mcc_cea608_708.mcc", true)]
    [InlineData("sample_mcc_cea608_only.mcc", false)]
    public void EmbedsMccCea608AndCea708(string mccFileName, bool hasCea708)
    {
        var mccPath = FilePath(mccFileName);
        var captions = ClosedCaptionBytes.FromFile(mccPath);
        Assert.Equal(hasCea708, captions.HasCea708);
        Assert.NotEmpty(captions.Field1);

        var input = File.ReadAllBytes(FilePath("sample_mpg_mpeg2_no_captions.mpg"));
        using var output = new MemoryStream();
        ProgramStreamClosedCaptionWriter.Write(new MemoryStream(input), output, captions, null);
        var tracks = ReadCaptions(output.ToArray());

        var expected = DecodeMcc(mccPath);
        Assert.Equal(expected.Keys, tracks.Keys);
        Assert.Equal(hasCea708, tracks.ContainsKey(ClosedCaptionDecoder.Cea708TrackKeyOffset + 1));
        foreach (var track in expected)
        {
            Assert.Equal(track.Value.Select(p => p.Text), tracks[track.Key].Select(p => p.Text));
            for (var i = 0; i < track.Value.Count; i++)
            {
                Assert.InRange(tracks[track.Key][i].StartTime.TotalMilliseconds, track.Value[i].StartTime.TotalMilliseconds - 35, track.Value[i].StartTime.TotalMilliseconds + 35);
            }
        }
    }

    /// <summary>
    /// The reference: the MCC's cc_data decoded as it is, one packet per 29.97 frame.
    /// </summary>
    private static SortedDictionary<int, List<Paragraph>> DecodeMcc(string path)
    {
        var decoder = new ClosedCaptionDecoder();
        var frame = 0;
        foreach (var line in File.ReadAllLines(path).Where(l => l.Length > 12 && l[11] == '\t'))
        {
            var ccData = MacCaption10.GetAllCcData(line.Substring(12))
                .Where(cc => cc.Valid && (cc.Type >= 2 || (cc.Data1 & 0x7F) != 0 || (cc.Data2 & 0x7F) != 0))
                .Select(cc => new CcData(cc.Type, cc.Data1, cc.Data2))
                .ToArray();
            decoder.AddFrame((long)System.Math.Round(frame++ * 1001.0 / 30), ccData);
        }

        return decoder.Finish(0);
    }

    [Fact]
    public void NoMpeg2VideoThrows()
    {
        var packHeaderOnly = new byte[] { 0, 0, 1, 0xBA, 0x44, 0, 4, 0, 4, 1, 1, 0x89, 0xC3, 0xF8, 0, 0, 1, 0xB9 };
        Assert.Throws<InvalidDataException>(() => Embed(packHeaderOnly, new List<SccBytePairs.TimedPair>(), new List<SccBytePairs.TimedPair>()));
    }

    /// <summary>
    /// Each byte pair gets its own slot from the line time on; a line that starts before the
    /// previous one is sent waits for it, and lines go out in time order. 00:00:01:00 is slot 30
    /// (1 second at 29.97).
    /// </summary>
    [Fact]
    public void SccLinesAreQueuedInTimeOrder()
    {
        var pairs = SccBytePairs.FromSccLines(new[]
        {
            "Scenarist_SCC V1.0",
            string.Empty,
            "00:00:01:00\t94ae 94ae 9420",
            "00:00:02:00\t942c 942c",
            "00:00:01:01\t942f 0000",
        });

        Assert.Equal(new long[] { 30, 31, 32, 33, 34, 60, 61 }, pairs.Select(p => p.Slot).ToArray());
        Assert.Equal(new byte[] { 0x94, 0xAE }, new[] { pairs[0].Data1, pairs[0].Data2 });
        Assert.Equal(new byte[] { 0x94, 0x2F }, new[] { pairs[3].Data1, pairs[3].Data2 });
        Assert.Equal(new byte[] { 0x80, 0x80 }, new[] { pairs[4].Data1, pairs[4].Data2 }); // 00 is not valid 608
    }
}
