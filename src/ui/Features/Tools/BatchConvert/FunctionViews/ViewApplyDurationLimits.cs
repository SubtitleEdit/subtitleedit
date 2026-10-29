using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewApplyDurationLimits
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var numericMin = FunctionViewChrome.MakeIntField(vm, nameof(vm.ApplyDurationLimitsMinDurationMs), 100, 60000, vm.ApplyDurationLimitsMinDurationMs, Se.Language.Tools.ApplyDurationLimits.FixMinDurationMs);
        var numericMax = FunctionViewChrome.MakeIntField(vm, nameof(vm.ApplyDurationLimitsMaxDurationMs), 100, 600000, vm.ApplyDurationLimitsMaxDurationMs, Se.Language.Tools.ApplyDurationLimits.FixMaxDurationMs);

        return FunctionViewChrome.MakePanel(vm, Se.Language.Tools.ApplyDurationLimits.Title,
            FunctionViewChrome.MakeSwitchRow(nameof(vm.ApplyDurationLimitsFixMin), Se.Language.Tools.ApplyDurationLimits.FixMinDurationMs, numericMin),
            FunctionViewChrome.MakeSwitchRow(nameof(vm.ApplyDurationLimitsFixMax), Se.Language.Tools.ApplyDurationLimits.FixMaxDurationMs, numericMax));
    }
}
