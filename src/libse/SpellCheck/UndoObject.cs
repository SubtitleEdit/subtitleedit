using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;

namespace Nikse.SubtitleEdit.Core.SpellCheck
{
    public class UndoObject
    {
        public int CurrentIndex { get; set; }
        public string UndoText { get; set; }
        public string UndoWord { get; set; }
        public string CurrentWord { get; set; }
        public SpellCheckAction Action { get; set; }
        public Subtitle Subtitle { get; set; }
        public int NoOfSkippedWords { get; set; }
        public int NoOfChangedWords { get; set; }
        public int NoOfCorrectWords { get; set; }
        public int NoOfNames { get; set; }
        public int NoOfAddedWords { get; set; }
    }
}
