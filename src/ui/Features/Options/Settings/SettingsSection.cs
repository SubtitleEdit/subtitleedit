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

    /// <summary>
    /// The section in the editor look: a small caps header with the category glyph, then the
    /// rows. <paramref name="first"/> leaves out the line above it (the first section shown).
    /// </summary>
    public Control Build(bool first = true)
    {
        // Exposed as a named group so a screen reader announces the section ("Video player,
        // grouping") when focus moves into it, e.g. after picking a category (#12087).
        Panel = new StackPanel
        {
            Spacing = 8,
            [AutomationProperties.NameProperty] = Title,
            [AutomationProperties.ControlTypeOverrideProperty] = AutomationControlType.Group,
            [AutomationProperties.IsControlElementOverrideProperty] = true,
        };

        // Section header: colored glyph + title, matching the category rows in the side panel
        // (and the group icons in the shortcuts window).
        var icon = new ContentControl
        {
            FontSize = 11,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Optris.Icons.Avalonia.Attached.SetIcon(icon, IconName);
        // Keep the glyph white on the colored square in the dark theme too (#12717).
        icon.Classes.Add(UiTheme.IconOnAccentClassName);

        // The header is shown in small caps; the group above carries the title as written.
        var header = EditorChrome.MakeSectionHeader(Title);
        AutomationProperties.SetHeadingLevel(header, 2);

        Panel.Children.Add(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 6),
            Children =
            {
                new Border
                {
                    Width = 18,
                    Height = 18,
                    CornerRadius = new CornerRadius(5),
                    Background = Brush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Child = icon,
                },
                header,
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

        return new Border
        {
            Child = Panel,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, first ? 0 : 1, 0, 0),
            Padding = new Thickness(20, 16, 20, 18),
        };
    }
}
