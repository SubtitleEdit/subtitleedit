using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;
using System;

namespace Nikse.SubtitleEdit.UiLogic.BatchConvert;

/// <summary>
/// Applies the Transport Stream screen size / position overrides to one export image
/// (port of SE4's TsToBluRaySup.WriteTrack placement logic).
/// </summary>
public static class TransportStreamExportOverride
{
    /// <summary>
    /// <paramref name="sourcePosition"/> is the bitmap's top-left in the source stream's
    /// coordinates (negative = unknown). Adjusts the parameter's screen size, bitmap and
    /// <see cref="ImageParameter.OverridePosition"/> in place.
    /// </summary>
    public static void Apply(ImageParameter param, SKPointI sourcePosition, TransportStreamExportSettings settings)
    {
        var position = sourcePosition;

        if (settings.OverrideScreenSize && settings.ScreenWidth > 0 && settings.ScreenHeight > 0 &&
            param.ScreenWidth > 0 && param.ScreenHeight > 0 &&
            (param.ScreenWidth != settings.ScreenWidth || param.ScreenHeight != settings.ScreenHeight))
        {
            var widthFactor = settings.ScreenWidth / (double)param.ScreenWidth;
            var heightFactor = settings.ScreenHeight / (double)param.ScreenHeight;

            if (param.Bitmap != null)
            {
                var newWidth = Math.Max(1, (int)Math.Round(param.Bitmap.Width * widthFactor));
                var newHeight = Math.Max(1, (int)Math.Round(param.Bitmap.Height * heightFactor));
                var resized = param.Bitmap.Resize(new SKImageInfo(newWidth, newHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
                if (resized != null)
                {
                    param.Bitmap.Dispose();
                    param.Bitmap = resized;
                }
            }

            if (position.X >= 0 && position.Y >= 0)
            {
                position = new SKPointI((int)Math.Round(position.X * widthFactor), (int)Math.Round(position.Y * heightFactor));
            }

            param.ScreenWidth = settings.ScreenWidth;
            param.ScreenHeight = settings.ScreenHeight;
        }

        var bitmapWidth = param.Bitmap?.Width ?? 0;
        var bitmapHeight = param.Bitmap?.Height ?? 0;

        if (settings.OverrideXPosition || settings.OverrideYPosition)
        {
            var x = position.X;
            var y = position.Y;

            if (settings.OverrideXPosition)
            {
                var marginX = (int)Math.Round(settings.HMarginPercent * param.ScreenWidth / 100.0);
                x = (int)Math.Round(param.ScreenWidth / 2.0 - bitmapWidth / 2.0);
                if (string.Equals(settings.HAlign, TransportStreamExportSettings.HAlignLeft, StringComparison.OrdinalIgnoreCase))
                {
                    x = marginX;
                }
                else if (string.Equals(settings.HAlign, TransportStreamExportSettings.HAlignRight, StringComparison.OrdinalIgnoreCase))
                {
                    x = param.ScreenWidth - marginX - bitmapWidth;
                }
            }

            if (settings.OverrideYPosition)
            {
                var marginY = (int)Math.Round(settings.BottomMarginPercent * param.ScreenHeight / 100.0);
                y = param.ScreenHeight - marginY - bitmapHeight;
            }

            // The axis we did not touch may be unknown (-1): then let the export handler
            // place the image by its alignment rather than feeding it a negative coordinate.
            param.OverridePosition = x >= 0 && y >= 0 ? new SKPointI(x, y) : null;
            return;
        }

        param.OverridePosition = position.X >= 0 && position.Y >= 0 ? position : null;
    }
}
