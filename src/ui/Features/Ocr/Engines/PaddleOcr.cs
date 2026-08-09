using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class PaddleOcr
{
    public string Error { get; set; }
    private List<PaddleOcrResultParser.TextDetectionResult> _textDetectionResults = new();
    private IProgress<PaddleOcrBatchProgress>? _batchProgress;
    private string _batchFileName = string.Empty;
    private List<PaddleOcrBatchInput> _batchFileNames = new List<PaddleOcrBatchInput>();
    private string _paddingOcrPath;
    private string _clsPath;
    private string _detPath;
    private string _recPath;
    private CancellationToken _cancellationToken;
    private readonly Stopwatch _batchStopwatch = new();
    private readonly StringBuilder _errorOutput = new();
    private readonly Lock _errorLock = new();

    public static List<string> UrlsWindowsCpu =
        ["https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.4.0/PaddleOCR-CPU-v1.4.0.7z"];

    public static List<string> UrlsLinuxCpu =
        ["https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.4.0/PaddleOCR-CPU-v1.4.0-Linux.7z"];

    public static List<string> UrlsWindowsGpuCuda11 =
    [
        "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.4.0/PaddleOCR-GPU-v1.4.0-CUDA-11.8.7z"
    ];

    public static List<string> UrlsWindowsGpuCuda12 =
    [
        "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.4.0/PaddleOCR-GPU-v1.4.0-CUDA-12.9.7z"
    ];

    public static List<string> UrlsLinuxGpu =
    [
        "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.4.0/PaddleOCR-GPU-v1.4.0-CUDA-12.9-Linux.7z.001",
        "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.4.0/PaddleOCR-GPU-v1.4.0-CUDA-12.9-Linux.7z.002"
    ];

    public static List<string> UrlsSupportFiles =
    [
        "https://github.com/timminator/PaddleOCR-Standalone/releases/download/v1.4.0/PaddleOCR.PP-OCRv5.support.files.VideOCR.7z"
    ];

    private const string TextlineOrientationModelName = "PP-LCNet_x1_0_textline_ori";

    // The script groups below mirror LATIN_LANGS/ARABIC_LANGS/ESLAV_LANGS/CYRILLIC_LANGS/
    // DEVANAGARI_LANGS in PaddleOCR 3.4 (paddleocr/_pipelines/ocr.py) - the version the
    // bundled standalone engine is built from. Keep them in sync with GetLanguages(); a
    // code offered in the dropdown but missing from every group here silently falls
    // through to the Latin recognition model and OCRs to garbage.
    private static readonly HashSet<string> LatinLanguageCodes = new HashSet<string>
    {
        "af", "az", "bs", "ca", "cs", "cy", "da", "de", "es", "et", "eu",
        "fi", "fr", "ga", "gl", "hr", "hu", "id", "is", "it", "ku", "la",
        "lb", "lt", "lv", "mi", "ms", "mt", "nl", "no", "oc", "pi", "pl",
        "pt", "qu", "rm", "ro", "rs_latin", "sk", "sl", "sq", "sv", "sw",
        "tl", "tr", "uz", "vi", "french", "german"
    };

    private static readonly HashSet<string> ArabicLanguageCodes = new HashSet<string>
    {
        "ar", "bal", "fa", "ps", "sd", "ug", "ur"
    };

    private static readonly HashSet<string> EslavLanguageCodes = new HashSet<string>
    {
        "ru", "be", "uk"
    };

    private static readonly HashSet<string> CyrillicLanguageCodes = new HashSet<string>
    {
        "rs_cyrillic", "bg", "mn", "abq", "ady", "kbd", "ava", "dar",
        "inh", "che", "lbe", "lez", "tab", "ba", "bua", "cv", "kaa",
        "kk", "kv", "ky", "mhr", "mk", "mo", "os", "sah", "tg", "tt",
        "tyv", "udm", "xal"
    };

    private static readonly HashSet<string> DevanagariLanguageCodes = new HashSet<string>
    {
        "hi", "mr", "ne", "bh", "mai", "ang", "bho", "mah",
        "sck", "new", "gom", "bgc", "sa"
    };

    // The languages with their own single-language PP-OCRv5 recognition model.
    private static readonly HashSet<string> OwnModelLanguageCodes = new HashSet<string>
    {
        "el", "ta", "te", "th"
    };

    internal static IReadOnlyCollection<string> GetLatinLanguageCodesForTest() => LatinLanguageCodes;

    internal static IEnumerable<string> GetAllScriptGroupCodesForTest() =>
        LatinLanguageCodes
            .Concat(ArabicLanguageCodes)
            .Concat(EslavLanguageCodes)
            .Concat(CyrillicLanguageCodes)
            .Concat(DevanagariLanguageCodes)
            .Concat(OwnModelLanguageCodes)
            .Distinct();

    public PaddleOcr()
    {
        Error = string.Empty;
        _paddingOcrPath = Se.PaddleOcrModelsFolder;
        _clsPath = Path.Combine(_paddingOcrPath, "cls");
        _detPath = Path.Combine(_paddingOcrPath, "det");
        _recPath = Path.Combine(_paddingOcrPath, "rec");

        _cancellationToken = new CancellationToken();
    }

    // Only the recognition models shipped in "PaddleOCR.PP-OCRv5.support.files" are on
    // disk - nothing is fetched per language. Returning a name that is not in that bundle
    // points at a folder that does not exist, and the run then fails when PaddleX tries to
    // read the model's inference.yml.
    internal static string GetRecName(string language, string mode)
    {
        string recName;
        if (language == "ch" ||
            language == "chinese_cht" ||
            language == "en" ||
            language == "japan")
        {
            recName = $"PP-OCRv5_{mode}_rec";
        }
        else if (ArabicLanguageCodes.Contains(language))
        {
            recName = "arabic_PP-OCRv5_mobile_rec";
        }
        else if (EslavLanguageCodes.Contains(language))
        {
            recName = "eslav_PP-OCRv5_mobile_rec";
        }
        else if (CyrillicLanguageCodes.Contains(language))
        {
            recName = "cyrillic_PP-OCRv5_mobile_rec";
        }
        else if (DevanagariLanguageCodes.Contains(language))
        {
            recName = "devanagari_PP-OCRv5_mobile_rec";
        }
        else if (language == "korean")
        {
            recName = "korean_PP-OCRv5_mobile_rec";
        }
        else if (OwnModelLanguageCodes.Contains(language))
        {
            recName = $"{language}_PP-OCRv5_mobile_rec";
        }
        else if (language == "ka")
        {
            // Georgian has no PP-OCRv5 recognition model yet.
            recName = "ka_PP-OCRv3_mobile_rec";
        }
        else
        {
            recName = "latin_PP-OCRv5_mobile_rec";
        }

        return recName;
    }

    internal static string GetDetectionName(string language, string mode)
    {
        // Georgian is the one remaining PP-OCRv3 language; everything else recognizes
        // with a PP-OCRv5 model and detects with the matching PP-OCRv5 detector.
        return language == "ka"
            ? "PP-OCRv3_mobile_det"
            : $"PP-OCRv5_{mode}_det";
    }

    private static SKBitmap MakeTransparentBlack(SKBitmap bitmap)
    {
        if (bitmap == null)
        {
            throw new ArgumentNullException(nameof(bitmap));
        }

        var workingBitmap = bitmap.IsImmutable
            ? new SKBitmap(bitmap.Width, bitmap.Height, bitmap.ColorType, bitmap.AlphaType)
            : bitmap;

        if (workingBitmap != bitmap)
        {
            using var canvas = new SKCanvas(workingBitmap);
            canvas.DrawBitmap(bitmap, 0, 0);
        }

        // Get all pixels at once
        var colors = workingBitmap.Pixels;
        var blackOpaque = new SKColor(0, 0, 0, 255);

        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].Alpha < 100)
            {
                colors[i] = blackOpaque;
            }
        }

        // Set all pixels back at once
        workingBitmap.Pixels = colors;

        return workingBitmap;
    }


    public async Task OcrBatch(OcrEngineType engineType, List<PaddleOcrBatchInput> bitmaps, string language,
        string mode, IProgress<PaddleOcrBatchProgress> progress, CancellationToken cancellationToken)
    {
        var detName = GetDetectionName(language, mode);
        var recName = GetRecName(language, mode);
        _batchProgress = progress;
        var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folder);
        _batchFileNames = new List<PaddleOcrBatchInput>(bitmaps.Count);

        var batchFileNamesList = new ConcurrentBag<PaddleOcrBatchInput>();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount // Adjust as needed
        };

        await Parallel.ForEachAsync(bitmaps, parallelOptions, async (input, ct) =>
        {
            if (input.Bitmap == null && !string.IsNullOrEmpty(input.SourceFileName))
            {
                // Image is already a file on disk (e.g. a video frame) - just copy it.
                try
                {
                    var tempImageFromFile = Path.Combine(folder,
                        input.Index.ToString("0000") + Path.GetExtension(input.SourceFileName));
                    File.Copy(input.SourceFileName, tempImageFromFile, true);
                    input.FileName = tempImageFromFile;
                    batchFileNamesList.Add(input);
                }
                catch
                {
                    // ignore
                }

                return;
            }

            SKBitmap? bitmap = null;
            SKBitmap? borderedBitmap = null;
            try
            {
                bitmap = input.Bitmap?.Copy() ?? new SKBitmap(1, 1, true);
                // bitmap = MakeTransparentBlack(bitmap);
                borderedBitmap = CreateDoubleBorder(bitmap, 10, SKColors.Black, new SKColor(0, 0, 0, 0));
                var tempImage = Path.Combine(folder, input.Index.ToString("0000") + ".png");
                input.FileName = tempImage;
                batchFileNamesList.Add(input);

                using var image = SKImage.FromBitmap(borderedBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                await File.WriteAllBytesAsync(tempImage, data.ToArray(), ct);
            }
            catch
            {
                // ignore
                return;
            }
            finally
            {
                bitmap?.Dispose();
                borderedBitmap?.Dispose();
            }
        });

        // Add all processed items back to the original collection
        _batchFileNames.AddRange(batchFileNamesList);

        // Resolve the executable before building the command line. When the standalone
        // binary is absent (no standalone build exists on macOS, and batch convert always
        // requests PaddleOcrStandalone), the run falls back to the pip-installed Python
        // CLI - and the whole I/O protocol below (result-file polling vs stdout parsing,
        // cls model, mkldnn) must follow the binary actually launched, not the engine
        // requested, or the fallback run exits successfully with zero parseable results.
        // The disk-scanning resolver only runs when neither standalone binary exists.
        string paddleOcrPath;
        if (engineType == OcrEngineType.PaddleOcrStandalone)
        {
            var standaloneExe = Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe");
            var standaloneBin = Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin");
            if (File.Exists(standaloneExe))
            {
                paddleOcrPath = standaloneExe;
            }
            else if (File.Exists(standaloneBin))
            {
                paddleOcrPath = standaloneBin;
            }
            else
            {
                paddleOcrPath = GetPaddleOcrPytonPath();
                engineType = OcrEngineType.PaddleOcrPython;
            }
        }
        else
        {
            paddleOcrPath = GetPaddleOcrPytonPath();
        }

        // Subtitles are always horizontal, so the Python engine skips text-line
        // orientation classification: it is noticeably faster and avoids loading the
        // extra cls model. The standalone engine keeps the original behavior.
        var useTextlineOrientation = engineType != OcrEngineType.PaddleOcrPython;

        var parameters = $"ocr -i \"{folder}\" " +
                         $"--use_textline_orientation {(useTextlineOrientation ? "true" : "false")} " +
                         "--use_doc_orientation_classify false " +
                         "--use_doc_unwarping false " +
                         $"--lang {language} " +
                         $"--text_detection_model_dir \"{_detPath + Path.DirectorySeparatorChar + detName}\" " +
                         $"--text_detection_model_name \"{detName}\" " +
                         $"--text_recognition_model_dir \"{_recPath + Path.DirectorySeparatorChar + recName}\" " +
                         $"--text_recognition_model_name \"{recName}\" " +
                         $"--textline_orientation_model_dir \"{_clsPath + Path.DirectorySeparatorChar + TextlineOrientationModelName}\" " +
                         $"--textline_orientation_model_name \"{TextlineOrientationModelName}\"";

        // The PaddleOCR 3.x Python CLI prints results as a (truncated) Python dict to
        // stderr instead of the old "ppocr INFO: [[...],('text',score)]" stdout format
        // that OutputHandlerBatch parses. So for the Python engine we let it write one
        // "<index>_res.json" per image with --save_path and read those instead.
        string? saveFolder = null;
        if (engineType == OcrEngineType.PaddleOcrPython)
        {
            saveFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(saveFolder);
            parameters += $" --save_path \"{saveFolder}\"";

            // A stock pip "paddlepaddle" build can crash inside the oneDNN/PIR executor on
            // PP-OCRv5 models (NotImplementedError: ConvertPirAttribute2RuntimeAttribute ...).
            // The bundled standalone build is known-good and faster with MKL-DNN, so only
            // disable it for the Python engine.
            parameters += " --enable_mkldnn False";
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = paddleOcrPath,
                Arguments = parameters,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                // A GUI app launched from Finder inherits "/" as current directory; use a
                // writable folder so relative writes from the tool cannot fail.
                WorkingDirectory = Path.GetTempPath(),
            },
        };

        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        // Without this, .NET decodes stderr with the OEM codepage on Windows, so any non-ASCII
        // text in Paddle's (chatty) stderr turns into mojibake in logs/error messages.
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
        process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
        // We always pass explicit local model dirs, so skip PaddleX's online model-source
        // connectivity check - otherwise it can hang the OCR run at "Initializing...".
        process.StartInfo.EnvironmentVariables["PADDLE_PDX_DISABLE_MODEL_SOURCE_CHECK"] = "True";
        process.OutputDataReceived += OutputHandlerBatch;
        process.ErrorDataReceived += ErrorHandler;
        _textDetectionResults.Clear();
        lock (_errorLock)
        {
            _errorOutput.Clear();
        }

        Se.WriteToolsLog($"Paddle OCR ({engineType}) starting - Cmd: \"{paddleOcrPath}\" {parameters}");

        _batchStopwatch.Restart();

#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416 // Validate platform compatibility;

        process.BeginOutputReadLine();
        // Drain stderr continuously: PaddleOCR is very chatty on stderr and if we let the
        // OS pipe fill up the process blocks mid-run. (We read it once at the end before.)
        process.BeginErrorReadLine();

        // For the Python engine PaddleOCR writes one "<index>_res.json" per image as it
        // goes, so poll the folder and report each result as soon as it appears - that
        // gives progress for every line instead of a single update at the very end.
        //
        // Important: the "paddleocr" launcher spawns a separate worker process to do the
        // actual OCR and can exit (or block) long before that worker finishes. So we poll
        // until results stop arriving, NOT until the launcher exits - otherwise only the
        // first couple of lines get reported while the worker keeps running in the background.
        var reportedStems = new HashSet<string>();
        if (saveFolder != null)
        {
            var roundsSinceProgress = 0;
            while (reportedStems.Count < _batchFileNames.Count && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(400, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                var before = reportedStems.Count;
                ReportNewPaddleOcrPythonResults(saveFolder, reportedStems);

                if (reportedStems.Count > before)
                {
                    roundsSinceProgress = 0;
                    continue;
                }

                // No new results this round - stop once they have clearly stopped arriving:
                // a short grace once the launcher exited, a long safety-net otherwise.
                roundsSinceProgress++;
                var maxIdleRounds = process.HasExited ? 150 : 750; // ~60s after exit, ~5 min otherwise
                if (roundsSinceProgress >= maxIdleRounds)
                {
                    break;
                }
            }
        }

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // User cancelled - make sure the launcher and its worker process are stopped.
            KillProcessTree(process);
            throw;
        }

        // Process has exited; block briefly so the async stderr handler flushes the tail.
        try
        {
            process.WaitForExit(3000);
        }
        catch
        {
            // ignore
        }

        if (process.ExitCode != 0 && reportedStems.Count == 0)
        {
            lock (_errorLock)
            {
                Error = _errorOutput.ToString();
            }

            Se.LogError($"PaddleOCR failed with exit code {process.ExitCode} and error: {Error}");
            Se.WriteToolsLog($"Paddle OCR ({engineType}) failed with exit code {process.ExitCode}: {Error}");
            return;
        }

        if (saveFolder != null)
        {
            // Final sweep - report any files written after the last poll.
            ReportNewPaddleOcrPythonResults(saveFolder, reportedStems);
        }
        else if (_textDetectionResults.Count > 0)
        {
            var input = _batchFileNames.First(p => p.FileName == _batchFileName);
            var p = new PaddleOcrBatchProgress
            {
                Index = input.Index,
                Text = MakeResult(_textDetectionResults),
                Item = input.Item,
            };
            _batchProgress?.Report(p);
            _textDetectionResults.Clear();
        }

        try
        {
            Directory.Delete(folder, true);
        }
        catch
        {
            // ignore
        }

        if (saveFolder != null)
        {
            try
            {
                Directory.Delete(saveFolder, true);
            }
            catch
            {
                // ignore
            }
        }
    }

    // Test seam: wires up the batch inputs and progress sink used by
    // ReportNewPaddleOcrPythonResults so the polling/reporting logic can be tested.
    internal void InitializeForTest(List<PaddleOcrBatchInput> inputs, IProgress<PaddleOcrBatchProgress> progress)
    {
        _batchFileNames = inputs;
        _batchProgress = progress;
    }

    // Reports any "<index>_res.json" files (written by the PaddleOCR 3.x Python CLI via
    // --save_path) that haven't been reported yet. Skips files still being written.
    internal void ReportNewPaddleOcrPythonResults(string saveFolder, HashSet<string> reportedStems)
    {
        foreach (var input in _batchFileNames.OrderBy(p => p.Index))
        {
            var stem = Path.GetFileNameWithoutExtension(input.FileName);
            if (reportedStems.Contains(stem))
            {
                continue;
            }

            var jsonPath = Path.Combine(saveFolder, stem + "_res.json");
            if (!File.Exists(jsonPath))
            {
                continue;
            }

            string json;
            try
            {
                json = File.ReadAllText(jsonPath);
            }
            catch
            {
                continue; // locked / mid-write - try again on the next poll
            }

            // A complete result file ends with the closing brace; if not, it is still
            // being written, so skip it for now and pick it up on the next poll.
            var trimmed = json.TrimEnd();
            if (trimmed.Length == 0 || trimmed[^1] != '}')
            {
                continue;
            }

            reportedStems.Add(stem);

            var results = ParsePaddleOcrJsonContent(json, jsonPath);
            Se.WriteToolsLog(
                $"Paddle OCR result {reportedStems.Count} (line index {input.Index}) ready at {_batchStopwatch.Elapsed.TotalSeconds:F1}s");
            _batchProgress?.Report(new PaddleOcrBatchProgress
            {
                Index = input.Index,
                Item = input.Item,
                Text = results.Count > 0 ? MakeResult(results) : string.Empty,
            });
        }
    }

    private void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (outLine.Data == null)
        {
            return;
        }

        lock (_errorLock)
        {
            // Cap the captured stderr so a long, chatty run doesn't grow unbounded.
            if (_errorOutput.Length < 100_000)
            {
                _errorOutput.AppendLine(outLine.Data);
            }
        }
    }

    private static void KillProcessTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignore - best effort
        }
    }

    internal static List<PaddleOcrResultParser.TextDetectionResult> ParsePaddleOcrJsonContent(string json, string sourceName = "")
    {
        var results = new List<PaddleOcrResultParser.TextDetectionResult>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("rec_texts", out var texts) || texts.ValueKind != JsonValueKind.Array)
            {
                return results;
            }

            root.TryGetProperty("rec_scores", out var scores);
            root.TryGetProperty("rec_polys", out var polys);

            for (var i = 0; i < texts.GetArrayLength(); i++)
            {
                var text = texts[i].GetString() ?? string.Empty;

                var confidence = 0.0;
                if (scores.ValueKind == JsonValueKind.Array && i < scores.GetArrayLength())
                {
                    confidence = scores[i].GetDouble();
                }

                var box = new PaddleOcrResultParser.BoundingBox(
                    new PaddleOcrResultParser.Point(0, 0),
                    new PaddleOcrResultParser.Point(0, 0),
                    new PaddleOcrResultParser.Point(0, 0),
                    new PaddleOcrResultParser.Point(0, 0));

                if (polys.ValueKind == JsonValueKind.Array && i < polys.GetArrayLength() &&
                    polys[i].ValueKind == JsonValueKind.Array && polys[i].GetArrayLength() >= 4)
                {
                    var poly = polys[i];
                    box = new PaddleOcrResultParser.BoundingBox(
                        ReadJsonPoint(poly[0]),
                        ReadJsonPoint(poly[1]),
                        ReadJsonPoint(poly[2]),
                        ReadJsonPoint(poly[3]));
                }

                results.Add(new PaddleOcrResultParser.TextDetectionResult
                {
                    Text = text,
                    Confidence = confidence,
                    BoundingBox = box,
                });
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Failed to parse PaddleOCR result JSON: {sourceName}");
        }

        return results;
    }

    private static PaddleOcrResultParser.Point ReadJsonPoint(JsonElement point)
    {
        return new PaddleOcrResultParser.Point(point[0].GetDouble(), point[1].GetDouble());
    }

    private static string GetPaddleOcrPytonPath()
    {
        var possiblePaths = new[]
        {
            // Windows user install
//            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Python"),

            // Windows pip scripts dir (per environment)
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                @"AppData\Local\Programs\Python"),

            // Mac default Frameworks path
            "/Library/Frameworks/Python.framework/Versions",

            // Mac Homebrew path
            "/usr/local/Cellar/python",
            "/opt/homebrew/Cellar/python",

            // Conda default paths
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "opt", "anaconda3"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "miniconda3")
        };

        string executableName;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            executableName = "paddleocr.exe";
        }
        else
        {
            executableName = "paddleocr"; // Mac/Linux - no .exe
        }

        var foundFiles = possiblePaths
            .Where(Directory.Exists)
            .SelectMany(baseDir => SafeGetFiles(baseDir, executableName))
            .Concat(GetCliShimCandidates(executableName))
            .Distinct()
            .ToList();

        if (foundFiles.Count == 0)
        {
            return "paddleocr"; // Fallback to just the command name
        }

        // Several Python installs may each have a "paddleocr" launcher, but only the
        // ones whose environment also has the "paddle" backend (and a usable interpreter)
        // can actually run. Picking the wrong one fails with:
        //   ModuleNotFoundError: No module named 'paddle'
        var usable = foundFiles
            .Where(p => HasPythonInterpreter(p) && HasPaddleBackend(p))
            .ToList();
        if (usable.Count > 0)
        {
            foundFiles = usable;
        }

        var sitePackages = foundFiles
            .Where(p => p.Contains("site-packages"))
            .OrderByDescending(p => p.Length)
            .ToList();
        if (sitePackages.Any())
        {
            return sitePackages.Last();
        }

        return foundFiles.Last();
    }

    // A conda/Python tree can contain unreadable directories or reparse-point cycles;
    // a resolver failure must degrade to "not found here", never abort the OCR run.
    private static string[] SafeGetFiles(string baseDir, string fileName)
    {
        try
        {
            return Directory.GetFiles(baseDir, fileName, SearchOption.AllDirectories);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    // A GUI app launched from Finder/launchd does not inherit the shell PATH, so
    // Process.Start("paddleocr") cannot find pip/Homebrew installs even though the
    // command works fine in a terminal (#12953). Probe the standard CLI-shim
    // directories directly, plus whatever PATH the process does have.
    private static IEnumerable<string> GetCliShimCandidates(string executableName)
    {
        var candidates = new List<string>();

        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidates.Add(Path.Combine(home, ".local", "bin", executableName)); // pip install --user / pipx
            candidates.Add("/usr/local/bin/" + executableName); // Homebrew (Intel) / system pip
            candidates.Add("/opt/homebrew/bin/" + executableName); // Homebrew (Apple Silicon)
            candidates.Add("/usr/bin/" + executableName); // system package manager
            candidates.Add("/opt/local/bin/" + executableName); // MacPorts

            // macOS "pip install --user": ~/Library/Python/X.Y/bin/paddleocr
            var macUserPython = Path.Combine(home, "Library", "Python");
            if (Directory.Exists(macUserPython))
            {
                try
                {
                    candidates.AddRange(Directory.GetFiles(macUserPython, executableName, SearchOption.AllDirectories));
                }
                catch
                {
                    // ignore access errors
                }
            }
        }

        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        foreach (var dir in pathEnv.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            try
            {
                candidates.Add(Path.Combine(dir.Trim(), executableName));
            }
            catch
            {
                // ignore malformed PATH entries
            }
        }

        return candidates.Where(File.Exists);
    }

    // Resolves the Python environment root for a "paddleocr" launcher:
    //   <root>\Scripts\paddleocr.exe  (Windows)  ->  <root>
    //   <root>/bin/paddleocr          (Mac/Linux) ->  <root>
    private static string? GetPythonEnvRoot(string paddleOcrExecutablePath)
    {
        var binDir = Path.GetDirectoryName(paddleOcrExecutablePath);
        return binDir == null ? null : Directory.GetParent(binDir)?.FullName;
    }

    private static bool HasPaddleBackend(string paddleOcrExecutablePath)
    {
        try
        {
            var root = GetPythonEnvRoot(paddleOcrExecutablePath);
            if (root == null || !Directory.Exists(root))
            {
                return false;
            }

            // Windows: <root>\Lib\site-packages\paddle
            if (Directory.Exists(Path.Combine(root, "Lib", "site-packages", "paddle")))
            {
                return true;
            }

            // Mac/Linux: <root>/lib/pythonX.Y/site-packages/paddle
            var unixLib = Path.Combine(root, "lib");
            if (Directory.Exists(unixLib))
            {
                foreach (var pyDir in Directory.EnumerateDirectories(unixLib, "python*"))
                {
                    if (Directory.Exists(Path.Combine(pyDir, "site-packages", "paddle")))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            // ignore - treat as "no paddle backend found"
        }

        return false;
    }

    private static bool HasPythonInterpreter(string paddleOcrExecutablePath)
    {
        try
        {
            var binDir = Path.GetDirectoryName(paddleOcrExecutablePath);
            var root = GetPythonEnvRoot(paddleOcrExecutablePath);
            if (binDir == null || root == null)
            {
                return false;
            }

            var names = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? new[] { "python.exe" }
                : new[] { "python3", "python" };

            foreach (var name in names)
            {
                if (File.Exists(Path.Combine(root, name)) || File.Exists(Path.Combine(binDir, name)))
                {
                    return true;
                }
            }
        }
        catch
        {
            // ignore - treat as "no interpreter found"
        }

        return false;
    }

    private static SKBitmap CreateDoubleBorder(SKBitmap source, int borderSize, SKColor innerColor, SKColor outerColor)
    {
        var totalBorder = borderSize * 2;
        var finalWidth = source.Width + totalBorder * 2;
        var finalHeight = source.Height + totalBorder * 2;

        var result = new SKBitmap(finalWidth, finalHeight);
        using var canvas = new SKCanvas(result);

        // Clear with outer border color
        canvas.Clear(outerColor);

        // Draw inner border rectangle
        using var paint = new SKPaint { Color = innerColor };
        canvas.DrawRect(borderSize, borderSize,
            finalWidth - borderSize * 2, finalHeight - borderSize * 2, paint);

        // Draw original bitmap in center
        canvas.DrawBitmap(source, totalBorder, totalBorder);

        return result;
    }

    public static SKBitmap AddBorder(SKBitmap originalBitmap, int borderWidth, SKColor color)
    {
        // Calculate new dimensions
        int newWidth = originalBitmap.Width + 2 * borderWidth;
        int newHeight = originalBitmap.Height + 2 * borderWidth;

        // Create a new bitmap with the new dimensions
        SKBitmap borderedBitmap = new(newWidth, newHeight);

        // Create a canvas to draw on the new bitmap
        using (var canvas = new SKCanvas(borderedBitmap))
        {
            // Fill the canvas with a border color (optional)
            var borderColor = color;
            canvas.Clear(borderColor);

            // Draw the original bitmap onto the canvas, offset by the border width
            canvas.DrawBitmap(originalBitmap, borderWidth, borderWidth);
        }

        return borderedBitmap;
    }

    private string MakeResult(List<PaddleOcrResultParser.TextDetectionResult> textDetectionResults)
    {
        var sb = new StringBuilder();
        var lines = MakeLines(textDetectionResults);
        foreach (var line in lines)
        {
            var text = string.Join(' ', line.Select(p => p.Text));
            sb.AppendLine(text);
        }

        return sb.ToString().Trim().Replace(" " + Environment.NewLine, Environment.NewLine);
    }

    private List<List<PaddleOcrResultParser.TextDetectionResult>> MakeLines(
        List<PaddleOcrResultParser.TextDetectionResult> input)
    {
        var result = new List<List<PaddleOcrResultParser.TextDetectionResult>>();
        var heightAverage = input.Average(p => p.BoundingBox.Height);
        var sorted = input.OrderBy(p => p.BoundingBox.Center.Y);
        var line = new List<PaddleOcrResultParser.TextDetectionResult>();
        PaddleOcrResultParser.TextDetectionResult? last = null;
        foreach (var element in sorted)
        {
            if (last == null)
            {
                line.Add(element);
            }
            else
            {
                if (element.BoundingBox.Center.Y > last.BoundingBox.TopLeft.Y + heightAverage)
                {
                    result.Add(line.OrderBy(p => p.BoundingBox.TopLeft.X).ToList());
                    line = new List<PaddleOcrResultParser.TextDetectionResult>();
                }

                line.Add(element);
            }

            last = element;
        }

        if (line.Count > 0)
        {
            result.Add(line.OrderBy(p => p.BoundingBox.TopLeft.X).ToList());
        }

        return result;
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data) || _cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!outLine.Data.Contains("ppocr INFO:"))
        {
            return;
        }

        var arr = outLine.Data.Split("ppocr INFO: ");
        if (arr.Length < 2)
        {
            return;
        }

        var data = arr[1];

        string pattern =
            @"\[\[\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+]],\s*\(['""].*['""],\s*\d+\.\d+\)\]";
        var match = Regex.Match(data, pattern);
        if (match.Success)
        {
            var parser = new PaddleOcrResultParser();
            var x = parser.Parse(data);
            _textDetectionResults.Add(x);
        }

        // Example: [[[92.0, 56.0], [735.0, 60.0], [734.0, 118.0], [91.0, 113.0]], ('My mommy always said', 0.9907816052436829)]
    }

    private Lock _lock = new Lock();

    private void OutputHandlerBatch(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data) || _cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!outLine.Data.Contains("ppocr INFO:"))
        {
            return;
        }

        lock (_lock)
        {
            foreach (var fileName in _batchFileNames)
            {
                if (outLine.Data.Contains(fileName.FileName))
                {
                    if (_textDetectionResults.Count > 0)
                    {
                        var old = _batchFileNames.First(p => p.FileName == _batchFileName);
                        var progress = new PaddleOcrBatchProgress
                        {
                            Index = old.Index,
                            Item = old.Item,
                            Text = MakeResult(_textDetectionResults),
                        };
                        _textDetectionResults.Clear();
                        _batchProgress?.Report(progress);
                    }

                    _batchFileName = fileName.FileName;
                    return;
                }
            }

            var arr = outLine.Data.Split("ppocr INFO: ");
            if (arr.Length < 2)
            {
                return;
            }

            var data = arr[1];

            string pattern =
                @"\[\[\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+]],\s*\(['""].*['""],\s*\d+\.\d+\)\]";
            var match = Regex.Match(data, pattern);
            if (match.Success)
            {
                var parser = new PaddleOcrResultParser();
                var x = parser.Parse(data);
                _textDetectionResults.Add(x);
            }
        }

        // Example: [[[92.0, 56.0], [735.0, 60.0], [734.0, 118.0], [91.0, 113.0]], ('My mommy always said', 0.9907816052436829)]
    }


    // Every language PaddleOCR 3.4 supports with a recognition model that ships in the
    // bundled support files. Adding a code here is enough to offer it - as long as the
    // code is also listed in the matching script group above, so GetRecName picks the
    // right model (PaddleOcrLanguageMappingTests guards that).
    public static List<OcrLanguage2> GetLanguages()
    {
        return new List<OcrLanguage2>
        {
            new("abq", "Abaza"),
            new("ady", "Adyghe"),
            new("af", "Afrikaans"),
            new("sq", "Albanian"),
            new("ang", "Angika"),
            new("ar", "Arabic"),
            new("ava", "Avar"),
            new("az", "Azerbaijani"),
            new("bal", "Balochi"),
            new("ba", "Bashkir"),
            new("eu", "Basque"),
            new("be", "Belarusian"),
            new("bho", "Bhojpuri"),
            new("bh", "Bihari"),
            new("bs", "Bosnian"),
            new("bg", "Bulgarian"),
            new("bua", "Buriat"),
            new("ca", "Catalan"),
            new("che", "Chechen"),
            new("ch", "Chinese and English"),
            new("chinese_cht", "Chinese traditional"),
            new("cv", "Chuvash"),
            new("hr", "Croatian"),
            new("cs", "Czech"),
            new("da", "Danish"),
            new("dar", "Dargwa"),
            new("nl", "Dutch"),
            new("en", "English"),
            new("et", "Estonian"),
            new("fi", "Finnish"),
            new("fr", "French"),
            new("gl", "Galician"),
            new("ka", "Georgian"),
            new("de", "German"),
            new("el", "Greek"),
            new("bgc", "Haryanvi"),
            new("hi", "Hindi"),
            new("hu", "Hungarian"),
            new("is", "Icelandic"),
            new("id", "Indonesian"),
            new("inh", "Ingush"),
            new("ga", "Irish"),
            new("it", "Italian"),
            new("japan", "Japanese"),
            new("kbd", "Kabardian"),
            new("xal", "Kalmyk"),
            new("kaa", "Karakalpak"),
            new("kk", "Kazakh"),
            new("kv", "Komi"),
            new("gom", "Konkani"),
            new("korean", "Korean"),
            new("ku", "Kurdish"),
            new("ky", "Kyrgyz"),
            new("lbe", "Lak"),
            new("la", "Latin"),
            new("lv", "Latvian"),
            new("lez", "Lezghian"),
            new("lt", "Lithuanian"),
            new("lb", "Luxembourgish"),
            new("mk", "Macedonian"),
            new("mah", "Magahi"),
            new("mai", "Maithili"),
            new("ms", "Malay"),
            new("mt", "Maltese"),
            new("mi", "Maori"),
            new("mhr", "Mari"),
            new("mr", "Marathi"),
            new("mo", "Moldovan"),
            new("mn", "Mongolian"),
            new("sck", "Nagpur"),
            new("ne", "Nepali"),
            new("new", "Newari"),
            new("no", "Norwegian"),
            new("oc", "Occitan"),
            new("os", "Ossetian"),
            new("pi", "Pali"),
            new("ps", "Pashto"),
            new("fa", "Persian"),
            new("pl", "Polish"),
            new("pt", "Portuguese"),
            new("qu", "Quechua"),
            new("ro", "Romanian"),
            new("rm", "Romansh"),
            new("ru", "Russian"),
            new("sah", "Sakha"),
            new("sa", "Sanskrit"),
            new("rs_cyrillic", "Serbian (cyrillic)"),
            new("rs_latin", "Serbian (latin)"),
            new("sd", "Sindhi"),
            new("sk", "Slovak"),
            new("sl", "Slovenian"),
            new("es", "Spanish"),
            new("sw", "Swahili"),
            new("sv", "Swedish"),
            new("tab", "Tabassaran"),
            new("tl", "Tagalog"),
            new("tg", "Tajik"),
            new("ta", "Tamil"),
            new("tt", "Tatar"),
            new("te", "Telugu"),
            new("th", "Thai"),
            new("tr", "Turkish"),
            new("tyv", "Tuvinian"),
            new("udm", "Udmurt"),
            new("uk", "Ukrainian"),
            new("ur", "Urdu"),
            new("ug", "Uyghur"),
            new("uz", "Uzbek"),
            new("vi", "Vietnamese"),
            new("cy", "Welsh"),
        }.OrderBy(p => p.Name).ToList();
    }

    /// <summary>
    /// Maps the legacy language codes the dropdown used to offer onto the ISO codes it
    /// offers now, so an already saved setting still selects the same language. PaddleOCR
    /// accepts both spellings, so only the stored value needs translating.
    /// </summary>
    public static string NormalizeLanguageCode(string? code) => code switch
    {
        "german" => "de",
        "french" => "fr",
        null => string.Empty,
        _ => code,
    };
}