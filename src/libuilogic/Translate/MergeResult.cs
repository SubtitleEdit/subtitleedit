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

        /// <summary>
        /// The rows of a continuous item were joined with a line break instead of a space
        /// (the engine is an <see cref="AutoTranslate.ILineBreakPreservingTranslator"/>), so
        /// its reply can be split back on those breaks.
        /// </summary>
        public bool ContinuousRowsJoinedWithLineBreak { get; set; }

        /// <summary>Language code of the merged text, for abbreviation-aware period counting.</summary>
        public string SourceLanguage { get; set; } = string.Empty;
    }
}