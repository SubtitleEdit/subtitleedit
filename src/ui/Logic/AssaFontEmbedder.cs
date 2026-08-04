using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Headless helpers for the [Fonts] attachment section of ASSA files: parsing embedded
/// fonts, embedding font files, and finding/embedding the fonts a subtitle actually uses.
/// Shared by the font collector, the ASSA styles dialog and batch convert.
/// </summary>
public static class AssaFontEmbedder
{
    /// <summary>Matches inline <c>\fn</c> font-name override tags.</summary>
    public static readonly Regex FontNameTagRegex = new(@"\\fn(?<name>[^\\}]+)", RegexOptions.Compiled);

    /// <summary>Parses the [Fonts] attachment section of an ASSA footer into decoded font files.</summary>
    public static List<(string FileName, byte[] Bytes)> GetEmbeddedFonts(string? footer)
    {
        var result = new List<(string, byte[])>();
        if (string.IsNullOrEmpty(footer))
        {
            return result;
        }

        var inFonts = false;
        var fileName = string.Empty;
        var content = new StringBuilder();

        void Flush()
        {
            if (fileName.Length > 0 && content.Length > 0)
            {
                try
                {
                    result.Add((Path.GetFileName(fileName), UUEncoding.UUDecode(content.ToString().Trim())));
                }
                catch
                {
                    // a malformed attachment should not break the caller
                }
            }

            content.Clear();
            fileName = string.Empty;
        }

        foreach (var line in footer.SplitToLines())
        {
            var s = line.Trim();
            if (s.Equals("[Fonts]", StringComparison.OrdinalIgnoreCase))
            {
                Flush();
                inFonts = true;
            }
            else if (s == "[Script Info]" || s == "[V4+ Styles]" || s == "[V4 Styles]" || s == "[Events]" ||
                     s.Equals("[Graphics]", StringComparison.OrdinalIgnoreCase))
            {
                // Only exact section headers end the fonts section - the UU-style encoding's
                // alphabet ('!'..'`') contains '[', so encoded lines can start with it.
                Flush();
                inFonts = false;
            }
            else if (!inFonts)
            {
                // skip
            }
            else if (s.StartsWith("fontname:", StringComparison.OrdinalIgnoreCase) ||
                     s.StartsWith("filename:", StringComparison.OrdinalIgnoreCase))
            {
                Flush();
                fileName = s.Remove(0, 9).Trim();
            }
            else if (s.Length == 0)
            {
                Flush();
            }
            else
            {
                content.AppendLine(s);
            }
        }

        Flush();
        return result;
    }

    /// <summary>
    /// Adds a font file to the [Fonts] attachment section of an ASSA footer, creating the
    /// section if needed. A font whose file name is already attached is not added twice.
    /// </summary>
    public static string AddFontToFooter(string? footer, string fontFilePath, byte[] fontBytes)
    {
        var fileName = Path.GetFileName(fontFilePath);
        if (GetEmbeddedFonts(footer).Any(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
        {
            return footer!;
        }

        var entryLines = new List<string> { "fontname: " + fileName };
        entryLines.AddRange(UUEncoding.UUEncode(fontBytes).Trim().SplitToLines());

        if (string.IsNullOrWhiteSpace(footer))
        {
            return "[Fonts]" + Environment.NewLine + string.Join(Environment.NewLine, entryLines) + Environment.NewLine;
        }

        var lines = footer.SplitToLines();
        var result = new List<string>(lines.Count + entryLines.Count + 2);
        var inFonts = false;
        var inserted = false;

        void InsertEntry()
        {
            while (result.Count > 0 && result[^1].Trim().Length == 0)
            {
                result.RemoveAt(result.Count - 1);
            }

            result.AddRange(entryLines);
            inserted = true;
        }

        foreach (var line in lines)
        {
            var s = line.Trim();
            if (s.Equals("[Fonts]", StringComparison.OrdinalIgnoreCase))
            {
                inFonts = true;
            }
            else if (inFonts && !inserted &&
                     (s == "[Script Info]" || s == "[V4+ Styles]" || s == "[V4 Styles]" || s == "[Events]" ||
                      s.Equals("[Graphics]", StringComparison.OrdinalIgnoreCase) ||
                      s.Equals("[Aegisub Extradata]", StringComparison.OrdinalIgnoreCase)))
            {
                // Only exact section headers end the fonts section (see GetEmbeddedFonts).
                InsertEntry();
                result.Add(string.Empty);
                inFonts = false;
            }

            result.Add(line);
        }

        if (inFonts && !inserted)
        {
            InsertEntry();
        }

        if (!inserted)
        {
            // No [Fonts] section - fonts go first, same order the attachments window writes.
            return "[Fonts]" + Environment.NewLine + string.Join(Environment.NewLine, entryLines) +
                   Environment.NewLine + Environment.NewLine + footer.TrimStart();
        }

        return string.Join(Environment.NewLine, result) + Environment.NewLine;
    }

    /// <summary>
    /// Collects the font names an ASSA renderer would need: fonts of styles that are
    /// actually used by lines, plus inline <c>\fn</c> overrides.
    /// </summary>
    public static List<string> GetUsedFontNames(Subtitle subtitle)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Add(string fontName)
        {
            fontName = fontName.Trim().TrimStart('@'); // "@" prefix = vertical variant of the same font
            if (fontName.Length > 0 && seen.Add(fontName))
            {
                result.Add(fontName);
            }
        }

        var header = subtitle.Header ?? string.Empty;
        var usedStyleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var paragraph in subtitle.Paragraphs)
        {
            usedStyleNames.Add(string.IsNullOrEmpty(paragraph.Extra) ? "Default" : paragraph.Extra.TrimStart('*'));

            foreach (Match match in FontNameTagRegex.Matches(paragraph.Text))
            {
                Add(match.Groups["name"].Value);
            }
        }

        if (header.Contains("[V4", StringComparison.Ordinal))
        {
            foreach (var styleName in AdvancedSubStationAlpha.GetStylesFromHeader(header))
            {
                if (usedStyleNames.Contains(styleName))
                {
                    Add(AdvancedSubStationAlpha.GetSsaStyle(styleName, header).FontName);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Embeds the font files the subtitle uses (styles + inline <c>\fn</c> tags) into its
    /// [Fonts] attachment section. Fonts already embedded are skipped; the rest are searched
    /// in SE's Fonts folder and the system font folders. Returns the number of files embedded.
    /// An optional cache (font name -> found files) lets batch runs skip repeated disk scans.
    /// </summary>
    public static int EmbedUsedFonts(Subtitle subtitle, CancellationToken cancellationToken, IDictionary<string, List<string>>? fontFileCache = null)
    {
        var usedNames = GetUsedFontNames(subtitle);
        if (usedNames.Count == 0)
        {
            return 0;
        }

        var embeddedNames = GetEmbeddedFontNames(subtitle.Footer);
        var missingNames = usedNames.Where(n => !embeddedNames.Contains(n)).ToList();
        if (missingNames.Count == 0)
        {
            return 0;
        }

        var found = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var namesToScan = new List<string>();
        foreach (var name in missingNames)
        {
            if (fontFileCache != null && fontFileCache.TryGetValue(name, out var cached))
            {
                found[name] = cached;
            }
            else
            {
                namesToScan.Add(name);
            }
        }

        if (namesToScan.Count > 0)
        {
            foreach (var kvp in FontHelper.FindFontFiles(namesToScan, cancellationToken))
            {
                found[kvp.Key] = kvp.Value;
                if (fontFileCache != null)
                {
                    fontFileCache[kvp.Key] = kvp.Value;
                }
            }
        }

        var embeddedFileNames = new HashSet<string>(
            GetEmbeddedFonts(subtitle.Footer).Select(f => f.FileName), StringComparer.OrdinalIgnoreCase);
        var footer = subtitle.Footer;
        var count = 0;
        foreach (var file in found.Values.SelectMany(f => f).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (!embeddedFileNames.Add(Path.GetFileName(file)))
            {
                continue;
            }

            try
            {
                footer = AddFontToFooter(footer, file, File.ReadAllBytes(file));
                count++;
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Could not embed font " + file);
            }
        }

        subtitle.Footer = footer;
        return count;
    }

    /// <summary>
    /// The family and libass face names of the fonts a footer already carries as
    /// [Fonts] attachments - those need not be embedded again.
    /// </summary>
    private static HashSet<string> GetEmbeddedFontNames(string? footer)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (fileName, bytes) in GetEmbeddedFonts(footer))
        {
            try
            {
                using var data = SKData.CreateCopy(bytes);
                for (var index = 0; index < 30; index++)
                {
                    using var typeface = SKTypeface.FromData(data, index);
                    if (typeface == null)
                    {
                        break;
                    }

                    names.Add(typeface.FamilyName);
                    names.Add(FontHelper.GetLibAssaFontName(typeface));
                }
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Could not read embedded font " + fileName);
            }
        }

        return names;
    }
}
