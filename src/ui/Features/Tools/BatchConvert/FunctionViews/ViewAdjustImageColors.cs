using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAdjustImageColors
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.BatchConvert.AdjustImageColorsTitle,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Bold,
        };

        var labelInfo = new TextBlock
        {
            Text = Se.Language.Tools.BatchConvert.AdjustImageColorsInfo,
            Opacity = 0.7,
            FontStyle = FontStyle.Italic,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 500,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.ImageAdjustColorValue));
        colorPicker[!Control.IsEnabledProperty] = new Binding(nameof(vm.ImageAdjustColorOn)) { Source = vm };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 6,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var row = 0;
        AddRow(grid, row++, labelHeader);
        AddRow(grid, row++, labelInfo);

        AddRow(grid, row++, MakeCheckBox(Se.Language.Tools.ImageBasedEdit.AdjustBrightness, vm, nameof(vm.ImageAdjustBrightnessOn)));
        AddPair(grid, row++, Se.Language.Tools.ImageBasedEdit.Brightness, MakeNumericUpDown(vm, nameof(vm.ImageAdjustBrightness), nameof(vm.ImageAdjustBrightnessOn), -100, 100, 1));
        AddPair(grid, row++, Se.Language.Tools.ImageBasedEdit.Contrast, MakeNumericUpDown(vm, nameof(vm.ImageAdjustContrast), nameof(vm.ImageAdjustBrightnessOn), -100, 100, 1));
        AddPair(grid, row++, Se.Language.Tools.ImageBasedEdit.Gamma + " (%)", MakeNumericUpDown(vm, nameof(vm.ImageAdjustGamma), nameof(vm.ImageAdjustBrightnessOn), 20, 400, 5));

        AddRow(grid, row++, MakeCheckBox(Se.Language.General.AdjustAlpha, vm, nameof(vm.ImageAdjustAlphaOn)));
        AddPair(grid, row++, Se.Language.General.AlphaAdjustment, MakeNumericUpDown(vm, nameof(vm.ImageAdjustAlpha), nameof(vm.ImageAdjustAlphaOn), -255, 255, 5));
        AddPair(grid, row++, Se.Language.General.AlphaThreshold, MakeNumericUpDown(vm, nameof(vm.ImageAdjustAlphaThreshold), nameof(vm.ImageAdjustAlphaOn), 0, 255, 5));

        AddRow(grid, row++, MakeCheckBox(Se.Language.Tools.ImageBasedEdit.AdjustColor, vm, nameof(vm.ImageAdjustColorOn)));
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        colorPicker.Margin = new Thickness(25, 0, 0, 0);
        grid.Add(colorPicker, row, 0);

        return grid;
    }

    private static void AddRow(Grid grid, int row, Control control)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        grid.Add(control, row, 0, 1, 2);
    }

    private static void AddPair(Grid grid, int row, string labelText, Control control)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        var label = new Label
        {
            Content = labelText,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(25, 0, 0, 0),
        };
        grid.Add(label, row, 0);
        grid.Add(control, row, 1);
    }

    private static CheckBox MakeCheckBox(string content, BatchConvertViewModel vm, string bindingProperty)
    {
        return new CheckBox
        {
            Content = content,
            VerticalAlignment = VerticalAlignment.Center,
            [!ToggleButton.IsCheckedProperty] = new Binding(bindingProperty) { Source = vm, Mode = BindingMode.TwoWay },
        };
    }

    private static NumericUpDown MakeNumericUpDown(BatchConvertViewModel vm, string bindingProperty, string enabledProperty, int minimum, int maximum, int increment)
    {
        return new NumericUpDown
        {
            Width = 130,
            Minimum = minimum,
            Maximum = maximum,
            Increment = increment,
            FormatString = "0",
            [!NumericUpDown.ValueProperty] = new Binding(bindingProperty) { Source = vm, Mode = BindingMode.TwoWay },
            [!Control.IsEnabledProperty] = new Binding(enabledProperty) { Source = vm },
        };
    }
}
