using System;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.LogicalTree;
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

        IsVisible = string.IsNullOrWhiteSpace(filter) || _label != null &&
                    _label.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    public Control Build(bool includeLabel = true)
    {
        if (!includeLabel)
        {
            return _controlFactory();
        }

        var labelTextBlock = new TextBlock
        {
            Text = _label,
            MinWidth = 200,
            VerticalAlignment = VerticalAlignment.Center,
        };

        if (_labelBindingPath != null && _labelBindingSource != null)
        {
            labelTextBlock.Bind(TextBlock.TextProperty, new Binding(_labelBindingPath)
            {
                Source = _labelBindingSource,
            });
        }

        var control = _controlFactory();

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
                    if ((AccessibleLabels.IsInput(input) || AccessibleLabels.IsContentButton(input))
                        && input.TemplatedParent == null && !AccessibleLabels.HasAccessibleName(input))
                    {
                        AccessibleLabels.LinkToLabel(input, AccessibleLabels.FindLabel(input) ?? labelTextBlock);
                    }
                }
            }
        }

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 15),
            Children =
            {
                labelTextBlock,
                control
            }
        };

        if (IsVisibleBinding != null)
        {
            stackPanel.Bind(Visual.IsVisibleProperty, new Binding(nameof(IsVisibleBinding)));
        }

        return stackPanel;
    }
}
