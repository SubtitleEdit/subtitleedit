using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// OCR via https://github.com/timminator/chrome-lens-py
/// </summary>
public class GoogleLensOcr
{
    public string Error { get; set; }
    private bool _hasErrors = false;
    private StringBuilder _log = new StringBuilder();
    public const string ExeFileName = "chrome-lens-cli.exe";
    private IProgress<PaddleOcrBatchProgress>? _batchProgress;
    
    /// Contains the file names of bitmaps that have OCR errors.
    private HashSet<string> _bitmapsWithErrors = [];

    public GoogleLensOcr()
    {
        Error = string.Empty;
    }

    private Lock _lockObject = new Lock();

    public void OcrBatch(List<PaddleOcrBatchInput> input, string language, IProgress<PaddleOcrBatchProgress> progress, CancellationToken cancellationToken)
    {
        _log.Clear();
        _hasErrors = false;
        _batchProgress = progress;
        var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempFolder);

        try
        {
            var imagePaths = new List<string>();
            var width = input.Count.ToString().Length; // Determine the number of digits for file name padding

            for (var i = 0; i < input.Count; i++)
            {
                var bmp = input[i].Bitmap;
                if (bmp == null)
                {
                    continue;
                }

                var imagePath = Path.Combine(tempFolder, string.Format("{0:D" + width + "}.png", i));
                File.WriteAllBytes(imagePath, bmp.ToPngArray());
                input[i].FileName = imagePath;
                imagePaths.Add(imagePath);
            }

            var parameters = $"\"{tempFolder}\" {language}";

            string googleLensPath = ExeFileName;
            if (File.Exists(Path.Combine(Se.GoogleLensOcrFolder, ExeFileName)))
            {
                googleLensPath = Path.GetFullPath(Path.Combine(Se.GoogleLensOcrFolder, ExeFileName));
            }

            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = googleLensPath,
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
                _bitmapsWithErrors = [];
                string? currentFileName = null;
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
                                lock (_lockObject)
                                {
                                    var progressReport = new PaddleOcrBatchProgress
                                    {
                                        Index = existingInput.Index,
                                        Text = existingInput.Text,
                                        Item = existingInput.Item,
                                    };
                                    _batchProgress?.Report(progressReport);
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
                        AddLineToResult(results, currentFileName, outLine.Data);
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            process.CancelOutputRead();
                            process.Kill(true); // Terminate GoogleLens process
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

                    _hasErrors = true;
                    _log.AppendLine(errorLine.Data);
                };

#pragma warning disable CA1416 // Validate platform compatibility
                process.Start();
#pragma warning restore CA1416 // Validate platform compatibility;

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                if (_hasErrors)
                {
                    SeLogger.Error("GoogleLensError: " + _log.ToString());
                }

                if (currentFileName != null)
                {
                    var existingInput = input.FirstOrDefault(p => Path.GetFileName(p.FileName) == currentFileName);
                    if (existingInput != null && results.ContainsKey(currentFileName))
                    {
                        existingInput.Text = results[currentFileName].Trim();
                        lock (_lockObject)
                        {
                            var progressReport = new PaddleOcrBatchProgress
                            {
                                Index = existingInput.Index,
                                Text = existingInput.Text,
                                Item = existingInput.Item,
                            };
                            _batchProgress?.Report(progressReport);
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
    
    private void AddLineToResult(in Dictionary<string, string> results, string fileName, string line)
    {
        if (_bitmapsWithErrors.Contains(fileName))
        {
            return;
        }

        if (line.Equals("No OCR text found.", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (line.Contains("Request error (possibly proxy-related)", StringComparison.OrdinalIgnoreCase))
        {
            _bitmapsWithErrors.Add(fileName);
            return;
        }

        results[fileName] += line + Environment.NewLine;
    }

    public static List<OcrLanguage2> GetLanguages()
    {
        return new List<OcrLanguage2>
            {
                new OcrLanguage2("ab", "Abkhaz"),
                new OcrLanguage2("ace", "Acehnese"),
                new OcrLanguage2("ach", "Acholi"),
                new OcrLanguage2("af", "Afrikaans"),
                new OcrLanguage2("ak", "Twi (Akan)"),
                new OcrLanguage2("sq", "Albanian"),
                new OcrLanguage2("alz", "Alur"),
                new OcrLanguage2("am", "Amharic"),
                new OcrLanguage2("ar", "Arabic"),
                new OcrLanguage2("hy", "Armenian"),
                new OcrLanguage2("as", "Assamese"),
                new OcrLanguage2("awa", "Awadhi"),
                new OcrLanguage2("ay", "Aymara"),
                new OcrLanguage2("az", "Azerbaijani"),
                new OcrLanguage2("ban", "Balinese"),
                new OcrLanguage2("bm", "Bambara"),
                new OcrLanguage2("ba", "Bashkir"),
                new OcrLanguage2("eu", "Basque"),
                new OcrLanguage2("btx", "Batak Karo"),
                new OcrLanguage2("bts", "Batak Simalungun"),
                new OcrLanguage2("bbc", "Batak Toba"),
                new OcrLanguage2("be", "Belarusian"),
                new OcrLanguage2("bem", "Bemba"),
                new OcrLanguage2("bn", "Bengali"),
                new OcrLanguage2("bew", "Betawi"),
                new OcrLanguage2("bho", "Bhojpuri"),
                new OcrLanguage2("bik", "Bikol"),
                new OcrLanguage2("bs", "Bosnian"),
                new OcrLanguage2("br", "Breton"),
                new OcrLanguage2("bg", "Bulgarian"),
                new OcrLanguage2("bua", "Buryat"),
                new OcrLanguage2("yue", "Cantonese"),
                new OcrLanguage2("ca", "Catalan"),
                new OcrLanguage2("ceb", "Cebuano"),
                new OcrLanguage2("ny", "Chichewa (Nyanja)"),
                new OcrLanguage2("zh-CN", "Chinese (Simplified)"),
                new OcrLanguage2("zh-TW", "Chinese (Traditional)"),
                new OcrLanguage2("cv", "Chuvash"),
                new OcrLanguage2("co", "Corsican"),
                new OcrLanguage2("crh", "Crimean Tatar"),
                new OcrLanguage2("hr", "Croatian"),
                new OcrLanguage2("cs", "Czech"),
                new OcrLanguage2("da", "Danish"),
                new OcrLanguage2("din", "Dinka"),
                new OcrLanguage2("dv", "Divehi"),
                new OcrLanguage2("doi", "Dogri"),
                new OcrLanguage2("dov", "Dombe"),
                new OcrLanguage2("nl", "Dutch"),
                new OcrLanguage2("dz", "Dzongkha"),
                new OcrLanguage2("en", "English"),
                new OcrLanguage2("eo", "Esperanto"),
                new OcrLanguage2("et", "Estonian"),
                new OcrLanguage2("ee", "Ewe"),
                new OcrLanguage2("fj", "Fijian"),
                new OcrLanguage2("fil", "Filipino (Tagalog)"),
                new OcrLanguage2("fi", "Finnish"),
                new OcrLanguage2("fr", "French"),
                new OcrLanguage2("fr-CA", "French (Canadian)"),
                new OcrLanguage2("fr-FR", "French (French)"),
                new OcrLanguage2("fy", "Frisian"),
                new OcrLanguage2("ff", "Fulfulde"),
                new OcrLanguage2("gaa", "Ga"),
                new OcrLanguage2("gl", "Galician"),
                new OcrLanguage2("lg", "Ganda (Luganda)"),
                new OcrLanguage2("ka", "Georgian"),
                new OcrLanguage2("de", "German"),
                new OcrLanguage2("el", "Greek"),
                new OcrLanguage2("gn", "Guarani"),
                new OcrLanguage2("gu", "Gujarati"),
                new OcrLanguage2("ht", "Haitian Creole"),
                new OcrLanguage2("cnh", "Hakha Chin"),
                new OcrLanguage2("ha", "Hausa"),
                new OcrLanguage2("haw", "Hawaiian"),
                new OcrLanguage2("iw", "Hebrew"),
                new OcrLanguage2("hil", "Hiligaynon"),
                new OcrLanguage2("hi", "Hindi"),
                new OcrLanguage2("hmn", "Hmong"),
                new OcrLanguage2("hu", "Hungarian"),
                new OcrLanguage2("hrx", "Hunsrik"),
                new OcrLanguage2("is", "Icelandic"),
                new OcrLanguage2("ig", "Igbo"),
                new OcrLanguage2("ilo", "Iloko"),
                new OcrLanguage2("id", "Indonesian"),
                new OcrLanguage2("ga", "Irish"),
                new OcrLanguage2("it", "Italian"),
                new OcrLanguage2("ja", "Japanese"),
                new OcrLanguage2("jw", "Javanese"),
                new OcrLanguage2("kn", "Kannada"),
                new OcrLanguage2("pam", "Kapampangan"),
                new OcrLanguage2("kk", "Kazakh"),
                new OcrLanguage2("km", "Khmer"),
                new OcrLanguage2("cgg", "Kiga"),
                new OcrLanguage2("rw", "Kinyarwanda"),
                new OcrLanguage2("ktu", "Kituba"),
                new OcrLanguage2("gom", "Konkani"),
                new OcrLanguage2("ko", "Korean"),
                new OcrLanguage2("kri", "Krio"),
                new OcrLanguage2("ku", "Kurdish (Kurmanji)"),
                new OcrLanguage2("ckb", "Kurdish (Sorani)"),
                new OcrLanguage2("ky", "Kyrgyz"),
                new OcrLanguage2("lo", "Lao"),
                new OcrLanguage2("ltg", "Latgalian"),
                new OcrLanguage2("la", "Latin"),
                new OcrLanguage2("lv", "Latvian"),
                new OcrLanguage2("lij", "Ligurian"),
                new OcrLanguage2("li", "Limburgan"),
                new OcrLanguage2("ln", "Lingala"),
                new OcrLanguage2("lt", "Lithuanian"),
                new OcrLanguage2("lmo", "Lombard"),
                new OcrLanguage2("luo", "Luo"),
                new OcrLanguage2("lb", "Luxembourgish"),
                new OcrLanguage2("mk", "Macedonian"),
                new OcrLanguage2("mai", "Maithili"),
                new OcrLanguage2("mak", "Makassar"),
                new OcrLanguage2("mg", "Malagasy"),
                new OcrLanguage2("ms", "Malay"),
                new OcrLanguage2("ms-Arab", "Malay (Jawi)"),
                new OcrLanguage2("ml", "Malayalam"),
                new OcrLanguage2("mt", "Maltese"),
                new OcrLanguage2("mi", "Maori"),
                new OcrLanguage2("mr", "Marathi"),
                new OcrLanguage2("chm", "Meadow Mari"),
                new OcrLanguage2("mni-Mtei", "Meiteilon (Manipuri)"),
                new OcrLanguage2("min", "Minang"),
                new OcrLanguage2("lus", "Mizo"),
                new OcrLanguage2("mn", "Mongolian"),
                new OcrLanguage2("my", "Myanmar (Burmese)"),
                new OcrLanguage2("nr", "Ndebele (South)"),
                new OcrLanguage2("new", "Nepalbhasa (Newari)"),
                new OcrLanguage2("ne", "Nepali"),
                new OcrLanguage2("nso", "Northern Sotho (Sepedi)"),
                new OcrLanguage2("no", "Norwegian"),
                new OcrLanguage2("nus", "Nuer"),
                new OcrLanguage2("oc", "Occitan"),
                new OcrLanguage2("or", "Odia (Oriya)"),
                new OcrLanguage2("om", "Oromo"),
                new OcrLanguage2("pag", "Pangasinan"),
                new OcrLanguage2("pap", "Papiamento"),
                new OcrLanguage2("ps", "Pashto"),
                new OcrLanguage2("fa", "Persian"),
                new OcrLanguage2("pl", "Polish"),
                new OcrLanguage2("pt", "Portuguese"),
                new OcrLanguage2("pt-BR", "Portuguese (Brazil)"),
                new OcrLanguage2("pt-PT", "Portuguese (Portugal)"),
                new OcrLanguage2("pa", "Punjabi"),
                new OcrLanguage2("pa-Arab", "Punjabi (Shahmukhi)"),
                new OcrLanguage2("qu", "Quechua"),
                new OcrLanguage2("rom", "Romani"),
                new OcrLanguage2("ro", "Romanian"),
                new OcrLanguage2("rn", "Rundi"),
                new OcrLanguage2("ru", "Russian"),
                new OcrLanguage2("sm", "Samoan"),
                new OcrLanguage2("sg", "Sango"),
                new OcrLanguage2("sa", "Sanskrit"),
                new OcrLanguage2("gd", "Scots Gaelic"),
                new OcrLanguage2("sr", "Serbian"),
                new OcrLanguage2("st", "Sesotho"),
                new OcrLanguage2("crs", "Seychellois Creole"),
                new OcrLanguage2("shn", "Shan"),
                new OcrLanguage2("sn", "Shona"),
                new OcrLanguage2("scn", "Sicilian"),
                new OcrLanguage2("szl", "Silesian"),
                new OcrLanguage2("sd", "Sindhi"),
                new OcrLanguage2("si", "Sinhala (Sinhalese)"),
                new OcrLanguage2("sk", "Slovak"),
                new OcrLanguage2("sl", "Slovenian"),
                new OcrLanguage2("so", "Somali"),
                new OcrLanguage2("es", "Spanish"),
                new OcrLanguage2("su", "Sundanese"),
                new OcrLanguage2("sw", "Swahili"),
                new OcrLanguage2("ss", "Swati"),
                new OcrLanguage2("sv", "Swedish"),
                new OcrLanguage2("tg", "Tajik"),
                new OcrLanguage2("ta", "Tamil"),
                new OcrLanguage2("tt", "Tatar"),
                new OcrLanguage2("te", "Telugu"),
                new OcrLanguage2("tet", "Tetum"),
                new OcrLanguage2("th", "Thai"),
                new OcrLanguage2("ti", "Tigrinya"),
                new OcrLanguage2("ts", "Tsonga"),
                new OcrLanguage2("tn", "Tswana"),
                new OcrLanguage2("tr", "Turkish"),
                new OcrLanguage2("tk", "Turkmen"),
                new OcrLanguage2("uk", "Ukrainian"),
                new OcrLanguage2("ur", "Urdu"),
                new OcrLanguage2("ug", "Uyghur"),
                new OcrLanguage2("uz", "Uzbek"),
                new OcrLanguage2("vi", "Vietnamese"),
                new OcrLanguage2("cy", "Welsh"),
                new OcrLanguage2("xh", "Xhosa"),
                new OcrLanguage2("yi", "Yiddish"),
                new OcrLanguage2("yo", "Yoruba"),
                new OcrLanguage2("yua", "Yucatec Maya"),
                new OcrLanguage2("zu", "Zulu"),
            }.OrderBy(p => p.Name).ToList();
    }
}