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
    public bool IsVisible { get; private set; } = true;
    public string? IsVisibleBinding { get; set; }

    public SettingsItem(string label, Func<Control> controlFactory, string? isVisibleBinding = null)
    {
        _label = label;
        _controlFactory = controlFactory;
        IsVisibleBinding = isVisibleBinding;
    }

    public void Filter(string filter)
    {
        IsVisible = string.IsNullOrWhiteSpace(filter) || _label != null &&
                    _label.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    public Control Build()
    {
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Margin = new Thickness(0, 0, 0, 15),
            Children =
            {
                new TextBlock
                {
                    Text = _label,
                    MinWidth = 200,
                    VerticalAlignment = VerticalAlignment.Center,
                },
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