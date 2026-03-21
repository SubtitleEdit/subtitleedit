using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public class WhisperPostProcessingWindow : Window
{
    private readonly WhisperPostProcessingViewModel _vm;

    public WhisperPostProcessingWindow(WhisperPostProcessingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Whisper post-processing";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 400;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelAdjustTimings = UiUtil.MakeTextBlock("Adjust timings");
        var checkAdjustTimings = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.AdjustTimings));

        var labelMergeShortLines = UiUtil.MakeTextBlock("Merge short lines");
        var checkMergeShortLines = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.MergeShortLines));

        var labelBreakSplitLongLines = UiUtil.MakeTextBlock("Break/split long lines");
        var checkBreakSplitLongLines = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.BreakSplitLongLines));

        var labelFixShortDuration = UiUtil.MakeTextBlock("Fix short duration");
        var checkFixShortDuration = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.FixShortDuration));

        var labelFixCasing = UiUtil.MakeTextBlock("Fix casing");
        var checkFixCasing = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.FixCasing));

        var labelAddPeriods = UiUtil.MakeTextBlock("Add periods");
        var checkAddPeriods = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.AddPeriods));

        var labelChangeUnderlineToColor = UiUtil.MakeTextBlock("Change underline to color");
        var checkChangeUnderlineToColor = UiUtil.MakeCheckBox(vm, nameof(WhisperPostProcessingViewModel.ChangeUnderlineToColor));
        var colorPickerUnderlineToColor = new ColorPicker()
        {
            Width = 200,
            IsAlphaEnabled = true,
            IsAlphaVisible = true,
            IsColorSpectrumSliderVisible = false,
            IsColorComponentsVisible = true,
            IsColorModelVisible = false,
            IsColorPaletteVisible = false,
            IsAccentColorsVisible = false,
            IsColorSpectrumVisible = true,
            IsComponentTextInputVisible = true,
            [!ColorPicker.ColorProperty] = new Binding(nameof(_vm.ChangeUnderlineToColorColor))
            {
                Source = _vm,
                Mode = BindingMode.TwoWay
            },
        };


        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.General.Ok, vm.OKCommand),
            UiUtil.MakeButton(Se.Language.General.Cancel, vm.CancelCommand)
        );

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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;

        grid.Add(labelAdjustTimings, row, 0);
        grid.Add(checkAdjustTimings, row, 1);
        row++;

        grid.Add(labelMergeShortLines, row, 0);
        grid.Add(checkMergeShortLines, row, 1);
        row++;

        grid.Add(labelBreakSplitLongLines, row, 0);
        grid.Add(checkBreakSplitLongLines, row, 1);
        row++;

        grid.Add(labelFixShortDuration, row, 0);
        grid.Add(checkFixShortDuration, row, 1);
        row++;

        grid.Add(labelFixCasing, row, 0);
        grid.Add(checkFixCasing, row, 1);
        row++;

        grid.Add(labelAddPeriods, row, 0);
        grid.Add(checkAddPeriods, row, 1);
        row++;

        grid.Add(labelChangeUnderlineToColor, row, 0);
        grid.Add(checkChangeUnderlineToColor, row, 1);
        grid.Add(colorPickerUnderlineToColor, row, 2);
        row++;

        grid.Add(buttonPanel, row, 0, 1, 3);

        Content = grid;

        Activated += delegate { Focus(); }; // hack to make OnKeyDown work
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
