using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic
{
    public static class FontHelper
    {
        private static List<FontFamily> _fonts;

        public static bool AreFontsLoaded => _fonts != null;

        public static List<FontFamily> GetFontFamilies()
        {
            if (_fonts != null)
            {
                return _fonts;
            }

            var fonts = new List<FontFamily>();
            foreach (var fontFamily in FontFamily.Families.OrderBy(p => p.Name))
            {
                if (fontFamily.IsStyleAvailable(FontStyle.Regular) && fontFamily.IsStyleAvailable(FontStyle.Bold))
                {
                    fonts.Add(fontFamily);  
                }
            }

            _fonts = fonts;
            return _fonts;
        }
    }
}
