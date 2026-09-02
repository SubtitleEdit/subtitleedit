using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewAssaChangeStyleProperties
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesTitle,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelOnlyAssa = new Label
        {
            Content = Se.Language.Tools.BatchConvert.AssaChangeResolutionOnlyAppliesToAssa,
            FontStyle = Avalonia.Media.FontStyle.Italic,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelInfo = new Label
        {
            Content = Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesInfo,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var checkBoxSetSpacing = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesSetSpacing, vm, nameof(vm.AssaChangeStylePropertiesSetSpacing));
        var numericUpDownSpacing = UiUtil.MakeNumericUpDownOneDecimal(-100, 100, 130, vm, nameof(vm.AssaChangeStylePropertiesSpacing));

        var checkBoxSetAlignment = UiUtil.MakeCheckBox(Se.Language.Tools.BatchConvert.AssaChangeStylePropertiesSetAlignment, vm, nameof(vm.AssaChangeStylePropertiesSetAlignment));
        var comboBoxAlignment = UiUtil.MakeComboBox(vm.AssaChangeStylePropertiesAlignmentOptions, vm, nameof(vm.SelectedAssaChangeStylePropertiesAlignment));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Header
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Only-ASSA note
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Info
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Spacing
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Alignment
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = new Avalonia.Thickness(10),
            ColumnSpacing = 10,
            RowSpacing = 10,
        };

        grid.Add(labelHeader, 0, 0, 1, 2);
        grid.Add(labelOnlyAssa, 1, 0, 1, 2);
        grid.Add(labelInfo, 2, 0, 1, 2);

        grid.Add(checkBoxSetSpacing, 3);
        grid.Add(numericUpDownSpacing, 3, 1);

        grid.Add(checkBoxSetAlignment, 4);
        grid.Add(comboBoxAlignment, 4, 1);

        return grid;
    }
}
