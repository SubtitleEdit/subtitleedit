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

        var label = new Label
        {
            Content = Se.Language.Sync.SpeedInPercentage,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var numericUpDownSpeed = new NumericUpDown
        {
            Width = 150,
            Margin = new Thickness(0, 0, 10, 0),
            Minimum = 0,
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
        var buttonToDropFrame = UiUtil.MakeButton(Se.Language.Sync.ToDropFrameValue, vm.SetToDropFrameValueCommand);

        var panelSpeed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 10, 0),
            Children =
            {
                label,
                numericUpDownSpeed,
                buttonFromDropFrame,
                buttonToDropFrame
            }
        };

        var panelRadioButtons = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(50, 10, 0, 0),
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelSpeed, 0);
        grid.Add(panelRadioButtons, 1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
