using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa;

/// <summary>
/// Applies the styles-storage default to subtitles that are about to use a (Advanced) SubStation
/// Alpha format. ASSA and SSA keep separate storages (<c>Se.Settings.Assa.StoredStyles</c> /
/// <c>Se.Settings.Ssa.StoredStyles</c>), so every entry point takes the target format and picks
/// the matching storage.
/// </summary>
public static class AssaStyleStorageHelper
{
    private static List<SeAssaStyle> GetStorage(SubtitleFormat format)
    {
        return format is SubStationAlpha ? Se.Settings.Ssa.StoredStyles : Se.Settings.Assa.StoredStyles;
    }

    /// <summary>
    /// True when the header already carries SubStation Alpha styles - either variant: a freshly
    /// format-switched file can hold an ASSA header while the selected format is SSA (and vice
    /// versa), and both save paths convert the header on write.
    /// </summary>
    private static bool HasSsaOrAssaStyles(string? header)
    {
        return !string.IsNullOrEmpty(header) &&
               (header.Contains("[V4+ Styles]", StringComparison.OrdinalIgnoreCase) ||
                header.Contains("[V4 Styles]", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns the storage style that is marked as default (if any) for the given format's storage.
    /// </summary>
    public static SeAssaStyle? GetDefaultStorageStyle(SubtitleFormat format)
    {
        return GetStorage(format).FirstOrDefault(s => s.IsDefault);
    }

    /// <summary>
    /// Returns the storage styles that make up the default template for new/converted ASSA/SSA
    /// subtitles, mirroring SE 4's "default style storage category" (issue #13653): the whole
    /// category of the style marked as default - or, when no style is marked, the built-in
    /// default category (empty category name) - so multi-style templates survive the conversion.
    /// The default style (marked, or the category's first) is returned first so callers can use
    /// it for the paragraphs.
    /// </summary>
    public static List<SeAssaStyle> GetDefaultStorageStyles(SubtitleFormat format)
    {
        var defaultStyle = GetDefaultStorageStyle(format);
        var category = defaultStyle?.Category ?? string.Empty;
        var categoryStyles = GetStorage(format)
            .Where(s => string.Equals(s.Category ?? string.Empty, category, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (defaultStyle != null)
        {
            categoryStyles.Remove(defaultStyle);
            categoryStyles.Insert(0, defaultStyle);
        }

        // styles are written to one styles section, where a duplicate name would be ambiguous
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return categoryStyles.Where(s => seenNames.Add(s.Name)).ToList();
    }

    /// <summary>
    /// Decides which style name a new ASSA/SSA paragraph should use.
    /// When the file already defines styles, see <see cref="ResolveExistingFileStyle"/>; otherwise
    /// the default storage styles (if any) become the file's styles and the default one is used.
    /// Falls back to "Default". The subtitle header is updated when the storage styles are applied.
    /// </summary>
    /// <param name="neighborStyle">
    /// Style of the line the new paragraph is inserted next to, when there is one.
    /// </param>
    public static string GetStyleNameForNewParagraph(Subtitle subtitle, SubtitleFormat format, string? neighborStyle = null)
    {
        // if the file already defines styles, pick one of those
        if (HasSsaOrAssaStyles(subtitle.Header))
        {
            var existingStyles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
            if (existingStyles.Count > 0)
            {
                return ResolveExistingFileStyle(existingStyles, format, neighborStyle);
            }
        }

        // otherwise use the default storage styles (if any) as the file's styles
        var storageStyles = GetDefaultStorageStyles(format);
        if (storageStyles.Count > 0)
        {
            subtitle.Header = MakeHeaderFromStorageStyles(subtitle.Header, storageStyles, format);
            return storageStyles[0].Name;
        }

        // fall back to the standard default style
        if (string.IsNullOrEmpty(subtitle.Header))
        {
            subtitle.Header = format is SubStationAlpha
                ? SubStationAlpha.DefaultHeader
                : AdvancedSubStationAlpha.DefaultHeader;
        }

        return AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header).FirstOrDefault() ?? "Default";
    }

    /// <summary>
    /// Picks a style for a new paragraph among the styles a file already defines (issue #13677).
    /// The header order is not a ranking - SE 4 kept the style of the line the new line was
    /// inserted next to, and just taking the first header style made every new line adopt
    /// whichever style happened to sit at the top. Order of preference:
    /// the neighbouring line's style, the style flagged as default in the storage, a style
    /// literally named "Default", and only then the first style in the header.
    /// </summary>
    private static string ResolveExistingFileStyle(List<string> existingStyles, SubtitleFormat format, string? neighborStyle)
    {
        return FindStyle(existingStyles, neighborStyle) ??
               FindStyle(existingStyles, GetDefaultStorageStyle(format)?.Name) ??
               FindStyle(existingStyles, "Default") ??
               existingStyles[0];
    }

    /// <summary>
    /// Returns the file's spelling of <paramref name="name"/>, or null when the file has no such
    /// style. Style names are matched case-insensitively, like the rest of the ASSA style lookups.
    /// </summary>
    private static string? FindStyle(List<string> existingStyles, string? name)
    {
        return string.IsNullOrEmpty(name)
            ? null
            : existingStyles.FirstOrDefault(s => s.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Applies the default storage styles when a subtitle is about to be saved/converted to a
    /// target format, but only when the conversion would otherwise lose the user's default styles:
    /// the target must be Advanced SubStation Alpha or SubStation Alpha and the subtitle must not
    /// already carry SSA/ASSA styles (e.g. it came from SRT). Without this, a "Save as" to ASS
    /// from a style-less format falls back to the hard-coded Arial default instead of the
    /// configured default styles (issue #11788). No-op for other target formats, for subtitles
    /// that already have a styles header, or when no default storage styles are configured.
    /// </summary>
    /// <returns>True if the default storage styles were applied; otherwise false.</returns>
    public static bool ApplyDefaultStorageStyleForFormatConversion(Subtitle subtitle, SubtitleFormat format)
    {
        if (subtitle == null || format is not (AdvancedSubStationAlpha or SubStationAlpha))
        {
            return false;
        }

        // an SSA source keeps its own styles when saved as ASSA (and vice versa) - both
        // ToText implementations convert the foreign header variant on write
        if (HasSsaOrAssaStyles(subtitle.Header))
        {
            return false;
        }

        return ApplyDefaultStorageStyle(subtitle, format);
    }

    /// <summary>
    /// Applies the default storage styles (if any) to a subtitle that is about to use the
    /// Advanced SubStation Alpha or SubStation Alpha format - used when creating a new subtitle
    /// or converting from a format without SSA/ASSA styles. Like SE 4's default style storage
    /// category, the whole default category becomes the file's styles (issue #13653) and existing
    /// paragraphs are re-pointed to the default style.
    /// </summary>
    /// <returns>True if default storage styles were applied; otherwise false.</returns>
    public static bool ApplyDefaultStorageStyle(Subtitle subtitle, SubtitleFormat format)
    {
        var storageStyles = GetDefaultStorageStyles(format);
        if (storageStyles.Count == 0)
        {
            return false;
        }

        subtitle.Header = MakeHeaderFromStorageStyles(subtitle.Header, storageStyles, format);

        foreach (var p in subtitle.Paragraphs)
        {
            p.Extra = storageStyles[0].Name;
        }

        return true;
    }

    private static string MakeHeaderFromStorageStyles(string? existingHeader, List<SeAssaStyle> storageStyles, SubtitleFormat format)
    {
        var styles = storageStyles.Select(s => new StyleDisplay(s).ToSsaStyle()).ToList();

        var baseHeader = existingHeader;
        if (string.IsNullOrEmpty(baseHeader) || !baseHeader.Contains("[V4+ Styles]", StringComparison.OrdinalIgnoreCase))
        {
            baseHeader = AdvancedSubStationAlpha.DefaultHeader;
        }

        var assaHeader = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(baseHeader, styles);
        return format is SubStationAlpha
            ? SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(assaHeader, string.Empty)
            : assaHeader;
    }
}
