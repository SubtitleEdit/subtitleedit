namespace Nikse.SubtitleEdit.UiLogic.Translate;

public static partial class MergeAndSplitHelper
{
    public class MergeResult
    {
        public string Text { get; set; } = string.Empty;
        public int ParagraphCount { get; set; }
        public List<MergeResultItem> MergeResultItems { get; set; } = [];
        public bool HasError { get; set; }
        public bool NoSentenceEndingSource { get; set; }
        public bool NoSentenceEndingTarget { get; set; }

        /// <summary>Language code of the merged text, for abbreviation-aware period counting.</summary>
        public string SourceLanguage { get; set; } = string.Empty;
    }
}