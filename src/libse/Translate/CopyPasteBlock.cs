using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class CopyPasteBlock
    {
        public string TargetText { get; set; }
        public List<Paragraph> Paragraphs { get; set; }
    }
}
