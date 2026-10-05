using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewConvertActors
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.ConvertActors.Title,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Thickness(0, 0, 0, 10),
        };

        var labelFrom = UiUtil.MakeLabel(Se.Language.Tools.ConvertActors.ConvertActorFrom);
        var comboBoxFrom = UiUtil.MakeComboBox(vm.ConvertActorsFromTypes, vm, nameof(vm.SelectedConvertActorsFromType));

        var labelTo = UiUtil.MakeLabel(Se.Language.Tools.ConvertActors.ConvertActorTo);
        var comboBoxTo = UiUtil.MakeComboBox(vm.ConvertActorsToTypes, vm, nameof(vm.SelectedConvertActorsToType));

        var checkBoxSetColor = UiUtil.MakeCheckBox(Se.Language.Tools.ConvertActors.SetColor, vm, nameof(vm.ConvertActorsSetColor));
        var colorPicker = UiUtil.MakeColorPickerButton(vm, nameof(vm.ConvertActorsColor));
        colorPicker.Bind(Visual.IsVisibleProperty, new Binding(nameof(vm.ConvertActorsSetColor)) { Source = vm, Mode = BindingMode.OneWay });

        var checkBoxChangeCasing = UiUtil.MakeCheckBox(Se.Language.General.ChangeCasing, vm, nameof(vm.ConvertActorsChangeCasing));
        var comboBoxCasing = new ComboBox
        {
            ItemsSource = vm.ConvertActorsCasingOptions,
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        comboBoxCasing.Bind(ComboBox.SelectedIndexProperty, new Binding(nameof(vm.ConvertActorsCasingIndex)) { Mode = BindingMode.TwoWay });
        comboBoxCasing.Bind(ComboBox.IsVisibleProperty, new Binding(nameof(vm.ConvertActorsChangeCasing)) { Source = vm, Mode = BindingMode.OneWay });

        var checkBoxOnlyNames = UiUtil.MakeCheckBox(Se.Language.Tools.ConvertActors.OnlyNames, vm, nameof(vm.ConvertActorsOnlyNames));

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Thickness(10),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0, 0, 1, 2);
        grid.Add(labelFrom, 1);
        grid.Add(comboBoxFrom, 1, 1);
        grid.Add(labelTo, 2);
        grid.Add(comboBoxTo, 2, 1);
        grid.Add(UiUtil.MakeHorizontalPanel(checkBoxSetColor, colorPicker), 3, 0, 1, 2);
        grid.Add(UiUtil.MakeHorizontalPanel(checkBoxChangeCasing, comboBoxCasing), 4, 0, 1, 2);
        grid.Add(checkBoxOnlyNames, 5, 0, 1, 2);

        return grid;
    }
}
