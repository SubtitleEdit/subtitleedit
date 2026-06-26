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

        var buttonShowEarlier = UiUtil.MakeButton(Se.Language.Sync.ShowEarlier, vm.ShowEarlierCommand)
            .WithMinWidth(110);
        buttonShowEarlier.Margin = new Thickness(0, 0, 0, 10);

        var buttonShowLater = UiUtil.MakeButton(Se.Language.Sync.ShowLater, vm.ShowLaterCommand)
            .WithMinWidth(110);

        var panelShowButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                buttonShowEarlier,
                buttonShowLater,
            },
        };

        var panelRadioButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(timeCodeUpDown,    0, 0);
        grid.Add(panelShowButtons,  0, 1, 2, 1);
        grid.Add(panelRadioButtons, 1, 0);
        grid.Add(labelStatus,       2, 0);
        grid.Add(panelButtons,      2, 1);

        Content = grid;

        Loaded += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
            UiUtil.RestoreWindowPosition(this);
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
        Closing += (_, e) => vm.OnClosing(e);
    }
}
