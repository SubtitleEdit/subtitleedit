using System.Diagnostics;
using System.Text;
using System.Text.Json;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// OCR via the PaddleOCR command-line tool on the system PATH. Supports the standalone
/// binary (<c>paddleocr</c> / <c>paddleocr.exe</c>) installed via <c>pip install paddleocr</c>.
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

    public static string? Detect()
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

    public static PaddleOcrEngine Create(string language = "en")
    {
        var path = Detect()
            ?? throw new InvalidOperationException(
                "PaddleOCR not found on PATH. Install it (e.g. `pip install paddleocr`) " +
                "and ensure the `paddleocr` binary is on PATH.");

        var workDir = Path.Combine(Path.GetTempPath(), "seconv_paddle_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);
        return new PaddleOcrEngine(path, language, workDir);
    }

    public string Recognize(SKBitmap bitmap)
    {
        if (bitmap is null || bitmap.Width == 0 || bitmap.Height == 0)
        {
            return string.Empty;
        }

        var pngPath = Path.Combine(_workDir, "in_" + Guid.NewGuid().ToString("N") + ".png");
        var resultDir = Path.Combine(_workDir, "result_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(resultDir);
        try
        {
            using var borderedBitmap = CreateDoubleBorder(bitmap, 10, SKColors.Black, new SKColor(0, 0, 0, 0));
            using (var image = SKImage.FromBitmap(borderedBitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
            using (var fs = File.Create(pngPath))
            {
                data.SaveTo(fs);
            }

            var psi = CreateProcessStartInfo(pngPath, resultDir);

            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start paddleocr process.");
            // Drain stderr concurrently — paddleocr is chatty on stderr, and reading stdout
            // to completion while stderr fills the pipe buffer would deadlock.
            var stderrTask = proc.StandardError.ReadToEndAsync();
            var stdout = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                var err = stderrTask.GetAwaiter().GetResult();
                throw new InvalidOperationException($"paddleocr exited with code {proc.ExitCode}: {err}");
            }
            var resultPath = Path.Combine(resultDir, Path.GetFileNameWithoutExtension(pngPath) + "_res.json");
            if (!File.Exists(resultPath)) { return string.Empty; }
            return ParseJsonResult(File.ReadAllText(resultPath));
        }
        finally
        {
            try { File.Delete(pngPath); } catch { /* best-effort */ }
            try { if (Directory.Exists(resultDir)) { Directory.Delete(resultDir, recursive: true); } }
            catch { /* best-effort */ }
        }
    }

    public IReadOnlyList<string> Recognize(IReadOnlyList<SKBitmap> bitmaps, Action<int>? progress = null)
    {
        if (bitmaps.Count == 0) { return Array.Empty<string>(); }

        var inputDir = Path.Combine(_workDir, "batch_" + Guid.NewGuid().ToString("N"));
        var resultDir = Path.Combine(_workDir, "results_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(inputDir);
        Directory.CreateDirectory(resultDir);

        try
        {
            for (var i = 0; i < bitmaps.Count; i++)
            {
                var pngPath = Path.Combine(inputDir, $"{i:D4}.png");
                using var borderedBitmap = CreateDoubleBorder(bitmaps[i], 10, SKColors.Black, new SKColor(0, 0, 0, 0));
                using var image = SKImage.FromBitmap(borderedBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                using var fs = File.Create(pngPath);
                data.SaveTo(fs);
            }

            var psi = CreateProcessStartInfo(inputDir, resultDir);

            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start paddleocr process.");

            var stderrTask = proc.StandardError.ReadToEndAsync();
            var stdoutTask = proc.StandardOutput.ReadToEndAsync();
            var results = new string[bitmaps.Count];
            var reported = new bool[bitmaps.Count];
            var done = 0;

            while (!proc.HasExited || done < bitmaps.Count)
            {
                for (var i = 0; i < bitmaps.Count; i++)
                {
                    if (reported[i]) { continue; }
                    var jsonPath = Path.Combine(resultDir, $"{i:D4}_res.json");
                    if (!TryReadJson(jsonPath, out _)) { continue; }
                    reported[i] = true;
                    done++;
                    progress?.Invoke(done);
                }

                if (proc.HasExited)
                {
                    if (done >= bitmaps.Count) { break; }
                    Thread.Sleep(100);
                }
                else
                {
                    Thread.Sleep(400);
                }
            }

            proc.WaitForExit();
            _ = stdoutTask.GetAwaiter().GetResult();

            var stderr = stderrTask.GetAwaiter().GetResult();
            if (proc.ExitCode != 0 && done == 0)
            {
                throw new InvalidOperationException($"paddleocr exited with code {proc.ExitCode}: {stderr}");
            }

            for (var i = 0; i < bitmaps.Count; i++)
            {
                var jsonPath = Path.Combine(resultDir, $"{i:D4}_res.json");
                if (TryReadJson(jsonPath, out var json))
                {
                    results[i] = ParseJsonResult(json);
                }
                else
                {
                    results[i] = string.Empty;
                }
            }

            return results;
        }
        finally
        {
            try
            {
                if (Directory.Exists(inputDir)) { Directory.Delete(inputDir, recursive: true); }
            }
            catch { /* best-effort */ }
            try
            {
                if (Directory.Exists(resultDir)) { Directory.Delete(resultDir, recursive: true); }
            }
            catch { /* best-effort */ }
        }
    }

    private ProcessStartInfo CreateProcessStartInfo(string inputPath, string resultDir)
    {
        var paddleOcrDir = Path.GetDirectoryName(ExecutablePath)
            ?? throw new InvalidOperationException("Unable to determine PaddleOCR directory.");

        var modelsDir = Path.Combine(paddleOcrDir, "models");
        var clsDir = Path.Combine(modelsDir, "cls");
        var detDir = Path.Combine(modelsDir, "det");
        var recDir = Path.Combine(modelsDir, "rec");

        const string detectionModel = "PP-OCRv6_medium_det";
        const string recognitionModel = "PP-OCRv6_medium_rec";
        const string textlineOrientationModel = "PP-LCNet_x1_0_textline_ori";

        var psi = new ProcessStartInfo(ExecutablePath)
        {
            ArgumentList = {
                "ocr",
                "-i",
                inputPath,
                "--use_textline_orientation",
                "true",
                "--use_doc_orientation_classify",
                "false",
                "--use_doc_unwarping",
                "false",
                "--lang",
                Language,
                "--text_detection_model_dir",
                Path.Combine(detDir, detectionModel),
                "--text_detection_model_name",
                detectionModel,
                "--text_recognition_model_dir",
                Path.Combine(recDir, recognitionModel),
                "--text_recognition_model_name",
                recognitionModel,
                "--textline_orientation_model_dir",
                Path.Combine(clsDir, textlineOrientationModel),
                "--textline_orientation_model_name",
                textlineOrientationModel,
                "--save_path",
                resultDir
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = System.Text.Encoding.UTF8,
            StandardErrorEncoding = System.Text.Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
        psi.EnvironmentVariables["PYTHONUTF8"] = "1";
        psi.EnvironmentVariables["PADDLE_PDX_DISABLE_MODEL_SOURCE_CHECK"] = "True";

        return psi;
    }

    private static bool TryReadJson(string path, out string json)
    {
        json = string.Empty;

        if (!File.Exists(path)) { return false; }

        try
        {
            json = File.ReadAllText(path);
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch
        {
            return false;
        }
    }

    private static string ParseJsonResult(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (!root.TryGetProperty("rec_texts", out var texts) ||
                texts.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            root.TryGetProperty("rec_polys", out var polys);

            var results = new List<(string Text, double Y, double X)>();

            for (var i = 0; i < texts.GetArrayLength(); i++)
            {
                var text = texts[i].GetString() ?? string.Empty;
                var x = 0.0;
                var y = 0.0;

                if (polys.ValueKind == JsonValueKind.Array &&
                    i < polys.GetArrayLength() &&
                    polys[i].ValueKind == JsonValueKind.Array &&
                    polys[i].GetArrayLength() >= 4)
                {
                    var poly = polys[i];
                    x = poly[0][0].GetDouble();
                    y = poly[0][1].GetDouble();

                    for (var p = 1; p < poly.GetArrayLength(); p++)
                    {
                        x = Math.Min(x, poly[p][0].GetDouble());
                        y = Math.Min(y, poly[p][1].GetDouble());
                    }
                }

                if (!string.IsNullOrWhiteSpace(text)) { results.Add((text, y, x)); }
            }

            results.Sort((a, b) => {
                if (Math.Abs(a.Y - b.Y) < 10) { return a.X.CompareTo(b.X); }
                return a.Y.CompareTo(b.Y);
            });

            return string.Join(Environment.NewLine, results.Select(r => r.Text)).Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Parses paddleocr's stdout. The CLI prints one or more <c>[bbox], (text, conf)</c>
    /// records; we extract just the recognised text from each, joining with newlines in
    /// vertical order.
    /// </summary>
    internal static string ParseStdout(string stdout)
    {
        // Match: ('text', 0.95)  -- the recognised text is before the comma in single quotes.
        var sb = new StringBuilder();
        var lines = stdout.Replace("\r\n", "\n").Split('\n');
        foreach (var line in lines)
        {
            var startIdx = line.IndexOf("('", StringComparison.Ordinal);
            if (startIdx < 0)
            {
                continue;
            }
            var endIdx = line.IndexOf("',", startIdx + 2, StringComparison.Ordinal);
            if (endIdx < 0)
            {
                continue;
            }
            var text = line[(startIdx + 2)..endIdx];
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            sb.Append(text);
        }
        return sb.ToString().Trim();
    }

    private static SKBitmap CreateDoubleBorder(
        SKBitmap source,
        int borderSize,
        SKColor innerColor,
        SKColor outerColor)
    {
        var totalBorder = borderSize * 2;
        var finalWidth = source.Width + totalBorder * 2;
        var finalHeight = source.Height + totalBorder * 2;

        var result = new SKBitmap(finalWidth, finalHeight);
        using var canvas = new SKCanvas(result);

        canvas.Clear(outerColor);

        using var paint = new SKPaint { Color = innerColor };
        canvas.DrawRect(
            borderSize,
            borderSize,
            finalWidth - borderSize * 2,
            finalHeight - borderSize * 2,
            paint);

        canvas.DrawBitmap(source, totalBorder, totalBorder);

        return result;
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
