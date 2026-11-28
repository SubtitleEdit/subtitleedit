using System;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

/// <summary>
/// Where the subtitle text box sits relative to the subtitle grid. Only applies to layouts that
/// dock the text box with the grid - layouts with a separate text box keep their own placement.
/// </summary>
public enum SubtitleEditBoxPosition
{
    /// <summary>Text box (with time up/downs) docked below the grid - the classic look.</summary>
    Below,

    /// <summary>No docked text box: the text is edited right in the selected row of the grid.</summary>
    Inline,

    /// <summary>Text box (with time up/downs) docked above the grid.</summary>
    Above,
}

public class SubtitleEditBoxPositionDisplay
{
    public SubtitleEditBoxPosition Position { get; }
    public string DisplayName { get; }

    public SubtitleEditBoxPositionDisplay(SubtitleEditBoxPosition position, string displayName)
    {
        Position = position;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;

    public static SubtitleEditBoxPositionDisplay[] GetAll()
    {
        return
        [
            new SubtitleEditBoxPositionDisplay(SubtitleEditBoxPosition.Below, Se.Language.Options.Settings.SubtitleEditBoxPositionBelow),
            new SubtitleEditBoxPositionDisplay(SubtitleEditBoxPosition.Inline, Se.Language.Options.Settings.SubtitleEditBoxPositionInline),
            new SubtitleEditBoxPositionDisplay(SubtitleEditBoxPosition.Above, Se.Language.Options.Settings.SubtitleEditBoxPositionAbove),
        ];
    }

    /// <summary>Parse the persisted setting, defaulting to <see cref="SubtitleEditBoxPosition.Below"/>.</summary>
    public static SubtitleEditBoxPosition FromSettings()
    {
        return Enum.TryParse<SubtitleEditBoxPosition>(Se.Settings.Appearance.SubtitleEditBoxPosition, out var position)
            ? position
            : SubtitleEditBoxPosition.Below;
    }
}
