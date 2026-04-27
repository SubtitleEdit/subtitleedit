using System.Diagnostics;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// OCR engine that delegates to a Tesseract binary on the system PATH. seconv does not
/// bundle Tesseract — users install it separately (e.g. <c>apt install tesseract-ocr</c>,
/// <c>brew install tesseract</c>, or the Windows installer from UB Mannheim).
/// </summary>
internal sealed class TesseractOcrEngine : IOcrEngine
{
    public string Name => "tesseract";
    public string ExecutablePath { get; }
    public string Language { get; }

    private readonly string _workDir;

    private TesseractOcrEngine(string executablePath, string language, string workDir)
    {
        ExecutablePath = executablePath;
        Language = language;
        _workDir = workDir;
    }

    /// <summary>
    /// Locates Tesseract on the system PATH (cross-platform). Returns null if missing.
    /// </summary>
    public static string? Detect()
    {
        var name = OperatingSystem.IsWindows() ? "tesseract.exe" : "tesseract";
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

    public static TesseractOcrEngine Create(string language = "eng")
    {
        var path = Detect()
            ?? throw new InvalidOperationException(
                "Tesseract not found on PATH. Install it from https://tesseract-ocr.github.io/ " +
                "(or `apt install tesseract-ocr` / `brew install tesseract`) and ensure the binary is on PATH.");

        var workDir = Path.Combine(Path.GetTempPath(), "seconv_ocr_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);
        return new TesseractOcrEngine(path, language, workDir);
    }

    /// <summary>
    /// Runs Tesseract on a single bitmap and returns the recognised text. The bitmap is
    /// composited onto white (Tesseract handles antialiased text better with an opaque
    /// background) and scaled up 2× for accuracy on small subtitle bitmaps.
    /// </summary>
    public string Recognize(SKBitmap bitmap)
    {
        if (bitmap is null || bitmap.Width == 0 || bitmap.Height == 0)
        {
            return string.Empty;
        }

        var prepped = Preprocess(bitmap);
        var pngPath = Path.Combine(_workDir, "in_" + Guid.NewGuid().ToString("N") + ".png");
        try
        {
            using (var image = SKImage.FromBitmap(prepped))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 90))
            using (var fs = File.Create(pngPath))
            {
                data.SaveTo(fs);
            }

            var psi = new ProcessStartInfo(ExecutablePath)
            {
                ArgumentList = { pngPath, "stdout", "-l", Language, "--psm", "6" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start tesseract process.");
            var stdout = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                var err = proc.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Tesseract exited with code {proc.ExitCode}: {err}");
            }
            return stdout.Trim();
        }
        finally
        {
            try { File.Delete(pngPath); } catch { /* best-effort */ }
            prepped.Dispose();
        }
    }

    private static SKBitmap Preprocess(SKBitmap source)
    {
        const int scale = 2;
        var w = source.Width * scale;
        var h = source.Height * scale;
        var prepped = new SKBitmap(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Opaque));
        using var canvas = new SKCanvas(prepped);
        canvas.Clear(SKColors.White);
        var paint = new SKPaint { FilterQuality = SKFilterQuality.High };
        canvas.DrawBitmap(source, new SKRect(0, 0, w, h), paint);
        canvas.Flush();
        return prepped;
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
        catch
        {
            // Ignore cleanup races.
        }
    }
}
