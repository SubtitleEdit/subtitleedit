using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.RosettaProperties;

public partial class RosettaPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _languages;
    [ObservableProperty] private string _selectedLanguage;
    [ObservableProperty] private string _selectedFontName;
    [ObservableProperty] private decimal _selectedFontSize;
    [ObservableProperty] private string _selectedLineHeight;

    public Window? Window { get; set; }

    public Subtitle Subtitle { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;

    private readonly List<string> _bcp47LanguageTags =
    [
        "aeb",
        "af",
        "af-NA",
        "af-ZA",
        "als",
        "am",
        "am-ET",
        "ar-AE",
        "ar-BH",
        "ar-DZ",
        "ar-EG",
        "ar-IQ",
        "ar-JO",
        "ar-KW",
        "ar-LB",
        "ar-LY",
        "ar-MA",
        "ar-OM",
        "ar-QA",
        "ar-SA",
        "ar-SD",
        "ar-SY",
        "ar-TN",
        "ar-YE",
        "arb",
        "ars",
        "as-IN",
        "ase",
        "az-AZ",
        "azj",
        "be",
        "be-BY",
        "bg",
        "bg-BG",
        "bi",
        "bi-VU",
        "bn-BD",
        "bn-IN",
        "bs",
        "bs-BA",
        "bzs",
        "ca-ES",
        "ca-FR",
        "ca-IT",
        "ca-valencia",
        "ckb",
        "cmn",
        "cmn-CN",
        "cmn-SG",
        "cmn-TW",
        "cnr",
        "cnr-Latn",
        "cnr-ME",
        "crs",
        "crs-SC",
        "cs",
        "cs-CZ",
        "cy",
        "cy-GB",
        "da",
        "da-DK",
        "da-GL",
        "de",
        "de-AT",
        "de-BE",
        "de-CH",
        "de-DE",
        "de-LI",
        "de-LU",
        "dv",
        "dv-MV",
        "dz",
        "dz-BT",
        "ekk",
        "el",
        "el-CY",
        "el-GR",
        "en",
        "en-419",
        "en-AE",
        "en-AG",
        "en-AI",
        "en-AS",
        "en-AU",
        "en-BB",
        "en-BE",
        "en-BM",
        "en-BW",
        "en-BZ",
        "en-CA",
        "en-CC",
        "en-CK",
        "en-CM",
        "en-CX",
        "en-DG",
        "en-DK",
        "en-ER",
        "en-FJ",
        "en-FK",
        "en-FM",
        "en-GB",
        "en-GD",
        "en-GG",
        "en-GH",
        "en-GI",
        "en-GM",
        "en-GU",
        "en-GY",
        "en-HK",
        "en-IE",
        "en-IL",
        "en-IN",
        "en-IO",
        "en-JE",
        "en-JM",
        "en-KE",
        "en-KI",
        "en-KN",
        "en-KY",
        "en-LC",
        "en-LR",
        "en-LS",
        "en-MG",
        "en-MH",
        "en-MO",
        "en-MP",
        "en-MS",
        "en-MT",
        "en-MU",
        "en-MW",
        "en-MY",
        "en-NA",
        "en-NG",
        "en-NZ",
        "en-PG",
        "en-PH",
        "en-PK",
        "en-PN",
        "en-PR",
        "en-PW",
        "en-RW",
        "en-SB",
        "en-SC",
        "en-SD",
        "en-SE",
        "en-SG",
        "en-SH",
        "en-SI",
        "en-SL",
        "en-SS",
        "en-SX",
        "en-SZ",
        "en-TC",
        "en-TK",
        "en-TO",
        "en-TT",
        "en-TV",
        "en-TZ",
        "en-UG",
        "en-UM",
        "en-US",
        "en-VC",
        "en-VG",
        "en-VI",
        "en-VU",
        "en-WS",
        "en-ZA",
        "en-ZM",
        "en-ZW",
        "es",
        "es-419",
        "es-AR",
        "es-BO",
        "es-CL",
        "es-CO",
        "es-CR",
        "es-CU",
        "es-DO",
        "es-EC",
        "es-ES",
        "es-GT",
        "es-HN",
        "es-MX",
        "es-NI",
        "es-PA",
        "es-PE",
        "es-PR",
        "es-PY",
        "es-SV",
        "es-US",
        "es-UY",
        "es-VE",
        "et-EE",
        "eu",
        "eu-ES",
        "fa-AF",
        "fa-IR",
        "fi",
        "fi-FI",
        "fil",
        "fil-PH",
        "fj",
        "fj-FJ",
        "fo",
        "fo-DK",
        "fo-FO",
        "fr",
        "fr-BE",
        "fr-BF",
        "fr-BI",
        "fr-BJ",
        "fr-CA",
        "fr-CD",
        "fr-CF",
        "fr-CG",
        "fr-CH",
        "fr-CI",
        "fr-CM",
        "fr-DJ",
        "fr-FR",
        "fr-GA",
        "fr-GF",
        "fr-GN",
        "fr-GP",
        "fr-HT",
        "fr-LU",
        "fr-MA",
        "fr-MC",
        "fr-MG",
        "fr-ML",
        "fr-MQ",
        "fr-MR",
        "fr-MU",
        "fr-NC",
        "fr-NE",
        "fr-PF",
        "fr-RE",
        "fr-RW",
        "fr-SN",
        "fr-SY",
        "fr-TD",
        "fr-TG",
        "fr-TN",
        "fr-VU",
        "fr-WF",
        "fr-YT",
        "fsl",
        "ga",
        "ga-IE",
        "gl",
        "gl-ES",
        "gu",
        "gu-IN",
        "he",
        "he-IL",
        "hi",
        "hi-IN",
        "hif",
        "hif-FJ",
        "hr",
        "hr-HR",
        "hu",
        "hu-HU",
        "hu-RO",
        "hu-SK",
        "hy",
        "hy-AM",
        "id",
        "id-ID",
        "ig-NG",
        "is",
        "is-IS",
        "it",
        "it-CH",
        "it-IT",
        "it-SM",
        "ja",
        "ja-Hira",
        "ja-Hira-JP",
        "ja-JP",
        "ja-Jpan",
        "ja-Jpan-JP",
        "ja-Kana",
        "ja-Kata-JP",
        "ka",
        "ka-GE",
        "khk",
        "khk-Cyrl",
        "khk-Mong",
        "kk",
        "kk-KZ",
        "kl",
        "kl-GL",
        "km",
        "km-KH",
        "kmr",
        "kn",
        "kn-IN",
        "ko",
        "ko-KP",
        "ko-KR",
        "ky",
        "ky-KG",
        "ky-Latn",
        "la",
        "lb",
        "lb-LU",
        "lg",
        "lg-UG",
        "lo",
        "lo-LA",
        "lt",
        "lt-LT",
        "lv-LV",
        "lvs",
        "meu",
        "meu-PG",
        "mg-MG",
        "mk",
        "mk-MK",
        "ml",
        "ml-IN",
        "mn-MN",
        "mr",
        "mr-IN",
        "ms-BN",
        "ms-MY",
        "mt",
        "mt-MT",
        "my",
        "my-MM",
        "na",
        "na-NR",
        "nan",
        "nan-TW",
        "nap",
        "nap-IT",
        "nb",
        "nb-NO",
        "ne-IN",
        "ne-NP",
        "nl",
        "nl-AW",
        "nl-BE",
        "nl-NL",
        "nl-SR",
        "nn",
        "nn-NO",
        "no",
        "no-NO",
        "npi",
        "ny",
        "ny-MW",
        "oc-aranes",
        "pa",
        "pa-IN",
        "pa-PK",
        "pau",
        "pau-PW",
        "pes",
        "pih",
        "pih-PN",
        "pl",
        "pl-PL",
        "pnb",
        "prs",
        "prs-AF",
        "ps-AF",
        "pt-AO",
        "pt-BR",
        "pt-CH",
        "pt-CV",
        "pt-GQ",
        "pt-GW",
        "pt-LU",
        "pt-MO",
        "pt-MZ",
        "pt-PT",
        "pt-ST",
        "pt-TL",
        "rn",
        "rn-BI",
        "ro",
        "ro-MD",
        "ro-RO",
        "ru",
        "ru-BY",
        "ru-KG",
        "ru-KZ",
        "ru-MD",
        "ru-RU",
        "ru-UA",
        "rw",
        "rw-RW",
        "sdh",
        "si",
        "si-LK",
        "sk",
        "sk-SK",
        "sl",
        "sl-SI",
        "sm",
        "sm-WS",
        "so",
        "so-SO",
        "sq-AL",
        "sq-MK",
        "sr",
        "sr-BA",
        "sr-Latn",
        "sr-Latn-RS",
        "sr-ME",
        "sr-RS",
        "srn",
        "srn-SR",
        "ss",
        "ss-SZ",
        "st",
        "st-ZA",
        "sv",
        "sv-FI",
        "sv-SE",
        "sw-CD",
        "sw-KE",
        "sw-TZ",
        "sw-UG",
        "swh",
        "swh-KE",
        "swh-TZ",
        "swl",
        "ta",
        "ta-IN",
        "ta-LK",
        "ta-MY",
        "ta-SG",
        "te",
        "te-IN",
        "tg",
        "tg-Cyrl",
        "tg-TJ",
        "th",
        "th-TH",
        "ti",
        "ti-ER",
        "ti-ET",
        "tk",
        "tk-TM",
        "tl",
        "tl-PH",
        "tn",
        "tn-BW",
        "to",
        "to-TO",
        "tpi",
        "tpi-PG",
        "tr",
        "tr-CY",
        "tr-TR",
        "ts",
        "ts-ZA",
        "tsq",
        "tvl",
        "tvl-TV",
        "ty",
        "ty-PF",
        "uk",
        "uk-UA",
        "und",
        "ur",
        "ur-IN",
        "ur-PK",
        "uz",
        "uz-UZ",
        "vi",
        "vi-VN",
        "vls",
        "wo",
        "xh-ZA",
        "yi",
        "yo-BJ",
        "yo-NG",
        "yue",
        "yue-HK",
        "zh",
        "zh-CN",
        "zh-HK",
        "zh-Hans-CN",
        "zh-Hans-SG",
        "zh-Hans-TW",
        "zh-Hant-HK",
        "zh-Hant-MO",
        "zh-Hant-TW",
        "zh-MO",
        "zh-SG",
        "zh-TW",
        "zlm-SG",
        "zsm",
        "zu",
        "zu-ZA",
        "zxx",
    ];

    public RosettaPropertiesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Languages =
        [
            Se.Language.General.Autodetect,
            .. _bcp47LanguageTags,
        ];
        SelectedLanguage = Languages[0];

        SelectedLanguage = string.Empty;
        SelectedFontName = string.Empty;
        SelectedFontSize = 5.3m;
        SelectedLineHeight = string.Empty;
        Subtitle = new Subtitle();
        LoadSettings();
    }

    private void LoadSettings()
    {
        SelectedLineHeight = Se.Settings.Formats.RosettaLineHeight;

        var fontSize = Se.Settings.Formats.RosettaFontSize.Replace("rh", string.Empty);
        if (decimal.TryParse(fontSize, CultureInfo.InvariantCulture, out var d))
        {
            SelectedFontSize = d;
        }
        else
        {
            SelectedFontSize = 5.3m;
        }

        if (Se.Settings.Formats.RosettaLanguageAutoDetect)
        {
            SelectedLanguage = Languages[0];
        }
        else
        {
            var lang = Languages.FirstOrDefault(l => l == Se.Settings.Formats.RosettaLanguage);
            SelectedLanguage = lang ?? Languages[0];
        }
    }

    private void SaveSettings()
    {
        Se.Settings.Formats.RosettaLineHeight = SelectedLineHeight;
        Se.Settings.Formats.RosettaFontSize = SelectedFontSize.ToString(CultureInfo.InvariantCulture) + "rh";

        if (SelectedLanguage == Languages[0])
        {
            Se.Settings.Formats.RosettaLanguageAutoDetect = true;
            Se.Settings.Formats.RosettaLanguage = "en";
        }
        else
        {
            Se.Settings.Formats.RosettaLanguageAutoDetect = false;
            Se.Settings.Formats.RosettaLanguage = SelectedLanguage;
        }

        Se.SaveSettings();
    }

    public void Initialize(Subtitle subtitle)
    {
        Subtitle = subtitle;
        ReadValuesFromXml(subtitle);
    }

    private void ReadValuesFromXml(Subtitle subtitle)
    {

        if (subtitle.Header?.Contains("imsc-rosetta") == true)
        {
            ReadValuesFromXml(subtitle.Header);
        }
    }

    private void ReadValuesFromXml(string header)
    {
        try
        {
            var doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(header)));
            XNamespace tt = "http://www.w3.org/ns/ttml";
            XNamespace ttp = "http://www.w3.org/ns/ttml#parameter";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            XNamespace xml = "http://www.w3.org/XML/1998/namespace";
            var ttElement = doc.Root;
            if (ttElement != null)
            {
                var language = (string?)ttElement.Attribute(xml + "lang");
                if (!string.IsNullOrEmpty(language))
                {
                    var l = Languages.FirstOrDefault(lang => lang == language);
                    if (l != null)
                    {
                        SelectedLanguage = l;
                    }
                }

                var rDefault = doc.Descendants(tt + "style").FirstOrDefault(e => (string?)e.Attribute(xml + "id") == "_r_default");
                if (rDefault != null)
                {
                    var fontSize = (string?)rDefault.Attribute(tts + "fontSize");
                    if (!string.IsNullOrEmpty(fontSize) && fontSize.EndsWith("rh"))
                    {
                        fontSize = fontSize.Replace("rh", string.Empty);
                        if (decimal.TryParse(fontSize, CultureInfo.InvariantCulture, out var d))
                        {
                            SelectedFontSize = d;
                        }
                    }

                    var lineHeight = (string?)rDefault.Attribute(tts + "lineHeight");
                    if (!string.IsNullOrEmpty(lineHeight))
                    {
                        SelectedLineHeight = lineHeight;
                    }
                }

                //var pFont1 = doc.Descendants(tt + "style").FirstOrDefault(e => (string?)e.Attribute(xml + "id") == "p_font1");
                //if (pFont1 != null)
                //{
                //    var fontName = (string?)pFont1.Attribute(tts + "fontFamily");
                //    if (!string.IsNullOrEmpty(fontName))
                //    {
                //        SelectedFontName = fontName;
                //    }
                //}
            }
        }
        catch
        {
            // ignore
        }
    }

    private void WriteValuesToXml(Subtitle subtitle)
    {
        try
        {
            if (subtitle.Header?.Contains("imsc-rosetta") == true)
            {
                var doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(subtitle.Header)));
                XNamespace tt = "http://www.w3.org/ns/ttml";
                XNamespace ttp = "http://www.w3.org/ns/ttml#parameter";
                XNamespace tts = "http://www.w3.org/ns/ttml#styling";
                XNamespace xml = "http://www.w3.org/XML/1998/namespace";
                var ttElement = doc.Root;
                if (ttElement != null)
                {
                    if (!string.IsNullOrEmpty(SelectedLanguage))
                    {
                        ttElement.SetAttributeValue(xml + "lang", SelectedLanguage);
                    }

                    var rDefault = doc.Descendants(tt + "style").FirstOrDefault(e => (string?)e.Attribute(xml + "id") == "_r_default");
                    if (rDefault != null)
                    {
                        var fontSize = SelectedFontSize.ToString(CultureInfo.InvariantCulture) + "rh";
                        rDefault.SetAttributeValue(tts + "fontSize", fontSize);

                        if (!string.IsNullOrEmpty(SelectedLineHeight))
                        {
                            rDefault.SetAttributeValue(tts + "lineHeight", SelectedLineHeight);
                        }
                    }

                    //var pFont1 = doc.Descendants(tt + "style").FirstOrDefault(e => (string?)e.Attribute(xml + "id") == "p_font1");
                    //if (pFont1 != null && !string.IsNullOrEmpty(SelectedFontName))
                    //{
                    //    pFont1.SetAttributeValue(tts + "fontFamily", SelectedFontName);
                    //}
                }

                subtitle.Header = doc.ToString();
            }
        }
        catch
        {
            // ignore
        }

    }

    [RelayCommand]
    private async Task Import()
    {

        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, "Import Rosetta properties", "IMSCR files", ".imscr");
        if (fileName == null)
        {
            return;
        }

        ReadValuesFromXml(File.ReadAllText(fileName));
    }

    [RelayCommand]
    private void Ok()
    {
        WriteValuesToXml(Subtitle);
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}