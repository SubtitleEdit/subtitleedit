using System.Diagnostics;
using System.Text;
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
        try
        {
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
            using (var fs = File.Create(pngPath))
            {
                data.SaveTo(fs);
            }

            var psi = new ProcessStartInfo(ExecutablePath)
            {
                ArgumentList = { "ocr", "-i", pngPath, "--lang", Language, "--use_angle_cls", "false" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start paddleocr process.");
            var stdout = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                var err = proc.StandardError.ReadToEnd();
                throw new InvalidOperationException($"paddleocr exited with code {proc.ExitCode}: {err}");
            }
            return ParseStdout(stdout);
        }
        finally
        {
            try { File.Delete(pngPath); } catch { /* best-effort */ }
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
