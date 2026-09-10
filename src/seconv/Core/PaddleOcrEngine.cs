using System.Diagnostics;
using System.Text;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Ocr.Paddle;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// OCR via the PaddleOCR command-line tool. Prefers the standalone install the GUI downloads
/// (SE data folder, OCR/PaddleOCR3-7, with its bundled models), then a <c>paddleocr</c> from
/// <c>pip install paddleocr</c> on the system PATH.
/// Limited subset compared to SE's UI implementation; assumes a single image per invocation.
/// </summary>
internal sealed class PaddleOcrEngine : IOcrEngine
{
    public string Name => "paddleocr";

    public string ExecutablePath { get; }
    public string Language { get; }

    private readonly string _workDir;

    private PaddleOcrEngine(string executablePath, string language, string workDir)
    {
        ExecutablePath = executablePath;
        Language = language;
        _workDir = workDir;
    }

    /// <summary>
    /// Folder name of the GUI's standalone PaddleOCR install, under the SE "OCR" data folder.
    /// Keep in sync with <c>Se.PaddleOcrFolder</c> in the UI project.
    /// </summary>
    private const string StandaloneFolderName = "PaddleOCR3-7";

    /// <summary>
    /// Locates PaddleOCR. The GUI's standalone install is preferred (portable SE first, then
    /// the installed GUI's data folder), so seconv reuses the engine and models the user
    /// already downloaded; then falls back to a <c>paddleocr</c> on the system PATH.
    /// Returns null if missing.
    /// </summary>
    public static string? Detect()
    {
        return DetectStandalone() ?? DetectOnPath();
    }

    internal static string? DetectStandalone()
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "OCR", StandaloneFolderName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit", "OCR", StandaloneFolderName),
        };

        foreach (var folder in candidates)
        {
            foreach (var name in new[] { "paddleocr.exe", "paddleocr.bin" })
            {
                var candidate = Path.Combine(folder, name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static string? DetectOnPath()
    {
        var name = OperatingSystem.IsWindows() ? "paddleocr.exe" : "paddleocr";
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var separator = OperatingSystem.IsWindows() ? ';' : ':';
        foreach (var dir in pathEnv.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = Path.Combine(dir.Trim(), name);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }
        return null;
    }

    /// <summary>
    /// The standalone install ships its models in a "models" folder next to the binary
    /// (cls/det/rec sub-folders). When present, they are passed explicitly like the GUI does.
    /// </summary>
    internal static string? GetModelsFolder(string executablePath)
    {
        var folder = Path.GetDirectoryName(executablePath);
        if (folder == null)
        {
            return null;
        }

        var models = Path.Combine(folder, "models");
        return Directory.Exists(Path.Combine(models, "rec")) ? models : null;
    }

    public static PaddleOcrEngine Create(string language = "en")
    {
        var path = Detect()
            ?? throw new InvalidOperationException(
                "PaddleOCR not found. Download it via Subtitle Edit (OCR > PaddleOCR), or install it " +
                "(e.g. `pip install paddleocr`) and ensure the `paddleocr` binary is on PATH.");

        var workDir = Path.Combine(Path.GetTempPath(), "seconv_paddle_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);
        return new PaddleOcrEngine(path, language, workDir);
    }

    /// <summary>
    /// One image, through the same batch machinery. Kept for <see cref="IOcrEngine"/> and the
    /// odd single-image caller: paying one process start for one image is what
    /// <see cref="RecognizeBatch"/> exists to avoid, so nothing that OCRs a whole subtitle
    /// should come through here.
    /// </summary>
    public string Recognize(SKBitmap bitmap)
    {
        if (bitmap is null || bitmap.Width == 0 || bitmap.Height == 0)
        {
            return string.Empty;
        }

        return RecognizeBatch(new[] { bitmap })[0];
    }

    /// <summary>
    /// OCRs every image in one paddleocr run and returns the text per image, by index.
    /// <para>
    /// This is the whole performance story of image-subtitle conversion: paddleocr spends
    /// roughly twenty seconds loading its models and about a second on the actual image, so
    /// starting it per image made a feature-length subtitle take hours. One run for the lot
    /// pays that load once.
    /// </para>
    /// </summary>
    /// <param name="bitmaps">Images in subtitle order; each is prepared and written to disk.</param>
    /// <param name="progress">Called with (finished, total) as results come in.</param>
    public IReadOnlyList<string> RecognizeBatch(
        IReadOnlyList<SKBitmap> bitmaps, Action<int, int>? progress = null)
    {
        return RecognizeBatch(
            bitmaps.Count,
            (index, path) => PaddleOcrImagePrep.WritePreparedPng(bitmaps[index], path),
            progress);
    }

    /// <summary>
    /// The streaming form: instead of holding every bitmap, the caller is asked to write each
    /// prepared image as the batch is assembled, so only one decoded frame is alive at a time.
    /// </summary>
    /// <param name="count">How many images the batch has.</param>
    /// <param name="writePreparedImage">
    /// Writes the prepared PNG for an index to the given path. It must write a file for every
    /// index - see <see cref="PaddleOcrImagePrep.WriteBlankPng"/> for one that has no image -
    /// or the results after it would be attributed to the wrong lines.
    /// </param>
    /// <param name="progress">Called with (finished, total) as results come in.</param>
    public IReadOnlyList<string> RecognizeBatch(
        int count, Action<int, string> writePreparedImage, Action<int, int>? progress = null)
    {
        var results = new string[count];
        for (var i = 0; i < results.Length; i++)
        {
            results[i] = string.Empty;
        }

        if (count == 0)
        {
            return results;
        }

        Warnings.Clear();

        var runDir = Path.Combine(_workDir, "run_" + Guid.NewGuid().ToString("N"));
        var inputDir = Path.Combine(runDir, "in");
        var outputDir = Path.Combine(runDir, "out");
        Directory.CreateDirectory(inputDir);
        Directory.CreateDirectory(outputDir);

        try
        {
            var stems = new List<string>(count);
            for (var i = 0; i < count; i++)
            {
                stems.Add(PaddleOcrImagePrep.InputStem(i));
                var inputPath = Path.Combine(inputDir, PaddleOcrImagePrep.InputFileName(i));
                writePreparedImage(i, inputPath);
                if (!File.Exists(inputPath))
                {
                    // The callback owes us a file for every index; without one the numbering
                    // goes sparse and every later result lands on the wrong line.
                    PaddleOcrImagePrep.WriteBlankPng(inputPath);
                }
            }

            var psi = new ProcessStartInfo(ExecutablePath)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true,
                // The tool may write relative to its current directory; make sure that is writable.
                WorkingDirectory = _workDir,
            };
            foreach (var arg in BuildArguments(inputDir, outputDir))
            {
                psi.ArgumentList.Add(arg);
            }

            // StandardOutputEncoding only fixes the decoding side. paddleocr is a Python CLI, and
            // on Windows Python encodes a *redirected* stdout with the ANSI codepage (until UTF-8
            // becomes the default in Python 3.15, PEP 686) - so the producer side must be forced
            // to UTF-8 too, or non-ASCII text still arrives as mojibake. Same env vars the UI's
            // Paddle engine sets.
            psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            psi.EnvironmentVariables["PYTHONUTF8"] = "1";
            // Skip PaddleX's online model-source connectivity check - it can hang the run at
            // "Initializing..." when offline, and with explicit local model dirs it is pointless.
            psi.EnvironmentVariables["PADDLE_PDX_DISABLE_MODEL_SOURCE_CHECK"] = "True";

            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start paddleocr process.");

            // Drain both streams concurrently - paddleocr is chatty on both, and a full pipe
            // buffer blocks the process mid-run.
            var stdoutTask = proc.StandardOutput.ReadToEndAsync();
            var stderrTask = proc.StandardError.ReadToEndAsync();

            var rightToLeft = PaddleOcrModels.IsArabicScript(Language);
            var poller = new PaddleOcrResultPoller(outputDir, stems);
            var parseErrors = new List<string>();

            poller.PollUntilDoneAsync(
                    () => proc.HasExited,
                    (position, regions) =>
                    {
                        results[position] = regions.Count > 0
                            ? PaddleOcrTextLayout.BuildText(regions, 0, rightToLeft, out _)
                            : string.Empty;
                        progress?.Invoke(poller!.ReportedCount, count);
                    },
                    (stem, message) => parseErrors.Add(stem + ": " + message),
                    CancellationToken.None)
                .GetAwaiter().GetResult();

            // Never wait forever: a wedged run (missing model, stuck initialisation, a worker
            // process that never returns) used to hang seconv with no output at all.
            if (!proc.WaitForExit(ProcessTimeout))
            {
                try { proc.Kill(entireProcessTree: true); } catch { /* best-effort */ }
                throw new InvalidOperationException(
                    $"paddleocr did not finish within {ProcessTimeout.TotalMinutes:0} minutes and was killed.");
            }

            proc.WaitForExit(); // flush the redirected streams
            stdoutTask.GetAwaiter().GetResult();
            var stderr = stderrTask.GetAwaiter().GetResult();

            // Nothing at all came back: the run failed, however it exited. A short count is a
            // different matter - those lines come back blank and are reported like any other
            // blank image, rather than throwing away the hundreds that did work.
            if (poller.ReportedCount == 0)
            {
                throw new InvalidOperationException(
                    $"paddleocr produced no results for {count} image(s) (exit code {proc.ExitCode}). {stderr}");
            }

            if (poller.ReportedCount < count)
            {
                Warnings.Add(
                    $"paddleocr returned {poller.ReportedCount} of {count} results; the rest are blank.");
            }

            if (parseErrors.Count > 0)
            {
                Warnings.Add($"paddleocr wrote {parseErrors.Count} unreadable result file(s): {parseErrors[0]}");
            }

            return results;
        }
        finally
        {
            try { Directory.Delete(runDir, recursive: true); } catch { /* best-effort */ }
        }
    }

    /// <summary>
    /// Non-fatal problems from the last run - a short result count, an unreadable result file.
    /// The caller surfaces these; they are not worth losing a finished conversion over.
    /// </summary>
    public List<string> Warnings { get; } = new();

    /// <summary>Upper bound for the paddleocr process to exit once its results are in.</summary>
    internal static readonly TimeSpan ProcessTimeout = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Command line for one run. The standalone install gets the GUI's full argument set
    /// with explicit model folders (it has no model download of its own); a PATH install
    /// keeps the plain invocation and lets paddleocr resolve models itself.
    /// </summary>
    /// <param name="inputPath">A single image, or the folder of images for a batch run.</param>
    /// <param name="savePath">Where PaddleOCR writes its "&lt;stem&gt;_res.json" result files.</param>
    internal List<string> BuildArguments(string inputPath, string savePath)
    {
        var args = new List<string> { "ocr", "-i", inputPath, "--lang", Language, "--save_path", savePath };

        var modelsFolder = GetModelsFolder(ExecutablePath);
        if (modelsFolder == null)
        {
            args.AddRange(new[] { "--use_angle_cls", "false" });
            return args;
        }

        // "server" = the PP-OCRv6 medium tier, the more accurate of the two bundled sizes.
        var detName = PaddleOcrModels.GetDetectionName(Language, "server");
        var recName = PaddleOcrModels.GetRecName(Language, "server");
        args.AddRange(new[]
        {
            "--use_textline_orientation", "true",
            "--use_doc_orientation_classify", "false",
            "--use_doc_unwarping", "false",
            "--text_detection_model_dir", Path.Combine(modelsFolder, "det", detName),
            "--text_detection_model_name", detName,
            "--text_recognition_model_dir", Path.Combine(modelsFolder, "rec", recName),
            "--text_recognition_model_name", recName,
            "--textline_orientation_model_dir", Path.Combine(modelsFolder, "cls", PaddleOcrModels.TextlineOrientationModelName),
            "--textline_orientation_model_name", PaddleOcrModels.TextlineOrientationModelName,
        });
        return args;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_workDir))
            {
                Directory.Delete(_workDir, recursive: true);
            }
        }
        catch { /* ignore */ }
    }
}
