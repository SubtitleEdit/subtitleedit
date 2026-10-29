using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.ItunesTimedTextProperties;

public partial class ItunesTimedTextPropertiesViewModel : ObservableObject
{
    private const string NotAvailable = "[N/A]";

    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _description = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _languages = null!;
    [ObservableProperty] private string _selectedLanguage = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _timeBases = null!;
    [ObservableProperty] private string _selectedTimeBase = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _frameRates = null!;
    [ObservableProperty] private string _selectedFrameRate = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _frameRateMultipliers = null!;
    [ObservableProperty] private string _selectedFrameRateMultiplier = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _dropModes = null!;
    [ObservableProperty] private string _selectedDropMode = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _defaultStyles = null!;
    [ObservableProperty] private string _selectedDefaultStyle = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _defaultRegions = null!;
    [ObservableProperty] private string _selectedDefaultRegion = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _styleAttributes = null!;
    [ObservableProperty] private string _selectedStyleAttribute = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _timeCodeFormats = null!;
    [ObservableProperty] private string _selectedTimeCodeFormat = string.Empty;

    [ObservableProperty] private string _topOrigin = string.Empty;
    [ObservableProperty] private string _topExtent = string.Empty;
    [ObservableProperty] private string _bottomOrigin = string.Empty;
    [ObservableProperty] private string _bottomExtent = string.Empty;

    // Export preset (#16257): the header every saved file gets, and named copies of the setup.
    [ObservableProperty] private Color _defaultColor = Colors.White;
    [ObservableProperty] private ObservableCollection<string> _headerSources = null!;
    [ObservableProperty] private string _selectedHeaderSource = string.Empty;
    [ObservableProperty] private string _templateFile = string.Empty;
    [ObservableProperty] private string _templateSummary = string.Empty;
    [ObservableProperty] private bool _isTemplateMode;
    [ObservableProperty] private bool _isCurrentFileMode = true;
    [ObservableProperty] private ObservableCollection<string> _presetNames = new();
    [ObservableProperty] private string? _selectedPresetName;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private Subtitle _subtitle = new();
    private bool _isApplyingPreset;

    // What the current file's header holds, so switching the header source back restores it.
    private List<string> _fileStyles = new();
    private List<string> _fileRegions = new();
    private string _fileBodyStyle = NotAvailable;
    private string _fileBodyRegion = NotAvailable;

    // The color the header's styles carry now - OK recolors the styles that still have it. Kept as
    // text so "white" stays "white" when the color was not touched.
    private string _originalDefaultColorText = "white";
    private Color _originalDefaultColor = Colors.White;

    public ItunesTimedTextPropertiesViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        Languages = ["en", "da", "de", "es", "fi", "fr", "it", "ja", "ko", "nb", "nl", "pl", "pt", "ru", "sv", "zh"];
        SelectedLanguage = "en";

        TimeBases = [NotAvailable, "media", "clock", "smpte"];
        SelectedTimeBase = TimeBases[0];

        FrameRates = ["23.976", "24.0", "25.0", "29.97", "30.0"];
        SelectedFrameRate = string.Empty;

        FrameRateMultipliers = ["999 1000", "1 1", "1000 1001"];
        SelectedFrameRateMultiplier = string.Empty;

        DropModes = [NotAvailable, "dropNTSC", "dropPAL", "nonDrop"];
        SelectedDropMode = DropModes[0];

        DefaultStyles = [NotAvailable];
        SelectedDefaultStyle = NotAvailable;

        DefaultRegions = [NotAvailable];
        SelectedDefaultRegion = NotAvailable;

        StyleAttributes = ["tts:fontStyle", "style"];
        SelectedStyleAttribute = StyleAttributes[0];

        TimeCodeFormats =
        [
            "Frames",
            "Source",
            "Seconds",
            "Milliseconds",
            "Ticks",
            "hh:mm:ss:ff",
            "hh:mm:ss.ms",
            "hh:mm:ss.ms-two-digits",
            "hh:mm:ss,ms",
        ];
        SelectedTimeCodeFormat = TimeCodeFormats[0];

        HeaderSources = [Se.Language.File.PropertyHeaderSourceCurrentFile, Se.Language.File.PropertyHeaderSourceTemplateFile];
        SelectedHeaderSource = HeaderSources[0];

        TopOrigin = Configuration.Settings.SubtitleSettings.TimedTextItunesTopOrigin;
        TopExtent = Configuration.Settings.SubtitleSettings.TimedTextItunesTopExtent;
        BottomOrigin = Configuration.Settings.SubtitleSettings.TimedTextItunesBottomOrigin;
        BottomExtent = Configuration.Settings.SubtitleSettings.TimedTextItunesBottomExtent;
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;

        var header = subtitle.Header;
        if (string.IsNullOrEmpty(header))
        {
            header = new ItunesTimedText().ToText(new Subtitle(), "tt");
            subtitle.Header = header;
        }

        ReadHeader(header);

        var formats = Se.Settings.Formats;

        // Style attribute
        var styleAttr = formats.TimedTextItunesStyleAttribute;
        if (!string.IsNullOrEmpty(styleAttr))
        {
            var match = StyleAttributes.FirstOrDefault(s => s == styleAttr);
            SelectedStyleAttribute = match ?? StyleAttributes[0];
        }

        // Time code format
        var timeCodeFormat = formats.TimedTextItunesTimeCodeFormat;
        if (!string.IsNullOrEmpty(timeCodeFormat))
        {
            var match = TimeCodeFormats.FirstOrDefault(f => string.Equals(f, timeCodeFormat, StringComparison.OrdinalIgnoreCase));
            SelectedTimeCodeFormat = match ?? TimeCodeFormats[0];
        }

        // Alignment settings
        TopOrigin = formats.TimedTextItunesTopOrigin;
        TopExtent = formats.TimedTextItunesTopExtent;
        BottomOrigin = formats.TimedTextItunesBottomOrigin;
        BottomExtent = formats.TimedTextItunesBottomExtent;

        // Language setting
        var lang2 = formats.TimedTextItunesLanguage;
        if (!string.IsNullOrEmpty(lang2))
        {
            SelectLanguage(lang2);
        }

        // Header source + template
        _isApplyingPreset = true;
        try
        {
            TemplateFile = formats.TimedTextItunesTemplateFile ?? string.Empty;
            SelectedHeaderSource = formats.TimedTextItunesHeaderSource == SeItunesTimedTextPreset.HeaderSourceTemplateFile
                ? HeaderSources[1]
                : HeaderSources[0];
            RefreshPresetNames(formats.TimedTextItunesSelectedPreset);
        }
        finally
        {
            _isApplyingPreset = false;
        }

        UpdateHeaderSourceMode();
    }

    /// <summary>
    /// Reads the current file's header into the fields: metadata, root attributes, the styles and
    /// regions it defines, what the body uses by default, and the color its styles carry.
    /// </summary>
    private void ReadHeader(string header)
    {
        try
        {
            var doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(header)));
            XNamespace tt = "http://www.w3.org/ns/ttml";
            XNamespace ttp = "http://www.w3.org/ns/ttml#parameter";
            XNamespace ttm = "http://www.w3.org/ns/10/ttml#metadata";
            XNamespace xml = "http://www.w3.org/XML/1998/namespace";
            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            // Title
            var titleNode = root.Descendants(ttm + "title").FirstOrDefault()
                            ?? root.Descendants("title").FirstOrDefault();
            if (titleNode != null)
            {
                Title = titleNode.Value;
            }

            // Description
            var descNode = root.Descendants(ttm + "desc").FirstOrDefault()
                           ?? root.Descendants("desc").FirstOrDefault();
            if (descNode != null)
            {
                Description = descNode.Value;
            }

            // xml:lang
            var lang = (string?)root.Attribute(xml + "lang");
            if (!string.IsNullOrEmpty(lang))
            {
                SelectLanguage(lang);
            }

            // ttp:timeBase
            var timeBase = (string?)root.Attribute(ttp + "timeBase");
            if (!string.IsNullOrEmpty(timeBase))
            {
                var match = TimeBases.FirstOrDefault(t => t == timeBase);
                SelectedTimeBase = match ?? TimeBases[0];
            }

            // ttp:frameRate
            var frameRate = (string?)root.Attribute(ttp + "frameRate");
            if (!string.IsNullOrEmpty(frameRate))
            {
                SelectedFrameRate = frameRate;
            }

            // ttp:frameRateMultiplier
            var frameRateMultiplier = (string?)root.Attribute(ttp + "frameRateMultiplier");
            if (!string.IsNullOrEmpty(frameRateMultiplier))
            {
                SelectedFrameRateMultiplier = frameRateMultiplier;
            }

            // ttp:dropMode
            var dropMode = (string?)root.Attribute(ttp + "dropMode");
            if (!string.IsNullOrEmpty(dropMode))
            {
                var match = DropModes.FirstOrDefault(d => d == dropMode);
                SelectedDropMode = match ?? DropModes[0];
            }

            // Styles and regions from header, and what the body uses by default
            _fileStyles = TimedText10.GetStylesFromHeader(header);
            _fileRegions = TimedText10.GetRegionsFromHeader(header);
            var body = root.Descendants(tt + "body").FirstOrDefault();
            _fileBodyStyle = body?.Attribute("style")?.Value ?? NotAvailable;
            _fileBodyRegion = body?.Attribute("region")?.Value ?? NotAvailable;

            // The color of the default style ("normal", the body's, or the first style with a color)
            var colorText = GetDefaultStyleColor(root, body?.Attribute("style")?.Value);
            if (!string.IsNullOrEmpty(colorText))
            {
                _originalDefaultColorText = colorText;
                _originalDefaultColor = ParseColor(colorText, Colors.White);
                DefaultColor = _originalDefaultColor;
            }
        }
        catch
        {
            // ignore
        }
    }

    private static string? GetDefaultStyleColor(XElement root, string? bodyStyle)
    {
        XNamespace tt = "http://www.w3.org/ns/ttml";
        XNamespace tts = "http://www.w3.org/ns/ttml#styling";
        XNamespace xml = "http://www.w3.org/XML/1998/namespace";
        var styles = root.Descendants(tt + "style").ToList();
        var defaultStyle = styles.FirstOrDefault(s => (string?)s.Attribute(xml + "id") == "normal")
                           ?? styles.FirstOrDefault(s => bodyStyle != null && (string?)s.Attribute(xml + "id") == bodyStyle)
                           ?? styles.FirstOrDefault(s => s.Attribute(tts + "color") != null);
        return (string?)defaultStyle?.Attribute(tts + "color");
    }

    internal static Color ParseColor(string text, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return fallback;
        }

        return Color.TryParse(text.Trim(), out var color) ? color : fallback;
    }

    private void SelectLanguage(string lang)
    {
        var match = Languages.FirstOrDefault(l => l == lang);
        if (match != null)
        {
            SelectedLanguage = match;
        }
        else
        {
            Languages.Add(lang);
            SelectedLanguage = lang;
        }
    }

    /// <summary>
    /// Refills the style/region lists in place - the window's combo boxes hold the collection
    /// instances, so a replaced collection would leave them showing the old items.
    /// </summary>
    private void SetStylesAndRegions(List<string> styles, List<string> regions, string? defaultStyle, string? defaultRegion)
    {
        Refill(DefaultStyles, styles);
        SelectedDefaultStyle = DefaultStyles.FirstOrDefault(s => s == defaultStyle) ?? DefaultStyles[0];

        Refill(DefaultRegions, regions);
        SelectedDefaultRegion = DefaultRegions.FirstOrDefault(r => r == defaultRegion) ?? DefaultRegions[0];
    }

    private static void Refill(ObservableCollection<string> target, List<string> items)
    {
        target.Clear();
        target.Add(NotAvailable);
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    partial void OnSelectedHeaderSourceChanged(string value)
    {
        if (_isApplyingPreset)
        {
            return;
        }

        UpdateHeaderSourceMode();
    }

    partial void OnTemplateFileChanged(string value)
    {
        if (_isApplyingPreset || !IsTemplateMode)
        {
            return;
        }

        LoadTemplate();
    }

    /// <summary>
    /// The style/region lists follow the header source: the current file's header, or the template's.
    /// </summary>
    private void UpdateHeaderSourceMode()
    {
        IsTemplateMode = SelectedHeaderSource == Se.Language.File.PropertyHeaderSourceTemplateFile;
        IsCurrentFileMode = !IsTemplateMode;

        if (IsTemplateMode)
        {
            LoadTemplate();
        }
        else
        {
            TemplateSummary = string.Empty;
            SetStylesAndRegions(_fileStyles, _fileRegions, _fileBodyStyle, _fileBodyRegion);
        }
    }

    private void LoadTemplate()
    {
        var formats = Se.Settings.Formats;
        var wantedStyle = SelectedDefaultStyle != NotAvailable ? SelectedDefaultStyle : formats.TimedTextItunesDefaultStyle;
        var wantedRegion = SelectedDefaultRegion != NotAvailable ? SelectedDefaultRegion : formats.TimedTextItunesDefaultRegion;

        if (string.IsNullOrWhiteSpace(TemplateFile) || !File.Exists(TemplateFile))
        {
            TemplateSummary = Se.Language.File.PropertyTemplateFileNotFound;
            SetStylesAndRegions(new List<string>(), new List<string>(), null, null);
            return;
        }

        string text;
        try
        {
            text = File.ReadAllText(TemplateFile);
        }
        catch (Exception exception)
        {
            TemplateSummary = exception.Message;
            SetStylesAndRegions(new List<string>(), new List<string>(), null, null);
            return;
        }

        if (ItunesTimedText.LoadHeaderTemplate(text) == null)
        {
            TemplateSummary = Se.Language.File.PropertyTemplateFileInvalid;
            SetStylesAndRegions(new List<string>(), new List<string>(), null, null);
            return;
        }

        var styles = TimedText10.GetStylesFromHeader(text);
        var regions = TimedText10.GetRegionsFromHeader(text);
        var language = "-";
        try
        {
            var doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(text)));
            XNamespace xml = "http://www.w3.org/XML/1998/namespace";
            language = (string?)doc.Root?.Attribute(xml + "lang") ?? "-";
        }
        catch
        {
            // ignore
        }

        TemplateSummary = string.Format(Se.Language.File.PropertyTemplateSummaryX, language, styles.Count == 0 ? "-" : string.Join(", ", styles), regions.Count == 0 ? "-" : string.Join(", ", regions));
        SetStylesAndRegions(styles, regions, wantedStyle, wantedRegion);
    }

    [RelayCommand]
    private async Task BrowseTemplateFile()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.File.PickTemplateFile, Se.Language.File.TtmlFiles, ".itt", "XML", ".xml");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        TemplateFile = fileName;
    }

    [RelayCommand]
    private async Task ChooseDefaultColor()
    {
        if (Window == null)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(
            Window, viewModel => viewModel.Initialize(DefaultColor));

        if (vm.OkPressed)
        {
            DefaultColor = vm.SelectedColor;
        }
    }

    /// <summary>
    /// The color as the settings and the header store it: the header's own text when the color was
    /// not changed (so "white" stays "white"), else #RRGGBB.
    /// </summary>
    private string DefaultColorText => DefaultColor == _originalDefaultColor
        ? _originalDefaultColorText
        : DefaultColor.FromColorToHex(includeAlpha: false);

    private void WriteValuesToXml()
    {
        var header = _subtitle.Header;
        if (string.IsNullOrEmpty(header))
        {
            return;
        }

        try
        {
            var doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(header)));
            XNamespace tt = "http://www.w3.org/ns/ttml";
            XNamespace tts = "http://www.w3.org/ns/ttml#styling";
            XNamespace ttp = "http://www.w3.org/ns/ttml#parameter";
            XNamespace ttm = "http://www.w3.org/ns/10/ttml#metadata";
            XNamespace xml = "http://www.w3.org/XML/1998/namespace";
            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            // xml:lang
            root.SetAttributeValue(xml + "lang", SelectedLanguage);

            // ttp:timeBase
            SetOrRemoveAttribute(root, ttp + "timeBase", SelectedTimeBase == NotAvailable ? null : SelectedTimeBase);

            // ttp:frameRate
            SetOrRemoveAttribute(root, ttp + "frameRate", string.IsNullOrWhiteSpace(SelectedFrameRate) ? null : SelectedFrameRate);

            // ttp:frameRateMultiplier
            SetOrRemoveAttribute(root, ttp + "frameRateMultiplier", string.IsNullOrWhiteSpace(SelectedFrameRateMultiplier) ? null : SelectedFrameRateMultiplier);

            // ttp:dropMode
            SetOrRemoveAttribute(root, ttp + "dropMode", SelectedDropMode == NotAvailable ? null : SelectedDropMode);

            // Title and Description in head/metadata
            var head = root.Element(tt + "head") ?? root.Descendants(tt + "head").FirstOrDefault();
            if (head == null)
            {
                head = new XElement(tt + "head");
                root.AddFirst(head);
            }

            var metadataNode = head.Element(ttm + "metadata") ?? head.Descendants(ttm + "metadata").FirstOrDefault()
                               ?? head.Descendants("metadata").FirstOrDefault();

            var hasTitle = !string.IsNullOrWhiteSpace(Title);
            var hasDesc = !string.IsNullOrWhiteSpace(Description);

            if (!hasTitle && !hasDesc)
            {
                metadataNode?.Remove();
            }
            else
            {
                if (metadataNode == null)
                {
                    metadataNode = new XElement(ttm + "metadata");
                    head.AddFirst(metadataNode);
                }

                if (hasTitle)
                {
                    var titleNode = metadataNode.Descendants(ttm + "title").FirstOrDefault()
                                    ?? metadataNode.Descendants("title").FirstOrDefault();
                    if (titleNode == null)
                    {
                        titleNode = new XElement(ttm + "title");
                        metadataNode.Add(titleNode);
                    }
                    titleNode.Value = Title;
                }
                else
                {
                    metadataNode.Descendants(ttm + "title").FirstOrDefault()?.Remove();
                    metadataNode.Descendants("title").FirstOrDefault()?.Remove();
                }

                if (hasDesc)
                {
                    var descNode = metadataNode.Descendants(ttm + "desc").FirstOrDefault()
                                   ?? metadataNode.Descendants("desc").FirstOrDefault();
                    if (descNode == null)
                    {
                        descNode = new XElement(ttm + "desc");
                        metadataNode.Add(descNode);
                    }
                    descNode.Value = Description;
                }
                else
                {
                    metadataNode.Descendants(ttm + "desc").FirstOrDefault()?.Remove();
                    metadataNode.Descendants("desc").FirstOrDefault()?.Remove();
                }
            }

            // body style / region
            var body = root.Descendants(tt + "body").FirstOrDefault();
            if (body != null)
            {
                if (SelectedDefaultStyle == NotAvailable || string.IsNullOrEmpty(SelectedDefaultStyle))
                {
                    body.Attribute("style")?.Remove();
                }
                else
                {
                    body.SetAttributeValue("style", SelectedDefaultStyle);
                }

                if (SelectedDefaultRegion == NotAvailable || string.IsNullOrEmpty(SelectedDefaultRegion))
                {
                    body.Attribute("region")?.Remove();
                }
                else
                {
                    body.SetAttributeValue("region", SelectedDefaultRegion);
                }
            }

            // Default color: every style that still has the old default color gets the new one
            if (DefaultColor != _originalDefaultColor)
            {
                var newColor = DefaultColorText;
                foreach (var style in root.Descendants(tt + "style"))
                {
                    var colorAttr = style.Attribute(tts + "color");
                    if (colorAttr != null && string.Equals(colorAttr.Value.Trim(), _originalDefaultColorText, StringComparison.OrdinalIgnoreCase))
                    {
                        colorAttr.Value = newColor;
                    }
                }
            }

            _subtitle.Header = doc.ToString();
        }
        catch
        {
            // ignore
        }
    }

    private static void SetOrRemoveAttribute(XElement element, XName name, string? value)
    {
        if (value == null)
        {
            element.Attribute(name)?.Remove();
        }
        else
        {
            element.SetAttributeValue(name, value);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        // With a template the saved header comes from the template file, not from this file's header.
        if (!IsTemplateMode)
        {
            WriteValuesToXml();
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    private void SaveSettings()
    {
        // These used to be written onto the libse Configuration singleton only, which SE5 never
        // persists - the regions, time code format and style attribute were back at their defaults
        // after every restart.
        var formats = Se.Settings.Formats;
        formats.TimedTextItunesTopOrigin = TopOrigin;
        formats.TimedTextItunesTopExtent = TopExtent;
        formats.TimedTextItunesBottomOrigin = BottomOrigin;
        formats.TimedTextItunesBottomExtent = BottomExtent;
        formats.TimedTextItunesTimeCodeFormat = SelectedTimeCodeFormat;
        formats.TimedTextItunesStyleAttribute = SelectedStyleAttribute;
        formats.TimedTextItunesLanguage = SelectedLanguage ?? string.Empty;
        formats.TimedTextItunesDefaultColor = DefaultColorText;
        formats.TimedTextItunesDefaultStyle = SelectedDefaultStyle == NotAvailable ? string.Empty : SelectedDefaultStyle;
        formats.TimedTextItunesDefaultRegion = SelectedDefaultRegion == NotAvailable ? string.Empty : SelectedDefaultRegion;
        formats.TimedTextItunesHeaderSource = IsTemplateMode ? SeItunesTimedTextPreset.HeaderSourceTemplateFile : SeItunesTimedTextPreset.HeaderSourceCurrentFile;
        formats.TimedTextItunesTemplateFile = TemplateFile ?? string.Empty;
        formats.TimedTextItunesSelectedPreset = SelectedPresetName ?? string.Empty;

        Se.SaveSettings();
        Se.UpdateLibSeSettings(); // the writer reads the libse mirror, so the next save sees these at once
    }

    private void RefreshPresetNames(string? selectName)
    {
        _isApplyingPreset = true;
        try
        {
            PresetNames.Clear();
            foreach (var name in Se.Settings.Formats.TimedTextItunesPresets
                         .Select(p => p.Name)
                         .OrderBy(p => p, StringComparer.CurrentCultureIgnoreCase))
            {
                PresetNames.Add(name);
            }

            SelectedPresetName = selectName != null && PresetNames.Contains(selectName) ? selectName : null;
        }
        finally
        {
            _isApplyingPreset = false;
        }
    }

    partial void OnSelectedPresetNameChanged(string? value)
    {
        if (_isApplyingPreset || string.IsNullOrEmpty(value))
        {
            return;
        }

        ApplyPreset(value);
    }

    private SeItunesTimedTextPreset ToPreset(string name)
    {
        return new SeItunesTimedTextPreset
        {
            Name = name,
            Language = SelectedLanguage ?? string.Empty,
            TimeBase = SelectedTimeBase == NotAvailable ? string.Empty : SelectedTimeBase,
            FrameRate = SelectedFrameRate ?? string.Empty,
            FrameRateMultiplier = SelectedFrameRateMultiplier ?? string.Empty,
            DropMode = SelectedDropMode == NotAvailable ? string.Empty : SelectedDropMode,
            DefaultStyle = SelectedDefaultStyle == NotAvailable ? string.Empty : SelectedDefaultStyle,
            DefaultRegion = SelectedDefaultRegion == NotAvailable ? string.Empty : SelectedDefaultRegion,
            DefaultColor = DefaultColorText,
            StyleAttribute = SelectedStyleAttribute,
            TimeCodeFormat = SelectedTimeCodeFormat,
            TopOrigin = TopOrigin,
            TopExtent = TopExtent,
            BottomOrigin = BottomOrigin,
            BottomExtent = BottomExtent,
            HeaderSource = IsTemplateMode ? SeItunesTimedTextPreset.HeaderSourceTemplateFile : SeItunesTimedTextPreset.HeaderSourceCurrentFile,
            TemplateFile = TemplateFile ?? string.Empty,
        };
    }

    private void ApplyPreset(string name)
    {
        var preset = Se.Settings.Formats.TimedTextItunesPresets.FirstOrDefault(p => p.Name == name);
        if (preset == null)
        {
            return;
        }

        _isApplyingPreset = true;
        try
        {
            if (!string.IsNullOrEmpty(preset.Language))
            {
                SelectLanguage(preset.Language);
            }

            SelectedTimeBase = TimeBases.FirstOrDefault(t => t == preset.TimeBase) ?? TimeBases[0];
            SelectedFrameRate = preset.FrameRate ?? string.Empty;
            SelectedFrameRateMultiplier = preset.FrameRateMultiplier ?? string.Empty;
            SelectedDropMode = DropModes.FirstOrDefault(d => d == preset.DropMode) ?? DropModes[0];
            SelectedStyleAttribute = StyleAttributes.FirstOrDefault(s => s == preset.StyleAttribute) ?? StyleAttributes[0];
            SelectedTimeCodeFormat = TimeCodeFormats.FirstOrDefault(f => string.Equals(f, preset.TimeCodeFormat, StringComparison.OrdinalIgnoreCase)) ?? TimeCodeFormats[0];
            TopOrigin = preset.TopOrigin ?? string.Empty;
            TopExtent = preset.TopExtent ?? string.Empty;
            BottomOrigin = preset.BottomOrigin ?? string.Empty;
            BottomExtent = preset.BottomExtent ?? string.Empty;
            DefaultColor = ParseColor(preset.DefaultColor, Colors.White);
            TemplateFile = preset.TemplateFile ?? string.Empty;
            SelectedHeaderSource = preset.HeaderSource == SeItunesTimedTextPreset.HeaderSourceTemplateFile ? HeaderSources[1] : HeaderSources[0];
        }
        finally
        {
            _isApplyingPreset = false;
        }

        UpdateHeaderSourceMode();
        SelectedDefaultStyle = DefaultStyles.FirstOrDefault(s => s == preset.DefaultStyle) ?? DefaultStyles[0];
        SelectedDefaultRegion = DefaultRegions.FirstOrDefault(r => r == preset.DefaultRegion) ?? DefaultRegions[0];
    }

    [RelayCommand]
    private async Task SavePreset()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window, vm =>
        {
            vm.Initialize(Se.Language.Tools.BatchConvert.PresetName, SelectedPresetName ?? string.Empty, 300, 20, returnSubmits: true);
        });

        var name = (result.Text ?? string.Empty).Trim();
        if (!result.OkPressed || name.Length == 0)
        {
            return;
        }

        var presets = Se.Settings.Formats.TimedTextItunesPresets;
        presets.RemoveAll(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        presets.Add(ToPreset(name));
        Se.SaveSettings();
        RefreshPresetNames(name);
    }

    [RelayCommand]
    private async Task DeletePreset()
    {
        var name = SelectedPresetName;
        if (Window == null || string.IsNullOrEmpty(name))
        {
            return;
        }

        var answer = await MessageBox.Show(
            Window,
            Se.Language.Tools.BatchConvert.DeletePreset,
            string.Format(Se.Language.Tools.BatchConvert.DeletePresetX, name),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        Se.Settings.Formats.TimedTextItunesPresets.RemoveAll(p => p.Name == name);
        Se.SaveSettings();
        RefreshPresetNames(null);
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/file", "format-properties");
        }
    }
}
