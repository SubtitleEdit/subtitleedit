using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.WaveformGuessTimeCodes;

public class WaveformGuessTimeCodesWindow : Window
{
    public WaveformGuessTimeCodesWindow(WaveformGuessTimeCodesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Waveform.GuessTimeCodes;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        vm.Window = this;
        DataContext = vm;

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

        grid.Add(MakeStartFromView(vm), 0);
        grid.Add(MakeDeleteLinesView(vm), 1);
        grid.Add(MakeDetectOptionsView(vm), 2);
        grid.Add(panelButtons, 3);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.KeyDown;
    }

    private static Border MakeStartFromView(WaveformGuessTimeCodesViewModel vm)
    {
        var labelStartFrom = UiUtil.MakeLabel(Se.Language.General.StartFrom);
        var checkBoxStartFromVideoPosition = UiUtil.MakeRadioButton(Se.Language.General.CurrentVideoPosition, vm, nameof(vm.StartFromVideoPosition), "start")
            .WithMarginLeft(10);
        var checkBoxStartFromBeginning = UiUtil.MakeRadioButton(Se.Language.General.Beginning, vm, nameof(vm.StartFromBeginning), "start")
            .WithMarginLeft(10);

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                labelStartFrom,
                checkBoxStartFromVideoPosition,
                checkBoxStartFromBeginning,
            }
        };

        return UiUtil.MakeBorderForControl(stackPanel);
    }

    private static Border MakeDeleteLinesView(WaveformGuessTimeCodesViewModel vm)
    {
        var labelDeleteLines = UiUtil.MakeLabel(Se.Language.General.DeleteLines);
        var checkBoxDeleteAll = UiUtil.MakeRadioButton(Se.Language.General.All, vm, nameof(vm.DeleteLinesAll), "del")
            .WithMarginLeft(10);
        var checkBoxDeleteNone = UiUtil.MakeRadioButton(Se.Language.General.None, vm, nameof(vm.DeleteLinesNone), "del")
            .WithMarginLeft(10);
        var checkBoxDeleteFromVideoPosition = UiUtil.MakeRadioButton(Se.Language.General.FromCurrentVideoPosition, vm, nameof(vm.DeleteLinesFromVideoPosition), "del")
            .WithMarginLeft(10);

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                labelDeleteLines,
                checkBoxDeleteAll,
                checkBoxDeleteNone,
                checkBoxDeleteFromVideoPosition,
            }
        };

        return UiUtil.MakeBorderForControl(stackPanel);
    }

    private static Border MakeDetectOptionsView(WaveformGuessTimeCodesViewModel vm)
    {
        var labelSettings = UiUtil.MakeLabel(Se.Language.General.Settings);

        var labelScanBlockSize = UiUtil.MakeLabel(Se.Language.Waveform.GuessTimeCodesScanBlockSize);
        var numeriUpDownScanBlockSize = UiUtil.MakeNumericUpDownInt(50, 5000, 100, 130, vm, nameof(vm.ScanBlockSize));
        var panelScanBlockSize = UiUtil.MakeHorizontalPanel(labelScanBlockSize, numeriUpDownScanBlockSize);

        var labelScanBlockAverageMin = UiUtil.MakeLabel(Se.Language.Waveform.GuessTimeCodesScanBlockAverageMin);
        var numeriUpDownScanBlockAverageMin = UiUtil.MakeNumericUpDownInt(0, 100, 35, 130, vm, nameof(vm.ScanBlockAverageMin));
        var panelScanBlockAverageMin = UiUtil.MakeHorizontalPanel(labelScanBlockAverageMin, numeriUpDownScanBlockAverageMin);

        var labelScanBlockAverageMax = UiUtil.MakeLabel(Se.Language.Waveform.GuessTimeCodesScanBlockAverageMax);
        var numeriUpDownScanBlockAverageMax = UiUtil.MakeNumericUpDownInt(0, 100, 70, 130, vm, nameof(vm.ScanBlockAverageMax));
        var panelScanBlockAverageMax = UiUtil.MakeHorizontalPanel(labelScanBlockAverageMax, numeriUpDownScanBlockAverageMax);

        var labelSpace = UiUtil.MakeLabel();

        var labelSplitLongSubtitlesAt = UiUtil.MakeLabel(Se.Language.Waveform.GuessTimeCodesSplitLongSubtitlesAt);
        var numeriUpDownSplitLongSubtitlesAt = UiUtil.MakeNumericUpDownInt(500, 20000, 3500, 130, vm, nameof(vm.SplitLongSubtitlesAtMs));
        var panelSplitLongSubtitlesAt = UiUtil.MakeHorizontalPanel(labelSplitLongSubtitlesAt, numeriUpDownSplitLongSubtitlesAt);

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                labelSettings,
                panelScanBlockSize,
                panelScanBlockAverageMin,
                panelScanBlockAverageMax,
                labelSpace,
                panelSplitLongSubtitlesAt,
            }
        };

        return UiUtil.MakeBorderForControl(stackPanel);
    }
}
