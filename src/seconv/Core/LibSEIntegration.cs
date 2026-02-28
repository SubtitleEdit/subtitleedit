using Nikse.SubtitleEdit.Core.Common;
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
        // TODO: Implement various operations using LibSE classes
        // For now, this is a placeholder that shows the structure

        switch (operation.ToLowerInvariant())
        {
            case "fixcommonerrors":
                // TODO: Use FixCommonErrors class from LibSE
                // var fce = new FixCommonErrors();
                // fce.Fix(subtitle);
                break;

            case "removetextforhi":
                // TODO: Use RemoveTextForHI class from LibSE
                // var rtfhi = new RemoveTextForHI();
                // rtfhi.Remove(subtitle);
                break;

            case "mergesametexts":
                // TODO: Merge paragraphs with same text
                break;

            case "mergesametimecodes":
                // TODO: Merge paragraphs with same time codes
                break;

            case "mergeshortlines":
                // TODO: Merge short lines
                break;

            case "splitlonglines":
                // TODO: Split long lines
                break;

            case "balancelines":
                // TODO: Balance line lengths
                break;

            case "redocasing":
                // TODO: Redo casing
                break;

            case "removeformatting":
                // TODO: Remove formatting tags
                RemoveFormatting(subtitle);
                break;

            case "removelinebreaks":
                // TODO: Remove line breaks
                break;

            case "applydurationlimits":
                // TODO: Apply duration limits
                break;

            case "beautifytimecodes":
                // TODO: Beautify time codes
                break;

            case "convertcolorstodialog":
                // TODO: Convert colors to dialog
                break;

            case "fixrtlviaunicodechars":
                // TODO: Fix RTL via Unicode characters
                break;

            case "removeunicodecontrolchars":
                // TODO: Remove Unicode control characters
                RemoveUnicodeControlChars(subtitle);
                break;

            case "reversertlstartend":
                // TODO: Reverse RTL start/end
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

