using System.Collections.Generic;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Features.Ocr.FixEngine;

[DebuggerDisplay("'{Word}' - {LinePartType} - pos {WordIndex} - spellcheck: {IsSpellCheckedOk}")]
public class OcrFixLinePartResult
{
    public OcrFixLinePartType LinePartType { get; set; }
    public int WordIndex { get; set; }
    public string Word { get; set; } = string.Empty;
    public string FixedWord { get; set; } = string.Empty;
    public bool? IsSpellCheckedOk { get; set; }
    public List<string> Suggestions { get; set; } = new();
    public bool GuessUsed { get; internal set; }
    public ReplacementUsedItem ReplacementUsed { get; internal set; } = new();
}