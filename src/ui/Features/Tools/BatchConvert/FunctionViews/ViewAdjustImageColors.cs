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
        };

        var brightnessPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
            Children =
            {
                MakeCheckBox(Se.Language.Tools.ImageBasedEdit.AdjustBrightness, vm, nameof(vm.ImageAdjustBrightnessOn)),
                MakeLabel(Se.Language.Tools.ImageBasedEdit.Brightness),
                MakeNumericUpDown(vm, nameof(vm.ImageAdjustBrightness), nameof(vm.ImageAdjustBrightnessOn), -100, 100, 1),
                MakeLabel(Se.Language.Tools.ImageBasedEdit.Contrast),
                MakeNumericUpDown(vm, nameof(vm.ImageAdjustContrast), nameof(vm.ImageAdjustBrightnessOn), -100, 100, 1),
                MakeLabel(Se.Language.Tools.ImageBasedEdit.Gamma + " (%)"),
                MakeNumericUpDown(vm, nameof(vm.ImageAdjustGamma), nameof(vm.ImageAdjustBrightnessOn), 20, 400, 5),
            }
        };

        var alphaPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
            Children =
            {
                MakeCheckBox(Se.Language.General.AdjustAlpha, vm, nameof(vm.ImageAdjustAlphaOn)),
                MakeLabel(Se.Language.General.AlphaAdjustment),
                MakeNumericUpDown(vm, nameof(vm.ImageAdjustAlpha), nameof(vm.ImageAdjustAlphaOn), -255, 255, 5),
                MakeLabel(Se.Language.General.AlphaThreshold),
                MakeNumericUpDown(vm, nameof(vm.ImageAdjustAlphaThreshold), nameof(vm.ImageAdjustAlphaOn), 0, 255, 5),
            }
        };

        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.ImageAdjustColorValue));
        colorPicker[!Control.IsEnabledProperty] = new Binding(nameof(vm.ImageAdjustColorOn)) { Source = vm };
        var colorPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
            Children =
            {
                MakeCheckBox(Se.Language.Tools.ImageBasedEdit.AdjustColor, vm, nameof(vm.ImageAdjustColorOn)),
                colorPicker,
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
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
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelHeader, 0);
        grid.Add(labelInfo, 1);
        grid.Add(brightnessPanel, 2);
        grid.Add(alphaPanel, 3);
        grid.Add(colorPanel, 4);

        return grid;
    }

    private static CheckBox MakeCheckBox(string content, BatchConvertViewModel vm, string bindingProperty)
    {
        return new CheckBox
        {
            Content = content,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            [!ToggleButton.IsCheckedProperty] = new Binding(bindingProperty) { Source = vm, Mode = BindingMode.TwoWay },
        };
    }

    private static Label MakeLabel(string content)
    {
        return new Label
        {
            Content = content,
            VerticalAlignment = VerticalAlignment.Center,
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
