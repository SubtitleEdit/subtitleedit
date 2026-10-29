using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAssaChangeResolution
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericUpDownTargetWidth = FunctionViewChrome.MakeIntField(vm, nameof(vm.AssaChangeResolutionTargetWidth), 1, 10000, 1920, Se.Language.General.Width);
        var numericUpDownTargetHeight = FunctionViewChrome.MakeIntField(vm, nameof(vm.AssaChangeResolutionTargetHeight), 1, 10000, 1080, Se.Language.General.Height);

        var labelTargetResolution = EditorChrome.MakeDimLabel(Se.Language.Assa.ResolutionResamplerTargetRes, new Avalonia.Thickness(0, 4, 0, 0));

        return FunctionViewChrome.MakePanel(vm, Se.Language.Assa.ResolutionResamplerTitle,
            EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.AssaChangeResolutionOnlyAppliesToAssa),
            labelTargetResolution,
            FunctionViewChrome.MakeRow(Se.Language.General.Width, numericUpDownTargetWidth),
            FunctionViewChrome.MakeRow(Se.Language.General.Height, numericUpDownTargetHeight),
            EditorChrome.MakeSwitchRow(nameof(vm.AssaChangeResolutionChangeMargins), Se.Language.Assa.ResolutionResamplerChangeMargins),
            EditorChrome.MakeSwitchRow(nameof(vm.AssaChangeResolutionChangeFontSize), Se.Language.Assa.ResolutionResamplerChangeFontSize),
            EditorChrome.MakeSwitchRow(nameof(vm.AssaChangeResolutionChangePosition), Se.Language.Assa.ResolutionResamplerChangePositions),
            EditorChrome.MakeSwitchRow(nameof(vm.AssaChangeResolutionChangeDrawing), Se.Language.Assa.ResolutionResamplerChangeDrawing));
    }
}
