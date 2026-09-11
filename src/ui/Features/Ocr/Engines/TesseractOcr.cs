using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
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
        if (OperatingSystem.IsWindows())
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

    /// <summary>
    /// Margin of white added around the text before it is handed to Tesseract. SE4 added the same
    /// 10 px (VobSubOcr.GetSubtitleBitmap → AddMargin(10)); without it glyphs touch the image edge
    /// and Tesseract misreads them (discussion #12929: "In"/"Is" for "in"/"is", "\What", "(o").
    /// </summary>
    public const int Margin = 10;

    /// <summary>Page segmentation mode for the normal pass: a single uniform block of text.</summary>
    public const int PsmSingleBlock = 6;

    /// <summary>Page segmentation mode for the resized retry: let Tesseract find the layout.</summary>
    public const int PsmAuto = 3;

    /// <summary>Page segmentation mode for the last-resort retry: a single text line.</summary>
    public const int PsmSingleLine = 7;

    /// <summary>
    /// Builds the image Tesseract is fed: a white margin around the subtitle, binarized to black
    /// text on white, and optionally stretched (SE4 retried unknown words with 3x width / 2x
    /// height, and blank results with 4x / 2x). Keys on brightness so coloured text (e.g. yellow)
    /// is kept rather than blanked the way the blue-only MakeOneColor did.
    /// </summary>
    internal static SKBitmap PrepareImage(SKBitmap bitmap, int scaleX = 1, int scaleY = 1)
    {
        var nbmp = new NikseBitmap(bitmap);
        nbmp.AddMargin(Margin);
        nbmp.MakeBlackAndWhiteForOcr();
        var prepared = nbmp.GetBitmap();
        if (scaleX <= 1 && scaleY <= 1)
        {
            return prepared;
        }

        using (prepared)
        {
            var info = new SKImageInfo(prepared.Width * scaleX, prepared.Height * scaleY, prepared.ColorType, prepared.AlphaType);
            return prepared.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell)) ?? prepared.Copy();
        }
    }

    /// <summary>
    /// A retry pass is only taken when it does not invent a digit the first pass did not see:
    /// the stretched image tends to read a stray outline pixel as "7" ("18 months" → "718 months").
    /// Same guard as SE4 used when choosing between Tesseract passes.
    /// </summary>
    internal static bool RetryIntroducesDigit(string firstPass, string retry)
    {
        foreach (var c in retry)
        {
            if (char.IsDigit(c) && !firstPass.Contains(c))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly System.Text.RegularExpressions.Regex WhitespaceSplit = new(@"(\s+)", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Word-wise merge of a retry pass into the first pass: only tokens the dictionary flagged as
    /// unknown in the first pass are taken from the retry, every other token is kept. The stretched
    /// retry tends to fix the unknown word but damage a known one ("diedq," → "died," but
    /// "18 months" → "718 months"), so taking it whole is rejected and taking it word-wise is not.
    /// Null when the two passes do not line up token for token or no unknown word changed.
    /// </summary>
    internal static string? MergeRetryUnknownWords(string firstPass, string retry, IReadOnlyCollection<string> unknownWords)
    {
        if (unknownWords.Count == 0)
        {
            return null;
        }

        var first = WhitespaceSplit.Split(firstPass);
        var second = WhitespaceSplit.Split(retry);
        if (first.Length != second.Length)
        {
            return null;
        }

        var changed = false;
        for (var i = 0; i < first.Length; i++)
        {
            if (first[i] == second[i])
            {
                continue;
            }

            if (i % 2 == 1)
            {
                return null; // whitespace differs - the passes do not line up
            }

            if (!ContainsUnknownWord(first[i], unknownWords))
            {
                continue; // a word the dictionary accepted: keep the first pass' reading
            }

            first[i] = second[i];
            changed = true;
        }

        return changed ? string.Concat(first) : null;
    }

    /// <summary>
    /// Whole-word match: the token is unknown when one of its words (the runs between punctuation
    /// and tags, e.g. "diedq" in "diedq,") equals a flagged word. A substring test would let a lone
    /// unknown "l" claim nearly every English token and replace words the dictionary accepted.
    /// </summary>
    private static bool ContainsUnknownWord(string token, IReadOnlyCollection<string> unknownWords)
    {
        var start = -1;
        for (var i = 0; i <= token.Length; i++)
        {
            // Same word characters as OcrFixEngine.SplitLine, which produced the unknown words.
            var isWordChar = i < token.Length && (char.IsLetterOrDigit(token[i]) || token[i] == '\'' || token[i] == '’' || token[i] == '-');
            if (isWordChar)
            {
                if (start < 0)
                {
                    start = i;
                }

                continue;
            }

            if (start >= 0)
            {
                var word = token.AsSpan(start, i - start);
                if (IsUnknownWord(word, unknownWords) || IsUnknownWord(word.Trim("'’-"), unknownWords))
                {
                    return true;
                }

                start = -1;
            }
        }

        return false;
    }

    private static bool IsUnknownWord(ReadOnlySpan<char> word, IReadOnlyCollection<string> unknownWords)
    {
        foreach (var unknown in unknownWords)
        {
            if (word.Length > 0 && word.Equals(unknown.AsSpan(), StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<string> Ocr(SKBitmap bitmap, string language, string tessDataFolder, CancellationToken cancellationToken, int engineMode = 3, int psm = PsmSingleBlock, int scaleX = 1, int scaleY = 1)
    {
        if (string.IsNullOrEmpty(_executablePath))
        {
            _executablePath = GetExecutablePath();
        }

        Error = string.Empty;
        using var oneColorBitmap = PrepareImage(bitmap, scaleX, scaleY);

        var tempImage = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        var tempTextFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // When the tools log is on, record what Tesseract is actually fed: how much "ink" survived
        // the black/white preprocessing (0% means the text was blanked, e.g. coloured subtitles) and
        // a copy of the exact image, so blank output can be diagnosed without guessing.
        if (Se.Settings.Tools.WriteToolsLog)
        {
            var inkPercent = GetInkPercent(new NikseBitmap(oneColorBitmap));
            Se.WriteToolsLog($"Tesseract OCR: input {oneColorBitmap.Width}x{oneColorBitmap.Height}, ink={inkPercent:0.0}% (0% = preprocessing blanked the text), lang={language}, oem={engineMode}, psm={psm}");
            try
            {
                var logDir = Path.GetDirectoryName(Se.GetToolsLogFilePath()) ?? Path.GetTempPath();
                var debugCopy = Path.Combine(logDir, "tesseract-input.png");
                await File.WriteAllBytesAsync(debugCopy, oneColorBitmap.ToPngArray(), cancellationToken);
                Se.WriteToolsLog($"Tesseract OCR: saved preprocessed image to {debugCopy}");
            }
            catch
            {
                // ignore debug-save failures
            }
        }

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
            psi.ArgumentList.Add(psm.ToString(System.Globalization.CultureInfo.InvariantCulture));
            psi.ArgumentList.Add("--oem");
            psi.ArgumentList.Add(engineMode.ToString(System.Globalization.CultureInfo.InvariantCulture));
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("tessedit_create_hocr=1");
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("tessedit_create_txt=0");

#pragma warning disable CA1416 // Validate platform compatibility
            using var process = new Process { StartInfo = psi };
            try
            {
                process.Start();
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Error = $"Could not start Tesseract at \"{_executablePath}\": {ex.Message}." +
                        (OperatingSystem.IsWindows()
                            ? string.Empty
                            : " Make sure Tesseract is installed (e.g. \"brew install tesseract\" on macOS, \"apt install tesseract-ocr\" on Linux).");
                return string.Empty;
            }
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
            if (Se.Settings.Tools.WriteToolsLog)
            {
                Se.WriteToolsLog($"Tesseract OCR: exit={process.ExitCode}" +
                                 (string.IsNullOrWhiteSpace(stderr) ? string.Empty : " stderr=" + stderr.Trim()));
            }

            if (process.ExitCode != 0)
            {
                Error = string.IsNullOrWhiteSpace(stderr)
                    ? $"Tesseract exited with code {process.ExitCode}."
                    : stderr.Trim();
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

    // Percentage of dark "ink" pixels in the preprocessed (black-on-white) image. ~0% means the
    // black/white conversion blanked the text (e.g. coloured subtitles), which yields empty OCR.
    // Runs once per image on every Tesseract call; works on the raw BGRA words instead of
    // constructing an SKColor per pixel: alpha > 0 is "any bit in the top byte", and
    // r,g,b < 128 is "no 0x80 bit in the low three bytes" - one masked compare per pixel.
    internal static double GetInkPercent(NikseBitmap nbmp)
    {
        long total = (long)nbmp.Width * nbmp.Height;
        if (total == 0)
        {
            return 0;
        }

        var pixels = MemoryMarshal.Cast<byte, uint>(nbmp.GetPixelData());
        long ink = 0;
        var i = 0;

        if (Vector.IsHardwareAccelerated && pixels.Length >= Vector<uint>.Count)
        {
            var alphaMask = new Vector<uint>(0xFF000000);
            var rgbHighBits = new Vector<uint>(0x00808080);
            var counts = Vector<uint>.Zero;
            var lastBlockStart = pixels.Length - Vector<uint>.Count;
            for (; i <= lastBlockStart; i += Vector<uint>.Count)
            {
                var p = new Vector<uint>(pixels.Slice(i));
                var alphaNonZero = Vector.OnesComplement(Vector.Equals(Vector.BitwiseAnd(p, alphaMask), Vector<uint>.Zero));
                var rgbDark = Vector.Equals(Vector.BitwiseAnd(p, rgbHighBits), Vector<uint>.Zero);
                // Matching lanes are all-ones (i.e. uint.MaxValue = -1); subtracting adds 1.
                counts -= Vector.BitwiseAnd(alphaNonZero, rgbDark);
            }

            for (var lane = 0; lane < Vector<uint>.Count; lane++)
            {
                ink += counts[lane];
            }
        }

        for (; i < pixels.Length; i++)
        {
            var p = pixels[i];
            if ((p & 0xFF000000) != 0 && (p & 0x00808080) == 0)
            {
                ink++;
            }
        }

        return ink * 100.0 / total;
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
