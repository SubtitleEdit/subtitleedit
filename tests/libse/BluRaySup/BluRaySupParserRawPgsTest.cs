using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.Text;

namespace LibSETests.BluRaySup;

public class BluRaySupParserRawPgsTest
{
    private static byte[] CreateSupContent(int count)
    {
        var sup = new List<byte>();
        for (var i = 0; i < count; i++)
        {
            using var bitmap = new SKBitmap(64, 32);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                canvas.DrawRect(new SKRect(4, 4, 60, 28), new SKPaint { Color = SKColors.Yellow });
            }

            var pic = new BluRaySupPicture
            {
                StartTime = 1000 + i * 4000,
                EndTime = 3000 + i * 4000,
                Width = 720,
                Height = 480,
                CompositionNumber = i * 2,
            };
            sup.AddRange(BluRaySupPicture.CreateSupFrame(pic, bitmap, SKColors.White, 25, 10, 10, BluRayContentAlignment.BottomCenter));
        }

        return sup.ToArray();
    }

    // A standalone SUP segment is "PG" + PTS(4) + DTS(4) + type(1) + size(2) + payload;
    // the raw elementary form (Matroska raw-mode extraction, issue #12683) is just
    // type + size + payload.
    private static byte[] StripPgHeaders(byte[] sup)
    {
        var raw = new List<byte>();
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            Assert.Equal(0x50, sup[position]);
            Assert.Equal(0x47, sup[position + 1]);
            var size = (sup[position + 11] << 8) + sup[position + 12];
            raw.AddRange(sup.Skip(position + 10).Take(3 + size));
            position += 13 + size;
        }

        Assert.Equal(sup.Length, position); // segments must chain exactly to the end
        return raw.ToArray();
    }

    private static void WithTempFile(byte[] content, Action<string> assert)
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, content);
            assert(path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void RawPgsStreamRoundTripsThroughSnifferAndParser()
    {
        var sup = CreateSupContent(2);
        var raw = StripPgHeaders(sup);

        // The proper SUP is detected as SUP, not as raw PGS - and vice versa.
        WithTempFile(sup, path =>
        {
            Assert.True(FileUtil.IsBluRaySup(path));
            Assert.False(FileUtil.IsRawPgsSegmentStream(path));
        });

        WithTempFile(raw, path =>
        {
            Assert.False(FileUtil.IsBluRaySup(path));
            Assert.True(FileUtil.IsRawPgsSegmentStream(path));

            var pcsDataList = BluRaySupParser.ParseRawPgsSegmentStream(path, new StringBuilder());
            var withImages = pcsDataList.Where(p => p.PcsObjects.Count > 0).ToList();
            Assert.Equal(2, withImages.Count);

            // Raw streams carry no PTS, so everything is untimed until the caller
            // assigns placeholder timings.
            Assert.All(pcsDataList, p => Assert.Equal(0, p.StartTime));

            foreach (var pcsData in withImages)
            {
                using var decoded = pcsData.GetBitmap();
                Assert.Equal(64, decoded.Width);
                Assert.Equal(32, decoded.Height);
            }
        });
    }

    [Fact]
    public void TruncatedOdsSegmentDoesNotCorruptOtherObjects()
    {
        // A truncated ODS (fewer than the 4 header bytes) must be skipped, not parsed
        // from stale rented-buffer bytes left by the previous segment - stale reads could
        // fabricate an ObjectId and overwrite a real object's image data.
        var raw = StripPgHeaders(CreateSupContent(2)).ToList();
        raw.AddRange(new byte[] { 0x15, 0x00, 0x02, 0x00, 0x00 }); // ODS, size 2: truncated
        raw.AddRange(new byte[] { 0x80, 0x00, 0x00 }); // END

        WithTempFile(raw.ToArray(), path =>
        {
            var pcsDataList = BluRaySupParser.ParseRawPgsSegmentStream(path, new StringBuilder());
            var withImages = pcsDataList.Where(p => p.PcsObjects.Count > 0).ToList();
            Assert.Equal(2, withImages.Count);

            foreach (var pcsData in withImages)
            {
                using var decoded = pcsData.GetBitmap();
                Assert.Equal(64, decoded.Width); // image data intact, not clobbered
                Assert.Equal(32, decoded.Height);
            }
        });
    }

    [Fact]
    public void SetPlaceholderTimingsAssignsSequentialNonOverlappingTimes()
    {
        var sup = CreateSupContent(3);
        var raw = StripPgHeaders(sup);

        WithTempFile(raw, path =>
        {
            var pcsDataList = BluRaySupParser.ParseRawPgsSegmentStream(path, new StringBuilder());
            Assert.True(pcsDataList.Count >= 3);

            BluRaySupParser.SetPlaceholderTimings(pcsDataList);

            Assert.Equal(0, pcsDataList[0].StartTimeCode.TotalMilliseconds, 3);
            Assert.Equal(3000, pcsDataList[0].EndTimeCode.TotalMilliseconds, 3);
            Assert.Equal(4000, pcsDataList[1].StartTimeCode.TotalMilliseconds, 3);
            for (var i = 1; i < pcsDataList.Count; i++)
            {
                Assert.True(pcsDataList[i].StartTime > pcsDataList[i - 1].EndTime,
                    $"Placeholder timings must not overlap (display set {i})");
            }
        });
    }
}
