using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Translate;

public static partial class MergeAndSplitHelper
{
    public class MergeResultItem
    {
        public string Text { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public bool AllItalic { get; set; }
        public bool AllBold { get; set; }
        public bool Continuous { get; set; }
        public char EndChar { get; set; }
        public int EndCharOccurrences { get; set; }
        public bool IsEmpty { get; set; }
        public bool HasError { get; set; }
        public int TextIndexStart { get; set; }
        public int TextIndexEnd { get; set; }
        public List<TranslateRow> Paragraphs { get; set; } = new();
    }
}