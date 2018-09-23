using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.Ocr.Tesseract
{
    /// <summary>
    /// Run multiple images per tesseract call
    /// </summary>
    public class TesseractMultiRunner
    {
        private readonly List<string> _tesseractErrors;

        public TesseractMultiRunner()
        {
            _tesseractErrors = new List<string>();
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

            _tesseractErrors.Add(msg);
        }

        public string Run(List<NikseBitmap> bmps, string language, string psmMode)
        {
            // change yellow color to white - easier for Tesseract
            string inputFileName = Path.GetTempPath() + Guid.NewGuid() + ".txt";
            var filesToDelete = new List<string>();
            var sb = new StringBuilder();
            foreach (var bmp in bmps)
            {
                bmp.ReplaceYellowWithWhite(); // optimized replace
                string pngFileName = Path.GetTempPath() + Guid.NewGuid() + ".png";
                using (var b = bmp.GetBitmap())
                {
                    b.Save(pngFileName, System.Drawing.Imaging.ImageFormat.Png);
                }
                filesToDelete.Add(pngFileName);
                sb.AppendLine(pngFileName);
            }

            File.WriteAllText(inputFileName, sb.ToString());
            filesToDelete.Add(inputFileName);
            var outputFileName = Path.GetTempPath() + Guid.NewGuid();
            var dir = @"C:\Data\SubtitleEdit\subtitleedit\src\bin\Debug\Tesseract4";
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(dir + "tesseract.exe")
                {
                    UseShellExecute = true,
                    Arguments = "\"" + inputFileName + "\" \"" + outputFileName + "\" -l " + language
                };

                if (!string.IsNullOrEmpty(psmMode))
                    process.StartInfo.Arguments += " " + psmMode.Trim();

                process.StartInfo.Arguments += " hocr";
                process.StartInfo.Arguments = " --tessdata-dir \"" + Path.Combine(dir, "tessdata") + "\" " + process.StartInfo.Arguments.Trim();
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                if (Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac())
                {
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.FileName = "tesseract";
                }
                else
                {
                    var tessdataPath = Path.Combine(Configuration.TesseractDirectory, "tessdata");
                    process.StartInfo.Arguments = " --tessdata-dir \"" + tessdataPath + "\" " + process.StartInfo.Arguments.Trim();
                    process.StartInfo.WorkingDirectory = Configuration.TesseractDirectory;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.ErrorDataReceived += TesseractErrorReceived;
                    process.EnableRaisingEvents = true;
                }

                try
                {
                    process.Start();
                    process.BeginErrorReadLine();
                }
                catch
                {
                    if (_tesseractErrors.Count <= 2)
                    {

                        if (Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac())
                        {
                            _tesseractErrors.Add("Unable to start 'Tesseract' - make sure tesseract-ocr 4.x is installed!");
                        }
                        else
                        {
                            _tesseractErrors.Add("Unable to start 'Tesseract' (" + Configuration.TesseractDirectory + "tesseract.exe) - make sure Subtitle Edit is install correctly + Visual Studio 2017 C++ runtime");
                        }
                    }
                }

                process.WaitForExit(5000 + bmps.Count * 500);

                string result = string.Empty;
                string resultFileName = outputFileName + ".html";
                if (!File.Exists(outputFileName))
                    resultFileName = outputFileName + ".hocr";
                filesToDelete.Add(resultFileName);
                try
                {
                    if (File.Exists(outputFileName))
                    {
                        result = File.ReadAllText(outputFileName, Encoding.UTF8);
                        result = ParseHocr(result);
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
            s = s.Replace("&amp;", "&");
            s = s.Replace("&lt;", "<");
            s = s.Replace("&gt;", ">");
            s = s.Replace("&quot;", "\"");
            s = s.Replace("&#39;", "'");
            s = s.Replace("&apos;", "'");

            while (s.Contains("\n\n"))
                s = s.Replace("\n\n", "\n");
            s = s.Replace("</i>\n<i>", "\n");
            s = s.Replace("\n", Environment.NewLine);

            return s;
        }

    }
}