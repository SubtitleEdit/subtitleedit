using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// UI-side accessor for the user's favorite languages (stored in <see cref="SeGeneral.FavoriteLanguages"/>).
/// Bubbles favorite languages to the top of the various language combo boxes.
/// </summary>
public static class LanguageFavoritesHelper
{
    /// <summary>
    /// The favorite language codes (normalized two-letter) in the user's chosen order.
    /// </summary>
    public static List<string> GetCodes()
    {
        return LanguageFavorites.ParseCodes(Se.Settings.General.FavoriteLanguages);
    }

    /// <summary>
    /// Orders the items so favorite languages come first (in the user's order), the rest unchanged.
    /// </summary>
    /// <param name="items">Items to order.</param>
    /// <param name="codeSelector">Returns an item's language code (any ISO form).</param>
    /// <param name="pinTop">Optional predicate for items that stay above the favorites (e.g. "Auto detect").</param>
    public static List<T> Order<T>(IEnumerable<T> items, Func<T, string> codeSelector, Func<T, bool>? pinTop = null)
    {
        return LanguageFavorites.OrderWithFavoritesFirst(items, codeSelector, GetCodes(), pinTop);
    }
}
