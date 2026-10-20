using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsItem
{
    private readonly string _label;
    private readonly Func<Control> _controlFactory;
    private readonly object? _labelBindingSource;
    private readonly string? _labelBindingPath;
    public bool IsVisible { get; private set; } = true;
    public string? IsVisibleBinding { get; set; }
    public bool IsHidden { get; set; } = false;
    public bool IsFullWidth { get; init; }

    /// <summary>
    /// An on/off setting: the control is a switch, shown before its label instead of after it.
    /// </summary>
    public bool IsSwitch { get; init; }

    /// <summary>
    /// A control that belongs to a switch (the "Edit" button of its settings), shown in the
    /// field column after the switch and its label.
    /// </summary>
    public Func<Control>? TrailingFactory { get; init; }

    /// <summary>
    /// A thin line between groups of settings, not a setting.
    /// </summary>
    public bool IsSeparator { get; init; }

    /// <summary>
    /// English text the search also matches, for a label not found as one translated string
    /// (e.g. built from several). Otherwise looked up via <see cref="EnglishLabelLookup"/>.
    /// </summary>
    public string? EnglishLabel { get; init; }

    public SettingsItem(string label, Func<Control> controlFactory, string? isVisibleBinding = null, bool isHidden = false)
    {
        _label = label;
        _controlFactory = controlFactory;
        IsVisibleBinding = isVisibleBinding;
        IsHidden = isHidden;
    }

    public SettingsItem(bool isHidden, string label, Func<Control> controlFactory)
    {
        _label = label;
        _controlFactory = controlFactory;
        IsVisibleBinding = null;
        IsHidden = isHidden;
    }

    public SettingsItem(string label, object labelBindingSource, string labelBindingPath, Func<Control> controlFactory)
    {
        _label = label;
        _controlFactory = controlFactory;
        _labelBindingSource = labelBindingSource;
        _labelBindingPath = labelBindingPath;
    }

    public void Filter(string filter)
    {
        if (IsHidden)
        {
            IsVisible = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(filter))
        {
            IsVisible = true;
            return;
        }

        if (_label != null && _label.Contains(filter, StringComparison.OrdinalIgnoreCase))
        {
            IsVisible = true;
            return;
        }

        // With a translated UI, also match the English name used in docs and tutorials (#15945).
        var english = EnglishLabel ?? EnglishLabelLookup.GetEnglish(_label);
        IsVisible = english != null && english.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    public Control Build(bool includeLabel = true)
    {
        if (!includeLabel)
        {
            var bare = _controlFactory();
            SettingsChrome.Compact(bare);
            return bare;
        }

        if (IsSeparator)
        {
            return new Border
            {
                Height = 1,
                Background = EditorChrome.LineBrush,
                Margin = new Thickness(0, 4),
            };
        }

        var labelTextBlock = IsSwitch
            ? new TextBlock
            {
                Text = _label,
                FontSize = EditorChrome.FieldFontSize,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
            }
            : SettingsChrome.MakeLabel(_label);

        if (_labelBindingPath != null && _labelBindingSource != null)
        {
            labelTextBlock.Bind(TextBlock.TextProperty, new Binding(_labelBindingPath)
            {
                Source = _labelBindingSource,
            });
        }

        var control = _controlFactory();
        SettingsChrome.Compact(control);

        // Associate the row's label with its input control so screen readers announce the label as the
        // control's name. Without this the settings controls (numeric fields, combo boxes, check boxes,
        // ...) are exposed to UI Automation as unnamed/generic elements and NVDA cannot identify them
        // (issue #11745). A live LabeledBy link also keeps the name correct for bound/localized labels.
        if (!string.IsNullOrEmpty(_label) || _labelBindingPath != null)
        {
            // A colour swatch button (content is a Border) needs an explicit name rather than
            // a link - see AccessibleLabels.LinkToLabel.
            AccessibleLabels.LinkToLabel(control, labelTextBlock);

            // When the factory returns a wrapper (a numeric field plus a browse button, a text
            // box with a hint, a check box plus an edit button, a bordered grid of sub-settings,
            // ...) the link above lands on the wrapper, not on the input a screen reader user
            // actually focuses - label the unnamed inputs and content buttons inside it too
            // (#12087). A control with its own label inside the wrapper (a grid row "Outline:
            // [swatch]") gets that label; the rest get the item's.
            if (control is Panel or Decorator)
            {
                foreach (var input in control.GetLogicalDescendants().OfType<Control>().ToList())
                {
                    if ((AccessibleLabels.IsInput(input) || AccessibleLabels.IsContentButton(input) || input is ToggleSwitch)
                        && input.TemplatedParent == null && !AccessibleLabels.HasAccessibleName(input))
                    {
                        AccessibleLabels.LinkToLabel(input, AccessibleLabels.FindLabel(input) ?? labelTextBlock);
                    }
                }
            }
        }

        Grid row;
        if (IsSwitch)
        {
            // "[switch] Label" - with a trailing control (an Edit button) the switch and its
            // label take the label column and the control lines up with the fields.
            var switchAndLabel = new Grid
            {
                ColumnSpacing = 10,
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
            };
            switchAndLabel.Add(control, 0, 0);
            switchAndLabel.Add(labelTextBlock, 0, 1);

            SettingsChrome.DimLabelWhenDisabled(control, labelTextBlock);

            if (TrailingFactory == null)
            {
                row = switchAndLabel;
            }
            else
            {
                var trailing = TrailingFactory();
                SettingsChrome.Compact(trailing);
                trailing.HorizontalAlignment = HorizontalAlignment.Left;
                trailing.VerticalAlignment = VerticalAlignment.Center;
                row = MakeColumns(switchAndLabel, trailing);
            }
        }
        else
        {
            if (control.VerticalAlignment == VerticalAlignment.Stretch)
            {
                control.VerticalAlignment = VerticalAlignment.Center;
            }

            // A wide combo box or text box shrinks with a narrow window instead of being cut off
            // at the right edge - its width becomes the most it takes.
            var field = control is ComboBox or TextBox && !double.IsNaN(control.Width)
                ? SettingsChrome.ShrinkToFit(control)
                : control;

            // A control of fixed width would be centered in the field column
            if (control.HorizontalAlignment == HorizontalAlignment.Stretch && !double.IsNaN(control.Width))
            {
                control.HorizontalAlignment = HorizontalAlignment.Left;
            }

            // Top aligned, in line with the first line of a taller control (a list, a group)
            labelTextBlock.VerticalAlignment = VerticalAlignment.Top;
            labelTextBlock.Margin = new Thickness(0, 6, 0, 0);
            row = MakeColumns(labelTextBlock, field);
        }

        row.MinHeight = 30;

        if (IsVisibleBinding != null)
        {
            row.Bind(Visual.IsVisibleProperty, new Binding(nameof(IsVisibleBinding)));
        }

        return row;
    }

    /// <summary>
    /// <paramref name="left"/> in the label column, <paramref name="content"/> in the field column.
    /// </summary>
    private static Grid MakeColumns(Control left, Control content)
    {
        // The label column has its full width where there is room; on a narrow window it gives
        // up some of it (the labels wrap) so the fields are not cut off at the right edge.
        var grid = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(2, GridUnitType.Star) { MaxWidth = SettingsChrome.LabelWidth },
                new ColumnDefinition(3, GridUnitType.Star),
            },
        };
        grid.Add(left, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }
}
