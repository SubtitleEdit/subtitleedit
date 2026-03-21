using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewChangeCasing
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.ChangeCasing,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = Avalonia.Media.FontWeight.Bold
        };

        var checkBoxNormalCasing = new RadioButton
        {
            Content = Se.Language.General.NormalCasing,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.NormalCasing)) { Mode = BindingMode.TwoWay },
            GroupName = "ChangeCasingType",
        };

        var checkBoxNormalCasingFixNames = new CheckBox
        {
            Content = Se.Language.Tools.ChangeCasing.FixNames,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 5),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.NormalCasingFixNames)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxNormalCasingOnlyUpper = new CheckBox
        {
            Content = Se.Language.Tools.ChangeCasing.OnlyFixUppercaseLines,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(15, 0, 0, 15),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.NormalCasingOnlyUpper)) { Mode = BindingMode.TwoWay },
        };

        var checkBoxFixNamesOnly = new RadioButton
        {
            Content = Se.Language.Tools.ChangeCasing.FixNamesOnly,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.FixNamesOnly)) { Mode = BindingMode.TwoWay },
            GroupName = "ChangeCasingType",
        };

        var checkBoxAllUppercase = new RadioButton
        {
            Content = Se.Language.Tools.ChangeCasing.AllUppercase,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AllUppercase)) { Mode = BindingMode.TwoWay },
            GroupName = "ChangeCasingType",
        };

        var checkBoxAllLowercase = new RadioButton
        {
            Content = Se.Language.Tools.ChangeCasing.AllLowercase,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AllLowercase)) { Mode = BindingMode.TwoWay },
            GroupName = "ChangeCasingType",
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelHeader, 0, 0);
        grid.Add(checkBoxNormalCasing, 1, 0);
        grid.Add(checkBoxNormalCasingFixNames, 2, 0);
        grid.Add(checkBoxNormalCasingOnlyUpper, 3, 0);
        grid.Add(checkBoxFixNamesOnly, 4, 0);
        grid.Add(checkBoxAllUppercase, 5, 0);
        grid.Add(checkBoxAllLowercase, 6, 0);

        return grid;
    }
}
