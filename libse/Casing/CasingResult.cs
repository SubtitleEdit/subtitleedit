using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Casing
{
    public class CasingResult
    {
        /// <summary>
        /// Text before modifications.
        /// </summary>
        public string Before { get; set; }

        /// <summary>
        /// Text after modification.
        /// </summary>
        public string After { get; set; }
    }
}
