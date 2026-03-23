using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class DialogStyleDisplay
{
    public string Name { get; set; }
    public string Code { get; set; }

    public DialogStyleDisplay()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public DialogStyleDisplay(DialogStyleDisplay other)
    {
        Name = other.Name;
        Code = other.Code;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<DialogStyleDisplay> List()
    {
        return
        [
            new()
            {
                Name = Se.Language.Options.Settings.DialogStyleDashBothLinesWithSpace,
                Code = Core.Enums.DialogType.DashBothLinesWithSpace.ToString() },
            new()
            {
                Name = Se.Language.Options.Settings.DialogStyleDashBothLinesWithoutSpace,
                Code = Core.Enums.DialogType.DashBothLinesWithoutSpace.ToString() },
            new()
            {
                Name = Se.Language.Options.Settings.DialogStyleDashSecondLineWithSpace,
                Code = Core.Enums.DialogType.DashSecondLineWithSpace.ToString() },
            new()
            {
                Name = Se.Language.Options.Settings.DialogStyleDashSecondLineWithoutSpace,
                Code = Core.Enums.DialogType.DashSecondLineWithoutSpace.ToString() },
        ];
    }
}

