using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// One item of the controls under the video player (order + visibility), see
/// <see cref="SeVideo.ControlsItems"/>.
/// </summary>
public class SeVideoControlsItem
{
    public SeVideoControlsItemType Type { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }

    public SeVideoControlsItem()
    {
    }

    public SeVideoControlsItem(SeVideoControlsItem item)
    {
        Type = item.Type;
        IsVisible = item.IsVisible;
        SortOrder = item.SortOrder;
    }

    public static List<SeVideoControlsItem> MakeDefaults()
    {
        return
        [
            new SeVideoControlsItem { Type = SeVideoControlsItemType.Play, SortOrder = 10 },
            new SeVideoControlsItem { Type = SeVideoControlsItemType.Stop, SortOrder = 20 },
            new SeVideoControlsItem { Type = SeVideoControlsItemType.FullScreen, SortOrder = 30 },
            new SeVideoControlsItem { Type = SeVideoControlsItemType.PositionSlider, SortOrder = 40 },
            new SeVideoControlsItem { Type = SeVideoControlsItemType.Volume, SortOrder = 50 },
            new SeVideoControlsItem { Type = SeVideoControlsItemType.PositionText, SortOrder = 60 },
            new SeVideoControlsItem { Type = SeVideoControlsItemType.VideoFileName, SortOrder = 70 },
            new SeVideoControlsItem { Type = SeVideoControlsItemType.PlayerName, SortOrder = 80 },
        ];
    }

    /// <summary>
    /// Returns the items in display order with exactly one entry per type: unknown and duplicate
    /// entries (hand-edited settings file) are dropped and types missing from an older settings
    /// file are appended with their defaults.
    /// </summary>
    public static List<SeVideoControlsItem> Normalize(IEnumerable<SeVideoControlsItem>? items)
    {
        var result = new List<SeVideoControlsItem>();
        foreach (var item in (items ?? []).Where(p => p != null).OrderBy(p => p.SortOrder))
        {
            if (System.Enum.IsDefined(item.Type) && !result.Exists(p => p.Type == item.Type))
            {
                result.Add(new SeVideoControlsItem(item));
            }
        }

        foreach (var item in MakeDefaults())
        {
            if (!result.Exists(p => p.Type == item.Type))
            {
                result.Add(item);
            }
        }

        for (var i = 0; i < result.Count; i++)
        {
            result[i].SortOrder = (i + 1) * 10;
        }

        return result;
    }
}
