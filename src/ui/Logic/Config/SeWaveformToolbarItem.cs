using Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveformToolbarItem
{
    public bool IsVisible { get; set; }
    public int FontSize { get; set; }
    public int LeftMargin { get; set; }
    public int RightMargin { get; set; }

    /// <summary>
    /// Width in pixels for the items that have one (see <see cref="GetDefaultWidth"/>); 0 means
    /// the item's default, which is also what a settings file from before the setting loads as.
    /// </summary>
    public int Width { get; set; }

    public int SortOrder { get; set; }
    public SeWaveformToolbarItemType Type { get; set; }

    public SeWaveformToolbarItem()
    {
        IsVisible = true;
        FontSize = 12;
        LeftMargin = 5;
        RightMargin = 5;
        SortOrder = 0;
    }

    public SeWaveformToolbarItem(SeWaveformToolbarItem item)
    {
        IsVisible = item.IsVisible;
        FontSize = item.FontSize;
        LeftMargin = item.LeftMargin;
        RightMargin = item.RightMargin;
        Width = item.Width;
        SortOrder = item.SortOrder;
        Type = item.Type;
    }

    public SeWaveformToolbarItem(ToolbarItemDisplay item)
    {
        IsVisible = item.IsVisible;
        FontSize = item.FontSize;
        LeftMargin = item.LeftMargin;
        RightMargin = item.RightMargin;
        Width = item.Width;
        Type = item.Type;
    }

    /// <summary>
    /// The width an item of the given type has until the user sets one; 0 for the items without
    /// a width setting (buttons and the like size to their content).
    /// </summary>
    public static int GetDefaultWidth(SeWaveformToolbarItemType type)
    {
        return type == SeWaveformToolbarItemType.InitialText ? 400 : 0;
    }

    public static bool HasWidth(SeWaveformToolbarItemType type)
    {
        return GetDefaultWidth(type) > 0;
    }

    public int GetWidthOrDefault()
    {
        var defaultWidth = GetDefaultWidth(Type);
        return Width > 0 && defaultWidth > 0 ? Width : defaultWidth;
    }
}