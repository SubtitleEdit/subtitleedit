using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsSection
{
    public string Title { get; init; }
    public string IconName { get; }
    public IBrush Brush { get; }
    private readonly List<SettingsItem> _items;
    public StackPanel? Panel { get; set; }
    public bool WrapItems { get; init; }

    public bool IsVisible => _items.Any(i => i.IsVisible);

    public SettingsSection(string title, string iconName, string colorHex, IEnumerable<SettingsItem> items)
    {
        Title = title;
        IconName = iconName;
        Brush = new SolidColorBrush(Color.Parse(colorHex));
        _items = items.ToList();
    }

    public void Filter(string filter)
    {
        foreach (var item in _items)
        {
            item.Filter(filter);
        }
    }

    public Control Build()
    {
        // Exposed as a named group so a screen reader announces the section ("Video player,
        // grouping") when focus moves into it, e.g. after picking a category (#12087).
        Panel = new StackPanel
        {
            Spacing = 6,
            [AutomationProperties.NameProperty] = Title,
            [AutomationProperties.ControlTypeOverrideProperty] = AutomationControlType.Group,
            [AutomationProperties.IsControlElementOverrideProperty] = true,
        };

        // Section header: colored glyph + title, matching the category tiles above the list
        // (and the group icons in the shortcuts window).
        var icon = new ContentControl
        {
            FontSize = UiUtil.ScaledFontSize(15),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Optris.Icons.Avalonia.Attached.SetIcon(icon, IconName);
        // Keep the glyph white on the colored square in the dark theme too (#12717).
        icon.Classes.Add(UiTheme.IconOnAccentClassName);

        Panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 10, 0, 5),
            Children =
            {
                new Border
                {
                    Width = 26,
                    Height = 26,
                    CornerRadius = new CornerRadius(7),
                    Background = Brush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = icon,
                },
                new TextBlock
                {
                    Text = Title,
                    FontSize = UiUtil.ScaledFontSize(16),
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    [AutomationProperties.HeadingLevelProperty] = 2,
                },
            }
        });

        Panel itemsPanel = WrapItems ? new WrapPanel { Orientation = Orientation.Horizontal } : Panel;
        foreach (var item in _items.Where(i => i.IsVisible && (!WrapItems || !i.IsFullWidth)))
        {
            itemsPanel.Children.Add(item.Build(includeLabel: !WrapItems));
        }

        if (WrapItems)
        {
            if (itemsPanel.Children.Count > 0)
            {
                Panel.Children.Add(itemsPanel);
            }

            foreach (var item in _items.Where(i => i.IsVisible && i.IsFullWidth))
            {
                Panel.Children.Add(item.Build());
            }
        }

        return Panel;
    }
}
