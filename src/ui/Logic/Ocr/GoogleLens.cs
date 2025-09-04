using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Nikse.SubtitleEdit.Forms.Ocr.VobSubOcr;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class GoogleLens
    {
        public string Error { get; set; }
        private bool hasErrors = false;
        private StringBuilder _log = new StringBuilder();

        public GoogleLens()
        {
            Error = string.Empty;
        }

        private object LockObject = new object();

        public void OcrBatch(List<OcrInput> input, string language, Action<OcrInput> progressCallback, Func<bool> abortCheck)
        {
            _log.Clear();
            hasErrors = false;

            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            try
            {
                var imagePaths = new List<string>();
                var width = input.Count.ToString().Length; // Determine the number of digits for file name padding

                for (var i = 0; i < input.Count; i++)
                {
                    var imagePath = Path.Combine(tempFolder, string.Format("{0:D" + width + "}.png", i));
                    input[i].Bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
                    input[i].FileName = imagePath;
                    imagePaths.Add(imagePath);
                }

                var parameters = $"\"{tempFolder}\" {language}";

                string GoogleLensPath = "GoogleLens";
                if (File.Exists(Path.Combine(Configuration.GoogleLensDirectory, "Chrome-Lens-CLI.exe")))
                {
                    GoogleLensPath = Path.GetFullPath(Path.Combine(Configuration.GoogleLensDirectory, "Chrome-Lens-CLI.exe"));
                }

                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = GoogleLensPath,
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
                    string currentFileName = null;
                    bool canCollectText = false;

                    if (input.Count == 1)
                    {
                        currentFileName = Path.GetFileName(input[0].FileName);
                        results[currentFileName] = string.Empty;
                    }

                    process.OutputDataReceived += (sendingProcess, outLine) =>
                    {
                        if (string.IsNullOrWhiteSpace(outLine.Data))
                        {
                            return;
                        }

                        _log.AppendLine(outLine.Data);

                        var fileMarkerMatch = Regex.Match(outLine.Data, @"- \(\d+/\d+\) Result for: (.*) -");
                        if (fileMarkerMatch.Success)
                        {
                            if (currentFileName != null)
                            {
                                var existingInput = input.FirstOrDefault(p => Path.GetFileName(p.FileName) == currentFileName);
                                if (existingInput != null && results.ContainsKey(currentFileName))
                                {
                                    existingInput.Text = results[currentFileName].Trim();
                                    lock (LockObject)
                                    {
                                        progressCallback?.Invoke(existingInput);
                                    }
                                }
                            }

                            currentFileName = fileMarkerMatch.Groups[1].Value.Trim();
                            if (!results.ContainsKey(currentFileName))
                            {
                                results[currentFileName] = string.Empty;
                            }
                            canCollectText = false;
                            return;
                        }

                        if (outLine.Data.StartsWith("OCR Results:"))
                        {
                            canCollectText = true;
                            return;
                        }

                        if (canCollectText && currentFileName != null)
                        {
                            if (!outLine.Data.Equals("No OCR text found.", StringComparison.OrdinalIgnoreCase))
                            {
                                results[currentFileName] += outLine.Data + Environment.NewLine;
                            }
                        }

                        if (abortCheck != null && abortCheck.Invoke())
                        {
                            try
                            {
                                process.CancelOutputRead();
                                process.Kill(); // Terminate GoogleLens process
                                process.WaitForExit(1000);
                            }
                            catch (Exception ex)
                            {
                                Error = $"Error terminating GoogleLens: {ex.Message}";
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
                        SeLogger.Error("GoogleLensError: " + _log.ToString());
                    }

                    if (currentFileName != null)
                    {
                        var existingInput = input.FirstOrDefault(p => Path.GetFileName(p.FileName) == currentFileName);
                        if (existingInput != null && results.ContainsKey(currentFileName))
                        {
                            existingInput.Text = results[currentFileName].Trim();
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

        public static List<OcrLanguage3> GetLanguages()
        {
            return new List<OcrLanguage3>
            {
                new OcrLanguage3("ab", "Abkhaz"),
                new OcrLanguage3("ace", "Acehnese"),
                new OcrLanguage3("ach", "Acholi"),
                new OcrLanguage3("af", "Afrikaans"),
                new OcrLanguage3("ak", "Twi (Akan)"),
                new OcrLanguage3("sq", "Albanian"),
                new OcrLanguage3("alz", "Alur"),
                new OcrLanguage3("am", "Amharic"),
                new OcrLanguage3("ar", "Arabic"),
                new OcrLanguage3("hy", "Armenian"),
                new OcrLanguage3("as", "Assamese"),
                new OcrLanguage3("awa", "Awadhi"),
                new OcrLanguage3("ay", "Aymara"),
                new OcrLanguage3("az", "Azerbaijani"),
                new OcrLanguage3("ban", "Balinese"),
                new OcrLanguage3("bm", "Bambara"),
                new OcrLanguage3("ba", "Bashkir"),
                new OcrLanguage3("eu", "Basque"),
                new OcrLanguage3("btx", "Batak Karo"),
                new OcrLanguage3("bts", "Batak Simalungun"),
                new OcrLanguage3("bbc", "Batak Toba"),
                new OcrLanguage3("be", "Belarusian"),
                new OcrLanguage3("bem", "Bemba"),
                new OcrLanguage3("bn", "Bengali"),
                new OcrLanguage3("bew", "Betawi"),
                new OcrLanguage3("bho", "Bhojpuri"),
                new OcrLanguage3("bik", "Bikol"),
                new OcrLanguage3("bs", "Bosnian"),
                new OcrLanguage3("br", "Breton"),
                new OcrLanguage3("bg", "Bulgarian"),
                new OcrLanguage3("bua", "Buryat"),
                new OcrLanguage3("yue", "Cantonese"),
                new OcrLanguage3("ca", "Catalan"),
                new OcrLanguage3("ceb", "Cebuano"),
                new OcrLanguage3("ny", "Chichewa (Nyanja)"),
                new OcrLanguage3("zh-CN", "Chinese (Simplified)"),
                new OcrLanguage3("zh-TW", "Chinese (Traditional)"),
                new OcrLanguage3("cv", "Chuvash"),
                new OcrLanguage3("co", "Corsican"),
                new OcrLanguage3("crh", "Crimean Tatar"),
                new OcrLanguage3("hr", "Croatian"),
                new OcrLanguage3("cs", "Czech"),
                new OcrLanguage3("da", "Danish"),
                new OcrLanguage3("din", "Dinka"),
                new OcrLanguage3("dv", "Divehi"),
                new OcrLanguage3("doi", "Dogri"),
                new OcrLanguage3("dov", "Dombe"),
                new OcrLanguage3("nl", "Dutch"),
                new OcrLanguage3("dz", "Dzongkha"),
                new OcrLanguage3("en", "English"),
                new OcrLanguage3("eo", "Esperanto"),
                new OcrLanguage3("et", "Estonian"),
                new OcrLanguage3("ee", "Ewe"),
                new OcrLanguage3("fj", "Fijian"),
                new OcrLanguage3("fil", "Filipino (Tagalog)"),
                new OcrLanguage3("fi", "Finnish"),
                new OcrLanguage3("fr", "French"),
                new OcrLanguage3("fr-CA", "French (Canadian)"),
                new OcrLanguage3("fr-FR", "French (French)"),
                new OcrLanguage3("fy", "Frisian"),
                new OcrLanguage3("ff", "Fulfulde"),
                new OcrLanguage3("gaa", "Ga"),
                new OcrLanguage3("gl", "Galician"),
                new OcrLanguage3("lg", "Ganda (Luganda)"),
                new OcrLanguage3("ka", "Georgian"),
                new OcrLanguage3("de", "German"),
                new OcrLanguage3("el", "Greek"),
                new OcrLanguage3("gn", "Guarani"),
                new OcrLanguage3("gu", "Gujarati"),
                new OcrLanguage3("ht", "Haitian Creole"),
                new OcrLanguage3("cnh", "Hakha Chin"),
                new OcrLanguage3("ha", "Hausa"),
                new OcrLanguage3("haw", "Hawaiian"),
                new OcrLanguage3("iw", "Hebrew"),
                new OcrLanguage3("hil", "Hiligaynon"),
                new OcrLanguage3("hi", "Hindi"),
                new OcrLanguage3("hmn", "Hmong"),
                new OcrLanguage3("hu", "Hungarian"),
                new OcrLanguage3("hrx", "Hunsrik"),
                new OcrLanguage3("is", "Icelandic"),
                new OcrLanguage3("ig", "Igbo"),
                new OcrLanguage3("ilo", "Iloko"),
                new OcrLanguage3("id", "Indonesian"),
                new OcrLanguage3("ga", "Irish"),
                new OcrLanguage3("it", "Italian"),
                new OcrLanguage3("ja", "Japanese"),
                new OcrLanguage3("jw", "Javanese"),
                new OcrLanguage3("kn", "Kannada"),
                new OcrLanguage3("pam", "Kapampangan"),
                new OcrLanguage3("kk", "Kazakh"),
                new OcrLanguage3("km", "Khmer"),
                new OcrLanguage3("cgg", "Kiga"),
                new OcrLanguage3("rw", "Kinyarwanda"),
                new OcrLanguage3("ktu", "Kituba"),
                new OcrLanguage3("gom", "Konkani"),
                new OcrLanguage3("ko", "Korean"),
                new OcrLanguage3("kri", "Krio"),
                new OcrLanguage3("ku", "Kurdish (Kurmanji)"),
                new OcrLanguage3("ckb", "Kurdish (Sorani)"),
                new OcrLanguage3("ky", "Kyrgyz"),
                new OcrLanguage3("lo", "Lao"),
                new OcrLanguage3("ltg", "Latgalian"),
                new OcrLanguage3("la", "Latin"),
                new OcrLanguage3("lv", "Latvian"),
                new OcrLanguage3("lij", "Ligurian"),
                new OcrLanguage3("li", "Limburgan"),
                new OcrLanguage3("ln", "Lingala"),
                new OcrLanguage3("lt", "Lithuanian"),
                new OcrLanguage3("lmo", "Lombard"),
                new OcrLanguage3("luo", "Luo"),
                new OcrLanguage3("lb", "Luxembourgish"),
                new OcrLanguage3("mk", "Macedonian"),
                new OcrLanguage3("mai", "Maithili"),
                new OcrLanguage3("mak", "Makassar"),
                new OcrLanguage3("mg", "Malagasy"),
                new OcrLanguage3("ms", "Malay"),
                new OcrLanguage3("ms-Arab", "Malay (Jawi)"),
                new OcrLanguage3("ml", "Malayalam"),
                new OcrLanguage3("mt", "Maltese"),
                new OcrLanguage3("mi", "Maori"),
                new OcrLanguage3("mr", "Marathi"),
                new OcrLanguage3("chm", "Meadow Mari"),
                new OcrLanguage3("mni-Mtei", "Meiteilon (Manipuri)"),
                new OcrLanguage3("min", "Minang"),
                new OcrLanguage3("lus", "Mizo"),
                new OcrLanguage3("mn", "Mongolian"),
                new OcrLanguage3("my", "Myanmar (Burmese)"),
                new OcrLanguage3("nr", "Ndebele (South)"),
                new OcrLanguage3("new", "Nepalbhasa (Newari)"),
                new OcrLanguage3("ne", "Nepali"),
                new OcrLanguage3("nso", "Northern Sotho (Sepedi)"),
                new OcrLanguage3("no", "Norwegian"),
                new OcrLanguage3("nus", "Nuer"),
                new OcrLanguage3("oc", "Occitan"),
                new OcrLanguage3("or", "Odia (Oriya)"),
                new OcrLanguage3("om", "Oromo"),
                new OcrLanguage3("pag", "Pangasinan"),
                new OcrLanguage3("pap", "Papiamento"),
                new OcrLanguage3("ps", "Pashto"),
                new OcrLanguage3("fa", "Persian"),
                new OcrLanguage3("pl", "Polish"),
                new OcrLanguage3("pt", "Portuguese"),
                new OcrLanguage3("pt-BR", "Portuguese (Brazil)"),
                new OcrLanguage3("pt-PT", "Portuguese (Portugal)"),
                new OcrLanguage3("pa", "Punjabi"),
                new OcrLanguage3("pa-Arab", "Punjabi (Shahmukhi)"),
                new OcrLanguage3("qu", "Quechua"),
                new OcrLanguage3("rom", "Romani"),
                new OcrLanguage3("ro", "Romanian"),
                new OcrLanguage3("rn", "Rundi"),
                new OcrLanguage3("ru", "Russian"),
                new OcrLanguage3("sm", "Samoan"),
                new OcrLanguage3("sg", "Sango"),
                new OcrLanguage3("sa", "Sanskrit"),
                new OcrLanguage3("gd", "Scots Gaelic"),
                new OcrLanguage3("sr", "Serbian"),
                new OcrLanguage3("st", "Sesotho"),
                new OcrLanguage3("crs", "Seychellois Creole"),
                new OcrLanguage3("shn", "Shan"),
                new OcrLanguage3("sn", "Shona"),
                new OcrLanguage3("scn", "Sicilian"),
                new OcrLanguage3("szl", "Silesian"),
                new OcrLanguage3("sd", "Sindhi"),
                new OcrLanguage3("si", "Sinhala (Sinhalese)"),
                new OcrLanguage3("sk", "Slovak"),
                new OcrLanguage3("sl", "Slovenian"),
                new OcrLanguage3("so", "Somali"),
                new OcrLanguage3("es", "Spanish"),
                new OcrLanguage3("su", "Sundanese"),
                new OcrLanguage3("sw", "Swahili"),
                new OcrLanguage3("ss", "Swati"),
                new OcrLanguage3("sv", "Swedish"),
                new OcrLanguage3("tg", "Tajik"),
                new OcrLanguage3("ta", "Tamil"),
                new OcrLanguage3("tt", "Tatar"),
                new OcrLanguage3("te", "Telugu"),
                new OcrLanguage3("tet", "Tetum"),
                new OcrLanguage3("th", "Thai"),
                new OcrLanguage3("ti", "Tigrinya"),
                new OcrLanguage3("ts", "Tsonga"),
                new OcrLanguage3("tn", "Tswana"),
                new OcrLanguage3("tr", "Turkish"),
                new OcrLanguage3("tk", "Turkmen"),
                new OcrLanguage3("uk", "Ukrainian"),
                new OcrLanguage3("ur", "Urdu"),
                new OcrLanguage3("ug", "Uyghur"),
                new OcrLanguage3("uz", "Uzbek"),
                new OcrLanguage3("vi", "Vietnamese"),
                new OcrLanguage3("cy", "Welsh"),
                new OcrLanguage3("xh", "Xhosa"),
                new OcrLanguage3("yi", "Yiddish"),
                new OcrLanguage3("yo", "Yoruba"),
                new OcrLanguage3("yua", "Yucatec Maya"),
                new OcrLanguage3("zu", "Zulu"),
            }.OrderBy(p => p.Name).ToList();
        }
    }
}
