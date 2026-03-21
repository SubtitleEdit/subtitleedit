using Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeWaveformToolbarItem
{
    public bool IsVisible { get; set; }
    public int FontSize { get; set; }
    public int LeftMargin { get; set; }
    public int RightMargin { get; set; }
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
        SortOrder = item.SortOrder;
        Type = item.Type;
    }

    public SeWaveformToolbarItem(ToolbarItemDisplay item)
    {
        IsVisible = item.IsVisible;
        FontSize = item.FontSize;
        LeftMargin = item.LeftMargin;
        RightMargin = item.RightMargin;
        Type = item.Type;
    }
}