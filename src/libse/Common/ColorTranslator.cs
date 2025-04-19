using SkiaSharp;
using System;
using System.Drawing;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Common
{
    internal class ColorTranslator
    {
        internal static SKColor FromHtml(string htmlColor)
        {
            return SKColor.Parse(htmlColor);
        }

        internal static object ToHtml(SKColor color)
        {
            if (color.Alpha == 255)
            {
                return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
            else
            {
                return $"#{color.Alpha:X2}{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
            }
        }

        internal static string ToWin32(SKColor color)
        {
            // Win32 color format is 0x00BBGGRR (BGR, no alpha)
            var number = (color.Blue << 16) | (color.Green << 8) | color.Red;
            return number.ToString(CultureInfo.InvariantCulture);   
        }
    }
}