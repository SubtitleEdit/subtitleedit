using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert.FunctionViews;

/// <summary>
/// The editor look of the function settings shown in the "Settings" pane of the batch convert
/// window: a small caps header with the function name, label/field rows with one label column,
/// compact fields and switches for on/off options.
/// </summary>
internal static class FunctionViewChrome
{
    /// <summary>Width of the label column, so the fields of every function line up.</summary>
    internal const double LabelWidth = 190;

    /// <summary>Width of a compact number field.</summary>
    internal const double FieldWidth = 100;

    /// <summary>Indent of options that belong to the switch or radio button above them.</summary>
    internal const double Indent = 28;

    /// <summary>
    /// The function's settings: the header followed by <paramref name="rows"/>. The view model is
    /// the data context, so the bindings also resolve before the view is hosted in the window.
    /// </summary>
    internal static StackPanel MakePanel(BatchConvertViewModel vm, string title, params Control[] rows)
    {
        var panel = new StackPanel
        {
            Spacing = 10,
            DataContext = vm,
        };
        var header = EditorChrome.MakeSectionHeader(title);
        header.Margin = new Thickness(0, 0, 0, 2);
        panel.Children.Add(header);
        foreach (var row in rows)
        {
            panel.Children.Add(row);
        }

        FitToViewport(panel);
        return panel;
    }

    /// <summary>
    /// The settings pane scrolls horizontally, so it measures its content with unlimited width and
    /// hint texts never wrapped - they ran off to the right. Keep the panel to the visible width,
    /// so text wraps and stretched fields end at the pane edge.
    /// </summary>
    private static void FitToViewport(Control panel)
    {
        ScrollViewer? scrollViewer = null;

        void Update()
        {
            // The viewport includes the scroll viewer's padding
            var width = scrollViewer == null
                ? 0
                : scrollViewer.Viewport.Width - scrollViewer.Padding.Left - scrollViewer.Padding.Right;
            panel.MaxWidth = width > 0 ? width : double.PositiveInfinity;
        }

        void OnScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ScrollViewer.ViewportProperty)
            {
                Update();
            }
        }

        panel.AttachedToVisualTree += (_, _) =>
        {
            scrollViewer = panel.FindAncestorOfType<ScrollViewer>();
            if (scrollViewer != null)
            {
                scrollViewer.PropertyChanged += OnScrollViewerPropertyChanged;
            }

            Update();
        };
        panel.DetachedFromVisualTree += (_, _) =>
        {
            if (scrollViewer != null)
            {
                scrollViewer.PropertyChanged -= OnScrollViewerPropertyChanged;
            }

            scrollViewer = null;
        };
    }

    /// <summary>
    /// "Label  [content]" with the label in the shared label column; the label wraps.
    /// </summary>
    internal static Grid MakeRow(string label, Control content, string? visibleProperty = null)
    {
        return EditorChrome.MakeLabeledRow(label, content, LabelWidth, wrap: true, visibleProperty: visibleProperty);
    }

    /// <summary>
    /// A compact whole-number field (no spinner) bound to an int property; keeps the
    /// <see cref="UiUtil.MakeNumericUpDownInt"/> nullable int conversion and mouse wheel handling.
    /// </summary>
    internal static NumericUpDown MakeIntField(BatchConvertViewModel vm, string property, int minimum, int maximum, int defaultValue, string accessibleName)
    {
        var field = UiUtil.MakeNumericUpDownInt(minimum, maximum, defaultValue, FieldWidth, vm, property);
        return Compact(field, accessibleName);
    }

    /// <summary>
    /// Gives a number field the compact "field" look and its accessible name.
    /// </summary>
    internal static NumericUpDown Compact(NumericUpDown field, string accessibleName)
    {
        return field.AsField(accessibleName, FieldWidth, alignLeft: true, center: true);
    }

    /// <summary>
    /// A compact combo box with its accessible name.
    /// </summary>
    internal static ComboBox Compact(ComboBox comboBox, string accessibleName, double minWidth = 200)
    {
        comboBox.MinWidth = minWidth;
        comboBox.HorizontalAlignment = HorizontalAlignment.Left;
        comboBox.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboBox, accessibleName);
        return comboBox;
    }

    /// <summary>
    /// A radio button in the size of the other option labels.
    /// </summary>
    internal static RadioButton Compact(RadioButton radioButton)
    {
        radioButton.FontSize = EditorChrome.FieldFontSize;
        radioButton.VerticalAlignment = VerticalAlignment.Center;
        radioButton.Margin = new Thickness(0);
        if (radioButton.Content is string text)
        {
            AutomationProperties.SetName(radioButton, text);
        }

        return radioButton;
    }

    /// <summary>
    /// An on/off switch with its label and a control that belongs to it (the value it turns on)
    /// at the end of the row, lined up with the fields of the label rows.
    /// </summary>
    internal static Grid MakeSwitchRow(string property, string text, Control trailing)
    {
        var left = EditorChrome.MakeSwitchRow(property, text);
        trailing.HorizontalAlignment = HorizontalAlignment.Left;
        trailing.VerticalAlignment = VerticalAlignment.Center;
        return MakeRowWithControl(left, trailing);
    }

    /// <summary>
    /// <paramref name="left"/> in the label column, <paramref name="content"/> in the field column.
    /// </summary>
    internal static Grid MakeRowWithControl(Control left, Control content)
    {
        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(left, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    /// <summary>
    /// A button with an icon and its text, for opening a function's own settings window.
    /// </summary>
    internal static Button MakeButton(string? icon, string text, System.Windows.Input.ICommand? command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7 };
        if (icon != null)
        {
            content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 13, VerticalAlignment = VerticalAlignment.Center });
        }

        content.Children.Add(new TextBlock { Text = text, FontSize = EditorChrome.FieldFontSize, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            MinHeight = EditorChrome.ScaledSize(30),
            Padding = new Thickness(12, 4),
            Command = command,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            CornerRadius = new CornerRadius(6),
        };

        // Icon + label in a panel: a screen reader needs the name spelled out (#12087)
        AutomationProperties.SetName(button, text);
        return button;
    }

    /// <summary>
    /// Moves <paramref name="control"/> in under the switch or radio button it belongs to.
    /// </summary>
    internal static T Indented<T>(T control) where T : Control
    {
        control.Margin = new Thickness(Indent, control.Margin.Top, control.Margin.Right, control.Margin.Bottom);
        return control;
    }
}
