using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewSortBy
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.Tools.SortBy.Title,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10),
        };

        var labelSortBy = UiUtil.MakeLabel(Se.Language.Tools.SortBy.SortOrder);
        var comboSortBy = UiUtil.MakeComboBox(vm.SortByOptions, vm, nameof(vm.SelectedSortByOption))
            .WithMinWidth(180);

        var checkBoxDescending = UiUtil.MakeCheckBox(Se.Language.General.Descending, vm, nameof(vm.SortByDescending));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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
        grid.Add(labelSortBy, 1);
        grid.Add(comboSortBy, 1, 1);
        grid.Add(checkBoxDescending, 2, 0, 1, 2);

        return grid;
    }
}
