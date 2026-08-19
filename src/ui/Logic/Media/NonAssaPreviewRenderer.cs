using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Renders a single-frame libass preview via ffmpeg (lavfi transparent color + subtitles filter),
/// so the style preview matches the generated video exactly (font metrics, spacing, boxes).
/// </summary>
public static class NonAssaPreviewRenderer
{
    /// <summary>
    /// Draws a transparent libass frame on the dark "video frame" background used by style previews.
    /// </summary>
    public static SKBitmap ComposeOnDarkFrame(SKBitmap bitmap)
    {
        var frame = new SKBitmap(bitmap.Width, bitmap.Height);
        using var canvas = new SKCanvas(frame);
        canvas.Clear(new SKColor(40, 40, 40));
        using (var border = new SKPaint { Color = new SKColor(90, 90, 90), Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
        {
            canvas.DrawRect(1, 1, bitmap.Width - 2, bitmap.Height - 2, border);
        }

        canvas.DrawBitmap(bitmap, 0, 0);
        return frame;
    }

    public static SKBitmap? Render(Subtitle subtitle, int width, int height)
    {
        var fileName = FfmpegGenerator.GetScreenShotWithSubtitle(subtitle, width, height);
        if (fileName == null)
        {
            return null;
        }

        try
        {
            return SKBitmap.Decode(fileName);
        }
        finally
        {
            try
            {
                File.Delete(fileName);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
