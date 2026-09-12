using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SeConv.Core;

namespace SeConv.Mcp;

/// <summary>
/// The MCP tool surface of <c>seconv mcp</c>. Each tool is a thin adapter over the same Core
/// helpers the CLI subcommands use (<see cref="SubtitleInfoGatherer"/>, <see cref="SubtitleLinter"/>,
/// <see cref="SubtitleConverter"/>, ...), so behaviour cannot drift between the two entry points.
/// Every tool returns a <see cref="CallToolResult"/> directly: on success a single compact JSON
/// text block; on failure <c>isError</c> with the exception message, so the model sees what went
/// wrong instead of the SDK's generic "an error occurred" placeholder.
/// </summary>
[McpServerToolType]
internal sealed class SubtitleTools
{
    private SubtitleTools()
    {
        // Static tool methods only; the SDK's WithTools<T>() needs a non-static type argument.
    }

    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private const int DefaultReadCount = 100;
    private const int MaxReadCount = 1000;

    [McpServerTool(Name = "list_formats", ReadOnly = true, Idempotent = true)]
    [Description("List the subtitle formats seconv can read and write. 'id' is the exact value convert_subtitle's 'format' parameter accepts; 'inputOnly' formats can be read but not written.")]
    public static CallToolResult ListFormats(
        [Description("Optional case-insensitive substring matched against id, name and extension (e.g. 'srt', 'ebu', 'vtt'). Omit to list everything.")] string? filter = null)
        => Run(() =>
        {
            var formats = LibSEIntegration.GetAvailableFormats()
                .Select(entry => new
                {
                    id = entry.Format.Name.Replace(" ", string.Empty),
                    name = entry.Format.Name,
                    extension = entry.Format.Extension,
                    type = entry.Kind.StartsWith("binary", StringComparison.Ordinal) ? "binary" : "text",
                    inputOnly = entry.Kind.Contains("(input)", StringComparison.Ordinal),
                })
                .Where(f => string.IsNullOrWhiteSpace(filter) ||
                            f.id.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                            f.name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                            f.extension.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return new
            {
                total = formats.Count,
                formats,
                extraIds = new[] { "plaintext", "customtext", "bluraysup", "vobsub", "bdnxml" },
            };
        });

    [McpServerTool(Name = "subtitle_info", ReadOnly = true, Idempotent = true)]
    [Description("Detect a subtitle file's format, encoding, paragraph count, first/last time codes, duration and language. Works for any of the 400+ supported formats, including binary ones.")]
    public static CallToolResult SubtitleInfo(
        [Description("Path to the subtitle file.")] string path)
        => Run(() => SubtitleInfoGatherer.Gather(path));

    [McpServerTool(Name = "read_subtitle", ReadOnly = true, Idempotent = true)]
    [Description("Read the paragraphs (number, start/end time, text) of a subtitle file in any supported text or binary format, paged. Use this instead of reading the raw file when the format is not plain SRT/VTT.")]
    public static CallToolResult ReadSubtitle(
        [Description("Path to the subtitle file.")] string path,
        [Description("1-based number of the first paragraph to return (default 1).")] int start = 1,
        [Description("Maximum number of paragraphs to return (default 100, max 1000).")] int count = DefaultReadCount,
        [Description("Input text encoding (e.g. 'windows-1252'). Default: auto-detect.")] string? encoding = null)
        => Run(() =>
        {
            var (subtitle, format) = LibSEIntegration.LoadSubtitleWithFormat(path, encoding);
            var total = subtitle.Paragraphs.Count;
            var first = Math.Max(1, start);
            var take = Math.Clamp(count, 1, MaxReadCount);

            var paragraphs = subtitle.Paragraphs
                .Skip(first - 1)
                .Take(take)
                .Select((p, i) => new
                {
                    number = first + i,
                    start = p.StartTime.ToDisplayString(),
                    end = p.EndTime.ToDisplayString(),
                    startMs = (long)p.StartTime.TotalMilliseconds,
                    endMs = (long)p.EndTime.TotalMilliseconds,
                    text = p.Text,
                })
                .ToList();

            return new
            {
                path,
                format = format.Name,
                total,
                start = first,
                returned = paragraphs.Count,
                hasMore = first - 1 + paragraphs.Count < total,
                paragraphs,
            };
        });

    [McpServerTool(Name = "lint_subtitle", ReadOnly = true, Idempotent = true)]
    [Description("Validate a subtitle file without modifying it: overlapping or too short/long display times, lines that are too long, too many lines, empty paragraphs, mismatched italic/bold tags. Returns the issues per paragraph number.")]
    public static CallToolResult LintSubtitle(
        [Description("Path to the subtitle file.")] string path)
        => Run(() => SubtitleLinter.Lint(path));

    [McpServerTool(Name = "list_fix_common_errors_rules", ReadOnly = true, Idempotent = true)]
    [Description("List the FixCommonErrors rule ids accepted by convert_subtitle's 'fixCommonErrorsRules' parameter, with the matching Subtitle Edit GUI label and the language gate (if any).")]
    public static CallToolResult ListFixCommonErrorsRules()
        => Run(() => new
        {
            total = FixCommonErrorsRunner.AvailableRuleIds.Count,
            rules = FixCommonErrorsRunner.AvailableRuleIds.Select(id => new
            {
                id,
                guiLabel = FixCommonErrorsRunner.GuiLabels.TryGetValue(id, out var label) ? label : null,
                languageGate = FixCommonErrorsRunner.LanguageGates.TryGetValue(id, out var lang) ? lang : null,
            }),
            syntax = new { all = "all", subset = "FixCommas,FixEllipsesStart", allExcept = "all,-FixDanishLetterI" },
            note = "A language-gated rule runs only when the subtitle's language matches (auto-detected, or forced with fixCommonErrorsLanguage). Naming a gated rule selects it but does not bypass the gate.",
        });

    [McpServerTool(Name = "list_remove_formatting_rules", ReadOnly = true, Idempotent = true)]
    [Description("List the RemoveFormatting rule ids accepted by convert_subtitle's 'removeFormattingRules' parameter, with the matching Subtitle Edit GUI label.")]
    public static CallToolResult ListRemoveFormattingRules()
        => Run(() => new
        {
            total = RemoveFormattingRunner.AvailableRuleIds.Count,
            rules = RemoveFormattingRunner.AvailableRuleIds.Select(id => new
            {
                id,
                guiLabel = RemoveFormattingRunner.GuiLabels.TryGetValue(id, out var label) ? label : null,
            }),
            syntax = new { all = "all", subset = "RemoveItalic,RemoveColor", allExcept = "all,-RemoveItalic" },
        });

    [McpServerTool(Name = "convert_subtitle", Idempotent = false)]
    [Description("Convert one or more subtitle files to another format, optionally shifting times, changing frame rate or applying operations (FixCommonErrors, RemoveFormatting, ...). Writes output files next to the inputs unless outputFolder is given; existing files are never overwritten unless overwrite is true. Returns per-file results with output paths, errors and warnings.")]
    public static Task<CallToolResult> ConvertSubtitle(
        [Description("Input file paths or glob patterns (e.g. 'C:/subs/*.srt'). Containers (.mkv/.mp4/.ts/.avi) and image-based subtitles (.sup, .sub+.idx) are accepted too.")] string[] inputs,
        [Description("Target format id from list_formats (e.g. 'SubRip', 'WebVTT', 'AdvancedSubStationAlpha'); short aliases such as 'srt', 'vtt', 'ass', 'ebu' and 'plaintext' also work.")] string format,
        [Description("Folder to write the output files to. Default: next to each input file.")] string? outputFolder = null,
        [Description("Explicit output file name. Only valid with a single input file.")] string? outputFilename = null,
        [Description("Output text encoding: 'utf-8' (default, with BOM), 'utf-8-nobom', a code page name such as 'windows-1252', or 'source' to keep the input file's encoding.")] string? encoding = null,
        [Description("Overwrite an existing output file. Default false: a numeric suffix is added instead.")] bool overwrite = false,
        [Description("Shift every time code by this offset, e.g. '00:00:02.500', '2.5', '-1.2' (seconds) or '-00:00:01,000'.")] string? offset = null,
        [Description("Source frame rate for frame-based formats (e.g. 25).")] double? fps = null,
        [Description("Convert timings from 'fps' to this target frame rate (e.g. 23.976).")] double? targetFps = null,
        [Description("Renumber paragraphs starting at this value.")] int? renumber = null,
        [Description("Add this many milliseconds to every paragraph's duration (negative shortens).")] int? adjustDurationMs = null,
        [Description("Speed change in percent: 125 = 1.25x faster, 80 = slower.")] double? changeSpeedPercent = null,
        [Description("Bridge gaps shorter than this many milliseconds by extending the previous paragraph.")] int? bridgeGapsMaxMs = null,
        [Description("Enforce a minimum gap of this many milliseconds between consecutive paragraphs.")] int? applyMinGapMs = null,
        [Description("Delete the first N paragraphs.")] int? deleteFirst = null,
        [Description("Delete the last N paragraphs.")] int? deleteLast = null,
        [Description("Delete every paragraph whose text contains this string.")] string? deleteContains = null,
        [Description("Operations to apply, in this order. Valid names: FixCommonErrors, RemoveFormatting, RemoveTextForHI, BalanceLines, SplitLongLines, MergeShortLines, MergeSameTexts, MergeSameTimeCodes, RedoCasing, ApplyDurationLimits, BeautifyTimeCodes, ConvertColorsToDialog, FixRtlViaUnicodeChars, ReverseRtlStartEnd, RemoveLineBreaks, RemoveUnicodeControlChars.")] string[]? operations = null,
        [Description("FixCommonErrors rule selection: comma-separated ids from list_fix_common_errors_rules, 'all', or 'all,-RuleId'. Implies the FixCommonErrors operation.")] string? fixCommonErrorsRules = null,
        [Description("Force the language used by FixCommonErrors' language-gated rules (two-letter code such as 'en' or 'es'). Default: auto-detect from the text.")] string? fixCommonErrorsLanguage = null,
        [Description("RemoveFormatting rule selection: comma-separated ids from list_remove_formatting_rules, or 'all,-RuleId'. Implies the RemoveFormatting operation.")] string? removeFormattingRules = null,
        [Description("Container inputs: subtitle track numbers to extract. Default: every text track.")] int[]? trackNumbers = null,
        [Description("Image-based inputs: keep only the time codes and skip OCR (text is left empty).")] bool timeCodesOnly = false,
        [Description("OCR engine for image-based inputs: tesseract (default), nocr, binaryocr, ollama, paddle or llamacpp.")] string? ocrEngine = null,
        [Description("OCR language for image-based inputs (Tesseract ISO 639-2 code such as 'eng').")] string? ocrLanguage = null,
        [Description("Output resolution for image-based targets, e.g. '1920x1080'.")] string? resolution = null)
        => RunAsync(async () =>
        {
            if (inputs is null || inputs.All(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("At least one input path or pattern is required.");
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentException("A target format is required. Use list_formats to see the ids.");
            }

            var ops = NormalizeOperations(operations);

            IReadOnlyList<string> fceRules = [];
            if (!string.IsNullOrWhiteSpace(fixCommonErrorsRules))
            {
                fceRules = FixCommonErrorsRunner.ResolveRuleIds(fixCommonErrorsRules);
                EnsureOperation(ops, "FixCommonErrors");
            }

            IReadOnlyList<string>? rfRules = null;
            if (!string.IsNullOrWhiteSpace(removeFormattingRules))
            {
                rfRules = RemoveFormattingRunner.ResolveRuleIds(removeFormattingRules);
                EnsureOperation(ops, "RemoveFormatting");
            }

            var options = new ConversionOptions
            {
                Patterns = inputs.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray(),
                Format = format,
                OutputFolder = outputFolder,
                OutputFilename = outputFilename,
                Encoding = encoding,
                Overwrite = overwrite,
                Offset = string.IsNullOrWhiteSpace(offset) ? null : OffsetParser.Parse(offset),
                Fps = fps,
                TargetFps = targetFps,
                Renumber = renumber,
                AdjustDurationMs = adjustDurationMs,
                ChangeSpeedPercent = changeSpeedPercent,
                BridgeGapsMaxMs = bridgeGapsMaxMs,
                ApplyMinGapMs = applyMinGapMs,
                DeleteFirst = deleteFirst,
                DeleteLast = deleteLast,
                DeleteContains = deleteContains,
                Operations = ops,
                FixCommonErrorsRules = fceRules,
                FixCommonErrorsLanguage = fixCommonErrorsLanguage,
                RemoveFormattingRules = rfRules,
                TrackNumbers = trackNumbers ?? [],
                TimeCodesOnly = timeCodesOnly,
                OcrEngine = string.IsNullOrWhiteSpace(ocrEngine) ? "tesseract" : ocrEngine,
                OcrLanguage = string.IsNullOrWhiteSpace(ocrLanguage) ? "eng" : ocrLanguage,
                Resolution = string.IsNullOrWhiteSpace(resolution) ? null : ResolutionParser.Parse(resolution),
                // The converter narrates progress on stdout when not quiet; stdout is the MCP channel.
                Quiet = true,
            };

            return await new SubtitleConverter().ConvertAsync(options);
        });

    /// <summary>
    /// Maps caller-supplied operation names onto the canonical names in
    /// <see cref="OperationOrderParser.ToggleOperations"/> (case-insensitive, dashes and
    /// underscores ignored, so "fix-common-errors" and "fixcommonerrors" both work). An unknown
    /// name is an error rather than a silent no-op, matching the CLI's strict option parsing.
    /// </summary>
    private static List<string> NormalizeOperations(string[]? operations)
    {
        var result = new List<string>();
        if (operations is null)
        {
            return result;
        }

        foreach (var raw in operations)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var key = raw.Replace("-", string.Empty).Replace("_", string.Empty);
            var canonical = OperationOrderParser.ToggleOperations
                .FirstOrDefault(op => op.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (canonical is null)
            {
                throw new ArgumentException(
                    $"Unknown operation '{raw}'. Valid operations: {string.Join(", ", OperationOrderParser.ToggleOperations)}.");
            }

            result.Add(canonical);
        }

        return result;
    }

    private static void EnsureOperation(List<string> operations, string name)
    {
        if (!operations.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            operations.Add(name);
        }
    }

    private static CallToolResult Run(Func<object> body)
    {
        try
        {
            return Ok(body());
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    private static async Task<CallToolResult> RunAsync(Func<Task<object>> body)
    {
        try
        {
            return Ok(await body());
        }
        catch (Exception ex)
        {
            return Error(ex);
        }
    }

    private static CallToolResult Ok(object value) => new()
    {
        Content = [new TextContentBlock { Text = JsonSerializer.Serialize(value, Json) }],
    };

    private static CallToolResult Error(Exception ex) => new()
    {
        IsError = true,
        Content = [new TextContentBlock { Text = ex.Message }],
    };
}
