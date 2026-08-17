using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewSnapTimeCodesToFrames
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Main.Menu.SnapAllTimesToFrames,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
        };

        var radioFrameRateFromVideo = UiUtil.MakeRadioButton(Se.Language.Tools.BeautifyTimeCodes.BatchFrameRateFromVideo, vm, nameof(vm.SnapTimeCodesToFramesUseVideoFrameRate), "SnapTimeCodesToFramesFrameRateSource");
        var radioFrameRateFixed = UiUtil.MakeRadioButton(Se.Language.Tools.BeautifyTimeCodes.BatchFrameRateFixed, vm, nameof(vm.SnapTimeCodesToFramesUseFixedFrameRate), "SnapTimeCodesToFramesFrameRateSource");

        var comboFrameRate = new ComboBox
        {
            ItemsSource = vm.SnapTimeCodesToFramesFrameRates,
            SelectedValue = vm.SelectedSnapTimeCodesToFramesFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
        }.WithBindSelected(nameof(vm.SelectedSnapTimeCodesToFramesFrameRate))
         .WithBindEnabled(nameof(vm.SnapTimeCodesToFramesUseFixedFrameRate));

        var panelFrameRateFixed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
            Children =
            {
                radioFrameRateFixed,
                comboFrameRate,
            }
        };

        var labelInfo = new TextBlock
        {
            Text = Se.Language.Tools.BatchConvert.SnapTimeCodesToFramesInfo,
            Opacity = 0.7,
            FontStyle = FontStyle.Italic,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 500,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Avalonia.Thickness(10),
            Spacing = 10,
            Children =
            {
                labelHeader,
                radioFrameRateFromVideo,
                panelFrameRateFixed,
                labelInfo,
            }
        };
    }
}
