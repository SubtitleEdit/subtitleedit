using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// A user-built shortcut slot: an optional name plus a list of steps that run in order when the
/// shortcut fires. A slot without steps does nothing.
/// </summary>
public class SeCustomShortcut
{
    public string Name { get; set; }
    public List<SeCustomShortcutStep> Steps { get; set; }

    /// <summary>
    /// Where the key works (a <see cref="ShortcutCategory"/> name) - "General" is everywhere.
    /// </summary>
    public string ActiveIn { get; set; }

    public SeCustomShortcut()
    {
        Name = string.Empty;
        Steps = new List<SeCustomShortcutStep>();
        ActiveIn = nameof(ShortcutCategory.General);
    }

    public ShortcutCategory GetActiveIn()
    {
        return Enum.TryParse<ShortcutCategory>(ActiveIn, true, out var category) && Enum.IsDefined(category)
            ? category
            : ShortcutCategory.General;
    }

    public SeCustomShortcut Clone()
    {
        return new SeCustomShortcut
        {
            Name = Name,
            Steps = Steps.Select(p => p.Clone()).ToList(),
            ActiveIn = ActiveIn,
        };
    }
}

public enum CustomShortcutStepType
{
    RunCommand,
    InsertText,
    Replace,
}

public enum CustomShortcutInsertPosition
{
    Cursor,
    Start,
    End,
}

/// <summary>
/// One step of a <see cref="SeCustomShortcut"/>. A flat record so it serializes simply; only the
/// fields that belong to <see cref="Type"/> are used.
/// </summary>
public class SeCustomShortcutStep
{
    public string Type { get; set; } = nameof(CustomShortcutStepType.RunCommand);

    // Run command
    public string ActionName { get; set; } = string.Empty;

    // Insert text
    public string Text { get; set; } = string.Empty;
    public string Position { get; set; } = nameof(CustomShortcutInsertPosition.Cursor);

    // Replace
    public string Find { get; set; } = string.Empty;
    public string ReplaceWith { get; set; } = string.Empty;
    public bool UseRegex { get; set; }
    public bool CaseSensitive { get; set; }

    public CustomShortcutStepType GetStepType()
    {
        return Enum.TryParse<CustomShortcutStepType>(Type, true, out var type) && Enum.IsDefined(type)
            ? type
            : CustomShortcutStepType.RunCommand;
    }

    public CustomShortcutInsertPosition GetPosition()
    {
        return Enum.TryParse<CustomShortcutInsertPosition>(Position, true, out var position) && Enum.IsDefined(position)
            ? position
            : CustomShortcutInsertPosition.Cursor;
    }

    public SeCustomShortcutStep Clone()
    {
        return (SeCustomShortcutStep)MemberwiseClone();
    }
}
