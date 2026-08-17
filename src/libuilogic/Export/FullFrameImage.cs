using SkiaSharp;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Builds "full frame" images: the rendered subtitle bitmap drawn onto a canvas the size of the
/// video frame. Video editors (Final Cut Pro, Premiere, DaVinci Resolve) can then place every
/// image at 0,0 and get the placement Subtitle Edit calculated, instead of having to position a
/// tightly cropped bitmap per line.
/// </summary>
public static class FullFrameImage
{
    /// <summary>
    /// Top left corner of a <paramref name="width"/> x <paramref name="height"/> subtitle bitmap
    /// inside the frame, from the alignment and the margins - or from the "{\pos(x,y)}" override
    /// when that falls inside the frame.
    /// </summary>
    public static SKPointI GetPosition(ImageParameter param, int width, int height)
    {
        if (param.OverridePosition.HasValue &&
            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.ScreenWidth &&
            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.ScreenHeight)
        {
            return param.OverridePosition.Value;
        }

        var x = param.Alignment switch
        {
            ExportAlignment.TopLeft or ExportAlignment.MiddleLeft or ExportAlignment.BottomLeft
                => param.LeftRightMargin,
            ExportAlignment.TopRight or ExportAlignment.MiddleRight or ExportAlignment.BottomRight
                => param.ScreenWidth - width - param.LeftRightMargin,
            _ => (param.ScreenWidth - width) / 2,
        };

        var y = param.Alignment switch
        {
            ExportAlignment.TopLeft or ExportAlignment.TopCenter or ExportAlignment.TopRight
                => param.BottomTopMargin,
            ExportAlignment.MiddleLeft or ExportAlignment.MiddleCenter or ExportAlignment.MiddleRight
                => (param.ScreenHeight - height) / 2,
            _ => param.ScreenHeight - height - param.BottomTopMargin,
        };

        return new SKPointI(x, y);
    }

    /// <summary>
    /// Returns a new frame-sized bitmap holding <see cref="ImageParameter.Bitmap"/> at its
    /// calculated position, on a <see cref="ImageParameter.FullFrameBackgroundColor"/> background.
    /// The caller owns the returned bitmap.
    /// </summary>
    public static SKBitmap Create(ImageParameter param)
    {
        var width = Math.Max(1, param.ScreenWidth);
        var height = Math.Max(1, param.ScreenHeight);

        // Not opaque even for an opaque background colour: a transparent background is the point
        // of the option for editing timelines, where the images go on a track above the video.
        var bitmap = new SKBitmap(width, height, false);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(param.FullFrameBackgroundColor);

        var position = GetPosition(param, param.Bitmap.Width, param.Bitmap.Height);
        canvas.DrawBitmap(param.Bitmap, position.X, position.Y);

        return bitmap;
    }
}
