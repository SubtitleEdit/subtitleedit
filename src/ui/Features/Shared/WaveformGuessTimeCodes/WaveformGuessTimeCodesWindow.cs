using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.WaveformGuessTimeCodes;

/// <summary>
/// The three option boxes used to be stacked in one column, which made the window tall enough
/// that a high UI scale (or a small working area) pushed it past the screen - it is then clamped
/// to the working area, and the clamp cut off the bottom options and the OK/Cancel buttons. The
/// boxes are laid out in two columns so the window fits at a high scale, and the options scroll
/// with the buttons pinned below them for the cases where it still does not.
/// </summary>
public class WaveformGuessTimeCodesWindow : Window
{
    internal const string OptionsScrollViewerName = "OptionsScrollViewer";

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

        // Two columns: the two short "which lines" boxes stacked on the left, the taller settings
        // box on the right - about half the height of one stacked column, which is what keeps the
        // window on screen at a high UI scale.
        var panelLeft = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                MakeStartFromView(vm, out var radioStartFromVideoPosition),
                MakeDeleteLinesView(vm),
            },
        };

        var panelOptions = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
        };
        panelOptions.Add(panelLeft, 0);
        var borderDetectOptions = MakeDetectOptionsView(vm);
        borderDetectOptions.VerticalAlignment = VerticalAlignment.Top;
        panelOptions.Add(borderDetectOptions, 0, 1);

        // Keeps the OK/Cancel buttons reachable when the window is clamped to the working area.
        var scrollViewer = new ScrollViewer
        {
            Name = OptionsScrollViewerName,
            Content = panelOptions,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(scrollViewer, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, radioStartFromVideoPosition); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Border MakeStartFromView(WaveformGuessTimeCodesViewModel vm, out Control radioStartFromVideoPosition)
    {
        var labelStartFrom = UiUtil.MakeLabel(Se.Language.General.StartFrom);
        var checkBoxStartFromVideoPosition = UiUtil.MakeRadioButton(Se.Language.General.CurrentVideoPosition, vm, nameof(vm.StartFromVideoPosition), "start")
            .WithMarginLeft(10);
        radioStartFromVideoPosition = checkBoxStartFromVideoPosition;
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
