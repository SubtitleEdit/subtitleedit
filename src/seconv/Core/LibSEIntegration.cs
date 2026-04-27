using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Spectre.Console;
using System.Text;

namespace SeConv.Core;

/// <summary>
/// Helper class to integrate with LibSE subtitle library
/// </summary>
internal static class LibSEIntegration
{
    /// <summary>
    /// Gets all subtitle formats from LibSE — text, binary (input-only), and "other text" lists combined.
    /// </summary>
    public static List<FormatEntry> GetAvailableFormats()
    {
        var entries = new List<FormatEntry>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var f in SubtitleFormat.AllSubtitleFormats)
        {
            if (seen.Add(f.Name))
            {
                entries.Add(new FormatEntry(f, "text"));
            }
        }

        foreach (var f in SubtitleFormat.GetBinaryFormats(true))
        {
            if (seen.Add(f.Name))
            {
                entries.Add(new FormatEntry(f, "binary (input)"));
            }
        }

        foreach (var f in SubtitleFormat.GetTextOtherFormats())
        {
            if (seen.Add(f.Name))
            {
                entries.Add(new FormatEntry(f, "text (input)"));
            }
        }

        return entries;
    }

    public sealed record FormatEntry(SubtitleFormat Format, string Kind);

    /// <summary>
    /// Loads a subtitle file using LibSE. When <paramref name="encodingName"/> is null/blank,
    /// the encoding is auto-detected from the file's content. Also returns the detected
    /// source format so the save side can apply <c>RemoveNativeFormatting</c> if the target
    /// is different.
    /// </summary>
    public static (Subtitle Subtitle, SubtitleFormat Format) LoadSubtitleWithFormat(string filePath, string? encodingName = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Subtitle file not found: {filePath}");
        }

        var subtitle = new Subtitle();
        var encoding = string.IsNullOrWhiteSpace(encodingName)
            ? LanguageAutoDetect.GetEncodingFromFile(filePath)
            : GetEncoding(encodingName);

        var lines = new List<string>(File.ReadAllLines(filePath, encoding));

        // 1. Try text-based formats by content sniffing
        foreach (var format in SubtitleFormat.AllSubtitleFormats)
        {
            if (format.IsMine(lines, filePath))
            {
                format.LoadSubtitle(subtitle, lines, filePath);
                if (subtitle.Paragraphs.Count > 0)
                {
                    return (subtitle, format);
                }
            }
        }

        // 2. Try binary formats (Pac, Ebu, Cavena890, ...) — they read raw bytes themselves
        foreach (var format in SubtitleFormat.GetBinaryFormats(true))
        {
            try
            {
                if (format.IsMine(lines, filePath))
                {
                    var freshSubtitle = new Subtitle();
                    format.LoadSubtitle(freshSubtitle, lines, filePath);
                    if (freshSubtitle.Paragraphs.Count > 0)
                    {
                        return (freshSubtitle, format);
                    }
                }
            }
            catch
            {
                // Binary IsMine can throw on text input; ignore and continue.
            }
        }

        // 3. Try the "other text" formats (NkhCuePoints, BdnXml, JSON variants, ...)
        foreach (var format in SubtitleFormat.GetTextOtherFormats())
        {
            if (format.IsMine(lines, filePath))
            {
                var freshSubtitle = new Subtitle();
                format.LoadSubtitle(freshSubtitle, lines, filePath);
                if (freshSubtitle.Paragraphs.Count > 0)
                {
                    return (freshSubtitle, format);
                }
            }
        }

        throw new InvalidOperationException($"Unable to determine subtitle format for: {filePath}");
    }

    /// <summary>
    /// Backwards-compatible wrapper that drops the detected format. Prefer
    /// <see cref="LoadSubtitleWithFormat"/> when the source format is needed.
    /// </summary>
    public static Subtitle LoadSubtitle(string filePath, string? encodingName = null)
        => LoadSubtitleWithFormat(filePath, encodingName).Subtitle;

    /// <summary>
    /// Saves a subtitle to a file using LibSE. Dispatches to format-specific Save methods
    /// for binary formats (Pac, Ebu, Cavena890, ...) and applies format-specific encoding
    /// rules for text formats (WebVTT/UTF-8 BOM, iTunes Timed Text/UTF-8 no-BOM, RTF/ASCII).
    /// </summary>
    public static void SaveSubtitle(
        Subtitle subtitle,
        string filePath,
        string formatName,
        string? encodingName = null,
        SubtitleFormat? sourceFormat = null,
        int? pacCodePage = null,
        string? ebuHeaderFile = null,
        ConversionOptions? options = null)
    {
        if (subtitle == null)
        {
            throw new ArgumentNullException(nameof(subtitle));
        }

        // Custom text format — render via UiLogic CustomTextFormatter
        if (formatName.Replace(" ", "").Equals("CustomTextFormat", StringComparison.OrdinalIgnoreCase) ||
            formatName.Replace(" ", "").Equals("CustomText", StringComparison.OrdinalIgnoreCase))
        {
            if (options is null || string.IsNullOrWhiteSpace(options.CustomFormatFile))
            {
                throw new InvalidOperationException("Custom text output requires --customformat=<path-to-template.xml>.");
            }
            var template = CustomFormatTemplateLoader.Load(options.CustomFormatFile);
            var rendered = Nikse.SubtitleEdit.UiLogic.Export.CustomTextFormatter.GenerateCustomText(
                template, subtitle.Paragraphs.ToList(), Path.GetFileNameWithoutExtension(filePath), string.Empty);
            var outputDirCustom = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDirCustom) && !Directory.Exists(outputDirCustom))
            {
                Directory.CreateDirectory(outputDirCustom);
            }
            File.WriteAllText(filePath, rendered, new UTF8Encoding(true));
            return;
        }

        // Plain text output — strip HTML, one paragraph per group separated by blank lines
        if (formatName.Replace(" ", "").Equals("PlainText", StringComparison.OrdinalIgnoreCase))
        {
            var outputDirPlain = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDirPlain) && !Directory.Exists(outputDirPlain))
            {
                Directory.CreateDirectory(outputDirPlain);
            }
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }
                sb.AppendLine(HtmlUtil.RemoveHtmlTags(p.Text, true));
            }
            var plainEncoding = string.IsNullOrWhiteSpace(encodingName)
                ? new UTF8Encoding(true)
                : GetEncoding(encodingName);
            File.WriteAllText(filePath, sb.ToString(), plainEncoding);
            return;
        }

        // Image-based output formats: render text → bitmaps → format-specific bytes
        var imageHandler = ImageOutputWriter.TryCreateHandler(formatName);
        if (imageHandler is not null)
        {
            var outputDirImg = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(outputDirImg) && !Directory.Exists(outputDirImg))
            {
                Directory.CreateDirectory(outputDirImg);
            }
            ImageOutputWriter.Write(subtitle, filePath, imageHandler, options ?? throw new InvalidOperationException("Image-based formats require ConversionOptions"));
            return;
        }

        var targetFormat = ResolveFormatByName(formatName)
            ?? throw new InvalidOperationException($"Unknown subtitle format: {formatName}");

        // Strip native source-format markup that the target wouldn't understand
        if (sourceFormat != null && !sourceFormat.GetType().Equals(targetFormat.GetType()))
        {
            targetFormat.RemoveNativeFormatting(subtitle, sourceFormat);
        }

        var outputDir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // Ebu (binary) — optional header file
        if (targetFormat is Ebu ebu)
        {
            Ebu.EbuGeneralSubtitleInformation? header = null;
            if (!string.IsNullOrEmpty(ebuHeaderFile))
            {
                if (!File.Exists(ebuHeaderFile))
                {
                    throw new FileNotFoundException($"EBU header file not found: {ebuHeaderFile}", ebuHeaderFile);
                }
                var headerBytes = File.ReadAllBytes(ebuHeaderFile);
                header = Ebu.ReadHeader(headerBytes);
            }
            ebu.Save(filePath, subtitle, true, header);
            return;
        }

        // Pac and PacUnicode (binary) — optional code page
        if (targetFormat is Pac pac)
        {
            if (pacCodePage.HasValue)
            {
                pac.CodePage = pacCodePage.Value;
            }
            pac.Save(filePath, subtitle);
            return;
        }

        // Other binary formats (Cavena890, CheetahCaption, CapMakerPlus, Ayato, ...)
        if (targetFormat is IBinaryPersistableSubtitle binary)
        {
            using var fs = File.Create(filePath);
            binary.Save(filePath, fs, subtitle, true);
            return;
        }

        // Text-based — apply format-specific encoding rules
        SaveTextFormat(subtitle, filePath, targetFormat, encodingName);
    }

    private static void SaveTextFormat(Subtitle subtitle, string filePath, SubtitleFormat format, string? encodingName)
    {
        var content = format.ToText(subtitle, Path.GetFileNameWithoutExtension(filePath));

        // WebVTT — always UTF-8 with BOM
        if (format is WebVTT)
        {
            File.WriteAllText(filePath, content, new UTF8Encoding(true));
            return;
        }

        // iTunes Timed Text — UTF-8 without BOM
        if (format is ItunesTimedText)
        {
            File.WriteAllText(filePath, content, new UTF8Encoding(false));
            return;
        }

        // .rtf — always ASCII
        if (string.Equals(Path.GetExtension(filePath), ".rtf", StringComparison.OrdinalIgnoreCase))
        {
            File.WriteAllText(filePath, content, Encoding.ASCII);
            return;
        }

        // Default: user-specified encoding (with auto UTF-8 BOM as fallback)
        var encoding = GetEncoding(encodingName);
        var textEncoding = GetTextEncoding(encodingName);
        if (textEncoding.DisplayName == TextEncoding.Utf8WithBom)
        {
            File.WriteAllText(filePath, content, new UTF8Encoding(true));
        }
        else if (textEncoding.DisplayName == TextEncoding.Utf8WithoutBom)
        {
            File.WriteAllText(filePath, content, new UTF8Encoding(false));
        }
        else
        {
            File.WriteAllText(filePath, content, encoding);
        }
    }

    private static SubtitleFormat? ResolveFormatByName(string formatName)
    {
        var normalized = formatName.Replace(" ", "");
        bool Matches(SubtitleFormat f) =>
            f.Name.Replace(" ", "").Equals(normalized, StringComparison.OrdinalIgnoreCase);

        return SubtitleFormat.AllSubtitleFormats.FirstOrDefault(Matches)
            ?? SubtitleFormat.GetBinaryFormats(true).FirstOrDefault(Matches)
            ?? SubtitleFormat.GetTextOtherFormats().FirstOrDefault(Matches);
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
        if (count <= 0)
        {
            return;
        }

        var keep = Math.Max(0, subtitle.Paragraphs.Count - count);
        var paragraphs = subtitle.Paragraphs.Take(keep).ToList();
        subtitle.Paragraphs.Clear();
        subtitle.Paragraphs.AddRange(paragraphs);
        subtitle.Renumber();
    }

    /// <summary>
    /// Applies ASSA-only options (<c>--resolution</c>, <c>--assa-style-file</c>) to the
    /// subtitle's header before save. Emits a stderr warning when the target format is
    /// not ASSA-compatible and either option is set.
    /// </summary>
    public static void ApplyAssaPostProcess(Subtitle subtitle, string normalizedFormat, (int Width, int Height)? resolution, string? assaStyleFile)
    {
        if (resolution is null && string.IsNullOrEmpty(assaStyleFile))
        {
            return;
        }

        var isAssaTarget = normalizedFormat.Replace(" ", "").Equals("AdvancedSubStationAlpha", StringComparison.OrdinalIgnoreCase)
                         || normalizedFormat.Replace(" ", "").Equals("SubStationAlpha", StringComparison.OrdinalIgnoreCase);

        if (!isAssaTarget)
        {
            AnsiConsole.MarkupLine("[yellow]Warning: --resolution and --assa-style-file only apply to ASSA/SSA targets; ignored.[/]");
            return;
        }

        if (string.IsNullOrEmpty(subtitle.Header) || !subtitle.Header.Contains("[V4+ Styles]"))
        {
            subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
        }

        if (resolution is { } r)
        {
            subtitle.Header = AdvancedSubStationAlpha.SetResolution(subtitle.Header, r.Width, r.Height);
        }

        if (!string.IsNullOrEmpty(assaStyleFile))
        {
            if (!File.Exists(assaStyleFile))
            {
                throw new FileNotFoundException($"ASSA style file not found: {assaStyleFile}", assaStyleFile);
            }
            var styleSubtitle = new Subtitle();
            var styleLines = new List<string>(File.ReadAllLines(assaStyleFile));
            new AdvancedSubStationAlpha().LoadSubtitle(styleSubtitle, styleLines, assaStyleFile);
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(styleSubtitle.Header);
            foreach (var style in styles)
            {
                subtitle.Header = AdvancedSubStationAlpha.UpdateOrAddStyle(subtitle.Header, style);
            }
        }
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
    /// Gets the appropriate file extension for a format. Looks across text, binary,
    /// and "other text" format lists to cover targets like PAC and EBU STL, and
    /// also handles image-based output targets (Blu-Ray sup, VobSub, BDN-XML, ...).
    /// </summary>
    public static string GetExtensionForFormat(string formatName)
    {
        var normalized = NormalizeFormatName(formatName);

        if (normalized.Replace(" ", "").Equals("PlainText", StringComparison.OrdinalIgnoreCase))
        {
            return ".txt";
        }

        if (normalized.Replace(" ", "").Equals("CustomTextFormat", StringComparison.OrdinalIgnoreCase) ||
            normalized.Replace(" ", "").Equals("CustomText", StringComparison.OrdinalIgnoreCase))
        {
            // Custom text extension is template-defined; default to .txt if we don't yet have it
            return ".txt";
        }

        var imageHandler = ImageOutputWriter.TryCreateHandler(normalized);
        if (imageHandler is not null)
        {
            return imageHandler.Extension;
        }

        var format = ResolveFormatByName(normalized);
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
            "pac" => Pac.NameOfFormat,
            "pacunicode" or "unipac" => "PAC Unicode (UniPac)",
            "ebu" or "ebustl" or "stl" => Ebu.NameOfFormat,
            "cavena" or "cavena890" => Cavena890.NameOfFormat,
            "cheetah" or "cheetahcaption" => CheetahCaption.NameOfFormat,
            "capmaker" or "capmakerplus" => CapMakerPlus.NameOfFormat,
            "ayato" => "Ayato",
            "bluraysup" or "blurayup" or "sup" => "Blu-ray sup",
            "vobsub" => "VobSub",
            "bdnxml" or "bdn-xml" => "BDN-XML",
            "dost" or "dostimage" => "DOST/image",
            "fcp" or "fcpimage" => "FCP/image",
            "dcinemainterop" or "dcinema-interop" => "D-Cinema interop/png",
            "dcinemasmpte2014" or "dcinema-smpte" => "D-Cinema SMPTE 2014/png",
            "imageswithtimecode" or "imagesintc" => "Images with time codes in file name",
            "plaintext" or "text" or "txt" => "Plain text",
            "customtext" or "customtextformat" => "Custom text format",
            _ => formatName
        };
    }
}

