using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.TimedText10Properties;

public partial class TimedText10PropertiesViewModel : ObservableObject
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

    [ObservableProperty] private ObservableCollection<string> _timeCodeFormats = null!;
    [ObservableProperty] private string _selectedTimeCodeFormat = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _fileExtensions = null!;
    [ObservableProperty] private string _selectedFileExtension = string.Empty;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    private Subtitle _subtitle = new();

    public TimedText10PropertiesViewModel()
    {
        var cultureNames = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToList();
        Languages = new ObservableCollection<string>(cultureNames);
        SelectedLanguage = string.Empty;

        TimeBases = [NotAvailable, "media", "smpte", "clock"];
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

        TimeCodeFormats =
        [
            "Source",
            "Seconds",
            "Milliseconds",
            "Ticks",
            "hh:mm:ss:ff",
            "hh:mm:ss.ms",
            "hh:mm:ss.ms-two-digits",
            "hh:mm:ss,ms",
            "Frames",
        ];
        SelectedTimeCodeFormat = TimeCodeFormats[0];

        FileExtensions = [".xml", ".ttml", ".dfxp"];
        SelectedFileExtension = FileExtensions[0];
    }

    public void Initialize(Subtitle subtitle)
    {
        _subtitle = subtitle;

        var header = subtitle.Header;
        if (string.IsNullOrEmpty(header))
        {
            header = new TimedText10().ToText(new Subtitle(), "tt");
            subtitle.Header = header;
        }

        try
        {
            var doc = XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(header)));
            XNamespace tt = "http://www.w3.org/ns/ttml";
            XNamespace ttp = "http://www.w3.org/ns/ttml#parameter";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
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
                SelectedLanguage = lang;
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

            // Styles from header
            var styles = TimedText10.GetStylesFromHeader(header);
            DefaultStyles = new ObservableCollection<string>([NotAvailable, .. styles]);

            var bodyStyleAttr = root.Descendants(tt + "body").FirstOrDefault()?.Attribute("style");
            if (bodyStyleAttr != null)
            {
                var match = DefaultStyles.FirstOrDefault(s => s == bodyStyleAttr.Value);
                SelectedDefaultStyle = match ?? DefaultStyles[0];
            }
            else
            {
                SelectedDefaultStyle = DefaultStyles[0];
            }

            // Regions from header
            var regions = TimedText10.GetRegionsFromHeader(header);
            DefaultRegions = new ObservableCollection<string>([NotAvailable, .. regions]);

            var bodyRegionAttr = root.Descendants(tt + "body").FirstOrDefault()?.Attribute("region");
            if (bodyRegionAttr != null)
            {
                var match = DefaultRegions.FirstOrDefault(r => r == bodyRegionAttr.Value);
                SelectedDefaultRegion = match ?? DefaultRegions[0];
            }
            else
            {
                SelectedDefaultRegion = DefaultRegions[0];
            }
        }
        catch
        {
            // ignore
        }

        // Time code format
        var timeCodeFormat = Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat;
        if (!string.IsNullOrEmpty(timeCodeFormat))
        {
            var match = TimeCodeFormats.FirstOrDefault(f => string.Equals(f, timeCodeFormat, System.StringComparison.OrdinalIgnoreCase));
            SelectedTimeCodeFormat = match ?? TimeCodeFormats[0];
        }

        // File extension
        var fileExt = Configuration.Settings.SubtitleSettings.TimedText10FileExtension;
        if (!string.IsNullOrEmpty(fileExt))
        {
            var match = FileExtensions.FirstOrDefault(e => e == fileExt);
            SelectedFileExtension = match ?? FileExtensions[0];
        }
    }

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
            XNamespace ttp = "http://www.w3.org/ns/ttml#parameter";
            XNamespace ttm = "http://www.w3.org/ns/ttml#metadata";
            XNamespace xml = "http://www.w3.org/XML/1998/namespace";
            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            // xml:lang
            if (!string.IsNullOrWhiteSpace(SelectedLanguage))
            {
                root.SetAttributeValue(xml + "lang", SelectedLanguage);
            }
            else
            {
                root.Attribute(xml + "lang")?.Remove();
            }

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

            var metadataNode = head.Descendants(ttm + "metadata").FirstOrDefault()
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

            _subtitle.Header = doc.ToString();
        }
        catch
        {
            // ignore
        }

        // Settings
        Configuration.Settings.SubtitleSettings.TimedText10TimeCodeFormat = SelectedTimeCodeFormat;

        var currentExt = Configuration.Settings.SubtitleSettings.TimedText10FileExtension;
        if (currentExt != SelectedFileExtension)
        {
            var favoriteFormats = Configuration.Settings.General.FavoriteSubtitleFormats;
            var currentWithExt = $"Timed Text 1.0 ({currentExt})";
            var newWithExt = $"Timed Text 1.0 ({SelectedFileExtension})";
            if (favoriteFormats != null && favoriteFormats.Contains(currentWithExt))
            {
                Configuration.Settings.General.FavoriteSubtitleFormats = favoriteFormats.Replace(currentWithExt, newWithExt);
            }

            Configuration.Settings.SubtitleSettings.TimedText10FileExtension = SelectedFileExtension;
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
        WriteValuesToXml();
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
