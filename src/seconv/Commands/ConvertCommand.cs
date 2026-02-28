using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using SeConv.Core;

namespace SeConv.Commands;

internal sealed class ConvertCommand : AsyncCommand<ConvertCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<pattern>")]
        [Description("One or more file name patterns separated by commas")]
        public string Pattern { get; init; } = string.Empty;

        [CommandArgument(1, "<format>")]
        [Description("Name of format without spaces")]
        public string Format { get; init; } = string.Empty;

        [CommandOption("--adjustduration")]
        [Description("Adjust duration in milliseconds")]
        public int? AdjustDuration { get; init; }

        [CommandOption("--assa-style-file")]
        [Description("ASSA style file name")]
        public string? AssaStyleFile { get; init; }

        [CommandOption("--ebuheaderfile")]
        [Description("EBU header file name")]
        public string? EbuHeaderFile { get; init; }

        [CommandOption("--encoding")]
        [Description("Encoding name")]
        public string? Encoding { get; init; }

        [CommandOption("--forcedonly")]
        [Description("Forced subtitles only")]
        public bool ForcedOnly { get; init; }

        [CommandOption("--fps")]
        [Description("Frame rate")]
        public double? Fps { get; init; }

        [CommandOption("--inputfolder")]
        [Description("Input folder name")]
        public string? InputFolder { get; init; }

        [CommandOption("--multiplereplace")]
        [Description("Multiple replace (default rules or comma separated file list)")]
        public string? MultipleReplace { get; init; }

        [CommandOption("--ocrengine")]
        [Description("OCR engine (tesseract/nOCR)")]
        public string? OcrEngine { get; init; }

        [CommandOption("--offset")]
        [Description("Offset time (hh:mm:ss:ms)")]
        public string? Offset { get; init; }

        [CommandOption("--outputfilename")]
        [Description("Output file name (for single file only)")]
        public string? OutputFilename { get; init; }

        [CommandOption("--outputfolder")]
        [Description("Output folder name")]
        public string? OutputFolder { get; init; }

        [CommandOption("--overwrite")]
        [Description("Overwrite existing files")]
        public bool Overwrite { get; init; }

        [CommandOption("--pac-codepage")]
        [Description("PAC code page")]
        public string? PacCodepage { get; init; }

        [CommandOption("--profile")]
        [Description("Profile name")]
        public string? Profile { get; init; }

        [CommandOption("--renumber")]
        [Description("Renumber starting from this number")]
        public int? Renumber { get; init; }

        [CommandOption("--resolution")]
        [Description("Resolution (widthxheight)")]
        public string? Resolution { get; init; }

        [CommandOption("--targetfps")]
        [Description("Target frame rate")]
        public double? TargetFps { get; init; }

        [CommandOption("--teletextonly")]
        [Description("Teletext only")]
        public bool TeletextOnly { get; init; }

        [CommandOption("--teletextonlypage")]
        [Description("Teletext page number")]
        public int? TeletextOnlyPage { get; init; }

        [CommandOption("--track-number")]
        [Description("Comma separated track number list")]
        public string? TrackNumber { get; init; }

        // Operations
        [CommandOption("--ApplyDurationLimits")]
        [Description("Apply duration limits")]
        public bool ApplyDurationLimits { get; init; }

        [CommandOption("--BalanceLines")]
        [Description("Balance lines")]
        public bool BalanceLines { get; init; }

        [CommandOption("--BeautifyTimeCodes")]
        [Description("Beautify time codes")]
        public bool BeautifyTimeCodes { get; init; }

        [CommandOption("--ConvertColorsToDialog")]
        [Description("Convert colors to dialog")]
        public bool ConvertColorsToDialog { get; init; }

        [CommandOption("--DeleteFirst")]
        [Description("Delete first N entries")]
        public int? DeleteFirst { get; init; }

        [CommandOption("--DeleteLast")]
        [Description("Delete last N entries")]
        public int? DeleteLast { get; init; }

        [CommandOption("--DeleteContains")]
        [Description("Delete entries containing this word")]
        public string? DeleteContains { get; init; }

        [CommandOption("--FixCommonErrors")]
        [Description("Fix common errors")]
        public bool FixCommonErrors { get; init; }

        [CommandOption("--FixRtlViaUnicodeChars")]
        [Description("Fix RTL via Unicode characters")]
        public bool FixRtlViaUnicodeChars { get; init; }

        [CommandOption("--MergeSameTexts")]
        [Description("Merge same texts")]
        public bool MergeSameTexts { get; init; }

        [CommandOption("--MergeSameTimeCodes")]
        [Description("Merge same time codes")]
        public bool MergeSameTimeCodes { get; init; }

        [CommandOption("--MergeShortLines")]
        [Description("Merge short lines")]
        public bool MergeShortLines { get; init; }

        [CommandOption("--RedoCasing")]
        [Description("Redo casing")]
        public bool RedoCasing { get; init; }

        [CommandOption("--RemoveFormatting")]
        [Description("Remove formatting")]
        public bool RemoveFormatting { get; init; }

        [CommandOption("--RemoveLineBreaks")]
        [Description("Remove line breaks")]
        public bool RemoveLineBreaks { get; init; }

        [CommandOption("--RemoveTextForHI")]
        [Description("Remove text for hearing impaired")]
        public bool RemoveTextForHI { get; init; }

        [CommandOption("--RemoveUnicodeControlChars")]
        [Description("Remove Unicode control characters")]
        public bool RemoveUnicodeControlChars { get; init; }

        [CommandOption("--ReverseRtlStartEnd")]
        [Description("Reverse RTL start/end")]
        public bool ReverseRtlStartEnd { get; init; }

        [CommandOption("--SplitLongLines")]
        [Description("Split long lines")]
        public bool SplitLongLines { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            AnsiConsole.MarkupLine("[bold cyan]Subtitle Edit - Batch Converter[/]");
            AnsiConsole.WriteLine();

            // Validate input
            if (string.IsNullOrWhiteSpace(settings.Pattern))
            {
                AnsiConsole.MarkupLine("[red]Error: Pattern is required[/]");
                return 1;
            }

            if (string.IsNullOrWhiteSpace(settings.Format))
            {
                AnsiConsole.MarkupLine("[red]Error: Format is required[/]");
                return 1;
            }

            // Build operations list
            var operations = new List<string>();
            if (settings.ApplyDurationLimits) operations.Add("ApplyDurationLimits");
            if (settings.BalanceLines) operations.Add("BalanceLines");
            if (settings.BeautifyTimeCodes) operations.Add("BeautifyTimeCodes");
            if (settings.ConvertColorsToDialog) operations.Add("ConvertColorsToDialog");
            if (settings.FixCommonErrors) operations.Add("FixCommonErrors");
            if (settings.FixRtlViaUnicodeChars) operations.Add("FixRtlViaUnicodeChars");
            if (settings.MergeSameTexts) operations.Add("MergeSameTexts");
            if (settings.MergeSameTimeCodes) operations.Add("MergeSameTimeCodes");
            if (settings.MergeShortLines) operations.Add("MergeShortLines");
            if (settings.RedoCasing) operations.Add("RedoCasing");
            if (settings.RemoveFormatting) operations.Add("RemoveFormatting");
            if (settings.RemoveLineBreaks) operations.Add("RemoveLineBreaks");
            if (settings.RemoveTextForHI) operations.Add("RemoveTextForHI");
            if (settings.RemoveUnicodeControlChars) operations.Add("RemoveUnicodeControlChars");
            if (settings.ReverseRtlStartEnd) operations.Add("ReverseRtlStartEnd");
            if (settings.SplitLongLines) operations.Add("SplitLongLines");

            // Create conversion options
            var options = new ConversionOptions
            {
                Pattern = settings.Pattern,
                Format = settings.Format,
                InputFolder = settings.InputFolder,
                OutputFolder = settings.OutputFolder,
                OutputFilename = settings.OutputFilename,
                Encoding = settings.Encoding,
                Fps = settings.Fps,
                TargetFps = settings.TargetFps,
                Overwrite = settings.Overwrite,
                Operations = operations
            };

            // Display conversion info
            var normalizedFormat = LibSEIntegration.NormalizeFormatName(settings.Format);
            var extension = LibSEIntegration.GetExtensionForFormat(settings.Format);
            var formatDisplay = $"{normalizedFormat} (*{extension})";

            var table = new Table();
            table.AddColumn("[yellow]Parameter[/]");
            table.AddColumn("[green]Value[/]");
            table.AddRow("Pattern", settings.Pattern);
            table.AddRow("Format", formatDisplay);

            if (!string.IsNullOrEmpty(settings.InputFolder))
                table.AddRow("Input Folder", settings.InputFolder);

            if (!string.IsNullOrEmpty(settings.OutputFolder))
                table.AddRow("Output Folder", settings.OutputFolder);

            if (settings.Fps.HasValue)
                table.AddRow("FPS", settings.Fps.Value.ToString());

            if (settings.TargetFps.HasValue)
                table.AddRow("Target FPS", settings.TargetFps.Value.ToString());

            if (!string.IsNullOrEmpty(settings.Encoding))
                table.AddRow("Encoding", settings.Encoding);

            if (operations.Count > 0)
                table.AddRow("Operations", string.Join(", ", operations));

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            // Perform conversion
            var converter = new SubtitleConverter();
            var result = await converter.ConvertAsync(options);

            // Display results
            AnsiConsole.WriteLine();
            if (result.Success)
            {
                AnsiConsole.MarkupLine($"[green]✓[/] Conversion completed successfully!");
                AnsiConsole.MarkupLine($"[dim]Converted {result.SuccessfulFiles} file(s)[/]");
            }
            else
            {
                if (result.SuccessfulFiles > 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]⚠[/] Conversion completed with errors");
                    AnsiConsole.MarkupLine($"[green]Successful: {result.SuccessfulFiles}[/]");
                    AnsiConsole.MarkupLine($"[red]Failed: {result.FailedFiles}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]✗[/] Conversion failed");
                }

                if (result.Errors.Count > 0)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[red]Errors:[/]");
                    foreach (var error in result.Errors)
                    {
                        AnsiConsole.MarkupLine($"  [red]•[/] {error}");
                    }
                }

                return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            if (ex.InnerException != null)
            {
                AnsiConsole.MarkupLine($"[dim]{ex.InnerException.Message}[/]");
            }
            return 1;
        }
    }
}
