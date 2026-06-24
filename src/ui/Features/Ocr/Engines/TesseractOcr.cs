using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Ocr;

public class TesseractOcr
{
    public string Error { get; set; }

    public TesseractOcr()
    {
        Error = string.Empty;
    }

    private string _executablePath = string.Empty;

    public static string GetExecutablePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowsPath = Path.Combine(Se.TesseractFolder, "tesseract.exe");
            return File.Exists(windowsPath) ? windowsPath : "tesseract.exe";
        }

        ReadOnlySpan<string> unixPaths =
        [
            "/opt/homebrew/bin/tesseract",
            "/opt/local/bin/tesseract",
            "/usr/local/bin/tesseract",
            "/usr/bin/tesseract",
            "/snap/bin/tesseract",
            "/opt/tesseract/bin/tesseract",
            "/home/linuxbrew/.linuxbrew/bin/tesseract",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/bin/tesseract"),
            "/app/bin/tesseract"
        ];

        foreach (var path in unixPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return "tesseract";
    }

    public async Task<string> Ocr(SKBitmap bitmap, string language, string tessDataFolder, CancellationToken cancellationToken, int engineMode = 3)
    {
        if (string.IsNullOrEmpty(_executablePath))
        {
            _executablePath = GetExecutablePath();
        }

        // Preprocess image: make black and white (black text on white background)
        var nbmp = new NikseBitmap(bitmap);
        nbmp.MakeOneColor(SKColors.Black);
        nbmp.ReplaceTransparentWith(SKColors.White);
        using var oneColorBitmap = nbmp.GetBitmap();

        var tempImage = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        var tempTextFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            await File.WriteAllBytesAsync(tempImage, oneColorBitmap.ToPngArray(), cancellationToken);

            // Use -c inline variables instead of the "hocr" configfile — avoids requiring a
            // configs/ subdirectory in the tessdata folder (user-downloaded packs don't include it).
            var psi = new ProcessStartInfo
            {
                FileName = _executablePath,
                UseShellExecute = false,
                RedirectStandardOutput = false, // output goes to temp .hocr file, not stdout
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add(tempImage);
            psi.ArgumentList.Add(tempTextFileName);
            psi.ArgumentList.Add("--tessdata-dir");
            psi.ArgumentList.Add(tessDataFolder);
            psi.ArgumentList.Add("-l");
            psi.ArgumentList.Add(language);
            psi.ArgumentList.Add("--psm");
            psi.ArgumentList.Add("6");
            psi.ArgumentList.Add("--oem");
            psi.ArgumentList.Add(engineMode.ToString(System.Globalization.CultureInfo.InvariantCulture));
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("tessedit_create_hocr=1");
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("tessedit_create_txt=0");

#pragma warning disable CA1416 // Validate platform compatibility
            using var process = new Process { StartInfo = psi };
            process.Start();
#pragma warning restore CA1416 // Validate platform compatibility

            var stderrTask = process.StandardError.ReadToEndAsync(CancellationToken.None);
            try
            {
                await process.WaitForExitAsync(cancellationToken);
            }
            catch
            {
                process.Kill();
                throw;
            }

            var stderr = await stderrTask;
            if (process.ExitCode != 0)
            {
                Error = stderr;
                return string.Empty;
            }
        }
        finally
        {
            try
            {
                File.Delete(tempImage);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        try
        {
            var htmlPath = tempTextFileName + ".html";
            if (File.Exists(htmlPath))
            {
                var result = await File.ReadAllTextAsync(htmlPath, Encoding.UTF8, cancellationToken);
                return ParseHOcr(result);
            }

            var hocrPath = tempTextFileName + ".hocr";
            if (File.Exists(hocrPath))
            {
                var result = await File.ReadAllTextAsync(hocrPath, Encoding.UTF8, cancellationToken);
                return ParseHOcr(result);
            }

            return string.Empty;
        }
        finally
        {
            try
            {
                File.Delete(tempTextFileName + ".html");
                File.Delete(tempTextFileName + ".hocr");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private static string ParseHOcr(string html)
    {
        var sb = new StringBuilder();
        var lineStart = html.IndexOf("<span class='ocr_line'", StringComparison.InvariantCulture);
        var alternateLineStart = html.IndexOf("<span class='ocr_header'", StringComparison.InvariantCulture);
        if (alternateLineStart > 0 && (lineStart < 0 || alternateLineStart < lineStart))
        {
            lineStart = alternateLineStart;
        }

        while (lineStart > 0)
        {
            var wordStart = html.IndexOf("<span class='ocrx_word'", lineStart, StringComparison.InvariantCulture);
            var wordMax = html.IndexOf("<span class='ocr_line'", lineStart + 1, StringComparison.InvariantCulture);
            if (wordMax <= 0)
            {
                wordMax = html.Length;
            }

            while (wordStart > 0 && wordStart <= wordMax)
            {
                var startText = html.IndexOf('>', wordStart + 1);
                if (startText > 0)
                {
                    startText++;
                    var endText = html.IndexOf("</span>", startText, StringComparison.InvariantCulture);
                    if (endText > 0)
                    {
                        var text = html.Substring(startText, endText - startText);
                        sb.Append(text.Trim()).Append(' ');
                    }
                }
                wordStart = html.IndexOf("<span class='ocrx_word'", wordStart + 1, StringComparison.InvariantCulture);
            }
            sb.AppendLine();
            lineStart = html.IndexOf("<span class='ocr_line'", lineStart + 1, StringComparison.InvariantCulture);
        }
        sb.Replace("<em>", "<i>")
          .Replace("</em>", "</i>")
          .Replace("<strong>", string.Empty)
          .Replace("</strong>", string.Empty)
          .Replace("</i> <i>", " ")
          .Replace("</i><i>", string.Empty);

        // html escape decoding
        sb.Replace("&amp;", "&")
          .Replace("&lt;", "<")
          .Replace("&gt;", ">")
          .Replace("&quot;", "\"")
          .Replace("&#39;", "'")
          .Replace("&apos;", "'");

        sb.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine)
          .Replace(" " + Environment.NewLine, Environment.NewLine);

        return sb.ToString().Trim();
    }
}
