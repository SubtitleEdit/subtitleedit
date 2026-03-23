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

    private readonly List<string> LatinLanguageCodes = new List<string>
    {
        "af", "az", "bs", "cs", "cy", "da", "de", "es", "et", "fr", "ga",
        "hr", "hu", "id", "is", "it", "ku", "la", "lt", "lv", "mi", "ms",
        "mt", "nl", "no", "oc", "pi", "pl", "pt", "ro", "rs_latin", "sk",
        "sl", "sq", "sv", "sw", "tl", "tr", "uz", "vi", "french", "german"
    };

    private readonly List<string> ArabicLanguageCodes = new List<string>
    {
        "ar", "fa", "ug", "ur"
    };

    private readonly List<string> EslavLanguageCodes = new List<string>
    {
        "ru", "be", "uk"
    };

    private readonly List<string> CyrillicLanguageCodes = new List<string>
    {
        "rs_cyrillic", "bg", "mn", "abq", "ady", "kbd", "ava", "dar",
        "inh", "che", "lbe", "lez", "tab"
    };

    private readonly List<string> DevanagariLanguageCodes = new List<string>
    {
        "hi", "mr", "ne", "bh", "mai", "ang", "bho", "mah",
        "sck", "new", "gom", "bgc", "sa"
    };

    public PaddleOcr()
    {
        Error = string.Empty;
        _paddingOcrPath = Se.PaddleOcrModelsFolder;
        _clsPath = Path.Combine(_paddingOcrPath, "cls");
        _detPath = Path.Combine(_paddingOcrPath, "det");
        _recPath = Path.Combine(_paddingOcrPath, "rec");

        _cancellationToken = new CancellationToken();
    }

    private string GetRecName(string language, string mode)
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
            recName = "arabic_PP-OCRv3_mobile_rec";
        }
        else if (EslavLanguageCodes.Contains(language))
        {
            recName = "eslav_PP-OCRv5_mobile_rec";
        }
        else if (CyrillicLanguageCodes.Contains(language))
        {
            recName = "cyrillic_PP-OCRv3_mobile_rec";
        }
        else if (DevanagariLanguageCodes.Contains(language))
        {
            recName = "devanagari_PP-OCRv3_mobile_rec";
        }
        else if (language == "korean")
        {
            recName = "korean_PP-OCRv5_mobile_rec";
        }
        else
        {
            recName = "latin_PP-OCRv5_mobile_rec";
        }

        return recName;
    }

    private string GetDetectionName(string language, string mode)
    {
        if (language == "ch" ||
            language == "chinese_cht" ||
            language == "en" ||
            language == "japan" ||
            language == "korean" ||
            LatinLanguageCodes.Contains(language) ||
            EslavLanguageCodes.Contains(language))
        {
            return $"PP-OCRv5_{mode}_det";
        }
        else
        {
            return "PP-OCRv3_mobile_det";
        }
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
        string detFilePrefix = MakeDetPrefix(language);
        string recFilePrefix = MakeRecPrefix(language);
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

        var parameters = $"ocr -i \"{folder}\" " +
                         "--use_textline_orientation true " +
                         "--use_doc_orientation_classify false " +
                         "--use_doc_unwarping false " +
                         $"--lang {language} " +
                         $"--text_detection_model_dir \"{_detPath + Path.DirectorySeparatorChar + detName}\" " +
                         $"--text_detection_model_name \"{detName}\" " +
                         $"--text_recognition_model_dir \"{_recPath + Path.DirectorySeparatorChar + recName}\" " +
                         $"--text_recognition_model_name \"{recName}\" " +
                         $"--textline_orientation_model_dir \"{_clsPath + Path.DirectorySeparatorChar + TextlineOrientationModelName}\" " +
                         $"--textline_orientation_model_name \"{TextlineOrientationModelName}\"";

        var paddleOcrPath = Path.Combine(Se.PaddleOcrFolder, "paddleocr.bin");
        if (engineType != OcrEngineType.PaddleOcrStandalone || !File.Exists(paddleOcrPath))
        {
            paddleOcrPath = "paddleocr";
        }

        if (engineType == OcrEngineType.PaddleOcrStandalone &&
            File.Exists(Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe")))
        {
            paddleOcrPath = Path.Combine(Se.PaddleOcrFolder, "paddleocr.exe");
        }

        if (engineType == OcrEngineType.PaddleOcrPython)
        {
            paddleOcrPath = GetPaddleOcrPytonPath();
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
            },
        };

        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
        process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
        process.OutputDataReceived += OutputHandlerBatch;
        _textDetectionResults.Clear();

#pragma warning disable CA1416 // Validate platform compatibility
        process.Start();
#pragma warning restore CA1416 // Validate platform compatibility;

        process.BeginOutputReadLine();

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            Error = await process.StandardError.ReadToEndAsync(cancellationToken);
            Se.LogError($"PaddleOCR failed with exit code {process.ExitCode} and error: {Error}");
            return;
        }

        if (_textDetectionResults.Count > 0)
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
            .SelectMany(baseDir => Directory.GetFiles(baseDir, executableName, SearchOption.AllDirectories))
            .ToList();

        if (foundFiles.Any())
        {
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
        else
        {
            return "paddleocr"; // Fallback to just the command name
        }
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

    private string MakeRecPrefix(string language)
    {
        var recFilePrefix = language;
        if (LatinLanguageCodes.Contains(language))
        {
            recFilePrefix = $"latin{Path.DirectorySeparatorChar}latin_PP-OCRv3_rec_infer";
        }
        else if (ArabicLanguageCodes.Contains(language))
        {
            recFilePrefix = $"arabic{Path.DirectorySeparatorChar}arabic_PP-OCRv4_rec_infer";
        }
        else if (CyrillicLanguageCodes.Contains(language))
        {
            recFilePrefix = $"cyrillic{Path.DirectorySeparatorChar}cyrillic_PP-OCRv3_rec_infer";
        }
        else if (DevanagariLanguageCodes.Contains(language))
        {
            recFilePrefix = $"devanagari{Path.DirectorySeparatorChar}devanagari_PP-OCRv4_rec_infer";
        }
        else if (language == "chinese_cht")
        {
            recFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv3_rec_infer";
        }
        else
        {
            recFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv4_rec_infer";
        }

        return recFilePrefix;
    }

    private static string MakeDetPrefix(string language)
    {
        var detFilePrefix = language;
        if (language != "en" && language != "ch")
        {
            detFilePrefix = $"ml{Path.DirectorySeparatorChar}Multilingual_PP-OCRv3_det_infer";
        }
        else if (language == "ch")
        {
            detFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv4_det_infer";
        }
        else
        {
            detFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv3_det_infer";
        }

        return detFilePrefix;
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

        Se.WriteWhisperLog(outLine.Data);

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


    public static List<OcrLanguage2> GetLanguages()
    {
        return new List<OcrLanguage2>
        {
            new("abq", "Abkhazian"),
            new("ady", "Adyghe"),
            new("af", "Afrikaans"),
            new("sq", "Albanian"),
            new("ang", "Angika"),
            new("ar", "Arabic"),
            new("ava", "Avar"),
            new("az", "Azerbaijani"),
            new("be", "Belarusian"),
            new("bho", "Bhojpuri"),
            new("bh", "Bihari"),
            new("bs", "Bosnian"),
            new("bg", "Bulgarian"),
            new("ch", "Chinese and English"),
            new("chinese_cht", "Chinese traditional"),
            new("hr", "Croatian"),
            new("cs", "Czech"),
            new("da", "Danish"),
            new("dar", "Dargwa"),
            new("nl", "Dutch"),
            new("en", "English"),
            new("et", "Estonian"),
            new("fr", "French"),
            new("german", "German"),
            new("japan", "Japanese"),
            new("kbd", "Kabardian"),
            new("korean", "Korean"),
            new("ku", "Kurdish"),
            new("lbe", "Lak"),
            new("lv", "Latvian"),
            new("lez", "Lezghian"),
            new("lt", "Lithuanian"),
            new("mah", "Magahi"),
            new("mai", "Maithili"),
            new("ms", "Malay"),
            new("mt", "Maltese"),
            new("mi", "Maori"),
            new("mr", "Marathi"),
            new("mn", "Mongolian"),
            new("sck", "Nagpur"),
            new("ne", "Nepali"),
            new("new", "Newari"),
            new("no", "Norwegian"),
            new("oc", "Occitan"),
            new("fa", "Persian"),
            new("pl", "Polish"),
            new("pt", "Portuguese"),
            new("ro", "Romanian"),
            new("ru", "Russian"),
            new("sa", "Sanskrit"),
            new("rs_cyrillic", "Serbian (cyrillic)"),
            new("rs_latin", "Serbian (latin)"),
            new("sk", "Slovak"),
            new("sl", "Slovenian"),
            new("es", "Spanish"),
            new("sw", "Swahili"),
            new("sv", "Swedish"),
            new("tab", "Tabassaran"),
            new("tl", "Tagalog"),
            new("ta", "Tamil"),
            new("te", "Telugu"),
            new("tr", "Turkish"),
            new("uk", "Ukrainian"),
            new("ur", "Urdu"),
            new("ug", "Uyghur"),
            new("uz", "Uzbek"),
            new("vi", "Vietnamese"),
            new("cy", "Welsh"),
        }.OrderBy(p => p.Name).ToList();
    }
}