using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Tools.AdjustDuration;

/// <summary>
/// Adjust durations, in the editor look (see <see cref="EditorChrome"/>): how to adjust and the
/// value(s) for the chosen way in one section, the overlap note under it, Cancel/OK at the bottom.
/// </summary>
public class AdjustDurationWindow : Window
{
    private const double LabelWidth = 200;
    // The value field lines up with the "Adjust via" box above it
    private const double FieldWidth = 180;

    public AdjustDurationWindow(AdjustDurationViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Tools.AdjustDurations.Title);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var combo = new ComboBox
        {
            ItemsSource = vm.AdjustTypes,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = FieldWidth,
        };
        combo.Bind(SelectingItemsControl.SelectedValueProperty, new Binding
        {
            Path = nameof(vm.SelectedAdjustType),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        AutomationProperties.SetName(combo, Se.Language.Tools.AdjustDurations.AdjustVia);

        var panelSeconds = MakeAdjustSeconds(vm);
        var panelPercent = MakeAdjustPercent(vm);
        var panelFixed = MakeAdjustFixed(vm);
        var panelRecalculate = MakeAdjustRecalculate(vm);

        // Only one of the four is visible at a time - the one for the chosen way to adjust.
        var panelValues = new Panel { Children = { panelSeconds, panelPercent, panelFixed, panelRecalculate } };

        var labelInfo = new TextBlock
        {
            Text = Se.Language.Tools.AdjustDurations.Note,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 360,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var rows = new StackPanel { Spacing = 10 };
        rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.Tools.AdjustDurations.AdjustVia, combo, LabelWidth));
        rows.Children.Add(panelValues);
        rows.Children.Add(labelInfo);
        var section = EditorChrome.MakeEditorSection(Se.Language.General.Adjustment, rows, first: true);
        section.MinWidth = 440;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, combo); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static NumericUpDown MakeNumber(string label, decimal minimum, decimal maximum)
    {
        var numericUpDown = new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
            Width = FieldWidth,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        numericUpDown.Classes.Add("field");
        AutomationProperties.SetName(numericUpDown, label);
        return numericUpDown;
    }

    private static Grid MakeAdjustSeconds(AdjustDurationViewModel vm)
    {
        var numericUpDownSeconds = MakeNumber(Se.Language.General.Seconds, -1000000, 1000000);
        numericUpDownSeconds.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustSeconds),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new DoubleToThreeDecimalConverter(),
        });

        var panel = EditorChrome.MakeLabeledRow(Se.Language.General.Seconds, numericUpDownSeconds, LabelWidth);
        panel.Bind(IsVisibleProperty, new Binding()
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsSecondsVisible)}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });

        return panel;
    }

    private static Grid MakeAdjustPercent(AdjustDurationViewModel vm)
    {
        var numericUpDownPercent = MakeNumber(Se.Language.General.Percent, -1000000, 1000000);
        numericUpDownPercent.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustPercent),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new NullableIntConverter(),
        });

        var panel = EditorChrome.MakeLabeledRow(Se.Language.General.Percent, numericUpDownPercent, LabelWidth);
        panel.Bind(IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsPercentVisible)}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });

        return panel;
    }

    private static Grid MakeAdjustFixed(AdjustDurationViewModel vm)
    {
        var numericUpDownFixed = MakeNumber(Se.Language.Tools.AdjustDurations.Fixed, -1000000, 1000000);
        numericUpDownFixed.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Mode = BindingMode.TwoWay,
            Source = vm,
            Path = nameof(vm.AdjustFixed),
            Converter = new DoubleToThreeDecimalConverter(),
        });

        var panel = EditorChrome.MakeLabeledRow(Se.Language.Tools.AdjustDurations.Fixed, numericUpDownFixed, LabelWidth);
        panel.Bind(IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsFixedVisible)}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });
        return panel;
    }

    private static StackPanel MakeAdjustRecalculate(AdjustDurationViewModel vm)
    {
        var numericUpDownMax = MakeNumber(Se.Language.General.MaxCharactersPerSecond, 0, 1000);
        numericUpDownMax.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustRecalculateMaxCharacterPerSecond),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new DoubleToOneDecimalConverter(),
        });

        var numericUpDownOptimal = MakeNumber(Se.Language.General.OptimalCharactersPerSecond, 0, 1000);
        numericUpDownOptimal.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustRecalculateOptimalCharacterPerSecond),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new DoubleToOneDecimalConverter(),
        });

        var toggleExtendOnly = new ToggleSwitch
        {
            OnContent = null,
            OffContent = null,
            MinWidth = 0,
            VerticalAlignment = VerticalAlignment.Center,
        };
        toggleExtendOnly.Bind(ToggleButton.IsCheckedProperty, new Binding
        {
            Path = nameof(vm.ExtendOnly),
            Mode = BindingMode.TwoWay,
            Source = vm,
        });
        AutomationProperties.SetName(toggleExtendOnly, Se.Language.Tools.AdjustDurations.ExtendOnly);
        var rowExtendOnly = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                toggleExtendOnly,
                new TextBlock { Text = Se.Language.Tools.AdjustDurations.ExtendOnly, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center },
            },
        };

        var panel = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.MaxCharactersPerSecond, numericUpDownMax, LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.OptimalCharactersPerSecond, numericUpDownOptimal, LabelWidth),
                EditorChrome.MakeLabeledRow(string.Empty, rowExtendOnly, LabelWidth),
            },
        };

        panel.Bind(IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(AdjustDurationDisplay.IsRecalculateVisible)}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });

        return panel;
    }
}
