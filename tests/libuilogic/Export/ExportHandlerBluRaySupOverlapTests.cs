using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System.Text;

namespace LibUiLogicTests.Export;

/// <summary>
/// Subtitles that overlap in time on a Blu-ray sup export (issue #14456). A Blu-ray shows one
/// display set at a time, so two subtitles can only be seen together when they are in the same
/// display set: the export cuts the timeline where a subtitle starts or ends and writes a display
/// set per slice, composing up to two objects in two windows.
/// </summary>
public class ExportHandlerBluRaySupOverlapTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "bluraysupoverlap_" + Guid.NewGuid().ToString("N"));

    public ExportHandlerBluRaySupOverlapTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private sealed record Segment(long Pts, byte Type, byte[] Payload);

    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;
    private const int Margin = 50;
    private const int CueWidth = 300;
    private const int CueHeight = 80;

    private static ImageParameter Cue(int index, double startSeconds, double endSeconds, ExportAlignment alignment, SKColor color, string text = "Hello")
    {
        var bitmap = new SKBitmap(CueWidth, CueHeight);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(color);
        }

        var parameter = new ImageParameter
        {
            Text = ExportTextTags.ToRenderableText(text),
            Bitmap = bitmap,
            StartTime = TimeSpan.FromSeconds(startSeconds),
            EndTime = TimeSpan.FromSeconds(endSeconds),
            Index = index,
            ScreenWidth = ScreenWidth,
            ScreenHeight = ScreenHeight,
            Alignment = alignment,
            BottomTopMargin = Margin,
            LeftRightMargin = 40,
            FramesPerSecond = 25,
            FontColor = color,
        };

        ExportTextTags.ApplyTransparencyTags(parameter, text);
        return parameter;
    }

    private string Export(params ImageParameter[] cues)
    {
        var fileName = Path.Combine(_dir, Guid.NewGuid().ToString("N") + ".sup");
        var handler = new ExportHandlerBluRaySup();
        handler.WriteHeader(fileName, cues[0]);
        foreach (var cue in cues)
        {
            handler.CreateParagraph(cue);
        }

        foreach (var cue in cues)
        {
            handler.WriteParagraph(cue);
        }

        handler.WriteFooter();
        return fileName;
    }

    /// <summary>
    /// Walks the file as "PG" + PTS(4) + DTS(4) + type(1) + size(2) + payload, which also proves
    /// the segments chain exactly to the end.
    /// </summary>
    private static List<Segment> ReadSegments(string fileName)
    {
        var sup = File.ReadAllBytes(fileName);
        var segments = new List<Segment>();
        var position = 0;
        while (position + 13 <= sup.Length)
        {
            Assert.Equal(0x50, sup[position]);
            Assert.Equal(0x47, sup[position + 1]);
            var pts = ((long)sup[position + 2] << 24) | ((long)sup[position + 3] << 16) | ((long)sup[position + 4] << 8) | sup[position + 5];
            var size = (sup[position + 11] << 8) + sup[position + 12];
            segments.Add(new Segment(pts, sup[position + 10], sup.Skip(position + 13).Take(size).ToArray()));
            position += 13 + size;
        }

        Assert.Equal(sup.Length, position);
        return segments;
    }

    private const byte Pds = 0x14;
    private const byte Ods = 0x15;
    private const byte Pcs = 0x16;
    private const byte Wds = 0x17;
    private const byte End = 0x80;

    private static int ObjectCount(Segment pcs) => pcs.Payload[10];
    private static int CompositionNumber(Segment pcs) => (pcs.Payload[5] << 8) + pcs.Payload[6];
    private static int Word(byte[] payload, int offset) => (payload[offset] << 8) + payload[offset + 1];
    private static int WindowY(Segment wds, int window) => Word(wds.Payload, 1 + window * 9 + 3);
    private static int WindowHeight(Segment wds, int window) => Word(wds.Payload, 1 + window * 9 + 7);
    private static int OdsWidth(Segment ods) => Word(ods.Payload, 7);
    private static int OdsHeight(Segment ods) => Word(ods.Payload, 9);
    private static int PaletteAlpha(Segment pds, int entry) => pds.Payload[2 + entry * 5 + 4];

    [Fact]
    public void TwoOverlappingLines_ShareTheScreenWhileBothAreOn()
    {
        var segments = ReadSegments(Export(
            Cue(0, 1, 3, ExportAlignment.BottomCenter, SKColors.White),
            Cue(1, 2, 4, ExportAlignment.TopCenter, SKColors.White)));

        // One slice per change: bottom alone, both, top alone, then the clear.
        Assert.Equal(new byte[]
        {
            Pcs, Wds, Pds, Ods, End,
            Pcs, Wds, Pds, Ods, Ods, End,
            Pcs, Wds, Pds, Ods, End,
            Pcs, Wds, End,
        }, segments.Select(s => s.Type));

        var pcs = segments.Where(s => s.Type == Pcs).ToList();
        Assert.Equal(new[] { 1, 2, 1, 0 }, pcs.Select(ObjectCount));
        Assert.Equal(new long[] { 1000 * 90, 2000 * 90, 3000 * 90, 4000 * 90 }, pcs.Select(p => p.Pts));

        // Every slice is an epoch start - it replaces what was on screen, so no clear is needed
        // between them.
        Assert.All(pcs.Take(3), p => Assert.Equal(0x80, p.Payload[7]));

        var numbers = pcs.Select(CompositionNumber).ToList();
        Assert.Equal(numbers.OrderBy(n => n), numbers);
        Assert.Equal(numbers.Count, numbers.Distinct().Count());

        // The slice with both: two windows that do not touch, the top one first.
        var both = segments.Where(s => s.Type == Wds).ElementAt(1);
        Assert.Equal(2, both.Payload[0]);
        Assert.Equal(Margin, WindowY(both, 0));
        Assert.Equal(ScreenHeight - Margin - CueHeight, WindowY(both, 1));
        Assert.True(WindowY(both, 0) + WindowHeight(both, 0) <= WindowY(both, 1));

        // ...and one object each, referenced from the PCS as objects 0 and 1 in windows 0 and 1.
        var bothPcs = pcs[1];
        Assert.Equal(0, Word(bothPcs.Payload, 11));
        Assert.Equal(0, bothPcs.Payload[13]);
        Assert.Equal(1, Word(bothPcs.Payload, 19));
        Assert.Equal(1, bothPcs.Payload[21]);
        var odsIds = segments.Skip(5).Take(6).Where(s => s.Type == Ods).Select(s => Word(s.Payload, 0));
        Assert.Equal(new[] { 0, 1 }, odsIds);
    }

    [Fact]
    public void TwoOverlappingLines_ReadBackAsBothLinesWhileTheyOverlap()
    {
        var fileName = Export(
            Cue(0, 1, 3, ExportAlignment.BottomCenter, SKColors.White),
            Cue(1, 2, 4, ExportAlignment.TopCenter, SKColors.White));

        var subtitles = BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());

        Assert.Equal(3, subtitles.Count);
        Assert.Equal(1000, subtitles[0].StartTimeCode.TotalMilliseconds, 1);
        Assert.Equal(2000, subtitles[0].EndTimeCode.TotalMilliseconds, 1);
        Assert.Equal(2000, subtitles[1].StartTimeCode.TotalMilliseconds, 1);
        Assert.Equal(3000, subtitles[1].EndTimeCode.TotalMilliseconds, 1);
        Assert.Equal(3000, subtitles[2].StartTimeCode.TotalMilliseconds, 1);
        Assert.Equal(4000, subtitles[2].EndTimeCode.TotalMilliseconds, 1);

        // The middle slice spans from the top line to the bottom line, with nothing in between.
        Assert.Equal(2, subtitles[1].PcsObjects.Count);
        Assert.Equal(Margin, subtitles[1].GetPosition().Top);
        using var bitmap = subtitles[1].GetBitmap();
        Assert.Equal(CueWidth, bitmap.Width);
        Assert.Equal(ScreenHeight - 2 * Margin, bitmap.Height);
        Assert.Equal(255, bitmap.GetPixel(CueWidth / 2, 0).Alpha);
        Assert.Equal(255, bitmap.GetPixel(CueWidth / 2, bitmap.Height - 1).Alpha);
        Assert.Equal(0, bitmap.GetPixel(CueWidth / 2, bitmap.Height / 2).Alpha);
    }

    [Fact]
    public void LinesThatDoNotOverlap_AreWrittenAsBefore()
    {
        var segments = ReadSegments(Export(
            Cue(0, 1, 2, ExportAlignment.BottomCenter, SKColors.White),
            Cue(1, 3, 4, ExportAlignment.TopCenter, SKColors.White)));

        var perLine = new byte[] { Pcs, Wds, Pds, Ods, End, Pcs, Wds, End };
        Assert.Equal(perLine.Concat(perLine), segments.Select(s => s.Type));
        Assert.Equal(new[] { 1, 0, 1, 0 }, segments.Where(s => s.Type == Pcs).Select(ObjectCount));
    }

    [Fact]
    public void LinesTouchingEndToStart_AreNotComposed()
    {
        var segments = ReadSegments(Export(
            Cue(0, 1, 2, ExportAlignment.BottomCenter, SKColors.White),
            Cue(1, 2, 3, ExportAlignment.BottomCenter, SKColors.White)));

        Assert.Equal(new[] { 1, 0, 1, 0 }, segments.Where(s => s.Type == Pcs).Select(ObjectCount));
    }

    [Fact]
    public void LinesOnTopOfEachOther_BecomeOneObject()
    {
        // Windows may not overlap, so two lines at the same place are drawn into one bitmap.
        var segments = ReadSegments(Export(
            Cue(0, 1, 3, ExportAlignment.BottomCenter, SKColors.White),
            Cue(1, 2, 4, ExportAlignment.BottomCenter, SKColors.White)));

        Assert.Equal(new[] { 1, 1, 1, 0 }, segments.Where(s => s.Type == Pcs).Select(ObjectCount));
        var bothOds = segments.Skip(5).First(s => s.Type == Ods);
        Assert.Equal(CueWidth, OdsWidth(bothOds));
        Assert.Equal(CueHeight, OdsHeight(bothOds));
    }

    [Fact]
    public void ThreeLinesAtOnce_ComposeNoMoreThanTwoObjects()
    {
        var segments = ReadSegments(Export(
            Cue(0, 1, 4, ExportAlignment.BottomCenter, SKColors.White),
            Cue(1, 1, 4, ExportAlignment.TopCenter, SKColors.White),
            Cue(2, 2, 3, ExportAlignment.MiddleCenter, SKColors.White)));

        var counts = segments.Where(s => s.Type == Pcs).Select(ObjectCount).ToList();
        Assert.Equal(new[] { 2, 2, 2, 0 }, counts);

        // The middle line shares a window with the closer of the other two - the bitmap of one
        // of the two objects in the middle slice spans from the middle to the bottom.
        var middleOds = segments.Skip(6).Where(s => s.Type == Ods).Take(2).ToList();
        Assert.Contains(middleOds, o => OdsHeight(o) > CueHeight);
    }

    [Fact]
    public void Fade_OnOneLine_LeavesTheOtherOpaque()
    {
        // Each object has a range of the shared palette, so a fade step scales the entries of
        // the fading line only.
        var segments = ReadSegments(Export(
            Cue(0, 1, 3, ExportAlignment.BottomCenter, SKColors.White, "{\\fad(400,400)}Hello"),
            Cue(1, 2, 4, ExportAlignment.TopCenter, SKColors.Red)));

        // The slice with both objects: from its PCS to the next epoch start.
        var pcsIndices = segments.Select((s, i) => (s, i)).Where(x => x.s.Type == Pcs && x.s.Payload[7] == 0x80).Select(x => x.i).ToList();
        var slice = segments.Skip(pcsIndices[1]).Take(pcsIndices[2] - pcsIndices[1]).ToList();
        Assert.Equal(2, ObjectCount(slice[0]));

        var palettes = slice.Where(s => s.Type == Pds).ToList();
        Assert.True(palettes.Count > 1, "the fade out of the bottom line should add palette updates to the slice");

        // Objects go top to bottom: the red top line is object 0 (entries 0 and 1, red then
        // transparent), the white bottom line object 1 (entries 2 and 3). The white fades out
        // inside the slice, the red never moves.
        Assert.Equal(255, PaletteAlpha(palettes[0], 2));
        Assert.True(PaletteAlpha(palettes[palettes.Count - 1], 2) < 100, "white alphas: " + string.Join(",", palettes.Select(p => PaletteAlpha(p, 2))));
        Assert.All(palettes, p => Assert.Equal(255, PaletteAlpha(p, 0)));

        // The palette updates re-send the composition of both objects.
        Assert.All(slice.Skip(1).Where(s => s.Type == Pcs), p =>
        {
            Assert.Equal(0x80, p.Payload[8]);
            Assert.Equal(2, ObjectCount(p));
        });
    }

    [Fact]
    public void OverlappingFullFrameLines_AreDrawnIntoOneFrame()
    {
        var bottom = Cue(0, 1, 3, ExportAlignment.BottomCenter, SKColors.White);
        var top = Cue(1, 2, 4, ExportAlignment.TopCenter, SKColors.White);
        bottom.IsFullFrame = true;
        top.IsFullFrame = true;

        var subtitles = BluRaySupParser.ParseBluRaySup(Export(bottom, top), new StringBuilder());

        Assert.Equal(3, subtitles.Count);
        using var bitmap = subtitles[1].GetBitmap();
        Assert.Equal(ScreenWidth, bitmap.Width);
        Assert.Equal(ScreenHeight, bitmap.Height);
        Assert.Equal(255, bitmap.GetPixel(ScreenWidth / 2, Margin).Alpha);
        Assert.Equal(255, bitmap.GetPixel(ScreenWidth / 2, ScreenHeight - Margin - 1).Alpha);
        Assert.Equal(0, bitmap.GetPixel(ScreenWidth / 2, ScreenHeight / 2).Alpha);
    }

    [Fact]
    public void BitmapsDisposedAfterWriteParagraph_StillCompose()
    {
        // seconv and the container track exports dispose each bitmap right after
        // WriteParagraph, before the group it overlaps with is written.
        var fileName = Path.Combine(_dir, "disposed.sup");
        var cues = new[]
        {
            Cue(0, 1, 3, ExportAlignment.BottomCenter, SKColors.White),
            Cue(1, 2, 4, ExportAlignment.TopCenter, SKColors.White),
        };
        var handler = new ExportHandlerBluRaySup();
        handler.WriteHeader(fileName, cues[0]);
        foreach (var cue in cues)
        {
            handler.CreateParagraph(cue);
            handler.WriteParagraph(cue);
            cue.Bitmap.Dispose();
        }

        handler.WriteFooter();

        var subtitles = BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());
        Assert.Equal(3, subtitles.Count);
        Assert.Equal(2, subtitles[1].PcsObjects.Count);
        using var bitmap = subtitles[1].GetBitmap();
        Assert.Equal(ScreenHeight - 2 * Margin, bitmap.Height);
        Assert.Equal(255, bitmap.GetPixel(CueWidth / 2, 0).Alpha);
        Assert.Equal(255, bitmap.GetPixel(CueWidth / 2, bitmap.Height - 1).Alpha);
    }

    [Fact]
    public void ReadyMadeBuffers_AreWrittenAsTheyAre()
    {
        // A cue that never went through CreateParagraph (a track copied out of a container)
        // carries its own display sets.
        var fileName = Path.Combine(_dir, "raw.sup");
        var handler = new ExportHandlerBluRaySup();
        handler.WriteHeader(fileName, new ImageParameter { ScreenWidth = ScreenWidth, ScreenHeight = ScreenHeight });
        handler.WriteParagraph(new ImageParameter { Buffer = [1, 2, 3] });
        handler.WriteParagraph(new ImageParameter { Buffer = [4, 5] });
        handler.WriteFooter();

        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, File.ReadAllBytes(fileName));
    }
}
