using System.Collections.Generic;
using System.Diagnostics;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

[DebuggerDisplay("Line {LineIndex + 1}, Word {WordIndex + 1}: '{Word}'")]
public class SpellCheckResult
{
    public int LineIndex { get; set; }
    public int WordIndex { get; set; }
    public SpellCheckWord Word { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public bool IsCommonMisspelling { get; set; }
    public SubtitleLineViewModel Paragraph { get; set; } = new();
}
