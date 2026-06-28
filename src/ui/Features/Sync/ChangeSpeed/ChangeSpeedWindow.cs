using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Sync.ChangeSpeed;

public class ChangeSpeedWindow : Window
{

    public ChangeSpeedWindow(ChangeSpeedViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChangeSpeed;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelSpeed = new Label
        {
            Content = Se.Language.Sync.SpeedInPercentage,
        };

        var numericUpDownSpeed = new NumericUpDown
        {
            Width = 150,
            HorizontalAlignment = HorizontalAlignment.Left,
            Minimum = 1, // a speed of 0% means a 100/0 factor (divide by zero / infinite times)
            Maximum = 1000,
            Increment = 0.1m,
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.SpeedPercent))
            {
                Mode = BindingMode.TwoWay,
                Converter = new DoubleToFourDecimalConverter(),
            },
        };

        var buttonFromDropFrame = UiUtil.MakeButton(Se.Language.Sync.FromDropFrameValue, vm.SetFromDropFrameValueCommand);
        buttonFromDropFrame.HorizontalAlignment = HorizontalAlignment.Stretch;
        var buttonToDropFrame = UiUtil.MakeButton(Se.Language.Sync.ToDropFrameValue, vm.SetToDropFrameValueCommand);
        buttonToDropFrame.HorizontalAlignment = HorizontalAlignment.Stretch;

        var panelFromToButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Top,
            Spacing = 10,
            Children =
            {
                buttonFromDropFrame,
                buttonToDropFrame,
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

        var buttonOk = UiUtil.MakeButton(Se.Language.General.Change, vm.OkCommand);
        var buttonApply = UiUtil.MakeButton(Se.Language.General.Apply, vm.ApplyCommand);
        var buttonDone = UiUtil.MakeButtonDone(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonApply, buttonDone);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(labelSpeed,         0, 0);
        grid.Add(numericUpDownSpeed, 1, 0);
        grid.Add(panelFromToButtons, 1, 1, 2, 1);
        grid.Add(panelRadioButtons,  2, 0);
        grid.Add(buttonPanel,        3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
