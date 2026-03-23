namespace Nikse.SubtitleEdit.Core.Cea608
{
    public class SerializedRow
    {
        public int Row { get; set; }

        /// <summary>
        /// Column indentation.
        /// </summary>
        public int Position { get; set; } 

        public CcStyle Style { get; set; }
        public SerializedStyledUnicodeChar[] Columns { get; set; }
    }
}
