using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewBridgeGaps
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var useFrames = Se.Settings.General.UseFrameMode;
        var textBridgeGapSmallerThan = useFrames
            ? Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThanFrames
            : Se.Language.Tools.BridgeGaps.BridgeGapsSmallerThan;
        var numericUpDownBridgeGapSmallerThan = FunctionViewChrome.MakeIntField(vm, nameof(vm.BridgeGapsSmallerThanMsOrFrames), 1, 10000, Se.Settings.Tools.BridgeGaps.GetBridgeGapsSmallerThan(useFrames), textBridgeGapSmallerThan);

        var textMinGap = useFrames
            ? Se.Language.Tools.BridgeGaps.MinGapFrames
            : Se.Language.Tools.BridgeGaps.MinGap;
        var numericUpDownMinGap = FunctionViewChrome.MakeIntField(vm, nameof(vm.BridgeGapsMinGapMsOrFrames), 0, 1000, Se.Settings.Tools.BridgeGaps.GetMinGap(useFrames), textMinGap);

        var numericUpDownPercentForLeft = FunctionViewChrome.MakeIntField(vm, nameof(vm.BridgeGapsPercentForLeft), 0, 100, Se.Settings.Tools.BridgeGaps.PercentForLeft, Se.Language.Tools.BridgeGaps.PercentFoPrevious);

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.BridgeGaps,
            FunctionViewChrome.MakeRow(textBridgeGapSmallerThan, numericUpDownBridgeGapSmallerThan),
            FunctionViewChrome.MakeRow(textMinGap, numericUpDownMinGap),
            FunctionViewChrome.MakeRow(Se.Language.Tools.BridgeGaps.PercentFoPrevious, numericUpDownPercentForLeft));
    }
}
