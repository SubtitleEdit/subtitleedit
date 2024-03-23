using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Forms.Translate
{
    public static partial class MergeAndSplitHelper
    {
        public class MergeResult
        {
            public string Text { get; set; }
            public int ParagraphCount { get; set; }
            public List<MergeResultItem> MergeResultItems { get; set; }
            public bool HasError { get; set; }
            public bool NoSentenceEndingSource { get; set; }
            public bool NoSentenceEndingTarget { get; set; }
        }
    }
}
