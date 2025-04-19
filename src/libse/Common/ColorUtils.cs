using SkiaSharp;
using System;

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
            return (byte)Math.Round(baseColor  + percentage * (targetColor - baseColor));
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
    }
    
    
}