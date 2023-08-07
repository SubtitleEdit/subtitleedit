using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic
{
    /// <summary>
    /// Provides a set of methods to help with operations on Fonts.
    /// </summary>
    public static class FontHelper
    {
        private static readonly FontFamily[] _fonts;

        /// <summary>
        /// Initializes static members of the <see cref="FontHelper"/> class.
        /// </summary>
        static FontHelper()
        {
            _fonts = FontFamily.Families.OrderBy(font => font.Name).ToArray();
        }

        /// <summary>
        /// Gets fonts which support both Bold and Regular styles.
        /// </summary>
        /// <returns>
        /// Collection of <see cref="FontFamily"/> which support both Bold and Regular styles.
        /// </returns>
        public static IEnumerable<FontFamily> GetRegularAndBoldCapableFontFamilies()
        {
            return _fonts.Where(font => font.IsStyleAvailable(FontStyle.Bold) && font.IsStyleAvailable(FontStyle.Regular));
        }

        /// <summary>
        /// Gets fonts which support either Bold or Regular style.
        /// </summary>
        /// <returns>
        /// Collection of <see cref="FontFamily"/> which support either Bold or Regular style.
        /// </returns>
        public static IEnumerable<FontFamily> GetRegularOrBoldCapableFontFamilies()
        {
            return _fonts.Where(font => font.IsStyleAvailable(FontStyle.Bold) || font.IsStyleAvailable(FontStyle.Regular));
        }

        /// <summary>
        /// Gets all supported font families.
        /// </summary>
        /// <returns>
        /// An enumerable collection of <see cref="FontFamily"/> objects.
        /// </returns>
        public static IEnumerable<FontFamily> GetAllSupportedFontFamilies()
        {
            return _fonts;
        }
    }
}