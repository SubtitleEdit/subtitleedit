using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

public class AdjustAllTimesWindow : Window
{
    public AdjustAllTimesWindow(AdjustAllTimesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.AdjustAllTimes;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var timeCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.Adjustment))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var gridAdjustment = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
            RowSpacing = 5,
        };

        gridAdjustment.Add(timeCodeUpDown, 0, 1);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowEarlier, vm.ShowEarlierCommand), 0, 2);
        gridAdjustment.Add(UiUtil.MakeButton(Se.Language.Sync.ShowLater, vm.ShowLaterCommand), 0, 3);

        var panelRadioButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 10, 0, 0),
            Children =
            {
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustAll,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustAll))
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLines,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLines))
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLinesAndForward,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLinesAndForward))
                }
            },
        };

        var buttonHelp = UiUtil.MakeButton(vm.ShowHelpCommand, IconNames.Help, Se.Language.Sync.AdjustAllShortcuts);
        var buttonOk = UiUtil.MakeButtonDone(vm.OkCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonHelp, buttonOk);
        
        var labelStatus = UiUtil.MakeLabel().WithBindText(vm, nameof(vm.StatusText));
        
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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(gridAdjustment, 0);
        grid.Add(panelRadioButtons, 1);
        grid.Add(labelStatus, 2);
        grid.Add(panelButtons, 2);


        Content = grid;

        Loaded += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += (_, e) => vm.OnClosing(e);
    }
}