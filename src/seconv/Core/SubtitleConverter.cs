using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using Spectre.Console;

namespace SeConv.Core;

internal class SubtitleConverter
{
    // Created once per run when --translate-to is set; validates the engine setup up front
    // (and, for local llama.cpp, resolves the server binary + model) so a broken translate
    // configuration fails before the first file is touched.
    private AutoTranslateRunner? _translateRunner;

    // Output names handed out during the current run - --overwrite must only clobber
    // files from before the run, never output this run just wrote (two "eng" tracks in
    // one mkv would otherwise silently overwrite each other).
    private readonly HashSet<string> _usedOutputFileNames = new(StringComparer.OrdinalIgnoreCase);

    public async Task<ConversionResult> ConvertAsync(ConversionOptions options)
    {
        var result = new ConversionResult();
        _usedOutputFileNames.Clear();

        try
        {
            if (!string.IsNullOrWhiteSpace(options.TranslateTo))
            {
                _translateRunner = AutoTranslateRunner.Create(options);
            }

            // Get input files
            var inputFiles = GetInputFiles(options);
            result.TotalFiles = inputFiles.Count;

            if (inputFiles.Count == 0)
            {
                result.Errors.Add($"No files found matching pattern(s): {string.Join(", ", options.Patterns)}");
                return result;
            }

            // DVD VOB inputs are handled as one batch — the subpicture stream spans every
            // VTS_xx_*.VOB chunk of a title, so we extract them together. (One output pair
            // per discovered subtitle stream / language — see ConvertVobBatchAsync.) The
            // caller's pattern (e.g. "*.VOB") naturally selects all chunks of the title.
            // GetFiles enumeration order isn't guaranteed and chunk order affects packet
            // ordering and idx timestamps, so sort up front. DVD spec caps a VTS at 9
            // chunks, so a plain ordinal sort puts VTS_xx_1..VTS_xx_9 in playback order.
            if (inputFiles.All(f => f.EndsWith(".vob", StringComparison.OrdinalIgnoreCase)))
            {
                inputFiles.Sort(StringComparer.OrdinalIgnoreCase);
                return await ConvertVobBatchAsync(inputFiles, options, result);
            }

            // --output-filename only makes sense for a single input file (matches old SE)
            if (inputFiles.Count > 1 && !string.IsNullOrEmpty(options.OutputFilename))
            {
                result.Errors.Add($"--output-filename can only be used with a single input file (got {inputFiles.Count} matches).");
                return result;
            }

            // Image-to-image preserve path: if the user asks for an image-based target
            // (.sup, .sub/.idx, BDN-XML, DOST, FCP, D-Cinema, ImagesWithTimeCode, WebVTT
            // thumbnail) and the input is an image-based source (.sup, .sub+.idx, MKV PGS,
            // TS DVB-sub), pass the source bitmaps straight through instead of OCR'ing them
            // to text and re-rasterising at the CLI's default font. Detect once up front so
            // the per-file loop below sees only text-eligible inputs.
            var imageTargetHandler = ImageOutputWriter.TryCreateHandler(LibSEIntegration.NormalizeFormatName(options.Format));

            var fileIndex = 1;
            foreach (var inputFile in inputFiles)
            {
                // --keep-timestamp: read the source's timestamps before anything is written, as
                // --overwrite can make the output the input file itself.
                var sourceTimestamps = options.KeepTimestamp ? FileTimestampHelper.Capture(inputFile) : null;
                try
                {
                    if (imageTargetHandler is not null
                        && await TryConvertImageToImageAsync(inputFile, options, result, fileIndex, sourceTimestamps))
                    {
                        fileIndex++;
                        continue;
                    }

                    // --translate-to rewrites the content's language, so the output name
                    // carries the target code ("way.zh-CN.srt") instead of the source
                    // track's - and a plain file no longer collides with its own input.
                    var translateToSuffix = _translateRunner?.TargetLanguageCode;

                    var tracks = ContainerSubtitleLoader.TryLoadTracks(inputFile, options);
                    if (tracks is null)
                    {
                        // Regular single-file path (text or binary subtitle file)
                        var outputFile = ResolveOutputFileName(inputFile, options, translateToSuffix, null, _usedOutputFileNames);
                        if (!options.Quiet)
                        {
                            AnsiConsole.MarkupInterpolated($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [dim]->[/] [green]{outputFile}[/]...");
                        }
                        var warnings = new List<string>();
                        await ConvertFileAsync(inputFile, outputFile, options, warnings);
                        result.SuccessfulFiles++;
                        ApplySourceTimestamp(sourceTimestamps, outputFile);
                        result.Files.Add(new FileConversionResult(inputFile, outputFile, true, null, warnings.Count > 0 ? warnings : null));
                        foreach (var warning in warnings)
                        {
                            result.Warnings.Add($"{Path.GetFileName(inputFile)}: {warning}");
                        }

                        if (!options.Quiet)
                        {
                            AnsiConsole.MarkupLine(" [green]done.[/]");
                            foreach (var warning in warnings)
                            {
                                AnsiConsole.MarkupLineInterpolated($"   [yellow]warning: {warning}[/]");
                            }
                        }
                    }
                    else
                    {
                        // Container input (.mkv / .mp4 / .mcc) — one output per usable track
                        if (tracks.Count > 1 && !string.IsNullOrEmpty(options.OutputFilename))
                        {
                            throw new InvalidOperationException(
                                "--output-filename can only target a single track. Use --track-number to select one.");
                        }

                        foreach (var track in tracks)
                        {
                            var outputFile = ResolveOutputFileName(inputFile, options, AppendForcedToken(translateToSuffix ?? track.LanguageCode, track.IsForced), track.TrackNumber, _usedOutputFileNames);
                            var trackLabel = track.TrackNumber.HasValue ? $"#{track.TrackNumber.Value} " : string.Empty;
                            var langLabel = string.IsNullOrEmpty(track.LanguageCode) ? string.Empty : $"[{track.LanguageCode}] ";
                            if (!options.Quiet)
                            {
                                AnsiConsole.MarkupInterpolated($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [yellow]{trackLabel}[/][blue]{langLabel}[/][dim]->[/] [green]{outputFile}[/]...");
                            }
                            await ConvertTrackAsync(track, outputFile, options);
                            result.SuccessfulFiles++;
                            ApplySourceTimestamp(sourceTimestamps, outputFile);
                            result.Files.Add(new FileConversionResult(inputFile, outputFile, true, null));
                            if (!options.Quiet)
                            {
                                AnsiConsole.MarkupLine(" [green]done.[/]");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = ErrorMessageFormatter.FormatForUser(ex, options.Verbose);
                    result.FailedFiles++;
                    result.Errors.Add($"{Path.GetFileName(inputFile)}: {msg}");
                    result.Files.Add(new FileConversionResult(inputFile, null, false, msg));

                    if (!options.Quiet)
                    {
                        AnsiConsole.MarkupLineInterpolated($" [red]error: {msg}[/]");
                    }
                }

                fileIndex++;
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Conversion failed: {ErrorMessageFormatter.FormatForUser(ex, options.Verbose)}");
        }

        return result;
    }

    /// <summary>
    /// Extract the subpicture stream from one or more DVD .VOB files into a single
    /// .sub + .idx pair. Issue #15 in subtitleedit-cli — VOBs are 1+ GB MPEG-PS
    /// containers, so the regular text-loading path used to error with
    /// "input file too large". This bypasses it.
    /// </summary>
    private async Task<ConversionResult> ConvertVobBatchAsync(List<string> vobFiles, ConversionOptions options, ConversionResult result)
    {
        if (!"vobsub".Equals(options.Format, StringComparison.OrdinalIgnoreCase))
        {
            result.Errors.Add(
                "VOB input is currently only supported with target format 'VobSub'. "
                + "Re-run with --format VobSub to extract subtitles to .sub + .idx, then convert that .sub to a text format (e.g. seconv movie.sub subrip) to OCR it.");
            result.FailedFiles = vobFiles.Count;
            return result;
        }

        // The VOB extractor copies subpicture packets verbatim - the timing transforms
        // never run on this path, so refuse them rather than silently ignore them.
        ThrowOnUnsupportedImagePassThroughOptions(options);

        // Derive output base name: --output-filename wins, otherwise use the first VOB's
        // stem dropped one underscore-segment (so VTS_01_1.VOB becomes VTS_01.sub for the
        // typical DVD layout). Falls back to the bare stem when there's no underscore.
        string outputBase;
        if (!string.IsNullOrEmpty(options.OutputFilename))
        {
            // Same rule as ResolveOutputFileName: a relative --output-filename still lands
            // in --output-folder; an absolute one wins outright.
            outputBase = !string.IsNullOrEmpty(options.OutputFolder) && !Path.IsPathRooted(options.OutputFilename)
                ? Path.Combine(options.OutputFolder, options.OutputFilename)
                : options.OutputFilename;
            // VobSubWriter derives the .idx path as Substring(0, length - 3) + "idx", so
            // a user-supplied --output-filename without ".sub" (e.g. "movie" or "movie.txt")
            // would produce a broken companion path like "vieidx" or "movie.tidx". Force
            // the extension here so both .sub and .idx land next to each other.
            if (!outputBase.EndsWith(".sub", StringComparison.OrdinalIgnoreCase))
            {
                outputBase = Path.ChangeExtension(outputBase, ".sub");
            }
        }
        else
        {
            var first = vobFiles[0];
            var stem = Path.GetFileNameWithoutExtension(first);
            var lastUnderscore = stem.LastIndexOf('_');
            if (lastUnderscore > 0 && int.TryParse(stem.AsSpan(lastUnderscore + 1), out _))
            {
                stem = stem[..lastUnderscore];
            }
            var outputFolder = string.IsNullOrEmpty(options.OutputFolder)
                ? Path.GetDirectoryName(first) ?? Directory.GetCurrentDirectory()
                : options.OutputFolder;
            outputBase = Path.Combine(outputFolder, stem + ".sub");
        }

        // Overwrite check is best-effort against the base path. Multi-stream DVDs land
        // additional outputs at <stem>.<n>.sub which we can't predict without parsing —
        // those existing files will be overwritten silently. Acceptable for now since
        // multi-stream DVDs are rare and the user opted into the batch by passing all VOBs.
        if (!options.Overwrite)
        {
            var pair = new[] { outputBase, Path.ChangeExtension(outputBase, ".idx") };
            foreach (var p in pair)
            {
                if (File.Exists(p))
                {
                    result.Errors.Add($"Output file already exists: {p}. Pass --overwrite to replace it.");
                    result.FailedFiles = vobFiles.Count;
                    return result;
                }
            }
        }

        if (!options.Quiet)
        {
            var label = vobFiles.Count == 1
                ? Path.GetFileName(vobFiles[0])
                : $"{vobFiles.Count} VOB files";
            AnsiConsole.MarkupInterpolated($"[dim]vob:[/] [cyan]{label}[/] [dim]->[/] [green]{outputBase}[/]...");
        }

        try
        {
            // IsPal — there's no single reliable auto-detect from VOB alone (would need
            // IFO parsing). Default to PAL to match the GUI's batch converter. Future
            // work: add --vob-pal/--vob-ntsc and/or read VIDEO_TS.IFO.
            var outputs = VobSubExtractor.Extract(vobFiles, outputBase, isPal: true);
            result.SuccessfulFiles = vobFiles.Count;
            // Report the first stream's output path against each input VOB. With multiple
            // streams there's no clean 1:1 mapping back to inputs, but the OutputFile slot
            // is meant as a hint for the user — they'll see the full list in the summary.
            var primaryOutput = outputs[0].Path;
            if (options.KeepTimestamp)
            {
                var newestVob = vobFiles.OrderByDescending(File.GetLastWriteTimeUtc).First();
                var vobTimestamps = FileTimestampHelper.Capture(newestVob);
                foreach (var o in outputs)
                {
                    ApplySourceTimestamp(vobTimestamps, o.Path);
                }
            }
            foreach (var f in vobFiles)
            {
                result.Files.Add(new FileConversionResult(f, primaryOutput, true, null));
            }
            if (!options.Quiet)
            {
                if (outputs.Count == 1)
                {
                    AnsiConsole.MarkupLine($" [green]done ({outputs[0].Written} subtitle(s)).[/]");
                }
                else
                {
                    var totalWritten = outputs.Sum(o => o.Written);
                    AnsiConsole.MarkupLine($" [green]done ({totalWritten} subtitle(s) across {outputs.Count} streams).[/]");
                    foreach (var o in outputs)
                    {
                        AnsiConsole.MarkupLineInterpolated($"  [dim]stream 0x{o.StreamId:X2}:[/] [green]{o.Path}[/] ({o.Written})");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var msg = ErrorMessageFormatter.FormatForUser(ex, options.Verbose);
            result.FailedFiles = vobFiles.Count;
            result.Errors.Add($"VOB extraction failed: {msg}");
            foreach (var f in vobFiles)
            {
                result.Files.Add(new FileConversionResult(f, null, false, msg));
            }
            if (!options.Quiet)
            {
                AnsiConsole.MarkupLineInterpolated($" [red]error: {msg}[/]");
            }
        }

        await Task.CompletedTask;
        return result;
    }

    /// <summary>
    /// Image-to-image conversion: when both source and target are image-based, parse the
    /// source's bitmaps directly and feed them into the image output handler — no OCR, no
    /// re-rasterise at Arial 50pt. Supports .sup, .sub+.idx, MKV PGS, and TS DVB-sub on
    /// input. Returns true if the input was handled (recorded in <paramref name="result"/>),
    /// false if it should fall through to the OCR / text pipeline.
    /// </summary>
    private async Task<bool> TryConvertImageToImageAsync(string inputFile, ConversionOptions options, ConversionResult result, int fileIndex, FileTimestamps? sourceTimestamps)
    {
        var ext = Path.GetExtension(inputFile).ToLowerInvariant();

        if (ext == ".sup")
        {
            return await PassThroughSingleStreamAsync(inputFile, options, result, fileIndex, sourceTimestamps,
                () => BitmapSubtitleLoader.LoadBluRaySup(inputFile));
        }

        if (ext == ".sub")
        {
            // Treat .sub as VobSub input when an .idx companion exists, or when the .sub is a
            // binary VobSub stream even without one (read directly, default palette). A text
            // MicroDVD .sub starts with text, not the MPEG pack header, so it's left for the
            // text loader to detect via IsMine.
            var idxPath = Path.ChangeExtension(inputFile, ".idx");
            var hasIdx = File.Exists(idxPath);
            if (hasIdx || BitmapSubtitleLoader.IsBinaryVobSub(inputFile))
            {
                if (!hasIdx && !options.Quiet)
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]Note: VobSub '.sub' has no '.idx' companion ({Path.GetFileName(idxPath).EscapeMarkup()}); "
                        + "reading timing from the stream and using a default color palette.[/]");
                }

                // IsPal: default to PAL to match VobSubExtractor. The .idx "size:" field
                // could disambiguate per-file, but a wrong guess only affects timing scale,
                // not bitmap content.
                return await PassThroughSingleStreamAsync(inputFile, options, result, fileIndex, sourceTimestamps,
                    () => BitmapSubtitleLoader.LoadVobSub(inputFile, idxPath, isPal: true));
            }
            return false;
        }

        if (ext == ".idx")
        {
            // Accept the .idx of a VobSub pair as input (5.0.0 did): redirect to the
            // companion .sub and use this .idx for timing + palette (issue #12772).
            var subPath = Path.ChangeExtension(inputFile, ".sub");
            if (!File.Exists(subPath))
            {
                throw new InvalidOperationException(
                    $"VobSub '.idx' input has no companion '.sub' ({Path.GetFileName(subPath)}) — "
                    + "the .idx only holds timing and palette; the subtitle images live in the .sub.");
            }

            return await PassThroughSingleStreamAsync(subPath, options, result, fileIndex, sourceTimestamps,
                () => BitmapSubtitleLoader.LoadVobSub(subPath, inputFile, isPal: true));
        }

        if (ext is ".mkv" or ".mks")
        {
            return await PassThroughMatroskaPgsAsync(inputFile, options, result, fileIndex, sourceTimestamps);
        }

        if (ext is ".ts" or ".m2ts" or ".mts")
        {
            return await PassThroughTransportStreamDvbAsync(inputFile, options, result, fileIndex, sourceTimestamps);
        }

        if (ext is ".avi" or ".divx")
        {
            return await PassThroughXSubAsync(inputFile, options, result, fileIndex, sourceTimestamps);
        }

        return false;
    }

    private async Task<bool> PassThroughSingleStreamAsync(
        string inputFile,
        ConversionOptions options,
        ConversionResult result,
        int fileIndex,
        FileTimestamps? sourceTimestamps,
        Func<IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem>> load)
    {
        var outputFile = ResolveOutputFileName(inputFile, options, usedNames: _usedOutputFileNames);
        if (!options.Quiet)
        {
            AnsiConsole.MarkupInterpolated($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [dim](img→img)→[/] [green]{outputFile}[/]...");
        }

        IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem>? items = null;
        try
        {
            items = load();
            WritePreservedBitmaps(items, outputFile, options);
            result.SuccessfulFiles++;
            ApplySourceTimestamp(sourceTimestamps, outputFile);
            result.Files.Add(new FileConversionResult(inputFile, outputFile, true, null));
            if (!options.Quiet)
            {
                AnsiConsole.MarkupLine($" [green]done ({items.Count} bitmap(s)).[/]");
            }
        }
        catch (Exception ex)
        {
            var msg = ErrorMessageFormatter.FormatForUser(ex, options.Verbose);
            result.FailedFiles++;
            result.Errors.Add($"{Path.GetFileName(inputFile)}: {msg}");
            result.Files.Add(new FileConversionResult(inputFile, null, false, msg));
            if (!options.Quiet)
            {
                AnsiConsole.MarkupLineInterpolated($" [red]error: {msg}[/]");
            }
        }
        finally
        {
            if (items is not null)
            {
                foreach (var item in items)
                {
                    item.Dispose();
                }
            }
        }

        await Task.CompletedTask;
        return true;
    }

    private async Task<bool> PassThroughMatroskaPgsAsync(string inputFile, ConversionOptions options, ConversionResult result, int fileIndex, FileTimestamps? sourceTimestamps)
    {
        using var matroska = new Nikse.SubtitleEdit.Core.ContainerFormats.Matroska.MatroskaFile(inputFile);
        if (!matroska.IsValid)
        {
            // Not really an MKV — let the regular container loader / text loader try.
            return false;
        }

        // Both bitmap track kinds are eligible for image-to-image: PGS, and VobSub (whose
        // subpictures previously fell through to the OCR pipeline and were re-rasterised as
        // text at the default font — issue #12772 part 3).
        var bitmapTracks = matroska.GetTracks(true)
            .Where(t => t.CodecId.Equals("S_HDMV/PGS", StringComparison.OrdinalIgnoreCase)
                        || t.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase))
            .Where(t => !options.ForcedOnly || t.IsForced)
            .Where(t => options.TrackNumbers.Count == 0 || options.TrackNumbers.Contains(t.TrackNumber))
            .ToList();

        if (bitmapTracks.Count == 0)
        {
            // MKV has no bitmap tracks usable for image-to-image; fall through so the regular
            // container loader can handle text tracks / OCR-friendly tracks.
            return false;
        }

        if (bitmapTracks.Count > 1 && !string.IsNullOrEmpty(options.OutputFilename))
        {
            throw new InvalidOperationException(
                "--output-filename can only target a single track. Use --track-number to select one.");
        }

        foreach (var track in bitmapTracks)
        {
            var isVobSub = track.CodecId.Equals("S_VOBSUB", StringComparison.OrdinalIgnoreCase);
            var outputFile = ResolveOutputFileName(
                inputFile, options, AppendForcedToken(ContainerSubtitleLoader.SanitizeLang(track.Language), track.IsForced), track.TrackNumber, _usedOutputFileNames);

            if (!options.Quiet)
            {
                var trackLabel = $"#{track.TrackNumber} ";
                var kind = isVobSub ? "VobSub" : "PGS";
                AnsiConsole.MarkupInterpolated($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [yellow]{trackLabel}[/][dim]({kind} img→img)→[/] [green]{outputFile}[/]...");
            }

            IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem>? items = null;
            try
            {
                items = isVobSub
                    ? BitmapSubtitleLoader.LoadMatroskaVobSub(matroska, track)
                    : BitmapSubtitleLoader.LoadMatroskaPgs(matroska, track);
                WritePreservedBitmaps(items, outputFile, options);
                result.SuccessfulFiles++;
                ApplySourceTimestamp(sourceTimestamps, outputFile);
                result.Files.Add(new FileConversionResult(inputFile, outputFile, true, null));
                if (!options.Quiet)
                {
                    AnsiConsole.MarkupLine($" [green]done ({items.Count} bitmap(s)).[/]");
                }
            }
            catch (Exception ex)
            {
                var msg = ErrorMessageFormatter.FormatForUser(ex, options.Verbose);
                result.FailedFiles++;
                result.Errors.Add($"{Path.GetFileName(inputFile)} track #{track.TrackNumber}: {msg}");
                result.Files.Add(new FileConversionResult(inputFile, null, false, msg));
                if (!options.Quiet)
                {
                    AnsiConsole.MarkupLineInterpolated($" [red]error: {msg}[/]");
                }
            }
            finally
            {
                if (items is not null)
                {
                    foreach (var item in items)
                    {
                        item.Dispose();
                    }
                }
            }
        }

        await Task.CompletedTask;
        return true;
    }

    private async Task<bool> PassThroughTransportStreamDvbAsync(string inputFile, ConversionOptions options, ConversionResult result, int fileIndex, FileTimestamps? sourceTimestamps)
    {
        // --teletext-only means "skip DVB-sub". The regular container path
        // (ContainerSubtitleLoader.LoadTransportStream) honors this by gating its DVB-sub
        // load on !options.TeletextOnly; this preserve path has to do the same or the
        // flag is bypassed whenever the target is an image format.
        if (options.TeletextOnly)
        {
            await Task.CompletedTask;
            return false;
        }

        var streams = BitmapSubtitleLoader.LoadTransportStreamDvbSub(inputFile);
        if (streams.Count == 0)
        {
            // No DVB-sub PIDs found; let the regular container loader handle teletext / etc.
            return false;
        }

        if (streams.Count > 1 && !string.IsNullOrEmpty(options.OutputFilename))
        {
            throw new InvalidOperationException(
                "--output-filename can only target a single DVB-sub PID. Found "
                + $"{streams.Count} PIDs in the transport stream.");
        }

        foreach (var (items, pid) in streams)
        {
            // PID is the natural per-stream identifier. ResolveOutputFileName only embeds
            // languageSuffix into the filename stem (trackNumber is only used as the
            // de-duplication fallback for overwrite collisions), so pass the PID *as* the
            // language suffix to keep each PID's output stably named — otherwise multiple
            // PIDs would collide and only differ by an opaque "_2", "_3" rotation.
            var outputFile = ResolveOutputFileName(inputFile, options, languageSuffix: $"dvb_pid{pid}", trackNumber: pid, usedNames: _usedOutputFileNames);

            if (!options.Quiet)
            {
                AnsiConsole.MarkupInterpolated($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [yellow]PID {pid} [/][dim](DVB img→img)→[/] [green]{outputFile}[/]...");
            }

            try
            {
                WritePreservedBitmaps(items, outputFile, options);
                result.SuccessfulFiles++;
                ApplySourceTimestamp(sourceTimestamps, outputFile);
                result.Files.Add(new FileConversionResult(inputFile, outputFile, true, null));
                if (!options.Quiet)
                {
                    AnsiConsole.MarkupLine($" [green]done ({items.Count} bitmap(s)).[/]");
                }
            }
            catch (Exception ex)
            {
                var msg = ErrorMessageFormatter.FormatForUser(ex, options.Verbose);
                result.FailedFiles++;
                result.Errors.Add($"{Path.GetFileName(inputFile)} PID {pid}: {msg}");
                result.Files.Add(new FileConversionResult(inputFile, null, false, msg));
                if (!options.Quiet)
                {
                    AnsiConsole.MarkupLineInterpolated($" [red]error: {msg}[/]");
                }
            }
            finally
            {
                foreach (var item in items)
                {
                    item.Dispose();
                }
            }
        }

        await Task.CompletedTask;
        return true;
    }

    /// <summary>
    /// .avi/.divx XSUB → image output without OCR. One output per subtitle stream; the stream
    /// number is used as the filename suffix (an AVI stream header carries no language) but only
    /// when the file has more than one, so the usual single-stream file keeps its plain name.
    /// </summary>
    private async Task<bool> PassThroughXSubAsync(string inputFile, ConversionOptions options, ConversionResult result, int fileIndex, FileTimestamps? sourceTimestamps)
    {
        var allStreams = BitmapSubtitleLoader.LoadXSub(inputFile);
        if (allStreams.Count == 0)
        {
            // No XSUB packets — let the regular loader report it (it also owns the error text).
            await Task.CompletedTask;
            return false;
        }

        // Every stream is decoded up front, so the ones --track-number rules out have bitmaps
        // to dispose of.
        var streams = new List<(IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem> Items, int? StreamNumber)>();
        foreach (var stream in allStreams)
        {
            var selected = options.TrackNumbers.Count == 0
                           || (stream.StreamNumber.HasValue && options.TrackNumbers.Contains(stream.StreamNumber.Value));
            if (selected)
            {
                streams.Add(stream);
                continue;
            }

            foreach (var item in stream.Items)
            {
                item.Dispose();
            }
        }

        try
        {
            if (streams.Count == 0)
            {
                throw new InvalidOperationException(
                    $"XSUB file has {allStreams.Count} subtitle stream(s) but none matched --track-number ({string.Join(",", options.TrackNumbers)}): {inputFile}");
            }

            if (streams.Count > 1 && !string.IsNullOrEmpty(options.OutputFilename))
            {
                throw new InvalidOperationException(
                    "--output-filename can only target a single subtitle stream. Found "
                    + $"{streams.Count} XSUB streams in the file.");
            }
        }
        catch
        {
            foreach (var (items, _) in streams)
            {
                foreach (var item in items)
                {
                    item.Dispose();
                }
            }

            throw;
        }

        foreach (var (items, streamNumber) in streams)
        {
            var languageSuffix = allStreams.Count > 1 && streamNumber.HasValue ? $"xsub_track{streamNumber.Value}" : null;
            var outputFile = ResolveOutputFileName(inputFile, options, languageSuffix, streamNumber, _usedOutputFileNames);

            if (!options.Quiet)
            {
                var trackLabel = streamNumber.HasValue ? $"#{streamNumber.Value} " : string.Empty;
                AnsiConsole.MarkupInterpolated($"[dim]{fileIndex}:[/] [cyan]{Path.GetFileName(inputFile)}[/] [yellow]{trackLabel}[/][dim](XSUB img→img)→[/] [green]{outputFile}[/]...");
            }

            try
            {
                WritePreservedBitmaps(items, outputFile, options);
                result.SuccessfulFiles++;
                ApplySourceTimestamp(sourceTimestamps, outputFile);
                result.Files.Add(new FileConversionResult(inputFile, outputFile, true, null));
                if (!options.Quiet)
                {
                    AnsiConsole.MarkupLine($" [green]done ({items.Count} bitmap(s)).[/]");
                }
            }
            catch (Exception ex)
            {
                var msg = ErrorMessageFormatter.FormatForUser(ex, options.Verbose);
                result.FailedFiles++;
                result.Errors.Add($"{Path.GetFileName(inputFile)}: {msg}");
                result.Files.Add(new FileConversionResult(inputFile, null, false, msg));
                if (!options.Quiet)
                {
                    AnsiConsole.MarkupLineInterpolated($" [red]error: {msg}[/]");
                }
            }
            finally
            {
                foreach (var item in items)
                {
                    item.Dispose();
                }
            }
        }

        await Task.CompletedTask;
        return true;
    }

    /// <summary>
    /// The bitmap pass-through path writes the source items' timestamps verbatim - none of
    /// the timing/deletion transforms run on it. Silently ignoring those options would make
    /// a run look successful while producing something other than what was asked for, so
    /// fail loudly instead (convert via a text/OCR format to apply them).
    /// </summary>
    private static void ThrowOnUnsupportedImagePassThroughOptions(ConversionOptions options)
    {
        var unsupported = new List<string>();
        if (options.Offset.HasValue) { unsupported.Add("--offset"); }
        if (options.ChangeSpeedPercent.HasValue) { unsupported.Add("--change-speed"); }
        if (options.DeleteFirst.HasValue) { unsupported.Add("--delete-first"); }
        if (options.DeleteLast.HasValue) { unsupported.Add("--delete-last"); }
        if (options.DeleteContains != null) { unsupported.Add("--delete-contains"); }
        if (options.Renumber.HasValue) { unsupported.Add("--renumber"); }
        if (options.AdjustDurationMs.HasValue) { unsupported.Add("--adjust-duration"); }
        if (options.BridgeGapsMaxMs.HasValue) { unsupported.Add("--bridge-gaps"); }
        if (options.ApplyMinGapMs.HasValue) { unsupported.Add("--apply-min-gap"); }
        if (options.TargetFps.HasValue) { unsupported.Add("--target-fps"); }

        if (unsupported.Count > 0)
        {
            throw new InvalidOperationException(
                $"Not supported when passing image subtitles straight through to an image format: {string.Join(", ", unsupported)}. " +
                "Convert to a text format (OCR) to apply timing transforms, or drop the option(s).");
        }
    }

    private static void WritePreservedBitmaps(
        IReadOnlyList<BitmapSubtitleLoader.BitmapSubtitleItem> items,
        string outputFile,
        ConversionOptions options)
    {
        ThrowOnUnsupportedImagePassThroughOptions(options);
        var handler = ImageOutputWriter.TryCreateHandler(LibSEIntegration.NormalizeFormatName(options.Format))
            ?? throw new InvalidOperationException($"No image handler for format '{options.Format}'");
        var outputDir = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        ImageOutputWriter.WritePreservedBitmaps(items, outputFile, handler, options);
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

    private async Task ConvertFileAsync(string inputFile, string outputFile, ConversionOptions options, List<string> warnings)
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

            await Task.Run(async () =>
            {
                // --encoding:source means "use the input file's detected encoding for both
                // read and write". Resolve it per-file before the load so the same encoding
                // name flows into the save side too.
                var resolvedOptions = options;
                if (LibSEIntegration.IsSourceEncodingSentinel(options.Encoding))
                {
                    var detected = LibSEIntegration.DetectSourceEncodingName(inputFile, options.InputEncodingFallback);
                    resolvedOptions = options with { Encoding = detected };
                }

                // Load subtitle file using LibSE — keep the detected format so the
                // save side can apply RemoveNativeFormatting when the target differs.
                var (subtitle, sourceFormat) = LibSEIntegration.LoadSubtitleWithFormat(inputFile, resolvedOptions.Encoding, resolvedOptions.InputEncodingFallback, warnings);

                if (subtitle == null || subtitle.Paragraphs.Count == 0)
                {
                    throw new InvalidOperationException($"No subtitles found in file: {inputFile}");
                }

                await ApplyTransformsAndSaveAsync(subtitle, sourceFormat, outputFile, resolvedOptions);
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

            await Task.Run(async () =>
            {
                if (track.Subtitle.Paragraphs.Count == 0)
                {
                    throw new InvalidOperationException("Track is empty.");
                }

                await ApplyTransformsAndSaveAsync(track.Subtitle, track.Format, outputFile, options);
            });
        }
        finally
        {
            Configuration.Settings.General.CurrentFrameRate = originalFrameRate;
        }
    }

    private async Task ApplyTransformsAndSaveAsync(Subtitle subtitle, SubtitleFormat sourceFormat, string outputFile, ConversionOptions options)
    {
        // Apply offset (must be first, before any time-based transforms)
        if (options.Offset.HasValue && options.Offset.Value != TimeSpan.Zero)
        {
            subtitle.AddTimeToAllParagraphs(options.Offset.Value);
        }

        // Apply target frame rate via libse (handles frame-based formats correctly).
        // --target-fps on its own used to be accepted and then silently ignored; SE4 converts from
        // the current frame rate in that case, and the image path here already does the same.
        if (options.TargetFps is > 0)
        {
            subtitle.ChangeFrameRate(options.Fps ?? Configuration.Settings.General.CurrentFrameRate, options.TargetFps.Value);
        }

        // Scale all times by 100/percent (matches Sync > Change Speed in the UI)
        if (options.ChangeSpeedPercent.HasValue && Math.Abs(options.ChangeSpeedPercent.Value - 100.0) > 0.0001)
        {
            var factor = 100.0 / options.ChangeSpeedPercent.Value;
            foreach (var p in subtitle.Paragraphs)
            {
                p.StartTime.TotalMilliseconds *= factor;
                p.EndTime.TotalMilliseconds *= factor;
            }
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

        // Auto-translate (--translate-to) before the cleanup operations, matching the UI's
        // batch convert order (fix-common-errors etc. run on the translated text).
        if (_translateRunner != null)
        {
            await _translateRunner.TranslateAsync(subtitle, CancellationToken.None);
        }

        if (options.Operations.Count > 0)
        {
            // Wire the shared spell-check / OCR-fix engine (libuilogic) to seconv's options so the
            // "Fix common OCR errors" pass can run headless. Use --dictionary-folder when given, else
            // fall back to the dictionaries bundled into seconv (English out of the box) (#11744).
            SpellCheckConfig.DictionariesFolder = () =>
                !string.IsNullOrEmpty(options.DictionaryFolder) ? options.DictionaryFolder : BundledDictionaries.GetFolder();
            SpellCheckConfig.UseWordSplitList = () => true;
            SpellCheckConfig.TreatInApostropheAsIng = () => false;

            LibSEIntegration.ApplyOperations(
                subtitle,
                options.Operations,
                options.FixCommonErrorsRules,
                options.FixCommonErrorsLanguage,
                options.RemoveFormattingRules);
        }

        if (options.BridgeGapsMaxMs.HasValue && options.BridgeGapsMaxMs.Value > 0)
        {
            LibSEIntegration.BridgeGaps(subtitle, options.BridgeGapsMaxMs.Value);
        }

        if (options.ApplyMinGapMs.HasValue && options.ApplyMinGapMs.Value > 0)
        {
            LibSEIntegration.ApplyMinGap(subtitle, options.ApplyMinGapMs.Value);
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
    /// Appends the player convention's "forced" name token (<c>movie.eng.forced.srt</c>)
    /// for forced tracks - MKV forced-display flag, or MP4 tx3g forced displayFlags -
    /// so a forced track no longer collides with its same-language full track.
    /// </summary>
    internal static string? AppendForcedToken(string? languageSuffix, bool isForced)
    {
        if (!isForced)
        {
            return languageSuffix;
        }

        return string.IsNullOrEmpty(languageSuffix) ? "forced" : $"{languageSuffix}.forced";
    }

    /// <summary>
    /// Resolves the output file name. Honors --output-filename / --output-folder. When
    /// <paramref name="languageSuffix"/> is non-empty (container tracks), inserts it
    /// between the stem and extension (<c>movie.eng.srt</c>). On collision: tries
    /// <c>movie.#&lt;track&gt;.eng.srt</c> first when a track number is given, then
    /// rotates to <c>name_2.ext</c>, <c>name_3.ext</c>, ... up to 9999.
    /// A name in <paramref name="usedNames"/> was handed out earlier in this run (e.g.
    /// the first of two "eng" tracks) and always counts as taken - --overwrite only
    /// clobbers files from before the run, never the run's own output.
    /// </summary>
    internal static string ResolveOutputFileName(
        string inputFile,
        ConversionOptions options,
        string? languageSuffix = null,
        int? trackNumber = null,
        ISet<string>? usedNames = null)
    {
        string baseName;
        string ext;
        if (!string.IsNullOrEmpty(options.OutputFilename))
        {
            // A relative --output-filename still lands in --output-folder; an absolute
            // one wins outright.
            baseName = !string.IsNullOrEmpty(options.OutputFolder) && !Path.IsPathRooted(options.OutputFilename)
                ? Path.Combine(options.OutputFolder, options.OutputFilename)
                : options.OutputFilename;
            ext = Path.GetExtension(baseName);
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
            // Track the real extension: for an extension-less format (BDN-XML writes a
            // folder) Path.GetExtension(baseName) would mistake the language suffix for
            // the extension and the collision names below would double it.
            ext = extension;
        }

        if ((options.Overwrite || !File.Exists(baseName)) && usedNames?.Contains(baseName) != true)
        {
            usedNames?.Add(baseName);
            return baseName;
        }

        var dir = Path.GetDirectoryName(baseName) ?? string.Empty;
        var baseFileName = Path.GetFileName(baseName);
        var stem = ext.Length > 0 && baseFileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)
            ? baseFileName.Substring(0, baseFileName.Length - ext.Length)
            : baseFileName;

        // For container collisions, try inserting the track number first
        if (trackNumber.HasValue && !string.IsNullOrEmpty(languageSuffix))
        {
            var fileName = Path.GetFileNameWithoutExtension(inputFile);
            var withTrack = Path.Combine(dir, $"{fileName}.#{trackNumber.Value}.{languageSuffix}{ext}");
            if ((options.Overwrite || !File.Exists(withTrack)) && usedNames?.Contains(withTrack) != true)
            {
                usedNames?.Add(withTrack);
                return withTrack;
            }
        }

        for (var i = 2; i < 10_000; i++)
        {
            var candidate = Path.Combine(dir, $"{stem}_{i}{ext}");
            if (!File.Exists(candidate) && usedNames?.Contains(candidate) != true)
            {
                usedNames?.Add(candidate);
                return candidate;
            }
        }
        throw new InvalidOperationException($"Could not find a free output filename for: {baseName}");
    }

    /// <summary>
    /// --keep-timestamp: stamp the written output (file, or folder for image exports, plus a
    /// sibling .idx for VobSub) with the source file's timestamps, captured before the
    /// conversion (null when --keep-timestamp is off or the source could not be read).
    /// Best-effort, never throws.
    /// </summary>
    private static void ApplySourceTimestamp(FileTimestamps? sourceTimestamps, string outputFile)
    {
        if (sourceTimestamps == null || string.IsNullOrEmpty(outputFile))
        {
            return;
        }

        var timestamps = sourceTimestamps.Value;
        if (Directory.Exists(outputFile))
        {
            FileTimestampHelper.CopyTimestampsToDirectoryContents(timestamps, outputFile);
            return;
        }

        FileTimestampHelper.CopyTimestamps(timestamps, outputFile);
        if (outputFile.EndsWith(".sub", StringComparison.OrdinalIgnoreCase))
        {
            FileTimestampHelper.CopyTimestamps(timestamps, Path.ChangeExtension(outputFile, ".idx"));
        }
    }
}

internal record class ConversionOptions
{
    public required IReadOnlyList<string> Patterns { get; init; }
    public required string Format { get; init; }
    public string? InputFolder { get; init; }
    public string? OutputFolder { get; init; }
    public string? OutputFilename { get; init; }
    public string? Encoding { get; init; }

    /// <summary>
    /// Encoding to assume when input has no BOM and is not detected as UTF-8.
    /// Replaces the ANSI codepage heuristic only — BOM and valid UTF-8 still win.
    /// Ignored when <see cref="Encoding"/> is set (which already forces input encoding).
    /// </summary>
    public string? InputEncodingFallback { get; init; }
    public double? Fps { get; init; }
    public double? TargetFps { get; init; }
    public bool Overwrite { get; init; }

    /// <summary>--keep-timestamp: copy the source file's creation/last-write time onto every output file.</summary>
    public bool KeepTimestamp { get; init; }
    public List<string> Operations { get; init; } = new();

    /// <summary>
    /// Optional FixCommonErrors rule selection. Empty/null = run all rules.
    /// Resolve via <see cref="FixCommonErrorsRunner.ResolveRuleIds"/>.
    /// </summary>
    public IReadOnlyList<string> FixCommonErrorsRules { get; init; } = [];

    /// <summary>
    /// Forces the language used for FCE gating and OCR-fix (from <c>--fce-language</c>).
    /// Null/blank = auto-detect from subtitle content. Accepts a two-letter code, three-letter
    /// code, or English name (see <see cref="FixCommonErrorsRunner.NormalizeLanguageOverride"/>).
    /// </summary>
    public string? FixCommonErrorsLanguage { get; init; }

    /// <summary>
    /// Optional formatting-removal rule selection (from <c>--remove-formatting-rules</c>).
    /// Null = bare <c>--remove-formatting</c> = strip every tag wholesale. Resolve via
    /// <see cref="RemoveFormattingRunner.ResolveRuleIds"/>.
    /// </summary>
    public IReadOnlyList<string>? RemoveFormattingRules { get; init; }
    public int? DeleteFirst { get; init; }
    public int? DeleteLast { get; init; }
    public string? DeleteContains { get; init; }
    public TimeSpan? Offset { get; init; }
    public int? Renumber { get; init; }
    public int? AdjustDurationMs { get; init; }

    /// <summary>Speed change as a percent: 125 = 1.25x faster (times scaled by 100/125), 80 = slower.</summary>
    public double? ChangeSpeedPercent { get; init; }

    /// <summary>Bridge gaps shorter than this many ms by extending the previous end time. Null disables.</summary>
    public int? BridgeGapsMaxMs { get; init; }

    /// <summary>Enforce a minimum gap of this many ms between consecutive paragraphs. Null disables.</summary>
    public int? ApplyMinGapMs { get; init; }
    public (int Width, int Height)? Resolution { get; init; }

    /// <summary>
    /// Styling for text → image output (font, outline, shadow, background box, ...).
    /// Resolved from defaults + the settings JSON's <c>exportImages</c> section + CLI flags.
    /// </summary>
    public ImageExportStyle ImageStyle { get; init; } = new();
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
    /// <summary>Null = let each OCR engine apply its own default code set.</summary>
    public string? OcrLanguage { get; init; }

    /// <summary>Path to a <c>.nocr</c> database file (required when <c>OcrEngine == "nocr"</c>).</summary>
    public string? OcrDb { get; init; }

    /// <summary>Folder with Hunspell dictionaries + *_OCRFixReplaceList.xml, enabling the "Fix common OCR errors" pass.</summary>
    public string? DictionaryFolder { get; init; }

    /// <summary>
    /// When true, image-based sources (Blu-Ray <c>.sup</c>, VobSub <c>.sub</c>+<c>.idx</c>,
    /// MKV PGS/VobSub, MP4 VobSub, TS DVB-sub) are converted to a text format keeping only
    /// their time codes — OCR is skipped entirely and each entry's text is left empty. No
    /// OCR engine is created, so this works without Tesseract/Paddle/etc. installed. Ignored
    /// for text inputs and image output targets.
    /// </summary>
    public bool TimeCodesOnly { get; init; }

    /// <summary>
    /// VobSub OCR only: before recognition, rebuild each subpicture as a crisp black-on-white
    /// bitmap using histogram-based colour isolation — the innermost colour plane (the
    /// glyph fill) becomes black and the outline / anti-alias tiers collapse into the white
    /// background. Improves recognition on discs whose gray outlines otherwise melt adjacent
    /// characters together. On by default; disable with <c>--no-vobsub-isolate-colors</c> to
    /// OCR the raw palette. See <see cref="VobSubColorIsolation"/>. Ignored for non-VobSub
    /// sources and in <see cref="TimeCodesOnly"/> mode.
    /// </summary>
    public bool VobSubIsolateColors { get; init; } = true;

    /// <summary>
    /// PGS / DVB-sub OCR only: before recognition, rebuild each bitmap as crisp black-on-white
    /// via <see cref="VobSubColorIsolation.BinarizeForOcr"/>. PGS glyphs are typically
    /// white fill with a black outline on transparency; composited onto the opaque white OCR
    /// canvas the fill vanishes and Tesseract receives hollow outline rings, which garbles
    /// some entries deterministically (issue #12291). On by default; disable with
    /// <c>--no-pgs-isolate-colors</c> to OCR the raw bitmap. Ignored in
    /// <see cref="TimeCodesOnly"/> mode.
    /// </summary>
    public bool PgsIsolateColors { get; init; } = true;

    /// <summary>Ollama API endpoint (default <c>http://localhost:11434/api/chat</c>).</summary>
    public string? OllamaUrl { get; init; }

    /// <summary>Ollama vision model (default <c>llama3.2-vision</c>).</summary>
    public string? OllamaModel { get; init; }

    /// <summary>llama.cpp OCR: endpoint of an already-running llama-server; skips the local auto-start.</summary>
    public string? OcrUrl { get; init; }

    /// <summary>llama.cpp OCR model: curated <c>.gguf</c> file name or full path (default: first installed OCR model).</summary>
    public string? OcrModel { get; init; }

    /// <summary>Auto-translate target language (code or English name). Non-null enables translation.</summary>
    public string? TranslateTo { get; init; }

    /// <summary>Auto-translate source language. Null = auto-detect per file.</summary>
    public string? TranslateFrom { get; init; }

    /// <summary>Translate engine id, see <see cref="AutoTranslateRunner.SupportedEngines"/>. Null/empty = llamacpp.</summary>
    public string? TranslateEngine { get; init; }

    /// <summary>Endpoint of an already-running translate server; for llamacpp this skips the local llama-server auto-start.</summary>
    public string? TranslateUrl { get; init; }

    /// <summary>Model: ollama/lmstudio model name, or llamacpp .gguf file name/path.</summary>
    public string? TranslateModel { get; init; }

    /// <summary>
    /// Prompt override for the LLM translate engines (llamacpp/ollama/lmstudio): inline text
    /// (<c>\n</c> for a line break) or the path to a text file. See
    /// <see cref="AutoTranslateRunner.ReadPromptOption"/>.
    /// </summary>
    public string? TranslatePrompt { get; init; }

    public bool TeletextOnly { get; init; }
    public bool SkipTeletext { get; init; }
    public int? TeletextOnlyPage { get; init; }

    /// <summary>Plain text output: merge every paragraph into one space-separated block (no blank lines).</summary>
    public bool PlainTextMerge { get; init; }

    /// <summary>Plain text output: unbreak each paragraph, joining its lines into one.</summary>
    public bool PlainTextUnbreak { get; init; }

    /// <summary>Plain text output: emit a blank line between consecutive paragraphs. Default true (legacy behaviour).</summary>
    public bool PlainTextLineBetweenSubtitles { get; init; } = true;

    public bool Quiet { get; init; }
    public bool Verbose { get; init; }
}

internal class ConversionResult
{
    public int TotalFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public List<string> Errors { get; set; } = new();

    /// <summary>Non-fatal per-file issues (e.g. undecodable rows skipped); do not affect <see cref="Success"/>.</summary>
    public List<string> Warnings { get; set; } = new();

    public List<FileConversionResult> Files { get; set; } = new();
    public bool Success => FailedFiles == 0 && Errors.Count == 0;
}

internal sealed record FileConversionResult(string Input, string? Output, bool Success, string? Error, IReadOnlyList<string>? Warnings = null);
