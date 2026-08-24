using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System.Text;

namespace LibUiLogicTests.Export;

/// <summary>
/// "{\fad(..)}" on a Blu-ray sup export: the caption is encoded once and the fade rides along as
/// palette update display sets - a PCS with palette_update_flag set plus a PDS whose alphas are
/// scaled - so the fade costs palettes instead of bitmaps.
/// </summary>
public class ExportHandlerBluRaySupFadeTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "bluraysupfade_" + Guid.NewGuid().ToString("N"));

    public ExportHandlerBluRaySupFadeTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch { /* best effort */ }
    }

    private sealed record Segment(long Pts, byte Type, byte[] Payload);

    private static ImageParameter Cue(string text)
    {
        var bitmap = new SKBitmap(300, 80);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.White };
            canvas.DrawRect(0, 0, 300, 80, paint);
        }

        var parameter = new ImageParameter
        {
            Text = ExportTextTags.ToRenderableText(text),
            Bitmap = bitmap,
            StartTime = TimeSpan.FromSeconds(1),
            EndTime = TimeSpan.FromSeconds(3),
            Index = 0,
            ScreenWidth = 1280,
            ScreenHeight = 720,
            Alignment = ExportAlignment.BottomCenter,
            BottomTopMargin = 50,
            LeftRightMargin = 40,
            FramesPerSecond = 25,
            FontColor = SKColors.White,
        };

        ExportTextTags.ApplyTransparencyTags(parameter, text);
        return parameter;
    }

    private string Export(string text)
    {
        var fileName = Path.Combine(_dir, Guid.NewGuid().ToString("N") + ".sup");
        var handler = new ExportHandlerBluRaySup();
        var cue = Cue(text);

        handler.WriteHeader(fileName, cue);
        handler.CreateParagraph(cue);
        handler.WriteParagraph(cue);
        handler.WriteFooter();

        return fileName;
    }

    /// <summary>
    /// Walks the file as "PG" + PTS(4) + DTS(4) + type(1) + size(2) + payload, which also proves
    /// the segments chain exactly to the end - a wrong buffer size would leave a tail behind.
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

    // Alpha of the first palette entry - the font colour, so it carries the fade.
    private static int FontAlpha(Segment pds) => pds.Payload[2 + 4];

    [Fact]
    public void NoFadeTag_WritesTheDisplaySetsItAlwaysDid()
    {
        var segments = ReadSegments(Export("Hello"));

        Assert.Equal(new byte[] { 0x16, 0x17, 0x14, 0x15, 0x80, 0x16, 0x17, 0x80 }, segments.Select(s => s.Type));
        Assert.Equal(255, FontAlpha(segments[2]));
    }

    [Fact]
    public void Fade_AddsPaletteUpdateDisplaySetsBetweenTheCaptionAndTheClear()
    {
        var segments = ReadSegments(Export("{\\fad(400,400)}Hello"));

        // Caption (PCS, WDS, PDS, ODS, END), then a PCS + PDS + END per step, then the clear.
        var updates = segments.Skip(5).SkipLast(3).ToList();
        Assert.NotEmpty(updates);
        Assert.True(updates.Count % 3 == 0);
        for (var i = 0; i < updates.Count; i += 3)
        {
            var pcs = updates[i];
            Assert.Equal(0x16, pcs.Type);
            Assert.Equal(0x00, pcs.Payload[7]);        // composition_state: normal case
            Assert.Equal(0x80, pcs.Payload[8]);        // palette_update_flag
            Assert.Equal(1, pcs.Payload[10]);          // the object stays composed
            Assert.Equal(0x14, updates[i + 1].Type);   // and only the palette is re-sent
            Assert.Equal(0x80, updates[i + 2].Type);
        }
    }

    [Fact]
    public void Fade_RampsTheAlphaUpAndDownInsideTheCaption()
    {
        var segments = ReadSegments(Export("{\\fad(400,400)}Hello"));
        var palettes = segments.Where(s => s.Type == 0x14).ToList();
        var alphas = palettes.Select(FontAlpha).ToList();

        Assert.Equal(0, alphas[0]);                     // appears invisible
        Assert.Equal(255, alphas.Max());                // reaches the palette's own alpha
        var peak = alphas.IndexOf(255);
        Assert.Equal(alphas.Take(peak + 1).OrderBy(a => a), alphas.Take(peak + 1));
        Assert.Equal(alphas.Skip(alphas.LastIndexOf(255)).OrderByDescending(a => a), alphas.Skip(alphas.LastIndexOf(255)));
        Assert.True(alphas[alphas.Count - 1] < 60, $"last step should be nearly gone, was {alphas[alphas.Count - 1]}");

        // Palette version has to move for a decoder to take an update.
        Assert.Equal(Enumerable.Range(0, palettes.Count), palettes.Select(p => (int)p.Payload[1]));

        // Every update is inside the caption, in presentation order.
        var updatePts = segments.Where(s => s.Type == 0x16).Select(s => s.Pts).ToList();
        Assert.Equal(1000 * 90, updatePts[0]);
        Assert.Equal(3000 * 90, updatePts[updatePts.Count - 1]);
        Assert.Equal(updatePts.OrderBy(p => p), updatePts);
        Assert.Equal(updatePts.Distinct(), updatePts);
    }

    [Fact]
    public void Fade_KeepsCompositionNumbersClimbing()
    {
        var segments = ReadSegments(Export("{\\fad(400,400)}Hello"));
        var compositionNumbers = segments
            .Where(s => s.Type == 0x16)
            .Select(s => (s.Payload[5] << 8) + s.Payload[6])
            .ToList();

        Assert.Equal(Enumerable.Range(compositionNumbers[0], compositionNumbers.Count), compositionNumbers);
    }

    [Fact]
    public void Fade_ReadsBackAsOneSubtitlePerLine()
    {
        // SE's own parser merges the identical bitmaps of a fade (it takes three faded lines
        // for its "is this a fade" heuristic to say yes), so a faded export still round-trips
        // into one line per subtitle rather than one per step.
        var fileName = Path.Combine(_dir, "multi.sup");
        var handler = new ExportHandlerBluRaySup();
        var cues = Enumerable.Range(0, 4).Select(i =>
        {
            var cue = Cue("{\\fad(400,400)}Hello");
            cue.Index = i;
            cue.StartTime = TimeSpan.FromSeconds(1 + i * 3);
            cue.EndTime = TimeSpan.FromSeconds(3 + i * 3);
            return cue;
        }).ToList();

        handler.WriteHeader(fileName, cues[0]);
        foreach (var cue in cues)
        {
            handler.CreateParagraph(cue);
            handler.WriteParagraph(cue);
        }

        handler.WriteFooter();

        var subtitles = BluRaySupParser.ParseBluRaySup(fileName, new StringBuilder());

        Assert.Equal(4, subtitles.Count);
        Assert.Equal(1000, subtitles[0].StartTimeCode.TotalMilliseconds, 1);
        Assert.Equal(3000, subtitles[0].EndTimeCode.TotalMilliseconds, 1);
        using var bitmap = subtitles[0].GetBitmap();
        Assert.Equal(300, bitmap.Width);
    }

    [Fact]
    public void LongFade_StaysWithinTheStepBudget()
    {
        var cue = Cue("{\\fade(255,0,255,0,20000,40000,60000)}Hello");
        cue.StartTime = TimeSpan.Zero;
        cue.EndTime = TimeSpan.FromSeconds(60);
        ExportTextTags.ApplyTransparencyTags(cue, "{\\fade(255,0,255,0,20000,40000,60000)}Hello");
        var steps = ExportFade.CreateSteps(cue.FadeKeyframes, 0, 60000, 25);

        Assert.InRange(steps.Count, 2, ExportFade.MaxSteps);
    }
}
