using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAllTimes;

public class BinaryAdjustAllTimesWindow : Window
{
    public BinaryAdjustAllTimesWindow(BinaryAdjustAllTimesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.AdjustAllTimes;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.General.Adjustment,
        };

        var timeCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.Adjustment))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var panelAdjustment = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                label,
                timeCodeUpDown,
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
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLines)),
                    [!RadioButton.IsEnabledProperty] = new Binding(nameof(vm.IsSelectionAvailable)),
                },
                new RadioButton
                {
                    Content = Se.Language.Sync.AdjustSelectedLinesAndForward,
                    [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.AdjustSelectedLinesAndForward)),
                    [!RadioButton.IsEnabledProperty] = new Binding(nameof(vm.IsSelectionAvailable)),
                }
            },
        };

        var totalAdjustmentLabel = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TotalAdjustmentInfo)),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttonShowEarlier = UiUtil.MakeButton(Se.Language.Sync.ShowEarlier, vm.ShowEarlierCommand)
            .WithLeftAlignment()
            .WithMargin(0, 0, 0, 10);
        var buttonShowLater = UiUtil.MakeButton(Se.Language.Sync.ShowLater, vm.ShowLaterCommand)
            .WithLeftAlignment();

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

        var buttonOk = UiUtil.MakeButtonDone(vm.OkCommand);

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
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelAdjustment, 0, 0);
        grid.Add(panelRadioButtons, 1, 0);
        grid.Add(totalAdjustmentLabel, 2, 0);
        grid.Add(panelShowButtons, 0, 1, 2, 1);
        grid.Add(buttonOk, 2, 1);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
