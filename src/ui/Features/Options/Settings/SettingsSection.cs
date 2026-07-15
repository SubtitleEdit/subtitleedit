using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsSection
{
    public string Title { get; init; }
    public string IconName { get; }
    public IBrush Brush { get; }
    private readonly List<SettingsItem> _items;
    public StackPanel? Panel { get; set; }

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
        Panel = new StackPanel { Spacing = 6 };

        // Section header: colored glyph + title, matching the category tiles above the list
        // (and the group icons in the shortcuts window).
        var icon = new ContentControl
        {
            FontSize = 15,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Optris.Icons.Avalonia.Attached.SetIcon(icon, IconName);

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
                    FontSize = 16,
                    FontWeight = FontWeight.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            }
        });

        foreach (var item in _items.Where(i => i.IsVisible))
        {
            Panel.Children.Add(item.Build());
        }

        return Panel;
    }
}
