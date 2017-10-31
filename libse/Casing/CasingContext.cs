using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.Casing
{
    public class CasingContext
    {
        /// <summary>
        /// Culture that will be used when converting character.
        /// </summary>
        public CultureInfo Culture { get; set; }

        public string Language { get; set; }

        public bool IsEnglish => Language?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true;
    }
}
