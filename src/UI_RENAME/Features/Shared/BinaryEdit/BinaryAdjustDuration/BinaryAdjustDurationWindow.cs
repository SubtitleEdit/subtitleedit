using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustDuration;

public class BinaryAdjustDurationWindow : Window
{
    private const int LabelMinWidth = 100;
    private const int NumericUpDownWidth = 150;

    public BinaryAdjustDurationWindow(BinaryAdjustDurationViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.AdjustDurations.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var label = new Label
        {
            Content = Se.Language.Tools.AdjustDurations.AdjustVia,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var combo = new ComboBox
        {
            ItemsSource = vm.AdjustTypes,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = 180,
        };
        combo.Bind(SelectingItemsControl.SelectedValueProperty, new Binding
        {
            Path = nameof(vm.SelectedAdjustType),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });

        var panelSeconds = MakeAdjustSeconds(vm);
        var panelPercent = MakeAdjustPercent(vm);
        var panelFixed = MakeAdjustFixed(vm);
        var panelRecalculate = MakeAdjustRecalculate(vm);

        var labelInfo = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Left,
            Text = Se.Language.Tools.AdjustDurations.Note,
            Margin = new Thickness(10, 25, 10, 15),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
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

        grid.Add(label, 0);
        grid.Add(combo, 0, 1);

        grid.Add(panelSeconds, 1, 0, 1, 2);
        grid.Add(panelPercent, 1, 0, 1, 2);
        grid.Add(panelFixed, 1, 0, 1, 2);
        grid.Add(panelRecalculate, 1, 0, 1, 2);

        grid.Add(labelInfo, 2, 0, 1, 2);

        grid.Add(panelButtons, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel MakeAdjustSeconds(BinaryAdjustDurationViewModel vm)
    {
        var textBlockSeconds = new TextBlock
        {
            Text = Se.Language.General.Seconds,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = LabelMinWidth,
        };
        var numericUpDownSeconds = new NumericUpDown
        {
            Minimum = -1000000,
            Maximum = 1000000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownSeconds.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustSeconds),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new DoubleToThreeDecimalConverter(),
        });

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 25, 0, 0),
            Children =
            {
                textBlockSeconds,
                numericUpDownSeconds
            }
        };
        panel.Bind(StackPanel.IsVisibleProperty, new Binding()
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(BinaryAdjustDurationDisplay.IsSecondsVisible)}",
            Source = vm,
            Mode = BindingMode.TwoWay,
        });

        return panel;
    }

    private static StackPanel MakeAdjustPercent(BinaryAdjustDurationViewModel vm)
    {
        var textBlockSeconds = new TextBlock
        {
            Text = Se.Language.General.Percent,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = LabelMinWidth,
        };
        var numericUpDownSeconds = new NumericUpDown
        {
            Minimum = -1000000,
            Maximum = 1000000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownSeconds.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustPercent),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new NullableIntConverter(),
        });

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 25, 0, 0),
            Children =
            {
                textBlockSeconds,
                numericUpDownSeconds
            }
        };
        panel.Bind(StackPanel.IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(BinaryAdjustDurationDisplay.IsPercentVisible)}",
            Source = vm,
            Mode = BindingMode.TwoWay,
        });

        return panel;
    }


    private static StackPanel MakeAdjustFixed(BinaryAdjustDurationViewModel vm)
    {
        var textBlockSeconds = new TextBlock
        {
            Text = Se.Language.Tools.AdjustDurations.Fixed,
            VerticalAlignment = VerticalAlignment.Center,
            MinWidth = LabelMinWidth,
        };
        var numericUpDownSeconds = new NumericUpDown
        {
            Minimum = -1000000,
            Maximum = 1000000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDownSeconds.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Mode = BindingMode.TwoWay,
            Source = vm,
            Path = nameof(vm.AdjustFixed),
            Converter = new DoubleToThreeDecimalConverter(),
        });

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 25, 0, 0),
            Children =
            {
                textBlockSeconds,
                numericUpDownSeconds
            }
        };
        panel.Bind(StackPanel.IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(BinaryAdjustDurationDisplay.IsFixedVisible)}",
            Source = vm,
            Mode = BindingMode.TwoWay,
        });
        return panel;
    }

    private static Grid MakeAdjustRecalculate(BinaryAdjustDurationViewModel vm)
    {
        var textBlockMax = new TextBlock
        {
            Text = Se.Language.General.MaxCharactersPerSecond,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownMax = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };
        numericUpDownMax.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustRecalculateMaxCharacterPerSecond),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new DoubleToOneDecimalConverter(),
        });

        var textBlockOptimal = new TextBlock
        {
            Text = Se.Language.General.OptimalCharactersPerSecond,
            VerticalAlignment = VerticalAlignment.Center,
        };
        var numericUpDownOptimal = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 1000,
            Width = NumericUpDownWidth,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };
        numericUpDownOptimal.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustRecalculateOptimalCharacterPerSecond),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new DoubleToOneDecimalConverter(),
        });

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            RowSpacing = 10,
            Margin = new Thickness(10, 25, 0, 0),
        };

        grid.Children.Add(textBlockMax);
        Grid.SetColumn(textBlockMax, 0);

        grid.Children.Add(numericUpDownMax);
        Grid.SetColumn(numericUpDownMax, 1);

        grid.Children.Add(textBlockOptimal);
        Grid.SetColumn(textBlockOptimal, 0);
        Grid.SetRow(textBlockOptimal, 1);

        grid.Children.Add(numericUpDownOptimal);
        Grid.SetColumn(numericUpDownOptimal, 1);
        Grid.SetRow(numericUpDownOptimal, 1);

        grid.Bind(Grid.IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(BinaryAdjustDurationDisplay.IsRecalculateVisible)}",
            Source = vm,
            Mode = BindingMode.TwoWay,
        });

        return grid;
    }
}
