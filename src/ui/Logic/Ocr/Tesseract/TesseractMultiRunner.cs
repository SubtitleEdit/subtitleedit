using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Ocr.Tesseract
{
    /// <summary>
    /// Run multiple images per tesseract call
    /// </summary>
    public class TesseractMultiRunner
    {
        public List<string> TesseractErrors { get; set; }
        public string LastError { get; set; }
        private readonly bool _runningOnWindows;

        public TesseractMultiRunner()
        {
            TesseractErrors = new List<string>();
            _runningOnWindows = Configuration.IsRunningOnWindows;
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

        public List<string> Run(string languageCode, string psmMode, string engineMode, List<string> imageFileNames, bool run302 = false)
        {
            var dir = run302 ? Configuration.Tesseract302Directory : Configuration.TesseractDirectory;
            string inputFileName = Path.GetTempPath() + Guid.NewGuid() + ".txt";
            var filesToDelete = new List<string>();
            var sb = new StringBuilder();
            foreach (var imageFileName in imageFileNames)
            {
                filesToDelete.Add(imageFileName);
                sb.AppendLine(imageFileName);
            }

            File.WriteAllText(inputFileName, sb.ToString());
            filesToDelete.Add(inputFileName);
            var outputFileName = Path.GetTempPath() + Guid.NewGuid();
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(dir + "tesseract.exe")
                {
                    UseShellExecute = true,
                    Arguments = "\"" + inputFileName + "\" \"" + outputFileName + "\" -l " + languageCode
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
                    return new List<string> { "Error!" };
                }

                process.WaitForExit(8000 + imageFileNames.Count * 500);

                var result = new List<string>();
                string resultFileName = outputFileName + ".html";
                if (!File.Exists(outputFileName))
                {
                    resultFileName = outputFileName + ".hocr";
                }

                filesToDelete.Add(resultFileName);
                try
                {
                    if (File.Exists(resultFileName))
                    {
                        result = ParseHocr(File.ReadAllText(resultFileName, Encoding.UTF8));
                    }
                    foreach (var fileName in filesToDelete)
                    {
                        if (File.Exists(fileName))
                        {
                            File.Delete(fileName);
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                return result;
            }
        }

        internal static List<string> ParseHocr(string html)
        {
            var pages = new List<string>();
            var pageStart = html.IndexOf("<div class='ocr_page'", StringComparison.InvariantCulture);
            while (pageStart > 0)
            {
                var pageEnd = html.IndexOf("<div class='ocr_page'", pageStart + 1, StringComparison.InvariantCulture);
                var sb = new StringBuilder();
                var lineStart = html.IndexOf("<span class='ocr_line'", pageStart, StringComparison.InvariantCulture);
                while (lineStart > 0 && lineStart < pageEnd)
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
                            var endText = html.IndexOf("</span>", startText + 1, StringComparison.InvariantCulture);
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
                var page = sb.ToString().Replace("<em>", "<i>").Replace("</em>", "</i>");
                page = page.Replace("<strong>", string.Empty).Replace("</strong>", string.Empty);
                page = page.Replace("</i> <i>", " ");
                page = page.Replace("</i><i>", string.Empty);

                // html escape decoding
                page = page.Replace("&amp;", "&")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">")
                    .Replace("&quot;", "\"")
                    .Replace("&#39;", "'")
                    .Replace("&apos;", "'");

                page = page.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
                page = page.Replace(" " + Environment.NewLine, Environment.NewLine);

                pages.Add(page.Trim());
                pageStart = pageEnd;
            }

            return pages;
        }

    }
}
