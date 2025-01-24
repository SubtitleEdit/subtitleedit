using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class PaddleOcr
    {
        public string Error { get; set; }
        private List<PaddleOcrResultParser.TextDetectionResult> _textDetectionResults = new List<PaddleOcrResultParser.TextDetectionResult>();
        private string _paddingOcrPath;
        private string _clsPath;
        private string _detPath;
        private string _recPath;

        private readonly List<string> LatinLanguageCodes = new List<string>()
        {
            "af",
            "az",
            "bs",
            "cs",
            "cy",
            "da",
            "de",
            "es",
            "et",
            "fr",
            "ga",
            "hr",
            "hu",
            "id",
            "is",
            "it",
            "ku",
            "la",
            "lt",
            "lv",
            "mi",
            "ms",
            "mt",
            "nl",
            "no",
            "oc",
            "pi",
            "pl",
            "pt",
            "ro",
            "rs_latin",
            "sk",
            "sl",
            "sq",
            "sv",
            "sw",
            "tl",
            "tr",
            "uz",
            "vi",
            "french",
            "german"
        };
        private readonly List<string> ArabicLanguageCodes = new List<string>() { "ar", "fa", "ug", "ur" };
        private readonly List<string> CyrillicLanguageCodes = new List<string>()
        {
            "ru",
            "rs_cyrillic",
            "be",
            "bg",
            "uk",
            "mn",
            "abq",
            "ady",
            "kbd",
            "ava",
            "dar",
            "inh",
            "che",
            "lbe",
            "lez",
            "tab"
        };

        private readonly List<string> DevanagariLanguageCodes = new List<string>()
        {
            "hi",
            "mr",
            "ne",
            "bh",
            "mai",
            "ang",
            "bho",
            "mah",
            "sck",
            "new",
            "gom",
            "bgc",
            "sa"
        };

        public PaddleOcr()
        {
            Error = string.Empty;
            _paddingOcrPath = Configuration.PaddleOcrDirectory;
            _clsPath = Path.Combine(_paddingOcrPath, "cls");
            _detPath = Path.Combine(_paddingOcrPath, "det");
            _recPath = Path.Combine(_paddingOcrPath, "rec");
        }

        public string Ocr(Bitmap bitmap, string language, bool useGpu)
        {
            var detFilePrefix = language;
            if (language != "en" && language != "ch")
            {
                detFilePrefix = $"ml{Path.DirectorySeparatorChar}Multilingual_PP-OCRv3_det_infer";
            }
            else if (language == "ch")
            {
                detFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv4_det_infer";
            }
            else
            {
                detFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv3_det_infer";
            }

            var recFilePrefix = language;
            if (LatinLanguageCodes.Contains(language))
            {
                recFilePrefix = $"latin{Path.DirectorySeparatorChar}latin_PP-OCRv3_rec_infer";
            }
            else if (ArabicLanguageCodes.Contains(language))
            {
                recFilePrefix = $"arabic{Path.DirectorySeparatorChar}arabic_PP-OCRv4_rec_infer";
            }
            else if (CyrillicLanguageCodes.Contains(language))
            {
                recFilePrefix = $"cyrillic{Path.DirectorySeparatorChar}cyrillic_PP-OCRv3_rec_infer";
            }
            else if (DevanagariLanguageCodes.Contains(language))
            {
                recFilePrefix = $"devanagari{Path.DirectorySeparatorChar}devanagari_PP-OCRv4_rec_infer";
            }
            else if (language == "chinese_cht")
            {
                recFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv3_rec_infer";
            }
            else
            {
                recFilePrefix = $"{language}{Path.DirectorySeparatorChar}{language}_PP-OCRv4_rec_infer";
            }

            var borderedBitmapTemp = AddBorder(bitmap, 10, Color.Black);
            var borderedBitmap = AddBorder(borderedBitmapTemp, 10, Color.Transparent);
            borderedBitmapTemp.Dispose();
            var tempImage = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
            borderedBitmap.Save(tempImage, System.Drawing.Imaging.ImageFormat.Png);
            var parameters = $"--image_dir \"{tempImage}\" --ocr_version PP-OCRv4 --use_angle_cls true --use_gpu {useGpu.ToString().ToLowerInvariant()} --lang {language} --show_log false --det_model_dir \"{_detPath}\\{detFilePrefix}\" --rec_model_dir \"{_recPath}\\{recFilePrefix}\" --cls_model_dir \"{_clsPath}\\ch_ppocr_mobile_v2.0_cls_infer\"";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "paddleocr",
                    Arguments = parameters,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
            };

            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            process.StartInfo.EnvironmentVariables["PYTHONUTF8"] = "1";
            process.OutputDataReceived += OutputHandler;
            _textDetectionResults.Clear();

#pragma warning disable CA1416 // Validate platform compatibility
            process.Start();
#pragma warning restore CA1416 // Validate platform compatibility;

            process.BeginOutputReadLine();

            process.WaitForExit();

            borderedBitmap.Dispose();

            if (process.ExitCode != 0)
            {
                Error = process.StandardError.ReadToEnd();
                return string.Empty;
            }

            File.Delete(tempImage);

            if (_textDetectionResults.Count == 0)
            {
                return string.Empty;
            }

            var result = MakeResult(_textDetectionResults);
            return result;
        }

        public static Bitmap AddBorder(Bitmap originalBitmap, int borderWidth, Color borderColor)
        {
            // Calculate new dimensions
            int newWidth = originalBitmap.Width + 2 * borderWidth;
            int newHeight = originalBitmap.Height + 2 * borderWidth;

            // Create a new bitmap with the new dimensions
            var borderedBitmap = new Bitmap(newWidth, newHeight);

            // Create a canvas to draw on the new bitmap
            using (Graphics graphics = Graphics.FromImage(borderedBitmap))
            {
                // Fill the entire bitmap with the border color
                graphics.Clear(borderColor);

                // Draw the original bitmap onto the new canvas, offset by the border width
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

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrWhiteSpace(outLine.Data))
            {
                return;
            }

            if (!outLine.Data.Contains("ppocr INFO:"))
            {
                return;
            }

            var arr = outLine.Data.Split(new[] { "ppocr INFO: " }, StringSplitOptions.None);
            if (arr.Length != 2)
            {
                return;
            }

            var data = arr[1];

            string pattern = @"\[\[\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+],\s*\[\d+\.\d+,\s*\d+\.\d+]],\s*\(['""].*['""],\s*\d+\.\d+\)\]";
            var match = Regex.Match(data, pattern);
            if (match.Success)
            {
                var parser = new PaddleOcrResultParser();
                var x = parser.Parse(data);
                _textDetectionResults.Add(x);
            }

            // Example: [[[92.0, 56.0], [735.0, 60.0], [734.0, 118.0], [91.0, 113.0]], ('My mommy always said', 0.9907816052436829)]
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
                new OcrLanguage2("japan", "Japanese"),
                new OcrLanguage2("kbd", "Kabardian"),
                new OcrLanguage2("korean", "Korean"),
                new OcrLanguage2("ku", "Kurdish"),
                new OcrLanguage2("lbe", "Lak"),
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
                new OcrLanguage2("uk", "Ukranian"),
                new OcrLanguage2("ur", "Urdu"),
                new OcrLanguage2("ug", "Uyghur"),
                new OcrLanguage2("uz", "Uzbek"),
                new OcrLanguage2("vi", "Vietnamese"),
                new OcrLanguage2("cy", "Welsh"),
            }.OrderBy(p => p.Name).ToList();
        }
    }
}
