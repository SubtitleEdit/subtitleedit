using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewSnapTimeCodesToFrames
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var radioFrameRateFromVideo = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.Tools.BeautifyTimeCodes.BatchFrameRateFromVideo, vm, nameof(vm.SnapTimeCodesToFramesUseVideoFrameRate), "SnapTimeCodesToFramesFrameRateSource"));
        var radioFrameRateFixed = FunctionViewChrome.Compact(UiUtil.MakeRadioButton(Se.Language.Tools.BeautifyTimeCodes.BatchFrameRateFixed, vm, nameof(vm.SnapTimeCodesToFramesUseFixedFrameRate), "SnapTimeCodesToFramesFrameRateSource"));

        var comboFrameRate = FunctionViewChrome.Compact(new ComboBox
        {
            ItemsSource = vm.SnapTimeCodesToFramesFrameRates,
            SelectedValue = vm.SelectedSnapTimeCodesToFramesFrameRate,
        }.WithFrameRateDisplay()
         .WithBindSelected(nameof(vm.SelectedSnapTimeCodesToFramesFrameRate))
         .WithBindEnabled(nameof(vm.SnapTimeCodesToFramesUseFixedFrameRate)), Se.Language.Tools.BeautifyTimeCodes.BatchFrameRateFixed, 110);

        var panelFrameRateFixed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 8,
            Children =
            {
                radioFrameRateFixed,
                comboFrameRate,
            }
        };

        return FunctionViewChrome.MakePanel(vm, Se.Language.Main.Menu.SnapAllTimesToFrames,
            radioFrameRateFromVideo,
            panelFrameRateFixed,
            EditorChrome.MakeHint(Se.Language.Tools.BatchConvert.SnapTimeCodesToFramesInfo));
    }
}
