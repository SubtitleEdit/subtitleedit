using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeFormatting;

public class ChangeFormattingTypeDisplay
{
    public ChangeFormattingType Type { get; set; }
    public string DisplayName { get; set; }

    public ChangeFormattingTypeDisplay(ChangeFormattingType type, string displayName)
    {
        Type = type;
        DisplayName = displayName;
    }

    public override string ToString()
    {
        return DisplayName;
    }

    public static List<ChangeFormattingTypeDisplay> GetFromTypes()
    {
        return
        [
            new ChangeFormattingTypeDisplay(ChangeFormattingType.Italic, Se.Language.General.Italic),
            new ChangeFormattingTypeDisplay(ChangeFormattingType.Bold, Se.Language.General.Bold),
            new ChangeFormattingTypeDisplay(ChangeFormattingType.Underline, Se.Language.General.Underline),
            new ChangeFormattingTypeDisplay(ChangeFormattingType.Color, Se.Language.General.Color),
        ];
    }

    public static List<ChangeFormattingTypeDisplay> GetToTypes()
    {
        return
        [
            new ChangeFormattingTypeDisplay(ChangeFormattingType.Italic, Se.Language.General.Italic),
            new ChangeFormattingTypeDisplay(ChangeFormattingType.Bold, Se.Language.General.Bold),
            new ChangeFormattingTypeDisplay(ChangeFormattingType.Underline, Se.Language.General.Underline),
            new ChangeFormattingTypeDisplay(ChangeFormattingType.Color, Se.Language.General.Color),
        ];
    }
}
