using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public class SpellCheckWordChangedEvent : EventArgs
{
    public int WordIndex { get; set; }
    public string FromWord { get; set; } = string.Empty;
    public string ToWord { get; set; } = string.Empty;
    public SubtitleLineViewModel Paragraph { get; set; } = new();
    public SpellCheckWord Word { get; set; } = new();
}