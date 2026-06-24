using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa;

public static class AssaStyleStorageHelper
{
    /// <summary>
    /// Returns the ASSA storage style that is marked as default (if any).
    /// </summary>
    public static SeAssaStyle? GetDefaultStorageStyle()
    {
        return Se.Settings.Assa.StoredStyles.FirstOrDefault(s => s.IsDefault);
    }

    /// <summary>
    /// Decides which style name a new ASSA paragraph should use:
    /// if the file already defines styles, the first file style is used; otherwise the default
    /// ASSA storage style (if any) becomes the file's style and is used. Falls back to "Default".
    /// The subtitle header is updated when the storage default style is applied.
    /// </summary>
    public static string GetStyleNameForNewParagraph(Subtitle subtitle)
    {
        // if the file already defines styles, use the first one
        if (!string.IsNullOrEmpty(subtitle.Header) &&
            subtitle.Header.Contains("[V4+ Styles]", StringComparison.OrdinalIgnoreCase))
        {
            var existingStyles = AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header);
            if (existingStyles.Count > 0)
            {
                return existingStyles[0];
            }
        }

        // otherwise use the default ASSA storage style (if any) as the file's style
        var defaultStorageStyle = GetDefaultStorageStyle();
        if (defaultStorageStyle != null)
        {
            var style = new StyleDisplay(defaultStorageStyle).ToSsaStyle();
            subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
                AdvancedSubStationAlpha.DefaultHeader, new List<SsaStyle> { style });
            return style.Name;
        }

        // fall back to the standard default style
        if (string.IsNullOrEmpty(subtitle.Header))
        {
            subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        return AdvancedSubStationAlpha.GetStylesFromHeader(subtitle.Header).FirstOrDefault() ?? "Default";
    }

    /// <summary>
    /// Applies the default ASSA storage style when a subtitle is about to be saved/converted to a
    /// target format, but only when the conversion would otherwise lose the user's default style:
    /// the target must be Advanced SubStation Alpha and the subtitle must not already carry ASSA
    /// styles (e.g. it came from SRT). Without this, a "Save as" to ASS from a style-less format
    /// falls back to the hard-coded Arial default instead of the configured default style
    /// (issue #11788). No-op for other target formats, for subtitles that already have a
    /// [V4+ Styles] header, or when no default storage style is configured.
    /// </summary>
    /// <returns>True if the default storage style was applied; otherwise false.</returns>
    public static bool ApplyDefaultStorageStyleForFormatConversion(Subtitle subtitle, SubtitleFormat format)
    {
        if (subtitle == null || format is not AdvancedSubStationAlpha)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(subtitle.Header) &&
            subtitle.Header.Contains("[V4+ Styles]", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ApplyDefaultStorageStyle(subtitle);
    }

    /// <summary>
    /// Applies the default ASSA storage style (if any) to a subtitle that is about to use the
    /// Advanced SubStation Alpha format - used when creating a new subtitle or converting to ASSA
    /// from a format without ASSA styles. The default storage style becomes the only style in the
    /// header and existing paragraphs are re-pointed to it.
    /// </summary>
    /// <returns>True if a default storage style was applied; otherwise false.</returns>
    public static bool ApplyDefaultStorageStyle(Subtitle subtitle)
    {
        var defaultStorageStyle = GetDefaultStorageStyle();
        if (defaultStorageStyle == null)
        {
            return false;
        }

        var style = new StyleDisplay(defaultStorageStyle).ToSsaStyle();

        var header = subtitle.Header;
        if (string.IsNullOrEmpty(header) || !header.Contains("[V4+ Styles]", StringComparison.OrdinalIgnoreCase))
        {
            header = AdvancedSubStationAlpha.DefaultHeader;
        }

        subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(header, new List<SsaStyle> { style });

        foreach (var p in subtitle.Paragraphs)
        {
            p.Extra = style.Name;
        }

        return true;
    }
}
