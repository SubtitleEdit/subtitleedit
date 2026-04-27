using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.WaveformSeekSilence;

public class WaveformSeekSilenceWindow : Window
{
    public WaveformSeekSilenceWindow(WaveformSeekSilenceViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Waveform.SeekSilence;
        CanResize = true;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

        var label = UiUtil.MakeLabel(Se.Language.General.SearchDirection);

        var checkBoxSeekForward = UiUtil.MakeRadioButton(Se.Language.General.Forward, vm, nameof(vm.SeekForward), "seek")
            .WithMarginLeft(10);
        var checkBoxSeekBackward = UiUtil.MakeRadioButton(Se.Language.General.Backward, vm, nameof(vm.SeekBackward), "seek")
            .WithMarginLeft(10);

        var labelMinSilenceDuration = UiUtil.MakeLabel(Se.Language.Waveform.MinSilenceDurationSeconds);
        var numericUpDownMinSilenceDuration = UiUtil.MakeNumericUpDownThreeDecimals(0, 10000, 140, vm, nameof(vm.SilenceMinDuration));
        var panelMinSilenceDuration = UiUtil.MakeHorizontalPanel(labelMinSilenceDuration, numericUpDownMinSilenceDuration);

        var labelMaxSilenceVolume = UiUtil.MakeLabel(Se.Language.Waveform.MaxSilenceVolume);
        var numericUpDownMaxSilenceVolume = UiUtil.MakeNumericUpDownTwoDecimals(0, 1, 140, vm, nameof(vm.SilenceMaxVolume));
        var panelMaxSilenceVolume = UiUtil.MakeHorizontalPanel(labelMaxSilenceVolume, numericUpDownMaxSilenceVolume);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(label, 0);
        grid.Add(checkBoxSeekForward, 1);
        grid.Add(checkBoxSeekBackward, 2);
        grid.Add(panelMinSilenceDuration, 3);
        grid.Add(panelMaxSilenceVolume, 4);
        grid.Add(panelButtons, 5);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }
}
