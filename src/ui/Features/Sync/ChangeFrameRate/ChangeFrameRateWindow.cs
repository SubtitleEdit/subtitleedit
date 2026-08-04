using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;

public class ChangeFrameRateWindow : Window
{
    public ChangeFrameRateWindow(ChangeFrameRateViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChangeFrameRate;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelFromFrameRate = new Label
        {
            Content = Se.Language.Sync.FromFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var comboFromFrameRate = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
        }
        .WithBindItemsSource(nameof(vm.FromFrameRates))
        .WithBindSelected(nameof(vm.SelectedFromFrameRate));

        var buttonFromFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseFromFrameRateCommand, accessibleName: Se.Language.Sync.FromFrameRate);

        var buttonSwitch = UiUtil.MakeButton(vm.SwitchFrameRatesCommand, IconNames.SwapVertical,
            $"{Se.Language.Sync.FromFrameRate} <-> {Se.Language.Sync.ToFrameRate}");

        var labelToFrameRate = new Label
        {
            Content = Se.Language.Sync.ToFrameRate,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var comboToFrameRate = new ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 90,
        }
        .WithBindItemsSource(nameof(vm.ToFrameRates))
        .WithBindSelected(nameof(vm.SelectedToFrameRate));

        var buttonToFrameRate = UiUtil.MakeButtonBrowse(vm.BrowseToFrameRateCommand, accessibleName: Se.Language.Sync.ToFrameRate);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var row = 0;
        grid.Add(labelFromFrameRate, row, 0);
        grid.Add(comboFromFrameRate, row, 1);
        grid.Add(buttonFromFrameRate, row, 2);
        grid.Add(buttonSwitch, row, 3, 2);
        row++;

        grid.Add(labelToFrameRate, row, 0);
        grid.Add(comboToFrameRate, row, 1);
        grid.Add(buttonToFrameRate, row, 2);
        row++;

        grid.Add(buttonPanel, row, 0, 1, 4);

        Content = grid;
        
        Activated += delegate { comboFromFrameRate.Focus(); }; // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
