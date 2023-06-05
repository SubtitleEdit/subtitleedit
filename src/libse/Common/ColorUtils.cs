using System;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class ColorUtils
    {
        public static Color Blend(Color baseColor, Color targetColor, double percentage = 0.5)
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
    }
}