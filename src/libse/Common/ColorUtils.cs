using System;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ColorUtils
    {
        public static Color Blend(this Color baseColor, Color targetColor, double percentage = 0.5)
        {
            percentage = Math.Abs(percentage);
            
            if (percentage == 0)
                return baseColor;
            if (percentage >= 1)
                return targetColor;

            return Color.FromArgb(
                Lerp(baseColor.A, targetColor.A, percentage),
                Lerp(baseColor.R, targetColor.R, percentage),
                Lerp(baseColor.G, targetColor.G, percentage),
                Lerp(baseColor.B, targetColor.B, percentage));
        }

        private static int Lerp(int baseColor, int targetColor, double percentage)
        {
            return (byte)Math.Round(baseColor  + percentage * (targetColor - baseColor));
        }
        
        public static double Luminance(this Color color) => (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255;
        
        public static Color OpposingLuminanceColor(this Color baseColor)
        {
            var luminance = baseColor.Luminance();
            var opposingLuminance = Math.Abs(0.5 - luminance) * 2;

            var opposingRed = (int)Math.Round(opposingLuminance * baseColor.R);
            var opposingGreen = (int)Math.Round(opposingLuminance * baseColor.G);
            var opposingBlue = (int)Math.Round(opposingLuminance * baseColor.B);

            return Color.FromArgb(opposingRed, opposingGreen, opposingBlue);
        }
    }
    
    
}