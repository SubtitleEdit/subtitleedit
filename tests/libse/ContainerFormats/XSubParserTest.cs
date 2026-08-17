using Nikse.SubtitleEdit.Core.ContainerFormats;
using SkiaSharp;
using System.IO;

namespace LibSETests.ContainerFormats;

/// <summary>
/// Guards reading XSUB ("DivX") subtitles out of an .avi. The fixture is a 10 second 640x480
/// clip muxed by ffmpeg with three XSUB events ("Hello world", a two-liner, "THE END").
/// </summary>
public class XSubParserTest
{
    private static string FixturePath => Path.Combine(Directory.GetCurrentDirectory(), "Files", "sample_XSUB.avi");

    [Fact]
    public void ParseAviReadsEveryPacketOfTheSubtitleStream()
    {
        var result = XSubParser.ParseAvi(FixturePath);

        var track = Assert.Single(result.Tracks);
        Assert.Equal(1, track.StreamNumber); // stream 0 is the video
        Assert.Equal(3, track.Subtitles.Count);

        Assert.Equal(1000, track.Subtitles[0].Start.TotalMilliseconds);
        Assert.Equal(2991, track.Subtitles[0].End.TotalMilliseconds);
        Assert.Equal(4000, track.Subtitles[1].Start.TotalMilliseconds);
        Assert.Equal(7000, track.Subtitles[2].Start.TotalMilliseconds);

        // The main header gives the frame the subtitle rectangles are positioned in.
        Assert.Equal(640, result.VideoWidth);
        Assert.Equal(480, result.VideoHeight);

        foreach (var xSub in track.Subtitles)
        {
            Assert.True(xSub.Width > 0 && xSub.Height > 0);
            Assert.True(xSub.Left > 0, "the caption is not flush against the left edge");
            Assert.True(xSub.Top + xSub.Height <= result.VideoHeight);
        }
    }

    /// <summary>
    /// An XSUB bitmap is stored as two fields, the first holding the even scan lines and the
    /// second the odd ones. Decoding the payload as one sequential run instead renders the even
    /// lines into the top half of the bitmap and the odd lines into the bottom half - i.e. the
    /// caption twice, squeezed to half height. The expected number of drawn pixels per line of
    /// the "Hello world" caption pins the interlaced arrangement down.
    /// </summary>
    [Fact]
    public void GetImageDecodesBothInterlacedFields()
    {
        var subtitles = XSubParser.ParseAviSubtitles(FixturePath);

        using var bitmap = subtitles[0].GetImage();
        Assert.Equal(246, bitmap.Width);
        Assert.Equal(42, bitmap.Height);

        var expected = new[]
        {
            44, 57, 62, 63, 63, 63, 63, 63, 63, 63, 145, 169, 178, 188, 195, 212, 219, 221, 226, 212, 204,
            202, 203, 201, 198, 184, 184, 184, 185, 186, 179, 182, 188, 202, 196, 193, 186, 180, 174, 163, 149, 119,
        };
        Assert.Equal(expected, DrawnPixelsPerLine(bitmap));

        // Every event decodes to something - a blank bitmap means the RLE ran out immediately.
        foreach (var xSub in subtitles)
        {
            using var image = xSub.GetImage();
            Assert.Contains(DrawnPixelsPerLine(image), count => count > 0);
        }
    }

    private static int[] DrawnPixelsPerLine(SKBitmap bitmap)
    {
        var perLine = new int[bitmap.Height];
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).Alpha != 0)
                {
                    perLine[y]++;
                }
            }
        }

        return perLine;
    }

    /// <summary>
    /// Files whose RIFF structure does not lead to the packets (damaged index, unusual muxer)
    /// fall back to scanning for packet headers.
    /// </summary>
    [Fact]
    public void PacketsAreStillFoundWhenTheRiffStructureIsUnusable()
    {
        var avi = File.ReadAllBytes(FixturePath);

        // Break the "LIST movi" fourCC so the walk finds no subtitle chunks at all.
        var moviIndex = IndexOf(avi, "movi");
        Assert.True(moviIndex > 0);
        avi[moviIndex] = (byte)'x';

        using var stream = new MemoryStream(avi);
        var result = XSubParser.ParseAvi(stream);

        var track = Assert.Single(result.Tracks);
        Assert.Equal(-1, track.StreamNumber); // recovered by the scan, not attributable to a stream
        Assert.Equal(3, track.Subtitles.Count);
        Assert.Equal(1000, track.Subtitles[0].Start.TotalMilliseconds);
    }

    /// <summary>
    /// Scanning reads the whole file, so it must stay reserved for files the RIFF walk cannot
    /// answer for. An .avi whose headers parse and declare no subtitle stream - the ordinary
    /// video file - is answered from the headers alone, even if subtitle-looking bytes exist
    /// further in.
    /// </summary>
    [Fact]
    public void AviWithoutADeclaredSubtitleStreamIsNotScanned()
    {
        var avi = File.ReadAllBytes(FixturePath);

        // Make the subtitle stream header describe something else than XSUB. ffmpeg declares an
        // XSUB stream as a video stream ("vids") with the "DXSB" handler, so the handler is what
        // identifies it.
        var handlerIndex = IndexOf(avi, "DXSB");
        Assert.True(handlerIndex > 0);
        avi[handlerIndex] = (byte)'X';

        using var stream = new MemoryStream(avi);
        Assert.Empty(XSubParser.ParseAvi(stream).Tracks);
    }

    [Fact]
    public void NonAviInputYieldsNoSubtitles()
    {
        using var stream = new MemoryStream(new byte[4096]);
        var result = XSubParser.ParseAvi(stream);
        Assert.Empty(result.Tracks);
        Assert.Empty(result.Subtitles);
    }

    private static int IndexOf(byte[] data, string text)
    {
        for (var i = 0; i + text.Length <= data.Length; i++)
        {
            var match = true;
            for (var j = 0; j < text.Length; j++)
            {
                if (data[i + j] != (byte)text[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                return i;
            }
        }

        return -1;
    }
}
