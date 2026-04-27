using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Spectre.Console;

namespace SeConv.Core;

internal class SubtitleConverter
{
    public async Task<ConversionResult> ConvertAsync(ConversionOptions options)
    {
        var result = new ConversionResult();

        try
        {
            // Get input files
            var inputFiles = GetInputFiles(options);
            result.TotalFiles = inputFiles.Count;

            if (inputFiles.Count == 0)
            {
                result.Errors.Add($"No files found matching pattern(s): {string.Join(", ", options.Patterns)}");
                return result;
            }

            // --outputfilename only makes sense for a single input file (matches old SE)
            if (inputFiles.Count > 1 && !string.IsNullOrEmpty(options.OutputFilename))
            {
                result.Errors.Add($"--outputfilename can only be used with a single input file (got {inputFiles.Count} matches).");
                return result;
            }

            var fileIndex = 1;
            foreach (var inputFile in inputFiles)
            {
                try
                {
                    var tracks = ContainerSubtitleLoader.TryLoadTracks(inputFile, options);
                    if (tracks is null)
                    {
                        // Regular single-file path (text or binary subtitle file)
                        var outputFile = ResolveOutputFileName(inputFile, options);
                        if (!options.Quiet)
                        {
                            AnsiConsole.Markup($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [dim]->[/] [green]{outputFile}[/]...");
                        }
                        await ConvertFileAsync(inputFile, outputFile, options);
                        result.SuccessfulFiles++;
                        if (!options.Quiet)
                        {
                            AnsiConsole.MarkupLine(" [green]done.[/]");
                        }
                    }
                    else
                    {
                        // Container input (.mkv / .mp4 / .mcc) — one output per usable track
                        if (tracks.Count > 1 && !string.IsNullOrEmpty(options.OutputFilename))
                        {
                            throw new InvalidOperationException(
                                "--outputfilename can only target a single track. Use --track-number to select one.");
                        }

                        foreach (var track in tracks)
                        {
                            var outputFile = ResolveOutputFileName(inputFile, options, track.LanguageCode, track.TrackNumber);
                            var trackLabel = track.TrackNumber.HasValue ? $"#{track.TrackNumber.Value} " : string.Empty;
                            var langLabel = string.IsNullOrEmpty(track.LanguageCode) ? string.Empty : $"[[{track.LanguageCode.EscapeMarkup()}]] ";
                            if (!options.Quiet)
                            {
                                AnsiConsole.Markup($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [yellow]{trackLabel.EscapeMarkup()}[/][blue]{langLabel}[/][dim]->[/] [green]{outputFile}[/]...");
                            }
                            await ConvertTrackAsync(track, outputFile, options);
                            result.SuccessfulFiles++;
                            if (!options.Quiet)
                            {
                                AnsiConsole.MarkupLine(" [green]done.[/]");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.FailedFiles++;
                    result.Errors.Add($"{Path.GetFileName(inputFile)}: {ex.Message}");

                    if (!options.Quiet)
                    {
                        AnsiConsole.MarkupLine($" [red]error: {ex.Message.EscapeMarkup()}[/]");
                    }
                }

                fileIndex++;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Conversion failed: {ex.Message}");
        }

        return result;
    }

    private List<string> GetInputFiles(ConversionOptions options)
    {
        var files = new List<string>();

        var baseFolder = string.IsNullOrEmpty(options.InputFolder)
            ? Directory.GetCurrentDirectory()
            : options.InputFolder;

        foreach (var entry in options.Patterns)
        {
            // Support comma-separated patterns within a single value for backward compatibility,
            // but only when the entry itself is not a rooted path (to avoid splitting paths that contain commas).
            var subPatterns = !Path.IsPathRooted(entry) && entry.Contains(',')
                ? entry.Split(',', StringSplitOptions.RemoveEmptyEntries)
                : (IEnumerable<string>)[entry];

            foreach (var pattern in subPatterns)
            {
                var trimmedPattern = pattern.Trim();
                var searchPath = Path.IsPathRooted(trimmedPattern)
                    ? trimmedPattern
                    : Path.Combine(baseFolder, trimmedPattern);

                var directory = Path.GetDirectoryName(searchPath) ?? baseFolder;
                var filePattern = Path.GetFileName(searchPath);

                if (Directory.Exists(directory))
                {
                    files.AddRange(Directory.GetFiles(directory, filePattern));
                }
            }
        }

        return files;
    }

    private async Task ConvertFileAsync(string inputFile, string outputFile, ConversionOptions options)
    {
        // Frame-based formats (e.g. MicroDVD) read Configuration.Settings.General.CurrentFrameRate
        // when loading. Set it before LoadSubtitle and restore in finally so concurrent or
        // subsequent files aren't affected.
        var originalFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            if (options.Fps.HasValue)
            {
                Configuration.Settings.General.CurrentFrameRate = options.Fps.Value;
            }

            await Task.Run(() =>
            {
                // Load subtitle file using LibSE — keep the detected format so the
                // save side can apply RemoveNativeFormatting when the target differs.
                var (subtitle, sourceFormat) = LibSEIntegration.LoadSubtitleWithFormat(inputFile, options.Encoding);

                if (subtitle == null || subtitle.Paragraphs.Count == 0)
                {
                    throw new InvalidOperationException($"No subtitles found in file: {inputFile}");
                }

                ApplyTransformsAndSave(subtitle, sourceFormat, outputFile, options);
            });
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalFrameRate;
        }
    }

    private async Task ConvertTrackAsync(ContainerSubtitleLoader.LoadedTrack track, string outputFile, ConversionOptions options)
    {
        var originalFrameRate = Configuration.Settings.General.CurrentFrameRate;
        try
        {
            if (options.Fps.HasValue)
            {
                Configuration.Settings.General.CurrentFrameRate = options.Fps.Value;
            }

            await Task.Run(() =>
            {
                if (track.Subtitle.Paragraphs.Count == 0)
                {
                    throw new InvalidOperationException("Track is empty.");
                }

                ApplyTransformsAndSave(track.Subtitle, track.Format, outputFile, options);
            });
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalFrameRate;
        }
    }

    private static void ApplyTransformsAndSave(Subtitle subtitle, SubtitleFormat sourceFormat, string outputFile, ConversionOptions options)
    {
        // Apply offset (must be first, before any time-based transforms)
        if (options.Offset.HasValue && options.Offset.Value != TimeSpan.Zero)
        {
            subtitle.AddTimeToAllParagraphs(options.Offset.Value);
        }

        // Apply target frame rate via libse (handles frame-based formats correctly)
        if (options.Fps.HasValue && options.TargetFps.HasValue)
        {
            subtitle.ChangeFrameRate(options.Fps.Value, options.TargetFps.Value);
        }

        if (options.Renumber.HasValue)
        {
            subtitle.Renumber(options.Renumber.Value);
        }

        if (options.AdjustDurationMs.HasValue)
        {
            subtitle.AdjustDisplayTimeUsingSeconds(options.AdjustDurationMs.Value / 1000.0, null);
        }

        if (options.DeleteFirst.HasValue)
            LibSEIntegration.DeleteFirst(subtitle, options.DeleteFirst.Value);

        if (options.DeleteLast.HasValue)
            LibSEIntegration.DeleteLast(subtitle, options.DeleteLast.Value);

        if (!string.IsNullOrEmpty(options.DeleteContains))
            LibSEIntegration.DeleteContains(subtitle, options.DeleteContains);

        if (options.Operations.Count > 0)
        {
            LibSEIntegration.ApplyOperations(subtitle, options.Operations);
        }

        if (!string.IsNullOrWhiteSpace(options.MultipleReplaceFile))
        {
            MultipleReplaceLoader.Apply(subtitle, options.MultipleReplaceFile);
        }

        var normalizedFormat = LibSEIntegration.NormalizeFormatName(options.Format);

        // ASSA-only post-processing: resolution + style-file overlay
        LibSEIntegration.ApplyAssaPostProcess(subtitle, normalizedFormat, options.Resolution, options.AssaStyleFile);

        LibSEIntegration.SaveSubtitle(
            subtitle,
            outputFile,
            normalizedFormat,
            options.Encoding,
            sourceFormat,
            options.PacCodePage,
            options.EbuHeaderFile,
            options);
    }

    /// <summary>
    /// Resolves the output file name. Honors --outputfilename / --outputfolder. When
    /// <paramref name="languageSuffix"/> is non-empty (container tracks), inserts it
    /// between the stem and extension (<c>movie.eng.srt</c>). On collision: tries
    /// <c>movie.#&lt;track&gt;.eng.srt</c> first when a track number is given, then
    /// rotates to <c>name_2.ext</c>, <c>name_3.ext</c>, ... up to 9999.
    /// </summary>
    internal static string ResolveOutputFileName(
        string inputFile,
        ConversionOptions options,
        string? languageSuffix = null,
        int? trackNumber = null)
    {
        string baseName;
        if (!string.IsNullOrEmpty(options.OutputFilename))
        {
            baseName = options.OutputFilename;
        }
        else
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var extension = LibSEIntegration.GetExtensionForFormat(options.Format);
            var outputFolder = string.IsNullOrEmpty(options.OutputFolder)
                ? Path.GetDirectoryName(inputFile) ?? Directory.GetCurrentDirectory()
                : options.OutputFolder;
            var stemWithLang = string.IsNullOrEmpty(languageSuffix)
                ? fileName
                : $"{fileName}.{languageSuffix}";
            baseName = Path.Combine(outputFolder, stemWithLang + extension);
        }

        if (options.Overwrite || !File.Exists(baseName))
        {
            return baseName;
        }

        var dir = Path.GetDirectoryName(baseName) ?? string.Empty;
        var stem = Path.GetFileNameWithoutExtension(baseName);
        var ext = Path.GetExtension(baseName);

        // For container collisions, try inserting the track number first
        if (trackNumber.HasValue && !string.IsNullOrEmpty(languageSuffix))
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var withTrack = Path.Combine(dir, $"{fileName}.#{trackNumber.Value}.{languageSuffix}{ext}");
            if (!File.Exists(withTrack))
            {
                return withTrack;
            }
        }

        for (var i = 2; i < 10_000; i++)
        {
            var candidate = Path.Combine(dir, $"{stem}_{i}{ext}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }
        throw new InvalidOperationException($"Could not find a free output filename for: {baseName}");
    }
}

internal class ConversionOptions
{
    public required IReadOnlyList<string> Patterns { get; init; }
    public required string Format { get; init; }
    public string? InputFolder { get; init; }
    public string? OutputFolder { get; init; }
    public string? OutputFilename { get; init; }
    public string? Encoding { get; init; }
    public double? Fps { get; init; }
    public double? TargetFps { get; init; }
    public bool Overwrite { get; init; }
    public List<string> Operations { get; init; } = new();
    public int? DeleteFirst { get; init; }
    public int? DeleteLast { get; init; }
    public string? DeleteContains { get; init; }
    public TimeSpan? Offset { get; init; }
    public int? Renumber { get; init; }
    public int? AdjustDurationMs { get; init; }
    public (int Width, int Height)? Resolution { get; init; }
    public string? AssaStyleFile { get; init; }
    public int? PacCodePage { get; init; }
    public string? EbuHeaderFile { get; init; }
    public string? MultipleReplaceFile { get; init; }
    public string? CustomFormatFile { get; init; }
    public IReadOnlyList<int> TrackNumbers { get; init; } = [];
    public bool ForcedOnly { get; init; }

    /// <summary>OCR engine identifier: <c>tesseract</c> | <c>nocr</c> | <c>ollama</c> | <c>paddle</c>.</summary>
    public string OcrEngine { get; init; } = "tesseract";

    /// <summary>Language code or human name passed to the OCR engine (Tesseract: ISO 639-2 like <c>eng</c>; Paddle: <c>en</c>; Ollama: human name like <c>English</c>).</summary>
    public string OcrLanguage { get; init; } = "eng";

    /// <summary>Path to a <c>.nocr</c> database file (required when <c>OcrEngine == "nocr"</c>).</summary>
    public string? OcrDb { get; init; }

    /// <summary>Ollama API endpoint (default <c>http://localhost:11434/api/chat</c>).</summary>
    public string? OllamaUrl { get; init; }

    /// <summary>Ollama vision model (default <c>llama3.2-vision</c>).</summary>
    public string? OllamaModel { get; init; }

    public bool TeletextOnly { get; init; }
    public bool SkipTeletext { get; init; }
    public int? TeletextOnlyPage { get; init; }

    public bool Quiet { get; init; }
    public bool Verbose { get; init; }
}

internal class ConversionResult
{
    public int TotalFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success => FailedFiles == 0 && Errors.Count == 0;
}
