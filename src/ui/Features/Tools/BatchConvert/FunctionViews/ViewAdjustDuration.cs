using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Nikse.SubtitleEdit.Features.Tools.AdjustDuration;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAdjustDuration
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var combo = FunctionViewChrome.Compact(new ComboBox
        {
            ItemsSource = vm.AdjustTypes,
            [!ComboBox.SelectedValueProperty] = new Binding(nameof(vm.SelectedAdjustType)) { Mode = BindingMode.TwoWay }
        }, Se.Language.Tools.AdjustDurations.AdjustVia);

        var panelSeconds = MakeAdjustSeconds(vm);
        var panelPercent = MakeAdjustPercent(vm);
        var panelFixed = MakeAdjustFixed(vm);
        var panelRecalculate = MakeAdjustRecalculate(vm);

        return FunctionViewChrome.MakePanel(vm, Se.Language.General.AdjustDisplayDuration,
            FunctionViewChrome.MakeRow(Se.Language.Tools.AdjustDurations.AdjustVia, combo),
            panelSeconds,
            panelPercent,
            panelFixed,
            panelRecalculate,
            EditorChrome.MakeHint(Se.Language.Tools.AdjustDurations.Note));
    }

    private static NumericUpDown MakeField(BatchConvertViewModel vm, string property, decimal minimum, decimal maximum, string accessibleName)
    {
        var numericUpDown = FunctionViewChrome.Compact(new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
        }, accessibleName);
        numericUpDown.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = property,
            Mode = BindingMode.TwoWay,
            Source = vm,
        });

        return numericUpDown;
    }

    private static Grid MakeVisibleRow(BatchConvertViewModel vm, string label, Control field, string visibleProperty)
    {
        var row = FunctionViewChrome.MakeRow(label, field);
        row.Bind(Visual.IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{visibleProperty}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });

        return row;
    }

    private static Grid MakeAdjustSeconds(BatchConvertViewModel vm)
    {
        var numericUpDownSeconds = MakeField(vm, nameof(vm.AdjustSeconds), -1000000, 1000000, Se.Language.General.Seconds);
        return MakeVisibleRow(vm, Se.Language.General.Seconds, numericUpDownSeconds, nameof(AdjustDurationDisplay.IsSecondsVisible));
    }

    private static Grid MakeAdjustPercent(BatchConvertViewModel vm)
    {
        var numericUpDownPercent = MakeField(vm, nameof(vm.AdjustPercent), -1000000, 1000000, Se.Language.General.Percent);
        return MakeVisibleRow(vm, Se.Language.General.Percent, numericUpDownPercent, nameof(AdjustDurationDisplay.IsPercentVisible));
    }

    private static Grid MakeAdjustFixed(BatchConvertViewModel vm)
    {
        // The bound value is stored and applied as milliseconds
        // (AdjustDurationFixedMilliseconds -> BatchConvertConfig.FixedMilliseconds).
        var numericUpDownFixed = MakeField(vm, nameof(vm.AdjustFixed), -1000000, 1000000, Se.Language.General.Milliseconds);
        return MakeVisibleRow(vm, Se.Language.General.Milliseconds, numericUpDownFixed, nameof(AdjustDurationDisplay.IsFixedVisible));
    }

    private static StackPanel MakeAdjustRecalculate(BatchConvertViewModel vm)
    {
        var numericUpDownMax = MakeField(vm, nameof(vm.AdjustRecalculateMaxCharacterPerSecond), 0, 1000, Se.Language.General.MaxCharactersPerSecond);
        var numericUpDownOptimal = MakeField(vm, nameof(vm.AdjustRecalculateOptimalCharacterPerSecond), 0, 1000, Se.Language.General.OptimalCharactersPerSecond);

        var panel = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                FunctionViewChrome.MakeRow(Se.Language.General.MaxCharactersPerSecond, numericUpDownMax),
                FunctionViewChrome.MakeRow(Se.Language.General.OptimalCharactersPerSecond, numericUpDownOptimal),
            },
        };
        panel.Bind(Visual.IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsRecalculateVisible)}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });

        return panel;
    }
}
