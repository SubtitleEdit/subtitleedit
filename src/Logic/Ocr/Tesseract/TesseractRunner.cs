using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.Ocr.Tesseract
{
    public class TesseractRunner
    {
        private readonly bool _runningOnWindows;

        public string LastError { get; private set; }
        public List<string> TesseractErrors { get; }

        public TesseractRunner()
            : this(true)
        {
        }

        public TesseractRunner(bool collectErrors)
        {
            if (collectErrors)
            {
                TesseractErrors = new List<string>();
            }
            _runningOnWindows = Configuration.IsRunningOnWindows;
        }

        public string Run(string languageCode, string psmMode, string engineMode, string imageFileName, bool run302)
        {
            LastError = null;

            var tempTextFileName = Path.GetTempPath() + Guid.NewGuid();
            using (var process = new Process())
            {
                process.StartInfo.Arguments = $@"""{imageFileName}"" ""{tempTextFileName}"" -l {languageCode}";

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
                    string tesseractDirectory;
                    if (run302)
                    {
                        tesseractDirectory = Configuration.Tesseract302Directory;
                        process.StartInfo.WorkingDirectory = tesseractDirectory;
                    }
                    else
                    {
                        tesseractDirectory = Configuration.TesseractDirectory;
                        process.StartInfo.Arguments = $@"--tessdata-dir ""{Path.Combine(tesseractDirectory, "tessdata")}"" {process.StartInfo.Arguments}";
                    }
                    process.StartInfo.FileName = Path.Combine(tesseractDirectory, "tesseract.exe");
                }
                else
                {
                    process.StartInfo.FileName = "tesseract";
                }
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += TesseractStandardErrorHandler;

                try
                {
                    process.Start();
                    process.BeginErrorReadLine();
                }
                catch (Exception exception)
                {
                    AddTesseractError(exception.Message + Environment.NewLine + exception.StackTrace);
                    return "Error!";
                }
                process.WaitForExit(8000);

                if (!process.HasExited)
                {
                    AddTesseractError("Tesseract timeout");
                    return "Timeout!";
                }
                if (process.ExitCode != 0 && string.IsNullOrEmpty(LastError))
                {
                    AddTesseractError("Tesseract exited with code " + process.ExitCode);
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
                    result = ParseHocr(File.ReadAllText(outputFileName, Encoding.UTF8));
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

        private void AddTesseractError(string message)
        {
            if (TesseractErrors != null)
            {
                LastError = message;
                TesseractErrors.Add(message);
            }
        }

        private void TesseractStandardErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (TesseractErrors != null && TesseractErrors.Count <= 100)
            {
                var message = e.Data;

                if (string.IsNullOrEmpty(message) ||
                    message.StartsWith("Tesseract Open Source OCR Engine", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("Too few characters", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("Empty page", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains(" diacritics", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("Weak margin", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (string.IsNullOrEmpty(LastError))
                {
                    LastError = message;
                }
                else if (!LastError.Contains(message))
                {
                    LastError = LastError + Environment.NewLine + message;
                }
                TesseractErrors.Add(message);
            }
        }

    }
}
