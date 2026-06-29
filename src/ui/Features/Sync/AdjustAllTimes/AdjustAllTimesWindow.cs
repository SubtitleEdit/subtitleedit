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

        // Independent extend: grow each line's duration by moving the start earlier
        // and/or the end later, leaving the opposite edge fixed. Uses the same All /
        // Selected / Selected-and-forward scope as the show earlier/later buttons above.
        var timeCodeExtendStart = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.ExtendStartEarlier))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var timeCodeExtendEnd = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.ExtendEndLater))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var rowExtendStart = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Label { Content = Se.Language.Sync.ExtendStartEarlier, Width = 120 },
                timeCodeExtendStart,
            },
        };

        var rowExtendEnd = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new Label { Content = Se.Language.Sync.ExtendEndLater, Width = 120 },
                timeCodeExtendEnd,
            },
        };

        var buttonExtend = UiUtil.MakeButton(Se.Language.Sync.Extend, vm.ExtendCommand)
            .WithMinWidth(110);

        var panelExtend = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Children =
            {
                new Label { Content = Se.Language.Sync.ExtendDuration },
                rowExtendStart,
                rowExtendEnd,
                buttonExtend,
            },
        };

        var borderExtend = UiUtil.MakeBorderForControl(panelExtend);

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
        grid.Add(borderExtend,      2, 0, 1, 2);
        grid.Add(labelStatus,       3, 0);
        grid.Add(panelButtons,      3, 1);

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
