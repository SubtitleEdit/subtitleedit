using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibSETests.ContainerFormats;

/// <summary>
/// CEA-608/708 captions embedded as ATSC A/53 SEI in the H.264/H.265 video of an MP4 (#16164).
/// sample_mp4_h264_no_captions.mp4: ffmpeg testsrc2, 6 seconds at 29.97 fps, x264 with B-frames,
/// AAC audio, index at the end. sample_mp4_hevc_no_captions.mp4: the same at 25 fps as x265 "hvc1"
/// with B-frames, index at the front (faststart).
/// </summary>
public class Mp4ClosedCaptionWriterTest
{
    private const string H264File = "sample_mp4_h264_no_captions.mp4";
    private const string HevcFile = "sample_mp4_hevc_no_captions.mp4";

    private static string FilePath(string name) => Path.Combine(Directory.GetCurrentDirectory(), "Files", name);

    private static Subtitle MakeSubtitle()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("Hello world", 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph("Second caption", 3000, 4200));
        return subtitle;
    }

    private static string Embed(string inputFileName, ClosedCaptionBytes captions)
    {
        var output = Path.Combine(Path.GetTempPath(), "se-mp4-cc-" + System.Guid.NewGuid().ToString("N") + ".mp4");
        Mp4ClosedCaptionWriter.Write(inputFileName, output, captions, null);
        return output;
    }

    private static SortedDictionary<int, List<Paragraph>> ReadCaptions(string fileName)
    {
        return new MP4Parser(fileName).ClosedCaptionTracks;
    }

    [Theory]
    [InlineData(H264File)]
    [InlineData(HevcFile)]
    public void EmbedsCaptionsReadBackAtTheirTimes(string fileName)
    {
        var input = FilePath(fileName);
        Assert.Empty(ReadCaptions(input));

        var output = Embed(input, new ClosedCaptionBytes { Field1 = SccBytePairs.FromSubtitle(MakeSubtitle()) });
        try
        {
            var cc1 = Assert.Single(ReadCaptions(output)).Value;
            Assert.Equal(2, cc1.Count);
            Assert.Equal("Hello world", cc1[0].Text);
            Assert.InRange(cc1[0].StartTime.TotalMilliseconds, 960, 1045);
            Assert.InRange(cc1[0].EndTime.TotalMilliseconds, 1960, 2045);
            Assert.Equal("Second caption", cc1[1].Text);
            Assert.InRange(cc1[1].StartTime.TotalMilliseconds, 2960, 3045);
            Assert.InRange(cc1[1].EndTime.TotalMilliseconds, 4160, 4245);
        }
        finally
        {
            File.Delete(output);
        }
    }

    /// <summary>
    /// Only the video samples change: the audio samples are where the new chunk offsets say,
    /// with the same bytes.
    /// </summary>
    [Theory]
    [InlineData(H264File)]
    [InlineData(HevcFile)]
    public void AudioIsCopiedAsItIs(string fileName)
    {
        var input = FilePath(fileName);
        var output = Embed(input, new ClosedCaptionBytes { Field1 = SccBytePairs.FromSubtitle(MakeSubtitle()) });
        try
        {
            var audioBefore = ReadTrackSamples(input, "soun");
            Assert.True(audioBefore.Length > 1000);
            Assert.Equal(audioBefore, ReadTrackSamples(output, "soun"));
            Assert.True(ReadTrackSamples(output, "vide").Length > ReadTrackSamples(input, "vide").Length);
        }
        finally
        {
            File.Delete(output);
        }
    }

    /// <summary>
    /// The samples of the first track with the handler type, read via its stco/stsc/stsz tables.
    /// </summary>
    private static byte[] ReadTrackSamples(string fileName, string handlerType)
    {
        var data = File.ReadAllBytes(fileName);
        var moov = FindBoxes(data, 0, data.Length, "moov").Single();
        foreach (var trak in FindBoxes(data, moov.Start, moov.End, "trak"))
        {
            var mdia = FindBoxes(data, trak.Start, trak.End, "mdia").Single();
            var hdlr = FindBoxes(data, mdia.Start, mdia.End, "hdlr").Single();
            if (System.Text.Encoding.ASCII.GetString(data, hdlr.Start + 8, 4) != handlerType)
            {
                continue;
            }

            var minf = FindBoxes(data, mdia.Start, mdia.End, "minf").Single();
            var stbl = FindBoxes(data, minf.Start, minf.End, "stbl").Single();
            var stco = FindBoxes(data, stbl.Start, stbl.End, "stco").Single();
            var stsc = FindBoxes(data, stbl.Start, stbl.End, "stsc").Single();
            var stsz = FindBoxes(data, stbl.Start, stbl.End, "stsz").Single();
            var chunkCount = ReadUInt(data, stco.Start + 4);
            var stscCount = ReadUInt(data, stsc.Start + 4);
            var fixedSize = ReadUInt(data, stsz.Start + 4);
            var sampleCount = ReadUInt(data, stsz.Start + 8);
            var result = new List<byte>();
            var sample = 0;
            var stscIndex = 0;
            for (var chunk = 1; chunk <= chunkCount; chunk++)
            {
                while (stscIndex + 1 < stscCount && ReadUInt(data, stsc.Start + 8 + (stscIndex + 1) * 12) <= chunk)
                {
                    stscIndex++;
                }

                var perChunk = ReadUInt(data, stsc.Start + 8 + stscIndex * 12 + 4);
                var offset = ReadUInt(data, stco.Start + 8 + (chunk - 1) * 4);
                for (var i = 0; i < perChunk && sample < sampleCount; i++, sample++)
                {
                    var size = fixedSize != 0 ? fixedSize : ReadUInt(data, stsz.Start + 12 + sample * 4);
                    result.AddRange(data.Skip(offset).Take(size));
                    offset += size;
                }
            }

            return result.ToArray();
        }

        return System.Array.Empty<byte>();
    }

    private static int ReadUInt(byte[] data, int offset) =>
        (int)System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));

    /// <summary>
    /// Payload ranges of the child boxes of the given type between start and end.
    /// </summary>
    private static List<(int Start, int End)> FindBoxes(byte[] data, int start, int end, string type)
    {
        var result = new List<(int Start, int End)>();
        var position = start;
        while (position + 8 <= end)
        {
            var size = ReadUInt(data, position);
            if (size < 8)
            {
                break;
            }

            if (System.Text.Encoding.ASCII.GetString(data, position + 4, 4) == type)
            {
                result.Add((position + 8, position + size));
            }

            position += size;
        }

        return result;
    }

    /// <summary>
    /// Embedding again replaces the caption SEI instead of adding a second copy (the file keeps
    /// its size), and embedding nothing leaves no captions.
    /// </summary>
    [Fact]
    public void EmbeddingAgainReplacesCaptions()
    {
        var first = Embed(FilePath(H264File), new ClosedCaptionBytes { Field1 = SccBytePairs.FromSubtitle(MakeSubtitle()) });
        var other = new Subtitle();
        other.Paragraphs.Add(new Paragraph("Replaced", 1500, 3000));
        var second = Embed(first, new ClosedCaptionBytes { Field1 = SccBytePairs.FromSubtitle(other) });
        var removed = Embed(second, new ClosedCaptionBytes());
        try
        {
            Assert.Equal(new FileInfo(first).Length, new FileInfo(second).Length);
            var cue = Assert.Single(Assert.Single(ReadCaptions(second)).Value);
            Assert.Equal("Replaced", cue.Text);
            Assert.Empty(ReadCaptions(removed));
        }
        finally
        {
            File.Delete(first);
            File.Delete(second);
            File.Delete(removed);
        }
    }

    /// <summary>
    /// Field 2 pairs with channel 1 codes are CC3.
    /// </summary>
    [Fact]
    public void Field2IsCc3()
    {
        var field2 = new Subtitle();
        field2.Paragraphs.Add(new Paragraph("Field two", 1000, 2000));
        var output = Embed(FilePath(H264File), new ClosedCaptionBytes
        {
            Field1 = SccBytePairs.FromSubtitle(MakeSubtitle()),
            Field2 = SccBytePairs.FromSubtitle(field2),
        });
        try
        {
            var tracks = ReadCaptions(output);
            Assert.Equal(new[] { 1, 3 }, tracks.Keys.ToArray());
            Assert.Equal("Field two", Assert.Single(tracks[3]).Text);
        }
        finally
        {
            File.Delete(output);
        }
    }

    /// <summary>
    /// An MCC file goes in with its CEA-608 and CEA-708 data, and both read back.
    /// </summary>
    [Fact]
    public void EmbedsMccCea608AndCea708()
    {
        var captions = ClosedCaptionBytes.FromFile(FilePath("sample_mcc_cea608_708.mcc"));
        Assert.True(captions.HasCea708);
        var output = Embed(FilePath(H264File), captions);
        try
        {
            var tracks = ReadCaptions(output);
            Assert.Contains(tracks.Keys, k => k < ClosedCaptionDecoder.Cea708TrackKeyOffset);
            Assert.Contains(ClosedCaptionDecoder.Cea708TrackKeyOffset + 1, tracks.Keys);
            Assert.NotEmpty(tracks[ClosedCaptionDecoder.Cea708TrackKeyOffset + 1]);
        }
        finally
        {
            File.Delete(output);
        }
    }

    [Fact]
    public void NotAnMp4Throws()
    {
        var ftypOnly = new byte[] { 0, 0, 0, 16, (byte)'f', (byte)'t', (byte)'y', (byte)'p', (byte)'i', (byte)'s', (byte)'o', (byte)'m', 0, 0, 2, 0 };
        Assert.Throws<InvalidDataException>(() => Mp4ClosedCaptionWriter.Write(new MemoryStream(ftypOnly), new MemoryStream(), new ClosedCaptionBytes(), null));
    }

    [Fact]
    public void FragmentedMp4Throws()
    {
        var output = Path.Combine(Path.GetTempPath(), "se-mp4-cc-" + System.Guid.NewGuid().ToString("N") + ".mp4");
        try
        {
            Assert.Throws<System.NotSupportedException>(() => Mp4ClosedCaptionWriter.Write(FilePath("sample_fragmented_mp4_cea608_708.mp4"), output, new ClosedCaptionBytes(), null));
        }
        finally
        {
            File.Delete(output);
        }
    }
}
