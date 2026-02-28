using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinarySettings;

public class BinarySettingsWindow : Window
{
    public BinarySettingsWindow(BinarySettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.Settings;
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var labelMarginLeft = new TextBlock
        {
            Text = Se.Language.General.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownMarginLeft = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownMarginLeft.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.MarginLeft),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });

        var labelMarginTop = new TextBlock
        {
            Text = "Top",
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownMarginTop = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownMarginTop.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.MarginTop),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });

        var labelMarginRight = new TextBlock
        {
            Text = Se.Language.General.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownMarginRight = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownMarginRight.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.MarginRight),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });

        var labelMarginBottom = new TextBlock
        {
            Text = "Bottom",
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownMarginBottom = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Width = 150,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownMarginBottom.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.MarginBottom),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });

        grid.Add(labelMarginLeft, 0, 0);
        grid.Add(numericUpDownMarginLeft, 0, 1);

        grid.Add(labelMarginTop, 1, 0);
        grid.Add(numericUpDownMarginTop, 1, 1);

        grid.Add(labelMarginRight, 2, 0);
        grid.Add(numericUpDownMarginRight, 2, 1);

        grid.Add(labelMarginBottom, 3, 0);
        grid.Add(numericUpDownMarginBottom, 3, 1);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        grid.Add(panelButtons, 4, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
