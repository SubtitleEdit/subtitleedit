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
        public List<string> TesseractErrors { get; set; }
        public string LastError { get; set; }
        private readonly bool _runningOnWindows;

        public TesseractRunner()
        {
            TesseractErrors = new List<string>();
            _runningOnWindows = Configuration.IsRunningOnWindows;
        }

        public string Run(string languageCode, string psmMode, string engineMode, string imageFileName, bool run302 = false)
        {
            LastError = null;
            var dir = run302 ? Configuration.Tesseract302Directory : Configuration.TesseractDirectory;
            string tempTextFileName = Path.GetTempPath() + Guid.NewGuid();
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(Path.Combine(dir, "tesseract.exe"))
                {
                    UseShellExecute = true,
                    Arguments = "\"" + imageFileName + "\" \"" + tempTextFileName + "\" -l " + languageCode
                };

                if (!string.IsNullOrEmpty(psmMode))
                {
                    var prefix = run302 ? " -psm " : " --psm ";
                    process.StartInfo.Arguments += prefix + psmMode;
                }

                if (!string.IsNullOrEmpty(engineMode) && !run302)
                {
                    process.StartInfo.Arguments += " --oem " + engineMode;
                }

                process.StartInfo.Arguments += " hocr";

                if (_runningOnWindows)
                {
                    if (run302)
                    {
                        process.StartInfo.WorkingDirectory = Configuration.Tesseract302Directory;
                    }
                    else
                    {
                        process.ErrorDataReceived += TesseractErrorReceived;
                        process.StartInfo.Arguments = " --tessdata-dir \"" + Path.Combine(dir, "tessdata") + "\" " + process.StartInfo.Arguments.Trim();
                    }
                }
                else
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.FileName = "tesseract";
                }

                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
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

            string result = string.Empty;
            string outputFileName = tempTextFileName + ".html";
            if (!File.Exists(outputFileName))
            {
                outputFileName = tempTextFileName + ".hocr";
            }

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
            while (lineStart > 0)
            {
                var wordStart = html.IndexOf("<span class='ocrx_word'", lineStart, StringComparison.InvariantCulture);
                int wordMax = html.IndexOf("<span class='ocr_line'", lineStart + 1, StringComparison.InvariantCulture);
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
                            sb.Append(text.Trim() + " ");
                        }
                    }
                    wordStart = html.IndexOf("<span class='ocrx_word'", wordStart + 1, StringComparison.InvariantCulture);
                }
                sb.AppendLine();
                lineStart = html.IndexOf("<span class='ocr_line'", lineStart + 1, StringComparison.InvariantCulture);
            }
            var result = sb.ToString().Replace("<em>", "<i>").Replace("</em>", "</i>");
            result = result.Replace("<strong>", string.Empty).Replace("</strong>", string.Empty);
            result = result.Replace("</i> <i>", " ");
            result = result.Replace("</i><i>", string.Empty);

            // html escape decoding
            result = result.Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&#39;", "'")
                .Replace("&apos;", "'");

            result = result.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
            result = result.Replace(" " + Environment.NewLine, Environment.NewLine);

            return result.Trim();
        }

        private void TesseractErrorReceived(object sender, DataReceivedEventArgs e)
        {
            string msg = e.Data;

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
