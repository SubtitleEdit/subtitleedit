using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewFixRightToLeft
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var radioButtonRtlFixViaUnicode = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.General.FixRightToLeftViaUnicodeTags, vm, nameof(vm.RtlFixViaUniCode), "rtl"));
        var radioButtonRtlRemoveUnicode = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.General.RemoveRightToLeftUnicodeTags, vm, nameof(vm.RtlRemoveUniCode), "rtl"));
        var radioButtonRtlReverseStartEnd = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.General.ReverseRightToLeftStartEnd, vm, nameof(vm.RtlReverseStartEnd), "rtl"));

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.FixRightToLeft,
            radioButtonRtlFixViaUnicode,
            radioButtonRtlRemoveUnicode,
            radioButtonRtlReverseStartEnd);
    }
}
