using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;

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

    public Control Build()
    {
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

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 15),
            Children =
            {
                labelTextBlock,
                _controlFactory()
            }
        };

        if (IsVisibleBinding != null)
        {
            stackPanel.Bind(Visual.IsVisibleProperty, new Binding(nameof(IsVisibleBinding)));
        }

        return stackPanel;
    }
}