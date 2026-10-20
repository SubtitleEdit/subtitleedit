using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustDuration;

/// <summary>
/// Adjust durations of image-based subtitles, in the editor look (see <see cref="EditorChrome"/>):
/// how to adjust and the value(s) for the chosen way in one section, the notes under it, Cancel/OK
/// at the bottom.
/// </summary>
public class BinaryAdjustDurationWindow : Window
{
    private const double LabelWidth = 200;
    // The value field lines up with the "Adjust via" box above it
    private const double NumericUpDownWidth = 180;

    public BinaryAdjustDurationWindow(BinaryAdjustDurationViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.AdjustDurations.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var combo = new ComboBox
        {
            ItemsSource = vm.AdjustTypes,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Width = NumericUpDownWidth,
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
            HorizontalAlignment = HorizontalAlignment.Left,
            Text = Se.Language.Tools.AdjustDurations.Note,
            FontSize = 12,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 380,
        };
        labelInfo.Bind(IsVisibleProperty, new Binding(nameof(vm.ShowAdjustNote)) { Source = vm });

        var labelRecalculateNote = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Text = Se.Language.Tools.AdjustDurations.RecalculateRequiresOcrNote,
            FontSize = 12,
            Foreground = Brushes.OrangeRed,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 380,
        };
        labelRecalculateNote.Bind(IsVisibleProperty, new Binding(nameof(vm.IsRecalculateBlocked)) { Source = vm });

        var rows = new StackPanel { Spacing = 10 };
        rows.Children.Add(BinaryEditChrome.MakeRow(Se.Language.Tools.AdjustDurations.AdjustVia, combo, LabelWidth));
        rows.Children.Add(panelValues);
        rows.Children.Add(labelInfo);
        rows.Children.Add(labelRecalculateNote);
        var section = EditorChrome.MakeEditorSection(Se.Language.General.Adjustment, rows, first: true);
        section.MinWidth = 440;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, combo); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static NumericUpDown MakeNumber(string label, decimal minimum, decimal maximum)
    {
        var numericUpDown = new NumericUpDown
        {
            Minimum = minimum,
            Maximum = maximum,
        };
        return EditorChrome.AsField(numericUpDown, label, NumericUpDownWidth, alignLeft: true, center: true);
    }

    private static Grid MakeAdjustSeconds(BinaryAdjustDurationViewModel vm)
    {
        var numericUpDownSeconds = MakeNumber(Se.Language.General.Seconds, -1000000, 1000000);
        numericUpDownSeconds.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustSeconds),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new DoubleToThreeDecimalConverter(),
        });

        var panel = BinaryEditChrome.MakeRow(Se.Language.General.Seconds, numericUpDownSeconds, LabelWidth);
        panel.Bind(IsVisibleProperty, new Binding()
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(BinaryAdjustDurationDisplay.IsSecondsVisible)}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });

        return panel;
    }

    private static Grid MakeAdjustPercent(BinaryAdjustDurationViewModel vm)
    {
        var numericUpDownPercent = MakeNumber(Se.Language.General.Percent, -1000000, 1000000);
        numericUpDownPercent.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Path = nameof(vm.AdjustPercent),
            Mode = BindingMode.TwoWay,
            Source = vm,
            Converter = new NullableIntConverter(),
        });

        var panel = BinaryEditChrome.MakeRow(Se.Language.General.Percent, numericUpDownPercent, LabelWidth);
        panel.Bind(IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(BinaryAdjustDurationDisplay.IsPercentVisible)}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });

        return panel;
    }

    private static Grid MakeAdjustFixed(BinaryAdjustDurationViewModel vm)
    {
        var numericUpDownFixed = MakeNumber(Se.Language.Tools.AdjustDurations.Fixed, -1000000, 1000000);
        numericUpDownFixed.Bind(NumericUpDown.ValueProperty, new Binding
        {
            Mode = BindingMode.TwoWay,
            Source = vm,
            Path = nameof(vm.AdjustFixed),
            Converter = new DoubleToThreeDecimalConverter(),
        });

        var panel = BinaryEditChrome.MakeRow(Se.Language.Tools.AdjustDurations.Fixed, numericUpDownFixed, LabelWidth);
        panel.Bind(IsVisibleProperty, new Binding
        {
            Path = $"{nameof(vm.SelectedAdjustType)}.{nameof(BinaryAdjustDurationDisplay.IsFixedVisible)}",
            Source = vm,
            Mode = BindingMode.OneWay,
        });
        return panel;
    }

    private static StackPanel MakeAdjustRecalculate(BinaryAdjustDurationViewModel vm)
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

        var panel = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                BinaryEditChrome.MakeRow(Se.Language.General.MaxCharactersPerSecond, numericUpDownMax, LabelWidth),
                BinaryEditChrome.MakeRow(Se.Language.General.OptimalCharactersPerSecond, numericUpDownOptimal, LabelWidth),
            },
        };

        panel.Bind(IsVisibleProperty, new Binding(nameof(vm.ShowRecalculateControls)) { Source = vm });

        return panel;
    }
}
