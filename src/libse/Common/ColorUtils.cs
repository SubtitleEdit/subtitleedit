using System;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class ColorUtils
    {
        /// <summary>
        /// Blends two colors by a specified percentage.
        /// </summary>
        /// <param name="baseColor">The base color to blend from.</param>
        /// <param name="targetColor">The target color to blend to.</param>
        /// <param name="percentage">The blending percentage. A value between 0 and 1. Defaults to 0.5.</param>
        /// <returns>A new <see cref="Color"/> that is the result of blending the base and target colors.</returns>
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

        /// <summary>
        /// Linearly interpolates between two integer color components by a specified percentage.
        /// </summary>
        /// <param name="baseColor">The starting color component value.</param>
        /// <param name="targetColor">The target color component value.</param>
        /// <param name="percentage">The interpolation percentage. A value between 0 and 1.</param>
        /// <returns>An integer representing the interpolated color component value.</returns>
        private static int Lerp(int baseColor, int targetColor, double percentage)
        {
            return (byte)Math.Round(baseColor  + percentage * (targetColor - baseColor));
        }

        /// <summary>
        /// Calculates the relative luminance of a color, which is a measure of the perceived brightness.
        /// </summary>
        /// <param name="color">The color for which to calculate luminance.</param>
        /// <returns>A double value representing the luminance of the color, ranging from 0 (darkest) to 1 (brightest).</returns>
        public static double Luminance(this Color color) => (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255;

        /// <summary>
        /// Calculates a color with the opposing luminance of the given color.
        /// </summary>
        /// <param name="baseColor">The base color for which to calculate the opposing luminance color.</param>
        /// <returns>A new <see cref="Color"/> that has the opposing luminance compared to the base color.</returns>
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