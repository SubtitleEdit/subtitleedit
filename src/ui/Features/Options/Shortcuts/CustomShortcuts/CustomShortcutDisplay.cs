using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomShortcuts;

/// <summary>
/// A command a "run command" step can pick.
/// </summary>
public class CustomShortcutCommandItem
{
    public string ActionName { get; }
    public string DisplayName { get; }
    public string GroupName { get; }

    public CustomShortcutCommandItem(string actionName, string displayName, string groupName)
    {
        ActionName = actionName;
        DisplayName = displayName;
        GroupName = groupName;
    }

    public override string ToString() => DisplayName;
}

public class CustomShortcutChoice<T>
{
    public T Value { get; }
    public string Name { get; }

    public CustomShortcutChoice(T value, string name)
    {
        Value = value;
        Name = name;
    }

    public override string ToString() => Name;
}

/// <summary>
/// Display texts for custom shortcuts and their steps.
/// </summary>
public static class CustomShortcutDisplay
{
    private const int MaxTextLength = 30;

    public static List<CustomShortcutChoice<CustomShortcutStepType>> GetStepTypes()
    {
        var language = Se.Language.Options.Shortcuts;
        return
        [
            new(CustomShortcutStepType.RunCommand, language.CustomShortcutStepRunCommand),
            new(CustomShortcutStepType.InsertText, language.CustomShortcutStepInsertText),
            new(CustomShortcutStepType.Replace, language.CustomShortcutStepReplace),
        ];
    }

    /// <summary>
    /// The areas a custom shortcut can be active in - same names as the "Active in" column.
    /// </summary>
    public static List<CustomShortcutChoice<ShortcutCategory>> GetActiveInChoices()
    {
        var language = Se.Language.Options.Shortcuts;
        return
        [
            new(ShortcutCategory.General, language.ActiveInEverywhere),
            new(ShortcutCategory.SubtitleGrid, language.CategorySubtitleGrid),
            new(ShortcutCategory.TextBox, language.CategoryTextBox),
            new(ShortcutCategory.SubtitleGridAndTextBox, language.CategorySubtitleGridAndTextBox),
            new(ShortcutCategory.Waveform, Se.Language.General.Waveform),
        ];
    }

    public static List<CustomShortcutChoice<CustomShortcutInsertPosition>> GetInsertPositions()
    {
        var language = Se.Language.Options.Shortcuts;
        return
        [
            new(CustomShortcutInsertPosition.Cursor, language.CustomShortcutInsertAtCursor),
            new(CustomShortcutInsertPosition.Start, language.CustomShortcutInsertAtStart),
            new(CustomShortcutInsertPosition.End, language.CustomShortcutInsertAtEnd),
        ];
    }

    public static string GetStepSummary(SeCustomShortcutStep step, IReadOnlyDictionary<string, CustomShortcutCommandItem> commands)
    {
        var language = Se.Language.Options.Shortcuts;
        switch (step.GetStepType())
        {
            case CustomShortcutStepType.InsertText:
                var position = GetInsertPositions().First(p => p.Value == step.GetPosition()).Name;
                return string.Format(language.CustomShortcutSummaryInsertXY, Shorten(step.Text), position);
            case CustomShortcutStepType.Replace:
                return string.Format(step.UseRegex ? language.CustomShortcutSummaryReplaceRegexXY : language.CustomShortcutSummaryReplaceXY,
                    Shorten(step.Find), Shorten(step.ReplaceWith));
            default:
                var name = commands.TryGetValue(step.ActionName, out var command) ? command.DisplayName : step.ActionName;
                return string.Format(language.CustomShortcutSummaryRunCommandX, name);
        }
    }

    public static string GetSummary(SeCustomShortcut custom, IReadOnlyDictionary<string, CustomShortcutCommandItem> commands)
    {
        return string.Join("  →  ", custom.Steps.Select(p => GetStepSummary(p, commands)));
    }

    /// <summary>
    /// One-line, length-capped version of user text: line breaks show as "↵".
    /// </summary>
    private static string Shorten(string text)
    {
        var s = (text ?? string.Empty).Replace("\r\n", "↵").Replace("\n", "↵").Replace("\r", "↵");
        return s.Length > MaxTextLength ? s.Substring(0, MaxTextLength) + "…" : s;
    }
}
