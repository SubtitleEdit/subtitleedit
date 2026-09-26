using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Nikse.SubtitleEdit.Features.Assa;

/// <summary>
/// A style category from a Subtitle Edit 4 category export (".template").
/// </summary>
public sealed record Se4StyleCategory(string Name, bool IsDefault, List<SsaStyle> Styles);

public static class StyleFileImportHelper
{
    public const string Se4CategoriesTemplateExtension = ".template";

    public static bool IsSe4CategoriesTemplate(string fileName)
        => fileName.EndsWith(Se4CategoriesTemplateExtension, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Reads the style categories from a Subtitle Edit 4 category export, e.g.
    /// "my_assa_categories.template" (#15332): XML with a "Category" element per category, holding
    /// "Name", "CategoryIsDefault" and "Style" elements. SE 4 wrote the numbers with the current
    /// culture and the colors as HTML colors (alpha dropped), and only these style properties.
    /// SE 4's own importer read "Primary"/"Outline"/... while its exporter wrote
    /// "PrimaryColor"/"OutlineColor"/..., so both names are accepted.
    /// Returns an empty list when the file cannot be read or parsed.
    /// </summary>
    public static List<Se4StyleCategory> LoadSe4CategoriesTemplate(string fileName)
    {
        var list = new List<Se4StyleCategory>();
        var doc = new XmlDocument { XmlResolver = null };
        try
        {
            using var reader = XmlReader.Create(fileName, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            doc.Load(reader);
        }
        catch
        {
            return list;
        }

        var categoryNodes = doc.DocumentElement?.SelectNodes("//Category");
        if (categoryNodes == null)
        {
            return list;
        }

        foreach (XmlNode categoryNode in categoryNodes)
        {
            var name = categoryNode.SelectSingleNode("Name")?.InnerText.Trim();
            var isDefault = bool.TryParse(categoryNode.SelectSingleNode("CategoryIsDefault")?.InnerText.Trim(), out var b) && b;
            var styles = new List<SsaStyle>();
            var styleNodes = categoryNode.SelectNodes("Style");
            if (styleNodes != null)
            {
                foreach (XmlNode styleNode in styleNodes)
                {
                    styles.Add(ReadSe4Style(styleNode));
                }
            }

            list.Add(new Se4StyleCategory(string.IsNullOrEmpty(name) ? "Untitled" : name, isDefault, styles));
        }

        return list;
    }

    private static SsaStyle ReadSe4Style(XmlNode styleNode)
    {
        var style = new SsaStyle();

        var name = GetText(styleNode, "Name");
        if (!string.IsNullOrWhiteSpace(name))
        {
            style.Name = name.Trim();
        }

        var fontName = GetText(styleNode, "FontName");
        if (!string.IsNullOrWhiteSpace(fontName))
        {
            style.FontName = fontName.Trim();
        }

        style.FontSize = GetDecimal(styleNode, style.FontSize, "FontSize");
        style.Primary = GetColor(styleNode, style.Primary, "PrimaryColor", "Primary");
        style.Secondary = GetColor(styleNode, style.Secondary, "SecondaryColor", "Secondary");
        style.Outline = GetColor(styleNode, style.Outline, "OutlineColor", "Outline");
        style.Background = GetColor(styleNode, style.Background, "BackgroundColor", "Background");
        style.ShadowWidth = GetDecimal(styleNode, style.ShadowWidth, "ShadowWidth");
        style.OutlineWidth = GetDecimal(styleNode, style.OutlineWidth, "OutlineWidth");
        style.MarginLeft = (int)GetDecimal(styleNode, style.MarginLeft, "MarginLeft");
        style.MarginRight = (int)GetDecimal(styleNode, style.MarginRight, "MarginRight");
        style.MarginVertical = (int)GetDecimal(styleNode, style.MarginVertical, "MarginVertical");

        var alignment = GetText(styleNode, "Alignment")?.Trim();
        if (!string.IsNullOrEmpty(alignment) && int.TryParse(alignment, NumberStyles.None, CultureInfo.InvariantCulture, out var a) && a >= 1 && a <= 9)
        {
            style.Alignment = alignment;
        }

        var borderStyle = GetText(styleNode, "BorderStyle")?.Trim();
        if (!string.IsNullOrEmpty(borderStyle))
        {
            style.BorderStyle = borderStyle;
        }

        return style;
    }

    private static string? GetText(XmlNode node, params string[] names)
    {
        foreach (var name in names)
        {
            var subNode = node.SelectSingleNode(name);
            if (subNode != null)
            {
                return subNode.InnerText;
            }
        }

        return null;
    }

    // SE 4 wrote decimals with the current culture, so "20,5" is as likely as "20.5"
    private static decimal GetDecimal(XmlNode node, decimal defaultValue, params string[] names)
    {
        var text = GetText(node, names)?.Trim().Replace(',', '.');
        return !string.IsNullOrEmpty(text) && decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)
            ? d
            : defaultValue;
    }

    private static SkiaSharp.SKColor GetColor(XmlNode node, SkiaSharp.SKColor defaultValue, params string[] names)
    {
        var text = GetText(node, names)?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return defaultValue;
        }

        try
        {
            return ColorTranslator.FromHtml(text);
        }
        catch
        {
            return defaultValue;
        }
    }

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
