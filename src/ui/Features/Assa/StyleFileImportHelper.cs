using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Assa;

public static class StyleFileImportHelper
{
    /// <summary>
    /// Reads ASSA/SSA styles from a file picked for style import.
    /// Handles Aegisub ".sty" files, normal subtitle files, and style-only files (like the ones
    /// made by "Export..." in the styles window) - the latter have no dialogue lines and are
    /// therefore not recognized as subtitles at all.
    /// </summary>
    public static List<SsaStyle> LoadStyles(string fileName, SubtitleFormat format)
    {
        if (fileName.EndsWith(".sty", StringComparison.OrdinalIgnoreCase))
        {
            var content = ReadAllText(fileName);
            if (content == null)
            {
                return new List<SsaStyle>();
            }

            var styHeader = "[V4+ Styles]" + Environment.NewLine +
                            SsaStyle.DefaultAssStyleFormat + Environment.NewLine +
                            content;
            return AdvancedSubStationAlpha.GetSsaStylesFromHeader(styHeader);
        }

        var subtitle = Subtitle.Parse(fileName, format);
        if (subtitle != null && !string.IsNullOrEmpty(subtitle.Header))
        {
            return AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);
        }

        return GetStylesFromStyleOnlyFile(ReadAllText(fileName));
    }

    /// <summary>
    /// Reads styles from a file with a "[V4+ Styles]"/"[V4 Styles]" section but no dialogue lines.
    /// Everything from "[Events]" and down is cut away, as the events "Format:" line would
    /// otherwise be read as a style format line.
    /// </summary>
    private static List<SsaStyle> GetStylesFromStyleOnlyFile(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new List<SsaStyle>();
        }

        var eventsIndex = text.IndexOf("[Events]", StringComparison.OrdinalIgnoreCase);
        var header = eventsIndex >= 0 ? text.Substring(0, eventsIndex) : text;

        if (!header.Contains("[V4+ Styles]", StringComparison.OrdinalIgnoreCase) &&
            !header.Contains("[V4 Styles]", StringComparison.OrdinalIgnoreCase))
        {
            return new List<SsaStyle>();
        }

        return AdvancedSubStationAlpha.GetSsaStylesFromHeader(header);
    }

    private static string? ReadAllText(string fileName)
    {
        try
        {
            return File.ReadAllText(fileName);
        }
        catch
        {
            return null;
        }
    }
}
