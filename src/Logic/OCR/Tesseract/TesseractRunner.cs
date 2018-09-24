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
            _runningOnWindows = !Configuration.IsRunningOnMac() && !Configuration.IsRunningOnLinux();
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
                    process.StartInfo.Arguments += " --psm " + psmMode;
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
                process.WaitForExit(5000);
            }

            string result = string.Empty;
            string outputFileName = tempTextFileName + ".html";
            if (!File.Exists(outputFileName))
                outputFileName = tempTextFileName + ".hocr";
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
            string s = html.Replace("<em>", "@001_____").Replace("</em>", "@002_____");

            int first = s.IndexOf('<');
            while (first >= 0)
            {
                int last = s.IndexOf('>');
                if (last > 0)
                {
                    s = s.Remove(first, last - first + 1);
                    first = s.IndexOf('<');
                }
                else
                {
                    first = -1;
                }
            }

            s = s.Trim();
            s = s.Replace("@001_____", "<i>").Replace("@002_____", "</i>");
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            s = s.Replace("</i> <i>", " ");

            // html escape decoding
            s = s.Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&#39;", "'")
                .Replace("&apos;", "'");

            while (s.Contains("\n\n"))
                s = s.Replace("\n\n", "\n");
            s = s.Replace("</i>\n<i>", "\n");
            s = s.Replace("\n", Environment.NewLine);

            return s;
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
