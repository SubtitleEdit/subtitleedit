using System;
using Avalonia.Controls;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

/// <summary>
/// How the subtitle grid renders the Text / Original text columns so the visible text can
/// adapt to the window size (feature request #11590).
/// </summary>
public enum SubtitleGridTextDisplayMode
{
    /// <summary>Current behaviour: text is not wrapped; long lines are clipped at the column edge.</summary>
    Clip,

    /// <summary>Text wraps within the column to the window width; rows grow to show the full text.</summary>
    Wrap,

    /// <summary>One line, truncated with an ellipsis that adapts to the column (window) width.</summary>
    Ellipsis,
}

public class SubtitleGridTextDisplayModeDisplay
{
    public SubtitleGridTextDisplayMode Mode { get; }
    public string DisplayName { get; }

    public SubtitleGridTextDisplayModeDisplay(SubtitleGridTextDisplayMode mode, string displayName)
    {
        Mode = mode;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;

    public static SubtitleGridTextDisplayModeDisplay[] GetAll()
    {
        return
        [
            new SubtitleGridTextDisplayModeDisplay(SubtitleGridTextDisplayMode.Clip, Se.Language.Options.Settings.SubtitleGridTextDisplayClip),
            new SubtitleGridTextDisplayModeDisplay(SubtitleGridTextDisplayMode.Wrap, Se.Language.Options.Settings.SubtitleGridTextDisplayWrap),
            new SubtitleGridTextDisplayModeDisplay(SubtitleGridTextDisplayMode.Ellipsis, Se.Language.Options.Settings.SubtitleGridTextDisplayEllipsis),
        ];
    }

    /// <summary>Parse the persisted setting string into the mode, defaulting to <see cref="SubtitleGridTextDisplayMode.Clip"/>.</summary>
    public static SubtitleGridTextDisplayMode FromSettings()
    {
        return Enum.TryParse<SubtitleGridTextDisplayMode>(Se.Settings.Appearance.SubtitleGridTextDisplay, out var mode)
            ? mode
            : SubtitleGridTextDisplayMode.Clip;
    }

    /// <summary>Apply the wrapping / max-lines / trimming that realises the mode on a grid text cell.</summary>
    public static void ApplyTo(TextBlock textBlock, SubtitleGridTextDisplayMode mode)
    {
        switch (mode)
        {
            case SubtitleGridTextDisplayMode.Wrap:
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.MaxLines = 0; // unlimited - row grows to fit
                textBlock.TextTrimming = TextTrimming.None;
                break;
            case SubtitleGridTextDisplayMode.Ellipsis:
                textBlock.TextWrapping = TextWrapping.NoWrap;
                textBlock.MaxLines = 1; // force a single visual line even for multi-line text
                textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                break;
            default: // Clip
                textBlock.TextWrapping = TextWrapping.NoWrap;
                textBlock.MaxLines = 0;
                textBlock.TextTrimming = TextTrimming.None;
                break;
        }
    }
}
