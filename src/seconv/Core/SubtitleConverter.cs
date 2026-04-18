using Nikse.SubtitleEdit.Core.Common;
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

            var fileIndex = 1;
            foreach (var inputFile in inputFiles)
            {
                var outputFile = GetOutputFileName(inputFile, options);

                // Show progress: "1: source.srt -> target.ass..."
                AnsiConsole.Markup($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [dim]->[/] [green]{outputFile}[/]...");

                try
                {
                    await ConvertFileAsync(inputFile, options);
                    result.SuccessfulFiles++;

                    // Show completion
                    AnsiConsole.MarkupLine(" [green]done.[/]");
                }
                catch (Exception ex)
                {
                    result.FailedFiles++;
                    result.Errors.Add($"{Path.GetFileName(inputFile)}: {ex.Message}");

                    // Show error
                    AnsiConsole.MarkupLine($" [red]error: {ex.Message.EscapeMarkup()}[/]");
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

    private async Task ConvertFileAsync(string inputFile, ConversionOptions options)
    {
        // Determine output file
        var outputFile = GetOutputFileName(inputFile, options);

        // Check if file exists and overwrite is not set
        if (File.Exists(outputFile) && !options.Overwrite)
        {
            throw new InvalidOperationException($"Output file already exists: {outputFile}. Use --overwrite to replace.");
        }

        // Simulate async work for UI responsiveness
        await Task.Run(() =>
        {
            // Load subtitle file using LibSE
            var subtitle = LibSEIntegration.LoadSubtitle(inputFile, options.Encoding);

            if (subtitle == null || subtitle.Paragraphs.Count == 0)
            {
                throw new InvalidOperationException($"No subtitles found in file: {inputFile}");
            }

            // Apply FPS conversion if needed
            if (options.Fps.HasValue && options.TargetFps.HasValue)
            {
                // Convert between different frame rates
                ChangeFrameRate(subtitle, options.Fps.Value, options.TargetFps.Value);
            }
            else if (options.Fps.HasValue)
            {
                // If only source FPS is specified, this might be for frame-based formats
                // The format itself should handle this during loading
            }

            // Apply delete operations
            if (options.DeleteFirst.HasValue)
                LibSEIntegration.DeleteFirst(subtitle, options.DeleteFirst.Value);

            if (options.DeleteLast.HasValue)
                LibSEIntegration.DeleteLast(subtitle, options.DeleteLast.Value);

            if (!string.IsNullOrEmpty(options.DeleteContains))
                LibSEIntegration.DeleteContains(subtitle, options.DeleteContains);

            // Apply operations in order
            if (options.Operations.Count > 0)
            {
                LibSEIntegration.ApplyOperations(subtitle, options.Operations);
            }

            // Normalize format name (handle shortcuts like "srt", "ass", etc.)
            var normalizedFormat = LibSEIntegration.NormalizeFormatName(options.Format);

            // Save to target format
            LibSEIntegration.SaveSubtitle(subtitle, outputFile, normalizedFormat, options.Encoding);
        });
    }

    private static void ChangeFrameRate(Subtitle subtitle, double fromFrameRate, double toFrameRate)
    {
        double ratio = toFrameRate / fromFrameRate;
        foreach (var p in subtitle.Paragraphs)
        {
            p.StartTime.TotalMilliseconds = p.StartTime.TotalMilliseconds * ratio;
            p.EndTime.TotalMilliseconds = p.EndTime.TotalMilliseconds * ratio;
        }
    }

    private string GetOutputFileName(string inputFile, ConversionOptions options)
    {
        if (!string.IsNullOrEmpty(options.OutputFilename))
        {
            return options.OutputFilename;
        }

        var fileName = Path.GetFileNameWithoutExtension(inputFile);
        var extension = LibSEIntegration.GetExtensionForFormat(options.Format);

        var outputFolder = string.IsNullOrEmpty(options.OutputFolder)
            ? Path.GetDirectoryName(inputFile) ?? Directory.GetCurrentDirectory()
            : options.OutputFolder;

        return Path.Combine(outputFolder, fileName + extension);
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
}

internal class ConversionResult
{
    public int TotalFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool Success => FailedFiles == 0 && Errors.Count == 0;
}
