using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.WebVttProperties;

public partial class WebVttPropertiesViewModel : ObservableObject
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
    ];

    public WebVttPropertiesViewModel(IFileHelper fileHelper)
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