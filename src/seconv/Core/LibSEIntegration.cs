using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;

namespace SeConv.Core;

/// <summary>
/// Helper class to integrate with LibSE subtitle library
/// </summary>
internal static class LibSEIntegration
{
    /// <summary>
    /// Gets all available subtitle formats from LibSE
    /// </summary>
    public static List<SubtitleFormat> GetAvailableFormats()
    {
        return SubtitleFormat.AllSubtitleFormats.ToList();
    }

    /// <summary>
    /// Loads a subtitle file using LibSE
    /// </summary>
    public static Subtitle LoadSubtitle(string filePath, string? encodingName = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Subtitle file not found: {filePath}");
        }

        var subtitle = new Subtitle();
        var encoding = GetEncoding(encodingName);

        // Read file content
        var lines = new List<string>(File.ReadAllLines(filePath, encoding));

        // Load the subtitle using the format's LoadSubtitle method
        // Try each format until one works
        SubtitleFormat? detectedFormat = null;
        foreach (var format in SubtitleFormat.AllSubtitleFormats)
        {
            if (format.IsMine(lines, filePath))
            {
                format.LoadSubtitle(subtitle, lines, filePath);
                detectedFormat = format;
                break;
            }
        }

        if (detectedFormat == null || subtitle.Paragraphs.Count == 0)
        {
            throw new InvalidOperationException($"Unable to determine subtitle format for: {filePath}");
        }

        return subtitle;
    }

    /// <summary>
    /// Saves a subtitle to a file using LibSE
    /// </summary>
    public static void SaveSubtitle(Subtitle subtitle, string filePath, string formatName, string? encodingName = null)
    {
        if (subtitle == null)
        {
            throw new ArgumentNullException(nameof(subtitle));
        }

        // Find the format by name (case-insensitive, without spaces)
        var format = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(f => 
            f.Name.Replace(" ", "").Equals(formatName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

        if (format == null)
        {
            throw new InvalidOperationException($"Unknown subtitle format: {formatName}");
        }

        // Generate subtitle content
        var content = format.ToText(subtitle, Path.GetFileNameWithoutExtension(filePath));

        // Get encoding
        var encoding = GetEncoding(encodingName);
        var textEncoding = GetTextEncoding(encodingName);

        // Write to file
        var outputDir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // Handle UTF-8 BOM based on TextEncoding
        if (textEncoding.DisplayName == TextEncoding.Utf8WithBom)
        {
            File.WriteAllText(filePath, content, new UTF8Encoding(true)); // with BOM
        }
        else if (textEncoding.DisplayName == TextEncoding.Utf8WithoutBom)
        {
            File.WriteAllText(filePath, content, new UTF8Encoding(false)); // without BOM
        }
        else
        {
            File.WriteAllText(filePath, content, encoding);
        }
    }

    /// <summary>
    /// Applies subtitle operations/transformations
    /// </summary>
    public static void ApplyOperations(Subtitle subtitle, List<string> operations)
    {
        if (subtitle == null || operations == null || operations.Count == 0)
        {
            return;
        }

        foreach (var operation in operations)
        {
            ApplyOperation(subtitle, operation);
        }
    }

    private static void ApplyOperation(Subtitle subtitle, string operation)
    {
        switch (operation.ToLowerInvariant())
        {
            case "fixcommonerrors":
                // TODO: Use FixCommonErrors class from LibSE
                break;

            case "removetextforhi":
                {
                    var hiSettings = new RemoveTextForHISettings(subtitle);
                    var hiLib = new RemoveTextForHI(hiSettings);
                    var hiLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                    var hiIndex = subtitle.Paragraphs.Count - 1;
                    while (hiIndex >= 0)
                    {
                        var p = subtitle.Paragraphs[hiIndex];
                        p.Text = hiLib.RemoveTextFromHearImpaired(p.Text, hiLanguage);
                        if (string.IsNullOrWhiteSpace(p.Text))
                        {
                            subtitle.Paragraphs.RemoveAt(hiIndex);
                        }
                        hiIndex--;
                    }
                    subtitle.Renumber();
                }
                break;

            case "mergesametexts":
                {
                    var merged = MergeLinesSameTextUtils.MergeLinesWithSameTextInSubtitle(subtitle, true, 250);
                    if (merged.Paragraphs.Count != subtitle.Paragraphs.Count)
                    {
                        subtitle.Paragraphs.Clear();
                        subtitle.Paragraphs.AddRange(merged.Paragraphs);
                    }
                }
                break;

            case "mergesametimecodes":
                {
                    var merged = MergeLinesWithSameTimeCodes.Merge(subtitle, new List<int>(), out _, true, false, false, 1000, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());
                    if (merged.Paragraphs.Count != subtitle.Paragraphs.Count)
                    {
                        subtitle.Paragraphs.Clear();
                        subtitle.Paragraphs.AddRange(merged.Paragraphs);
                    }
                }
                break;

            case "mergeshortlines":
                {
                    var merged = MergeShortLinesUtils.MergeShortLinesInSubtitle(subtitle, Configuration.Settings.Tools.MergeShortLinesMaxGap, Configuration.Settings.General.SubtitleLineMaximumLength, Configuration.Settings.Tools.MergeShortLinesOnlyContinuous);
                    if (merged.Paragraphs.Count != subtitle.Paragraphs.Count)
                    {
                        subtitle.Paragraphs.Clear();
                        subtitle.Paragraphs.AddRange(merged.Paragraphs);
                    }
                }
                break;

            case "splitlonglines":
                try
                {
                    var split = SplitLongLinesHelper.SplitLongLinesInSubtitle(subtitle, Configuration.Settings.General.SubtitleLineMaximumLength * 2, Configuration.Settings.General.SubtitleLineMaximumLength);
                    subtitle.Paragraphs.Clear();
                    subtitle.Paragraphs.AddRange(split.Paragraphs);
                }
                catch
                {
                    // ignore
                }
                break;

            case "balancelines":
                {
                    var balanceLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                    foreach (var p in subtitle.Paragraphs)
                    {
                        p.Text = Utilities.AutoBreakLine(p.Text, balanceLanguage, false);
                    }
                }
                break;

            case "redocasing":
                {
                    var casingLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                    var fixCasing = new FixCasing(casingLanguage)
                    {
                        FixNormal = true,
                        FixNormalOnlyAllUppercase = false,
                        FixMakeUppercase = false,
                        FixMakeLowercase = false,
                    };
                    fixCasing.Fix(subtitle);
                }
                break;

            case "removeformatting":
                RemoveFormatting(subtitle);
                break;

            case "removelinebreaks":
                foreach (var p in subtitle.Paragraphs)
                {
                    p.Text = Utilities.RemoveLineBreaks(p.Text);
                }
                break;

            case "applydurationlimits":
                {
                    var fixDurationLimits = new FixDurationLimits(
                        Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds,
                        Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds,
                        new List<double>());
                    var fixedSub = fixDurationLimits.Fix(subtitle);
                    subtitle.Paragraphs.Clear();
                    subtitle.Paragraphs.AddRange(fixedSub.Paragraphs);
                }
                break;

            case "beautifytimecodes":
                // TODO: Beautify time codes
                break;

            case "convertcolorstodialog":
                {
                    var ctdLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
                    ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false, false, false, ctdLanguage);
                }
                break;

            case "fixrtlviaunicodechars":
                foreach (var p in subtitle.Paragraphs)
                {
                    p.Text = Utilities.FixRtlViaUnicodeChars(p.Text);
                }
                break;

            case "removeunicodecontrolchars":
                RemoveUnicodeControlChars(subtitle);
                break;

            case "reversertlstartend":
                foreach (var p in subtitle.Paragraphs)
                {
                    p.Text = Utilities.ReverseStartAndEndingForRightToLeft(p.Text);
                }
                break;

            default:
                // Unknown operation - ignore or log warning
                break;
        }
    }

    private static void RemoveFormatting(Subtitle subtitle)
    {
        foreach (var p in subtitle.Paragraphs)
        {
            p.Text = HtmlUtil.RemoveHtmlTags(p.Text, true);
        }
    }

    private static void RemoveUnicodeControlChars(Subtitle subtitle)
    {
        foreach (var p in subtitle.Paragraphs)
        {
            p.Text = p.Text.RemoveControlCharacters();
        }
    }

    public static void DeleteFirst(Subtitle subtitle, int count)
    {
        if (count <= 0)
        {
            return;
        }

        var paragraphs = subtitle.Paragraphs.Skip(count).ToList();
        subtitle.Paragraphs.Clear();
        subtitle.Paragraphs.AddRange(paragraphs);
        subtitle.Renumber();
    }

    public static void DeleteLast(Subtitle subtitle, int count)
    {
        if (count <= 0 || count >= subtitle.Paragraphs.Count)
        {
            return;
        }

        var paragraphs = subtitle.Paragraphs.Take(subtitle.Paragraphs.Count - count).ToList();
        subtitle.Paragraphs.Clear();
        subtitle.Paragraphs.AddRange(paragraphs);
        subtitle.Renumber();
    }

    public static void DeleteContains(Subtitle subtitle, string deleteContains)
    {
        if (string.IsNullOrEmpty(deleteContains))
        {
            return;
        }

        var deleted = 0;
        for (var index = subtitle.Paragraphs.Count - 1; index >= 0; index--)
        {
            var paragraph = subtitle.Paragraphs[index];
            if (paragraph.Text.Contains(deleteContains, StringComparison.Ordinal))
            {
                deleted++;
                subtitle.Paragraphs.RemoveAt(index);
            }
        }

        if (deleted > 0)
        {
            subtitle.Renumber();
        }
    }

    /// <summary>
    /// Gets an encoding from a string name
    /// </summary>
    private static Encoding GetEncoding(string? encodingName)
    {
        if (string.IsNullOrWhiteSpace(encodingName))
        {
            return new UTF8Encoding(true); // UTF-8 with BOM by default
        }

        var name = encodingName.Trim().ToLowerInvariant();

        // Handle UTF-8 variations
        if (name is "utf8" or "utf-8" or "utf-8-bom")
        {
            return new UTF8Encoding(true); // with BOM
        }

        if (name is "utf-8-no-bom" or "utf-8-nobom" or "utf8-nobom")
        {
            return new UTF8Encoding(false); // without BOM
        }

        // Try to get encoding by name or code page
        try
        {
            if (int.TryParse(encodingName, out var codePage))
            {
                return Encoding.GetEncoding(codePage);
            }

            return Encoding.GetEncoding(encodingName);
        }
        catch
        {
            // Fall back to UTF-8 with BOM if encoding not found
            return new UTF8Encoding(true);
        }
    }

    /// <summary>
    /// Gets a TextEncoding from a string name
    /// </summary>
    private static TextEncoding GetTextEncoding(string? encodingName)
    {
        if (string.IsNullOrWhiteSpace(encodingName))
        {
            return new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
        }

        var name = encodingName.Trim().ToLowerInvariant();

        // Handle UTF-8 variations
        if (name is "utf8" or "utf-8" or "utf-8-bom")
        {
            return new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
        }

        if (name is "utf-8-no-bom" or "utf-8-nobom" or "utf8-nobom")
        {
            return new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithoutBom);
        }

        // For other encodings, create TextEncoding with the encoding
        try
        {
            Encoding encoding;
            if (int.TryParse(encodingName, out var codePage))
            {
                encoding = Encoding.GetEncoding(codePage);
            }
            else
            {
                encoding = Encoding.GetEncoding(encodingName);
            }

            return new TextEncoding(encoding, null);
        }
        catch
        {
            // Fall back to UTF-8 with BOM if encoding not found
            return new TextEncoding(Encoding.UTF8, TextEncoding.Utf8WithBom);
        }
    }

    /// <summary>
    /// Gets the appropriate file extension for a format
    /// </summary>
    public static string GetExtensionForFormat(string formatName)
    {
        // Normalize the format name first to handle shortcuts
        var normalized = NormalizeFormatName(formatName);

        var format = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(f => 
            f.Name.Replace(" ", "").Equals(normalized.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

        return format?.Extension ?? ".txt";
    }

    /// <summary>
    /// Normalizes format name (handles common shortcuts)
    /// </summary>
    public static string NormalizeFormatName(string formatName)
    {
        var normalized = formatName.Trim().ToLowerInvariant().Replace(" ", "");

        return normalized switch
        {
            "ass" or "assa" => "AdvancedSubStationAlpha",
            "ssa" => "SubStationAlpha",
            "srt" => "SubRip",
            "smi" => "SAMI",
            "vtt" => "WebVTT",
            "sbv" => "YouTubeSbv",
            _ => formatName
        };
    }
}

