using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

public static class ViewChangeFrameRate
{
    public static Control Make(BatchConvertViewModel vm)
    {
        var labelHeader = new Label
        {
            Content = Se.Language.General.ChangeFrameRate,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0,0,0, 10),
        };

        var labelFromFrameRate = new Label
        {
            Content = Se.Language.Sync.FromFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var comboFromFrameRate = new ComboBox
        {
            ItemsSource = vm.FromFrameRates,
            SelectedValue = vm.SelectedFromFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
        }.WithBindSelected(nameof(vm.SelectedFromFrameRate));

        //var buttonFromFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseFromFrameRateCommand);

        //var buttonSwitch = UiUtil.MakeButton(vm.SwitchFrameRatesCommand, IconNames.MdiSwapVertical);

        var labelToFrameRate = new Label
        {
            Content = Se.Language.Sync.ToFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var comboToFrameRate = new ComboBox
        {
            ItemsSource = vm.ToFrameRates,
            SelectedValue = vm.SelectedToFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
        }.WithBindSelected(nameof(vm.SelectedToFrameRate));

        //var buttonToFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseToFrameRateCommand);
        
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;
        grid.Add(labelHeader, row, 0, 1, 3);
        row++;
        
        grid.Add(labelFromFrameRate, row, 0);
        grid.Add(comboFromFrameRate, row, 1);
        row++;

        grid.Add(labelToFrameRate, row, 0);
        grid.Add(comboToFrameRate, row, 1);

        return grid;
    }
}
