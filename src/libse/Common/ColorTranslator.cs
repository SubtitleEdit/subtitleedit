using SkiaSharp;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class ColorTranslator
    {
        public static SKColor ParseNamedColor(string colorName)
        {
            var type = typeof(SKColors);

            // Search fields (most SKColors are here)
            var field = type.GetFields(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(f =>
                                f.FieldType == typeof(SKColor) &&
                                string.Equals(f.Name, colorName, StringComparison.OrdinalIgnoreCase));

            if (field != null)
            {
                return (SKColor)field.GetValue(null);
            }

            // Search properties (in case any are defined as such)
            var property = type.GetProperties(BindingFlags.Public | BindingFlags.Static)
                               .FirstOrDefault(p =>
                                   p.PropertyType == typeof(SKColor) &&
                                   string.Equals(p.Name, colorName, StringComparison.OrdinalIgnoreCase));

            if (property != null)
            {
                return (SKColor)property.GetValue(null);
            }

            throw new ArgumentException($"Color '{colorName}' not found in SKColors.");
        }

        public static SKColor FromHtml(string htmlColor)
        {
            return SKColor.TryParse(htmlColor, out var color) ? color : ParseNamedColor(htmlColor);
        }

        public static object ToHtml(SKColor color)
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

        public static string ToWin32(SKColor color)
        {
            // Win32 color format is 0x00BBGGRR (BGR, no alpha)
            var number = (color.Blue << 16) | (color.Green << 8) | color.Red;
            return number.ToString(CultureInfo.InvariantCulture);
        }
    }
}