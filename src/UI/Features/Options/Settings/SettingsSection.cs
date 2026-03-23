using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class SettingsSection
{
    public string Title { get; init; }
    private readonly List<SettingsItem> _items;
    public StackPanel? Panel { get; set; }

    public bool IsVisible => _items.Any(i => i.IsVisible);

    public SettingsSection(string title, IEnumerable<SettingsItem> items)
    {
        Title = title;
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

        Panel.Children.Add(new TextBlock
        {
            Text = Title,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Margin = new Thickness(0, 10, 0, 5)
        });

        foreach (var item in _items.Where(i => i.IsVisible))
        {
            Panel.Children.Add(item.Build());
        }

        return Panel;
    }
}
