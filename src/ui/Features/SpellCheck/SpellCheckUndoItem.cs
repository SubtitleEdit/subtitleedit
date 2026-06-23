using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.SpellCheck;

public enum SpellCheckUndoAction
{
    Change,
    ChangeAll,
    SkipOnce,
    SkipAll,
    AddToNames,
    AddToDictionary,
    ChangeWholeText,
}

/// <summary>
/// A single undoable spell-check step. Captured before the action runs so it can be reverted:
/// paragraph texts and counters are restored from the snapshot, and word-list/dictionary mutations
/// are reversed via the action + word. <see cref="ResumeFrom"/> is the position to re-scan from so
/// the word that was acted on is shown again.
/// </summary>
public class SpellCheckUndoItem
{
    public string Description { get; set; } = string.Empty;
    public SpellCheckUndoAction Action { get; set; }
    public string ActionWord { get; set; } = string.Empty;
    public int NoOfChangedWords { get; set; }
    public int NoOfSkippedWords { get; set; }
    public List<(SubtitleLineViewModel Paragraph, string Text)> ParagraphTexts { get; set; } = new();
    public SpellCheckResult? ResumeFrom { get; set; }
}
