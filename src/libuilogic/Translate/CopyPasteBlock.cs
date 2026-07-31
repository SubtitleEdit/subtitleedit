using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.UiLogic.Translate
{
    public class CopyPasteBlock
    {
        public string TargetText { get; set; } = string.Empty;
        public List<Paragraph> Paragraphs { get; set; } = new();
        public List<Formatting> Formattings { get; set; } = new();
    }
}
