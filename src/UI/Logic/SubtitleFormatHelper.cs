using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public static class SubtitleFormatHelper
{

    public static List<SubtitleFormat> GetSubtitleFormatsWithFavoritesAtTop()
    {
        var list = SubtitleFormat.AllSubtitleFormats.ToList();
        var defaultFormat = Se.Settings.General.DefaultSubtitleFormat;
        var favorites = Se.Settings.General.FavoriteSubtitleFormats.Split([';'], StringSplitOptions.RemoveEmptyEntries);

        var result = new List<SubtitleFormat>();

        // Add favorites in order (keeping the order from settings)
        foreach (var favorite in favorites)
        {
            var favoriteFormat = list.FirstOrDefault(f => f.FriendlyName == favorite);
            if (favoriteFormat != null)
            {
                result.Add(favoriteFormat);
                list.Remove(favoriteFormat);
            }
        }

        // Add remaining formats
        result.AddRange(list);

        return result;
    }
}
