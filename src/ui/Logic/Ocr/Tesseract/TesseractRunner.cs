using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Ocr.Tesseract
{
    public class TesseractRunner
    {
        public string LastError { get; private set; }
        public List<string> TesseractErrors { get; }
        private readonly bool _runningOnWindows;

        public TesseractRunner()
        {
            TesseractErrors = new List<string>();
            _runningOnWindows = Configuration.IsRunningOnWindows;
        }

        public string Run(string languageCode, string psmMode, string engineMode, string imageFileName, bool run302 = false)
        {
            LastError = null;
            var tempTextFileName = Path.GetTempPath() + Guid.NewGuid();
            var tesseractDirectory = run302 ? Configuration.Tesseract302Directory : Configuration.TesseractDirectory;

            if (!_runningOnWindows)
            {
                // Tesseract 3.02 is only for Windows
                run302 = false;
                tesseractDirectory = Configuration.TesseractDirectory;
            }

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(Path.Combine(tesseractDirectory, "tesseract.exe"))
                {
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = $@"""{imageFileName}"" ""{tempTextFileName}"" -l {languageCode}"
                };

                if (!string.IsNullOrEmpty(psmMode))
                {
                    var prefix = run302 ? "-psm" : "--psm";
                    process.StartInfo.Arguments += $" {prefix} {psmMode}";
                }

                if (!string.IsNullOrEmpty(engineMode) && !run302)
                {
                    process.StartInfo.Arguments += $" --oem {engineMode}";
                }

                process.StartInfo.Arguments += " hocr";

                if (_runningOnWindows)
                {
                    process.StartInfo.WorkingDirectory = tesseractDirectory;
                    if (!run302)
                    {
                        process.ErrorDataReceived += TesseractErrorReceived;
                        process.StartInfo.Arguments = $@"--tessdata-dir ""{Path.Combine(tesseractDirectory, "tessdata")}"" {process.StartInfo.Arguments.Trim()}";
                    }
                }
                else
                {
                    process.StartInfo.FileName = "tesseract";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                }

                try
                {
                    process.Start();
                }
                catch (Exception exception)
                {
                    LastError = exception.Message + Environment.NewLine + exception.StackTrace;
                    TesseractErrors.Add(LastError);
                    return "Error!";
                }
                process.WaitForExit(8000);

                if (process.HasExited && process.ExitCode != 0)
                {
                    LastError = "Tesseract returned with code " + process.ExitCode;
                    TesseractErrors.Add(LastError);
                }
            }

            var outputFileName = tempTextFileName + ".html";
            if (!File.Exists(outputFileName))
            {
                outputFileName = tempTextFileName + ".hocr";
            }

            var result = string.Empty;
            try
            {
                if (File.Exists(outputFileName))
                {
                    result = File.ReadAllText(outputFileName, Encoding.UTF8);
                    result = ParseHocr(result);
                    File.Delete(outputFileName);
                }
                File.Delete(imageFileName);
            }
            catch
            {
                // ignored
            }

            return result;
        }

        private static string ParseHocr(string html)
        {
            var sb = new StringBuilder();
            var lineStart = html.IndexOf("<span class='ocr_line'", StringComparison.InvariantCulture);
            var alternateLineStart = html.IndexOf("<span class='ocr_header'", StringComparison.InvariantCulture);
            if (alternateLineStart > 0)
            {
                lineStart = Math.Min(lineStart, alternateLineStart);
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

        private void TesseractErrorReceived(object sender, DataReceivedEventArgs e)
        {
            var msg = e.Data;

            if (string.IsNullOrEmpty(msg) ||
                msg.StartsWith("Tesseract Open Source OCR Engine", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Too few characters", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Empty page", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains(" diacritics", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Weak margin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (TesseractErrors.Count <= 100)
            {
                if (string.IsNullOrEmpty(LastError))
                {
                    LastError = msg;
                }
                else if (!LastError.Contains(msg))
                {
                    LastError = LastError + Environment.NewLine + msg;
                }
                TesseractErrors.Add(msg);
            }
        }

    }
}
