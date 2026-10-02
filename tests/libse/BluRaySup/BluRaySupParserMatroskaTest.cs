using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using SkiaSharp;
using System.Text;

namespace LibSETests.BluRaySup;

/// <summary>
/// A PGS track in Matroska must time like the same track extracted to .sup (mkvextract), since the
/// display sets themselves (not the Matroska block durations) say when an image is cleared (issue #14269).
/// </summary>
public class BluRaySupParserMatroskaTest
{
    private sealed record Segment(long Pts, byte[] Raw);

    private static byte[] CreateSup(params (int StartMs, int EndMs, SKColor Color)[] cues)
    {
        var sup = new List<byte>();
        for (var i = 0; i < cues.Length; i++)
        {
            using var bitmap = new SKBitmap(64, 32);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Transparent);
                canvas.DrawRect(new SKRect(4, 4, 60, 28), new SKPaint { Color = cues[i].Color });
            }

            var pic = new BluRaySupPicture
            {
                StartTime = cues[i].StartMs,
                EndTime = cues[i].EndMs,
                Width = 720,
                Height = 480,
                CompositionNumber = i * 2,
            };
            sup.AddRange(BluRaySupPicture.CreateSupFrame(pic, bitmap, SKColors.White, 25, 10, 10, BluRayContentAlignment.BottomCenter));
        }

        return sup.ToArray();
    }

    // "PG" + PTS(4) + DTS(4) + type(1) + size(2) + payload -> PTS + (type + size + payload), as stored in Matroska
    private static List<Segment> ToSegments(byte[] sup)
    {
        var segments = new List<Segment>();
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            var pts = ((long)sup[position + 2] << 24) | ((long)sup[position + 3] << 16) | ((long)sup[position + 4] << 8) | sup[position + 5];
            var size = (sup[position + 11] << 8) + sup[position + 12];
            segments.Add(new Segment(pts, sup.Skip(position + 10).Take(3 + size).ToArray()));
            position += 13 + size;
        }

        return segments;
    }

    // One block per display set (PCS ... END), time code from the PCS, with the given block duration.
    private static List<MatroskaSubtitle> ToDisplaySetBlocks(byte[] sup, long blockDurationMs)
    {
        var blocks = new List<MatroskaSubtitle>();
        var data = new List<byte>();
        long startMs = -1;
        foreach (var segment in ToSegments(sup))
        {
            if (startMs < 0)
            {
                startMs = segment.Pts / 90;
            }

            data.AddRange(segment.Raw);
            if (segment.Raw[0] == 0x80)
            {
                blocks.Add(new MatroskaSubtitle(data.ToArray(), startMs, blockDurationMs));
                data.Clear();
                startMs = -1;
            }
        }

        return blocks;
    }

    private static List<(long Start, long End)> Timings(List<BluRaySupParser.PcsData> list) =>
        list.Select(p => (p.StartTime, p.EndTime)).ToList();

    private static List<BluRaySupParser.PcsData> ParseSup(byte[] sup) =>
        BluRaySupParser.ParseBluRaySup(new MemoryStream(sup), new StringBuilder(), false, new Dictionary<int, List<PaletteInfo>>(), new Dictionary<int, List<BluRaySupParser.OdsData>>());

    private static readonly MatroskaTrackInfo Track = new() { TrackNumber = 1, ContentEncodingType = -1 };

    [Fact]
    public void ShortBlockDurations_DoNotCutCues_MatchesSupTiming()
    {
        var sup = CreateSup((9560, 13240, SKColors.Yellow), (14000, 16000, SKColors.Red));
        var expected = Timings(ParseSup(sup));

        // A muxer that sets each block's duration to the gap to the next PGS packet (or anything short)
        var actual = Timings(BluRaySupParser.ParseBluRaySupFromMatroska(ToDisplaySetBlocks(sup, 21), Track));

        Assert.Equal(2, expected.Count);
        Assert.Equal(expected, actual);
        Assert.True(actual[0].End - actual[0].Start > 3000 * 90);
    }

    [Fact]
    public void NoBlockDurations_MatchesSupTiming()
    {
        var sup = CreateSup((1000, 3000, SKColors.Yellow), (4000, 6000, SKColors.Red), (6000, 8000, SKColors.Blue));
        var actual = Timings(BluRaySupParser.ParseBluRaySupFromMatroska(ToDisplaySetBlocks(sup, 0), Track));
        Assert.Equal(Timings(ParseSup(sup)), actual);
    }

    [Fact]
    public void StartTimeIsTheBlockTimeCode_NotOneMillisecondEarlier()
    {
        var sup = CreateSup((9560, 13240, SKColors.Yellow));
        var actual = BluRaySupParser.ParseBluRaySupFromMatroska(ToDisplaySetBlocks(sup, 0), Track);
        Assert.Single(actual);
        Assert.Equal(ToSegments(sup)[0].Pts / 90 * 90, actual[0].StartTime);
    }

    [Fact]
    public void OneSegmentPerBlock_MatchesSupTiming()
    {
        var sup = CreateSup((1000, 3000, SKColors.Yellow), (4000, 6000, SKColors.Red));
        var blocks = ToSegments(sup).Select(s => new MatroskaSubtitle(s.Raw, s.Pts / 90)).ToList();
        var actual = Timings(BluRaySupParser.ParseBluRaySupFromMatroska(blocks, Track));
        Assert.Equal(Timings(ParseSup(sup)).Select(t => (t.Start / 90 * 90, t.End / 90 * 90)).ToList(), actual);
    }

    [Fact]
    public void SegmentSplitAcrossBlocks_IsReassembled()
    {
        var sup = CreateSup((1000, 3000, SKColors.Yellow));
        var raw = ToDisplaySetBlocks(sup, 0).SelectMany(b => b.GetData(Track)).ToArray();
        var blocks = new List<MatroskaSubtitle>();
        for (var i = 0; i < raw.Length; i += 7)
        {
            blocks.Add(new MatroskaSubtitle(raw.Skip(i).Take(7).ToArray(), 1000));
        }

        var actual = BluRaySupParser.ParseBluRaySupFromMatroska(blocks, Track);
        Assert.Single(actual);
        Assert.Equal(1000 * 90, actual[0].StartTime);
        Assert.NotEmpty(actual[0].BitmapObjects);
    }
}
