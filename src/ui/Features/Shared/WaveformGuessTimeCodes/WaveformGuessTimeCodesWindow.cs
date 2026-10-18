using Avalonia;
using Avalonia.Automation;
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
/// with the buttons pinned below them for the cases where it still does not. In the editor look
/// (see <see cref="EditorChrome"/>).
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Picker);
        vm.Window = this;
        DataContext = vm;

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        // Two columns: the two short "which lines" sections stacked on the left, the taller
        // settings section on the right - about half the height of one stacked column, which is
        // what keeps the window on screen at a high UI scale.
        var panelLeft = new StackPanel
        {
            Orientation = Orientation.Vertical,
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
        };
        panelOptions.Add(panelLeft, 0);
        var borderDetectOptions = new Border
        {
            Child = MakeDetectOptionsView(vm),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        };
        panelOptions.Add(borderDetectOptions, 0, 1);

        // Keeps the OK/Cancel buttons reachable when the window is clamped to the working area.
        var scrollViewer = new ScrollViewer
        {
            Name = OptionsScrollViewerName,
            Content = panelOptions,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Content = EditorChrome.MakeRoot(scrollViewer, EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, radioStartFromVideoPosition); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.KeyDown;
    }

    private static Border MakeStartFromView(WaveformGuessTimeCodesViewModel vm, out Control radioStartFromVideoPosition)
    {
        var checkBoxStartFromVideoPosition = UiUtil.MakeRadioButton(Se.Language.General.CurrentVideoPosition, vm, nameof(vm.StartFromVideoPosition), "start");
        radioStartFromVideoPosition = checkBoxStartFromVideoPosition;
        var checkBoxStartFromBeginning = UiUtil.MakeRadioButton(Se.Language.General.Beginning, vm, nameof(vm.StartFromBeginning), "start");

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children =
            {
                checkBoxStartFromVideoPosition,
                checkBoxStartFromBeginning,
            }
        };

        return EditorChrome.MakeEditorSection(Se.Language.General.StartFrom, stackPanel, first: true);
    }

    private static Border MakeDeleteLinesView(WaveformGuessTimeCodesViewModel vm)
    {
        var checkBoxDeleteAll = UiUtil.MakeRadioButton(Se.Language.General.All, vm, nameof(vm.DeleteLinesAll), "del");
        var checkBoxDeleteNone = UiUtil.MakeRadioButton(Se.Language.General.None, vm, nameof(vm.DeleteLinesNone), "del");
        var checkBoxDeleteFromVideoPosition = UiUtil.MakeRadioButton(Se.Language.General.FromCurrentVideoPosition, vm, nameof(vm.DeleteLinesFromVideoPosition), "del");

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            Children =
            {
                checkBoxDeleteAll,
                checkBoxDeleteNone,
                checkBoxDeleteFromVideoPosition,
            }
        };

        return EditorChrome.MakeEditorSection(Se.Language.General.DeleteLines, stackPanel);
    }

    private static Border MakeDetectOptionsView(WaveformGuessTimeCodesViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 12,
            RowSpacing = 8,
        };

        void AddRow(string label, NumericUpDown field, Thickness? margin = null)
        {
            field.Classes.Add("field"); // the compact field, not the theme's big spinner
            var row = grid.RowDefinitions.Count;
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var text = EditorChrome.MakeDimLabel(label, margin ?? new Thickness(0));
            AutomationProperties.SetName(field, label);
            if (margin != null)
            {
                field.Margin = margin.Value;
            }

            grid.Add(text, row, 0);
            grid.Add(field, row, 1);
        }

        AddRow(Se.Language.Waveform.GuessTimeCodesScanBlockSize, UiUtil.MakeNumericUpDownInt(50, 5000, 100, 130, vm, nameof(vm.ScanBlockSize)));
        AddRow(Se.Language.Waveform.GuessTimeCodesScanBlockAverageMin, UiUtil.MakeNumericUpDownInt(0, 100, 35, 130, vm, nameof(vm.ScanBlockAverageMin)));
        AddRow(Se.Language.Waveform.GuessTimeCodesScanBlockAverageMax, UiUtil.MakeNumericUpDownInt(0, 100, 70, 130, vm, nameof(vm.ScanBlockAverageMax)));

        // A gap: splitting long subtitles is a step of its own, after the detection
        AddRow(Se.Language.Waveform.GuessTimeCodesSplitLongSubtitlesAt, UiUtil.MakeNumericUpDownInt(500, 20000, 3500, 130, vm, nameof(vm.SplitLongSubtitlesAtMs)), new Thickness(0, 12, 0, 0));

        return EditorChrome.MakeEditorSection(Se.Language.General.Settings, grid, first: true);
    }
}
