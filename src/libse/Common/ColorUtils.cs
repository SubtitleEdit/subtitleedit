using SkiaSharp;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ColorUtils
    {
        public static SKColor Blend(this SKColor baseColor, SKColor targetColor, double percentage = 0.5)
        {
            percentage = Math.Abs(percentage);

            if (percentage == 0)
                return baseColor;
            if (percentage >= 1)
                return targetColor;

            return new SKColor(
                Lerp(baseColor.Red, targetColor.Red, percentage),
                Lerp(baseColor.Green, targetColor.Green, percentage),
                Lerp(baseColor.Blue, targetColor.Blue, percentage),
                Lerp(baseColor.Alpha, targetColor.Alpha, percentage)
                );
        }

        private static byte Lerp(int baseColor, int targetColor, double percentage)
        {
            return (byte)Math.Round(baseColor + percentage * (targetColor - baseColor));
        }

        public static double Luminance(this SKColor color) => (color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114) / 255;

        public static SKColor OpposingLuminanceColor(this SKColor baseColor)
        {
            var luminance = baseColor.Luminance();
            var opposingLuminance = Math.Abs(0.5 - luminance) * 2;

            var opposingRed = (byte)Math.Round(opposingLuminance * baseColor.Red);
            var opposingGreen = (byte)Math.Round(opposingLuminance * baseColor.Green);
            var opposingBlue = (byte)Math.Round(opposingLuminance * baseColor.Blue);

            return new SKColor(opposingRed, opposingGreen, opposingBlue);
        }

        public static uint ToArgb(this SKColor color)
        {
            var number = (uint)color.Alpha << 24 | (uint)color.Red << 16 | (uint)color.Green << 8 | (uint)color.Blue;
            return number;
        }
        public static string ToArgbString(this SKColor color)
        {
            var number = (uint)color.Alpha << 24 | (uint)color.Red << 16 | (uint)color.Green << 8 | (uint)color.Blue;
            return number.ToString(CultureInfo.InvariantCulture);
        }

        public static SKColor FromArgb(string text)
        {
            return new SKColor((uint)Convert.ToInt32(text, CultureInfo.InvariantCulture));
        }

        public static SKColor FromArgb(int red, int green, int blue)
        {
            return new SKColor((byte)red, (byte)green, (byte)blue);
        }

        public static SKColor FromArgb(int alpha, int red, int green, int blue)
        {
            return new SKColor((byte)red, (byte)green, (byte)blue, (byte)alpha);
        }

        public static SKColor FromArgb(int number)
        {
            return new SKColor((uint)number);
        }

        internal static SKColor FromArgb(int alpha, SKColor c)
        {
            return new SKColor(c.Red, c.Green, c.Blue, (byte)alpha);    
        }

        internal static SKColor FromArgb(int alpha, byte blue, byte green, byte red)
        {
            return new SKColor(red, green, blue, (byte)alpha);
        }
    }
}