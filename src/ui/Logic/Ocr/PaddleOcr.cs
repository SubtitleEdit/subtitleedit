using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Nikse.SubtitleEdit.Forms.Ocr.VobSubOcr;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class PaddleOcr
    {
        private List<PaddleOcrResultParser.TextDetectionResult> _textDetectionResults = new List<PaddleOcrResultParser.TextDetectionResult>(); public string Error { get; set; }
        private bool hasErrors = false;
        private bool processingStarted = false;
        private string _paddingOcrPath;
        private string _clsPath;
        private string _detPath;
        private string _recPath;
        private StringBuilder _log = new StringBuilder();
        private const string TextlineOrientationModelName = "PP-LCNet_x1_0_textline_ori";

        private readonly List<string> LatinLanguageCodes = new List<string>
        {
            "af", "az", "bs", "cs", "cy", "da", "de", "es", "et", "fr", "ga",
            "hr", "hu", "id", "is", "it", "ku", "la", "lt", "lv", "mi", "ms",
            "mt", "nl", "no", "oc", "pi", "pl", "pt", "ro", "rs_latin", "sk",
            "sl", "sq", "sv", "sw", "tl", "tr", "uz", "vi", "french", "german"
        };

        private readonly List<string> ArabicLanguageCodes = new List<string>
        {
            "ar", "fa", "ug", "ur"
        };

        private readonly List<string> EslavLanguageCodes = new List<string>
        {
            "ru", "be", "uk"
        };

        private readonly List<string> CyrillicLanguageCodes = new List<string>
        {
            "rs_cyrillic", "bg", "mn", "abq", "ady", "kbd", "ava", "dar",
            "inh", "che", "lbe", "lez", "tab"
        };

        private readonly List<string> DevanagariLanguageCodes = new List<string>
        {
            "hi", "mr", "ne", "bh", "mai", "ang", "bho", "mah",
            "sck", "new", "gom", "bgc", "sa"
        };

        public PaddleOcr()
        {
            Error = string.Empty;
            _paddingOcrPath = Configuration.PaddleOcrDirectory;
            _clsPath = Path.Combine(_paddingOcrPath, "cls");
            _detPath = Path.Combine(_paddingOcrPath, "det");
            _recPath = Path.Combine(_paddingOcrPath, "rec");
        }

        private string GetRecName(string language, string mode)
        {
            string recName;
            if (language == "ch" ||
                language == "chinese_cht" ||
                language == "en" ||
                language == "japan")
            {
                recName = $"PP-OCRv5_{mode}_rec";
            }
            else if (ArabicLanguageCodes.Contains(language))
            {
                recName = "arabic_PP-OCRv3_mobile_rec";
            }
            else if (EslavLanguageCodes.Contains(language))
            {
                recName = "eslav_PP-OCRv5_mobile_rec";
            }
            else if (CyrillicLanguageCodes.Contains(language))
            {
                recName = "cyrillic_PP-OCRv3_mobile_rec";
            }
            else if (DevanagariLanguageCodes.Contains(language))
            {
                recName = "devanagari_PP-OCRv3_mobile_rec";
            }
            else if (language == "korean")
            {
                recName = "korean_PP-OCRv5_mobile_rec";
            }
            else
            {
                recName = "latin_PP-OCRv5_mobile_rec";
            }

            return recName;
        }

        private string GetDetectionName(string language, string mode)
        {
            if (language == "ch" ||
                language == "chinese_cht" ||
                language == "en" ||
                language == "japan" ||
                language == "korean" ||
                LatinLanguageCodes.Contains(language) ||
                EslavLanguageCodes.Contains(language))
            {
                return $"PP-OCRv5_{mode}_det";
            }
            else
            {
                return "PP-OCRv3_mobile_det";
            }
        }

        private void SaveText(DataReceivedEventArgs outLine)
        {
            // Note: It's *very* important to keep the box positions
            //       as they are used to determine the line breaking/order of the text
            var pattern = @"\[\[\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+]],\s*\(['""].*['""],\s*\d+\.\d+\)\]";
            var match = Regex.Match(outLine.Data, pattern);
            if (match.Success)
            {
                var parser = new PaddleOcrResultParser();
                var x = parser.Parse(outLine.Data);
                _textDetectionResults.Add(x);
            }
        }

        private object LockObject = new object();

        public void OcrBatch(List<OcrInput> input, string language, bool useGpu, string mode, Action<OcrInput> progressCallback, Func<bool> abortCheck)
        {
            _log.Clear();
            hasErrors = false;
            processingStarted = false;

            var detName = GetDetectionName(language, mode);
            var recName = GetRecName(language, mode);

            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            try
            {
                var imagePaths = new List<string>();
                var width = input.Count.ToString().Length; // Determine the number of digits for file name padding

                for (var i = 0; i < input.Count; i++)
                {
                    var borderedBitmapTemp = AddBorder(input[i].Bitmap, 10, Color.Black);
                    var borderedBitmap = AddBorder(borderedBitmapTemp, 10, Color.Transparent);
                    borderedBitmapTemp.Dispose();

                    var imagePath = Path.Combine(tempFolder, string.Format("{0:D" + width + "}.png", i));
                    borderedBitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
                    input[i].FileName = imagePath;
                    borderedBitmap.Dispose();
                    imagePaths.Add(imagePath);
                }

                var parameters = $"ocr -i \"{tempFolder}\" " +
                   "--use_textline_orientation true " +
                   "--use_doc_orientation_classify false " +
                   "--use_doc_unwarping false " +
                   $"--device {(useGpu ? "gpu" : "cpu")} " +
                   $"--lang {language} " +
                   $"--text_detection_model_dir \"{_detPath + Path.DirectorySeparatorChar + detName}\" " +
                   $"--text_detection_model_name \"{detName}\" " +
                   $"--text_recognition_model_dir \"{_recPath + Path.DirectorySeparatorChar + recName}\" " +
                   $"--text_recognition_model_name \"{recName}\" " +
                   $"--textline_orientation_model_dir \"{_clsPath + Path.DirectorySeparatorChar + TextlineOrientationModelName}\" " +
                   $"--textline_orientation_model_name \"{TextlineOrientationModelName}\"";

                string PaddleOCRPath = "paddleocr";
                if (File.Exists(Path.Combine(Configuration.PaddleOcrDirectory, "paddleocr.exe")))
                {
                    PaddleOCRPath = Path.GetFullPath(Path.Combine(Configuration.PaddleOcrDirectory, "paddleocr.exe"));
                }

                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = PaddleOCRPath,
                        Arguments = parameters,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                    }
                })
                {
                    process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
                    process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";

                    var results = new Dictionary<string, string>();
                    var currentProgress = 0;
                    var oldFileName = string.Empty;

                    process.OutputDataReceived += (sendingProcess, outLine) =>
                    {
                        if (string.IsNullOrWhiteSpace(outLine.Data))
                        {
                            return;
                        }

                        _log.AppendLine(outLine.Data);

                        if (outLine.Data.Contains("ppocr INFO: **********"))
                        {
                            if (!processingStarted)
                            {
                                processingStarted = true;
                                hasErrors = false;
                                _log.Clear();
                                _log.AppendLine(outLine.Data);
                            }

                            if (!string.IsNullOrEmpty(oldFileName))
                            {
                                var existingInput = input.FirstOrDefault(p => p.FileName == oldFileName);
                                if (existingInput != null)
                                {
                                    existingInput.Text = results[oldFileName].Trim();

                                    var result = string.Empty;
                                    if (_textDetectionResults.Count > 0)
                                    {
                                        result = MakeResult(_textDetectionResults);
                                    }

                                    existingInput.Text = result;
                                    _textDetectionResults.Clear();

                                    lock (LockObject)
                                    {
                                        progressCallback?.Invoke(existingInput);
                                    }
                                }
                            }

                            currentProgress++;

                            var fileName = outLine.Data.Split(new[] { "ppocr INFO: **********" }, StringSplitOptions.None)[1].Trim('*', ' ');
                            oldFileName = fileName;

                            if (!results.ContainsKey(fileName))
                            {
                                results[fileName] = string.Empty;
                            }
                        }
                        else if (outLine.Data.Contains("ppocr WARNING: No text found in image"))
                        {
                            return;
                        }
                        else if (outLine.Data.Contains("ppocr INFO:"))
                        {
                            if (results.Count > 0)
                            {
                                var lastFile = results.Keys.Last();
                                SaveText(outLine);
                            }
                        }

                        if (abortCheck != null && abortCheck.Invoke())
                        {
                            try
                            {
                                process.CancelOutputRead();
                                process.Kill(); // Terminate PaddleOCR process
                                process.WaitForExit(1000);
                            }
                            catch (Exception ex)
                            {
                                Error = $"Error terminating PaddleOCR: {ex.Message}";
                                SeLogger.Error(ex, "Log: " + _log.ToString());
                            }
                        }
                    };

                    process.ErrorDataReceived += (sendingProcess, errorLine) =>
                    {
                        if (errorLine == null || string.IsNullOrWhiteSpace(errorLine.Data))
                        {
                            return;
                        }
                        Error = errorLine.Data;

                        hasErrors = true;
                        _log.AppendLine(errorLine.Data);
                    };

#pragma warning disable CA1416 // Validate platform compatibility
                    process.Start();
#pragma warning restore CA1416 // Validate platform compatibility;

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    if (hasErrors)
                    {
                        SeLogger.Error("PaddleOcrError: " + _log.ToString());
                    }

                    if (!string.IsNullOrEmpty(oldFileName))
                    {
                        var existingInput = input.FirstOrDefault(p => p.FileName == oldFileName);
                        if (existingInput != null)
                        {
                            existingInput.Text = results[oldFileName].Trim();

                            var result = string.Empty;
                            if (_textDetectionResults.Count > 0)
                            {
                                result = MakeResult(_textDetectionResults);
                            }

                            existingInput.Text = result;
                            _textDetectionResults.Clear();

                            lock (LockObject)
                            {
                                progressCallback?.Invoke(existingInput);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SeLogger.Error(ex, "Log: " + _log.ToString());
                throw;
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempFolder))
                    {
                        Directory.Delete(tempFolder, true);
                    }
                }
                catch (Exception ex)
                {
                    Error = $"An unexpected error occurred during cleanup: {ex.Message}";
                }
            }
        }

        public static Bitmap AddBorder(Bitmap originalBitmap, int borderWidth, Color borderColor)
        {
            int newWidth = originalBitmap.Width + 2 * borderWidth;
            int newHeight = originalBitmap.Height + 2 * borderWidth;

            var borderedBitmap = new Bitmap(newWidth, newHeight);

            using (Graphics graphics = Graphics.FromImage(borderedBitmap))
            {
                graphics.Clear(borderColor);
                graphics.DrawImage(originalBitmap, borderWidth, borderWidth);
            }

            return borderedBitmap;
        }

        private string MakeResult(List<PaddleOcrResultParser.TextDetectionResult> textDetectionResults)
        {
            var sb = new StringBuilder();
            var lines = MakeLines(textDetectionResults);
            foreach (var line in lines)
            {
                var text = string.Join(" ", line.Select(p => p.Text));
                sb.AppendLine(text);
            }

            return sb.ToString().Trim().Replace(" " + Environment.NewLine, Environment.NewLine);
        }

        private List<List<PaddleOcrResultParser.TextDetectionResult>> MakeLines(List<PaddleOcrResultParser.TextDetectionResult> input)
        {
            var result = new List<List<PaddleOcrResultParser.TextDetectionResult>>();
            var heightAverage = input.Average(p => p.BoundingBox.Height);
            var sorted = input.OrderBy(p => p.BoundingBox.Center.Y);
            var line = new List<PaddleOcrResultParser.TextDetectionResult>();
            PaddleOcrResultParser.TextDetectionResult last = null;
            foreach (var element in sorted)
            {
                if (last == null)
                {
                    line.Add(element);
                }
                else
                {
                    if (element.BoundingBox.Center.Y > last.BoundingBox.TopLeft.Y + heightAverage)
                    {

                        result.Add(line.OrderBy(p => p.BoundingBox.TopLeft.X).ToList());
                        line = new List<PaddleOcrResultParser.TextDetectionResult>
                        {
                            element
                        };
                    }
                    else
                    {
                        line.Add(element);
                    }
                }

                last = element;
            }

            if (line.Count > 0)
            {
                result.Add(line.OrderBy(p => p.BoundingBox.TopLeft.X).ToList());
            }

            return result;
        }

        public static List<OcrLanguage2> GetLanguages()
        {
            return new List<OcrLanguage2>
            {
                new OcrLanguage2("abq", "Abkhazian"),
                new OcrLanguage2("ady", "Adyghe"),
                new OcrLanguage2("af", "Afrikaans"),
                new OcrLanguage2("sq", "Albanian"),
                new OcrLanguage2("ang", "Angika"),
                new OcrLanguage2("ar", "Arabic"),
                new OcrLanguage2("ava", "Avar"),
                new OcrLanguage2("az", "Azerbaijani"),
                new OcrLanguage2("be", "Belarusian"),
                new OcrLanguage2("bho", "Bhojpuri"),
                new OcrLanguage2("bh", "Bihari"),
                new OcrLanguage2("bs", "Bosnian"),
                new OcrLanguage2("bg", "Bulgarian"),
                new OcrLanguage2("ch", "Chinese and English"),
                new OcrLanguage2("che", "Chechen"),
                new OcrLanguage2("chinese_cht", "Chinese traditional"),
                new OcrLanguage2("hr", "Croatian"),
                new OcrLanguage2("cs", "Czech"),
                new OcrLanguage2("da", "Danish"),
                new OcrLanguage2("dar", "Dargwa"),
                new OcrLanguage2("nl", "Dutch"),
                new OcrLanguage2("en", "English"),
                new OcrLanguage2("et", "Estonian"),
                new OcrLanguage2("fr", "French"),
                new OcrLanguage2("german", "German"),
                new OcrLanguage2("gom", "Goan Konkani"),
                new OcrLanguage2("bgc", "Haryanvi"),
                new OcrLanguage2("hi", "Hindi"),
                new OcrLanguage2("hu", "Hungarian"),
                new OcrLanguage2("is", "Icelandic"),
                new OcrLanguage2("id", "Indonesian"),
                new OcrLanguage2("inh", "Ingush"),
                new OcrLanguage2("ga", "Irish"),
                new OcrLanguage2("it", "Italian"),
                new OcrLanguage2("japan", "Japanese"),
                new OcrLanguage2("kbd", "Kabardian"),
                new OcrLanguage2("korean", "Korean"),
                new OcrLanguage2("ku", "Kurdish"),
                new OcrLanguage2("lbe", "Lak"),
                new OcrLanguage2("la", "Latin"),
                new OcrLanguage2("lv", "Latvian"),
                new OcrLanguage2("lez", "Lezghian"),
                new OcrLanguage2("lt", "Lithuanian"),
                new OcrLanguage2("mah", "Magahi"),
                new OcrLanguage2("mai", "Maithili"),
                new OcrLanguage2("ms", "Malay"),
                new OcrLanguage2("mt", "Maltese"),
                new OcrLanguage2("mi", "Maori"),
                new OcrLanguage2("mr", "Marathi"),
                new OcrLanguage2("mn", "Mongolian"),
                new OcrLanguage2("sck", "Nagpur"),
                new OcrLanguage2("ne", "Nepali"),
                new OcrLanguage2("new", "Newari"),
                new OcrLanguage2("no", "Norwegian"),
                new OcrLanguage2("oc", "Occitan"),
                new OcrLanguage2("pi", "Pali"),
                new OcrLanguage2("fa", "Persian"),
                new OcrLanguage2("pl", "Polish"),
                new OcrLanguage2("pt", "Portuguese"),
                new OcrLanguage2("ro", "Romanian"),
                new OcrLanguage2("ru", "Russian"),
                new OcrLanguage2("sa", "Sanskrit"),
                new OcrLanguage2("rs_cyrillic", "Serbian(cyrillic)"),
                new OcrLanguage2("rs_latin", "Serbian(latin)"),
                new OcrLanguage2("sk", "Slovak"),
                new OcrLanguage2("sl", "Slovenian"),
                new OcrLanguage2("es", "Spanish"),
                new OcrLanguage2("sw", "Swahili"),
                new OcrLanguage2("sv", "Swedish"),
                new OcrLanguage2("tab", "Tabassaran"),
                new OcrLanguage2("tl", "Tagalog"),
                new OcrLanguage2("ta", "Tamil"),
                new OcrLanguage2("te", "Telugu"),
                new OcrLanguage2("tr", "Turkish"),
                new OcrLanguage2("uk", "Ukrainian"),
                new OcrLanguage2("ur", "Urdu"),
                new OcrLanguage2("ug", "Uyghur"),
                new OcrLanguage2("uz", "Uzbek"),
                new OcrLanguage2("vi", "Vietnamese"),
                new OcrLanguage2("cy", "Welsh"),
            }.OrderBy(p => p.Name).ToList();
        }
    }
}
