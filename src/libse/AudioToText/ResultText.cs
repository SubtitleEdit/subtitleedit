namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class ResultText
    {
        public string Text { get; set; }

        /// <summary>
        /// Start seconds
        /// </summary>
        public decimal Start { get; set; }

        /// <summary>
        /// End seconds
        /// </summary>
        public decimal End { get; set; }

        public decimal Confidence { get; set; }
    }
}
