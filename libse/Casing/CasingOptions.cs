namespace Nikse.SubtitleEdit.Core.Casing
{
    public class CasingOptions
    {
        /// <summary>
        /// True if name casing should be performed.
        /// </summary>
        public bool Names { get; set; }

        /// <summary>
        /// True if only text with all uppercase letter should be processed.
        /// </summary>
        public bool OnlyWhereUppercase { get; set; }
    }
}
