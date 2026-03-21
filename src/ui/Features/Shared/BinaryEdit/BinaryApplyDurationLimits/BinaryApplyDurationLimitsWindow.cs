using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryApplyDurationLimits;

public class BinaryApplyDurationLimitsWindow : Window
{
    public BinaryApplyDurationLimitsWindow(BinaryApplyDurationLimitsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ApplyDurationLimits.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

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
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        // Minimum duration
        var minimumLabel = new Label
        {
            Content = "Minimum duration (milliseconds):", // TODO: Add to language resources
            VerticalAlignment = VerticalAlignment.Center,
        };
        grid.Add(minimumLabel, 0, 0);

        var minimumNumeric = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 10000,
            Increment = 100,
            FormatString = "F0",
            Width = 150,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.MinimumDurationMs))
            {
                Mode = BindingMode.TwoWay,
            }
        };
        grid.Add(minimumNumeric, 1, 0);

        // Maximum duration
        var maximumLabel = new Label
        {
            Content = "Maximum duration (milliseconds):", // TODO: Add to language resources
            VerticalAlignment = VerticalAlignment.Center,
        };
        grid.Add(maximumLabel, 0, 1);

        var maximumNumeric = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 100000,
            Increment = 100,
            FormatString = "F0",
            Width = 150,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.MaximumDurationMs))
            {
                Mode = BindingMode.TwoWay,
            }
        };
        grid.Add(maximumNumeric, 1, 1);

        // Radio buttons panel
        var radioPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Margin = new Thickness(0, 10, 0, 0),
        };

        var applyToAllRadio = new RadioButton
        {
            Content = "Apply to all", // TODO: Add Se.Language.General.ApplyToAll
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.ApplyToAll))
            {
                Mode = BindingMode.TwoWay,
            }
        };
        radioPanel.Children.Add(applyToAllRadio);

        var applyToSelectedRadio = new RadioButton
        {
            Content = "Apply to selected lines", // TODO: Add Se.Language.General.ApplyToSelected
            [!RadioButton.IsCheckedProperty] = new Binding(nameof(vm.ApplyToSelectedLines))
            {
                Mode = BindingMode.TwoWay,
            }
        };
        radioPanel.Children.Add(applyToSelectedRadio);

        grid.Add(radioPanel, 2, 0, 1, 2);

        // Button panel
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);
        grid.Add(buttonPanel, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}

