using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.DCinemaSmpteProperties;

public partial class DCinemaSmptePropertiesViewModel : ObservableObject
{
    [ObservableProperty] private string _windowTitle = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _languages = null!;
    [ObservableProperty] private ObservableCollection<string> _timeCodeRates = null!;
    [ObservableProperty] private ObservableCollection<string> _fontEffects = null!;

    [ObservableProperty] private bool _generateIdAuto;
    [ObservableProperty] private string _subtitleId = string.Empty;
    [ObservableProperty] private string _movieTitle = string.Empty;
    [ObservableProperty] private int _reelNumber;
    [ObservableProperty] private string _selectedLanguage = string.Empty;
    [ObservableProperty] private string _issueDate = string.Empty;
    [ObservableProperty] private string _editRate = string.Empty;
    [ObservableProperty] private string _selectedTimeCodeRate = string.Empty;
    [ObservableProperty] private string _startTime = string.Empty;

    [ObservableProperty] private string _fontId = string.Empty;
    [ObservableProperty] private string _fontUri = string.Empty;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private string _selectedFontEffect = string.Empty;
    [ObservableProperty] private Color _fontEffectColor;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private int _topBottomMargin;
    [ObservableProperty] private int _fadeUpTime;
    [ObservableProperty] private int _fadeDownTime;

    // MXF track file export: the font embedded as the LoadFont resource
    [ObservableProperty] private bool _isMxfExport;
    [ObservableProperty] private string _mxfFontFile = string.Empty;
    [ObservableProperty] private string _reelDuration = string.Empty;

    public Window? Window { get; set; }

    public Subtitle Subtitle { get; set; } = new();

    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    public DCinemaSmptePropertiesViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        _languages = new ObservableCollection<string>();
        foreach (var x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
        {
            _languages.Add(x.TwoLetterISOLanguageName);
        }
        var sortedLanguages = _languages.OrderBy(l => l).ToList();
        _languages = new ObservableCollection<string>(sortedLanguages);

        _timeCodeRates = new ObservableCollection<string> { "24", "25", "30", "48" };
        _fontEffects = new ObservableCollection<string> { "None", "Border", "Shadow" };
    }

    private void LoadSettings()
    {
        var ss = Se.Settings.File.DCinemaSmpte;

        GenerateIdAuto = ss.DCinemaAutoGenerateSubtitleId;

        // Determine if this is an existing subtitle or a new one
        var isExistingSubtitle = Subtitle.Paragraphs.Count > 0;

        if (isExistingSubtitle)
        {
            // For existing subtitles, use current/last saved settings
            SubtitleId = !string.IsNullOrEmpty(ss.CurrentDCinemaSubtitleId)
                ? ss.CurrentDCinemaSubtitleId
                : DCinemaSmpte2007.GenerateId();

            ReelNumber = int.TryParse(ss.CurrentDCinemaReelNumber, out int reelNumber) && reelNumber > 0
                ? reelNumber
                : 1;

            MovieTitle = ss.CurrentDCinemaMovieTitle ?? string.Empty;
            SelectedLanguage = !string.IsNullOrEmpty(ss.CurrentDCinemaLanguage)
                ? ss.CurrentDCinemaLanguage
                : "en";

            FontId = !string.IsNullOrEmpty(ss.CurrentDCinemaFontId)
                ? ss.CurrentDCinemaFontId
                : "theFontId";

            EditRate = !string.IsNullOrEmpty(ss.CurrentDCinemaEditRate)
                ? ss.CurrentDCinemaEditRate
                : "24 1";

            SelectedTimeCodeRate = !string.IsNullOrEmpty(ss.CurrentDCinemaTimeCodeRate)
                ? ss.CurrentDCinemaTimeCodeRate
                : "24";

            StartTime = !string.IsNullOrEmpty(ss.CurrentDCinemaStartTime)
                ? ss.CurrentDCinemaStartTime
                : "00:00:00:00";

            FontUri = !string.IsNullOrEmpty(ss.CurrentDCinemaFontUri)
                ? ss.CurrentDCinemaFontUri
                : "urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391";

            IssueDate = !string.IsNullOrEmpty(ss.CurrentDCinemaIssueDate)
                ? ss.CurrentDCinemaIssueDate
                : DateTime.Now.ToString("s");

            FontColor = ColorFromString(ss.CurrentDCinemaFontColor, Colors.White);

            if (ss.CurrentDCinemaFontEffect?.Equals("border", StringComparison.OrdinalIgnoreCase) == true)
            {
                SelectedFontEffect = "Border";
            }
            else if (ss.CurrentDCinemaFontEffect?.Equals("shadow", StringComparison.OrdinalIgnoreCase) == true)
            {
                SelectedFontEffect = "Shadow";
            }
            else
            {
                SelectedFontEffect = "None";
            }

            FontEffectColor = ColorFromString(ss.CurrentDCinemaFontEffectColor, Colors.Black);

            FontSize = ss.CurrentDCinemaFontSize > 0
                ? ss.CurrentDCinemaFontSize
                : ss.DCinemaFontSize;

            TopBottomMargin = ss.DCinemaBottomMargin;
            FadeUpTime = ss.DCinemaFadeUpTime;
            FadeDownTime = ss.DCinemaFadeDownTime;
        }
        else
        {
            // For new subtitles, use default settings (not Current settings for most properties)
            SubtitleId = DCinemaSmpte2007.GenerateId();
            ReelNumber = 1;
            MovieTitle = string.Empty;
            SelectedLanguage = "en";
            FontId = "theFontId";
            EditRate = "24 1";
            SelectedTimeCodeRate = "24";
            StartTime = "00:00:00:00";
            FontUri = "urn:uuid:3dec6dc0-39d0-498d-97d0-928d2eb78391";
            IssueDate = DateTime.Now.ToString("s");

            // Load colors from Current settings (last used colors)
            FontColor = ColorFromString(ss.CurrentDCinemaFontColor, Colors.White);

            if (ss.CurrentDCinemaFontEffect?.Equals("border", StringComparison.OrdinalIgnoreCase) == true)
            {
                SelectedFontEffect = "Border";
            }
            else if (ss.CurrentDCinemaFontEffect?.Equals("shadow", StringComparison.OrdinalIgnoreCase) == true)
            {
                SelectedFontEffect = "Shadow";
            }
            else
            {
                SelectedFontEffect = "None";
            }

            FontEffectColor = ColorFromString(ss.CurrentDCinemaFontEffectColor, Colors.Black);

            FontSize = ss.DCinemaFontSize > 0 ? ss.DCinemaFontSize : 42;
            TopBottomMargin = ss.DCinemaBottomMargin > 0 ? ss.DCinemaBottomMargin : 8;
            FadeUpTime = ss.DCinemaFadeUpTime;
            FadeDownTime = ss.DCinemaFadeDownTime;
        }

        MxfFontFile = ss.MxfFontFile ?? string.Empty;
    }

    private void SaveSettings()
    {
        var ss = Se.Settings.File.DCinemaSmpte;

        ss.DCinemaAutoGenerateSubtitleId = GenerateIdAuto;

        // Save to Current settings (for existing subtitles)
        ss.CurrentDCinemaSubtitleId = SubtitleId;
        ss.CurrentDCinemaMovieTitle = MovieTitle;
        ss.CurrentDCinemaReelNumber = ReelNumber.ToString();
        ss.CurrentDCinemaEditRate = EditRate;
        ss.CurrentDCinemaTimeCodeRate = SelectedTimeCodeRate;
        ss.CurrentDCinemaStartTime = StartTime;
        ss.CurrentDCinemaLanguage = SelectedLanguage;
        ss.CurrentDCinemaIssueDate = IssueDate;
        ss.CurrentDCinemaFontId = FontId;
        ss.CurrentDCinemaFontUri = FontUri;

        ss.CurrentDCinemaFontColor = ColorToString(FontColor);

        if (SelectedFontEffect?.Equals("Border", StringComparison.OrdinalIgnoreCase) == true)
        {
            ss.CurrentDCinemaFontEffect = "border";
        }
        else if (SelectedFontEffect?.Equals("Shadow", StringComparison.OrdinalIgnoreCase) == true)
        {
            ss.CurrentDCinemaFontEffect = "shadow";
        }
        else
        {
            ss.CurrentDCinemaFontEffect = string.Empty;
        }

        ss.CurrentDCinemaFontEffectColor = ColorToString(FontEffectColor);
        ss.CurrentDCinemaFontSize = FontSize;

        // Save to Default settings (for new subtitles)
        ss.DCinemaFontSize = FontSize;
        ss.DCinemaBottomMargin = TopBottomMargin;
        ss.DCinemaFadeUpTime = FadeUpTime;
        ss.DCinemaFadeDownTime = FadeDownTime;
        if (IsMxfExport)
        {
            ss.MxfFontFile = MxfFontFile;
        }

        Se.SaveSettings();
    }

    private static Color ColorFromString(string colorString, Color defaultColor)
    {
        if (string.IsNullOrEmpty(colorString))
        {
            return defaultColor;
        }

        try
        {
            return colorString.FromHexToColor();
        }
        catch
        {
            return defaultColor;
        }
    }

    private string ColorToString(Color color)
    {
        return color.FromColorToHex();
    }

    public void Initialize(Subtitle subtitle)
    {
        InitializeCore(subtitle, null);
    }

    [RelayCommand]
    private void GenerateSubtitleId()
    {
        SubtitleId = DCinemaSmpte2007.GenerateId();
    }

    [RelayCommand]
    private void GenerateFontUri()
    {
        FontUri = DCinemaSmpte2007.GenerateId();
    }

    [RelayCommand]
    private void SetTodayIssueDate()
    {
        IssueDate = DateTime.Now.ToString("s");
    }

    [RelayCommand]
    private async Task ChooseFontColor()
    {
        if (Window == null)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(
            Window, viewModel => viewModel.Initialize(FontColor));

        if (vm.OkPressed)
        {
            FontColor = vm.SelectedColor;
        }
    }

    [RelayCommand]
    private async Task ChooseFontEffectColor()
    {
        if (Window == null)
        {
            return;
        }

        var vm = await _windowService.ShowDialogAsync<ColorPickerWindow, ColorPickerViewModel>(
            Window, viewModel => viewModel.Initialize(FontEffectColor));

        if (vm.OkPressed)
        {
            FontEffectColor = vm.SelectedColor;
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, "Import D-Cinema properties", "D-Cinema profile", ".DCinema-interop-profile");
        if (fileName == null)
        {
            return;
        }

        try
        {
            var importer = new DcPropertiesSmpte();
            if (importer.Load(fileName))
            {
                GenerateIdAuto = Convert.ToBoolean(importer.GenerateIdAuto, CultureInfo.InvariantCulture);

                if (int.TryParse(importer.ReelNumber, out var reelNumber))
                {
                    ReelNumber = reelNumber;
                }

                SelectedLanguage = importer.Language ?? "en";
                EditRate = importer.EditRate ?? "24 1";
                SelectedTimeCodeRate = importer.TimeCodeRate ?? "24";

                if (double.TryParse(importer.StartTime, NumberStyles.Float, CultureInfo.InvariantCulture, out var startTimeMs))
                {
                    var timeCode = new TimeCode(startTimeMs);
                    StartTime = timeCode.ToHHMMSSFF();
                }

                FontId = importer.FontId ?? "theFontId";
                FontUri = importer.FontUri ?? string.Empty;
                FontColor = ColorFromString(importer.FontColor, Colors.White);
                SelectedFontEffect = importer.Effect ?? "None";
                FontEffectColor = ColorFromString(importer.EffectColor, Colors.Black);

                if (int.TryParse(importer.FontSize, out var fontSize))
                {
                    FontSize = fontSize;
                }

                if (int.TryParse(importer.TopBottomMargin, out var margin))
                {
                    TopBottomMargin = margin;
                }

                if (int.TryParse(importer.FadeUpTime, out var fadeUp))
                {
                    FadeUpTime = fadeUp;
                }

                if (int.TryParse(importer.FadeDownTime, out var fadeDown))
                {
                    FadeDownTime = fadeDown;
                }
            }
        }
        catch
        {
            // ignore import errors
        }
    }

    [RelayCommand]
    private async Task Export()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".DCinema-interop-profile", "D-Cinema profile", "Export D-Cinema properties");
        if (fileName == null)
        {
            return;
        }

        try
        {
            // HH:MM:SS:FF - the last field is frames (import reads it back with ToHHMMSSFF).
            var tc = TimeCode.ParseHHMMSSFFToMilliseconds(StartTime);

            var exporter = new DcPropertiesSmpte
            {
                GenerateIdAuto = GenerateIdAuto.ToString(CultureInfo.InvariantCulture),
                ReelNumber = ReelNumber.ToString(CultureInfo.InvariantCulture),
                Language = SelectedLanguage,
                EditRate = EditRate,
                TimeCodeRate = SelectedTimeCodeRate,
                StartTime = tc.ToString(CultureInfo.InvariantCulture),
                FontId = FontId,
                FontUri = FontUri,
                FontColor = ColorToString(FontColor),
                Effect = SelectedFontEffect,
                EffectColor = ColorToString(FontEffectColor),
                FontSize = FontSize.ToString(CultureInfo.InvariantCulture),
                TopBottomMargin = TopBottomMargin.ToString(CultureInfo.InvariantCulture),
                FadeUpTime = FadeUpTime.ToString(CultureInfo.InvariantCulture),
                FadeDownTime = FadeDownTime.ToString(CultureInfo.InvariantCulture),
            };

            exporter.Save(fileName);
        }
        catch
        {
            // ignore export errors
        }
    }

    [RelayCommand]
    private async Task BrowseMxfFontFile()
    {
        if (Window == null)
        {
            return;
        }

        var folder = string.IsNullOrEmpty(MxfFontFile) ? null : Path.GetDirectoryName(MxfFontFile);
        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.File.PropertiesDCinema.ChooseFontFile, Se.Language.General.Font, ".ttf;.otf", suggestedStartFolder: folder);
        if (!string.IsNullOrEmpty(fileName))
        {
            MxfFontFile = fileName;
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (IsMxfExport)
        {
            if (string.IsNullOrWhiteSpace(MxfFontFile) || !File.Exists(MxfFontFile))
            {
                await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.File.PropertiesDCinema.FontFileMissing, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(ReelDuration) && !TimedTextMxfWriter.TryParseTimeCode(ReelDuration, GetEditRateFrames(), out _))
            {
                await MessageBox.Show(Window!, Se.Language.General.Error, Se.Language.File.PropertiesDCinema.ReelDurationInvalid, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // The track file carries the font under the LoadFont urn:uuid
            if (!FontUri.StartsWith("urn:uuid:", StringComparison.OrdinalIgnoreCase) || !Guid.TryParse(FontUri.Substring(9), out _))
            {
                FontUri = "urn:uuid:" + Guid.NewGuid();
            }
        }

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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/file", "format-properties");
        }
    }

    internal void Initialize(SubtitleFormat format)
    {
        InitializeCore(null, format);
    }

    internal void Initialize(Subtitle subtitle, SubtitleFormat format)
    {
        InitializeCore(subtitle, format);
    }

    internal void InitializeMxfExport(Subtitle subtitle, TimeSpan? videoDuration)
    {
        IsMxfExport = true;
        InitializeCore(subtitle, null);
        if (videoDuration is { TotalMilliseconds: > 0 } duration)
        {
            // The picture reel has duration x the video's real rate frames (1 h of 23.976 = 86314,
            // not 86400) - count those, then express them as edit rate time code.
            var editRateFrames = GetEditRateFrames();
            var videoFrameRate = Se.Settings.General.CurrentFrameRate;
            var frameRate = videoFrameRate > 0 && (int)Math.Round(videoFrameRate, MidpointRounding.AwayFromZero) == editRateFrames
                ? videoFrameRate
                : editRateFrames;
            var frames = (long)Math.Round(duration.TotalSeconds * frameRate, MidpointRounding.AwayFromZero);
            ReelDuration = TimedTextMxfWriter.ToTimeCode(frames, editRateFrames);
        }

        WindowTitle = string.Format(Se.Language.File.XProperties, Se.Language.File.Export.TitleExportDCinemaSmpte2014Mxf);
    }

    /// <summary>
    /// The integer time code rate of the edit rate ("24000 1001" -> 24), 24 if unparsable.
    /// </summary>
    internal int GetEditRateFrames()
    {
        var parts = (EditRate ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2 &&
            double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var numerator) &&
            double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var denominator) &&
            numerator > 0 && denominator > 0)
        {
            return (int)Math.Round(numerator / denominator, MidpointRounding.AwayFromZero);
        }

        return 24;
    }

    private void InitializeCore(Subtitle? subtitle, SubtitleFormat? format)
    {
        if (subtitle != null)
        {
            Subtitle = subtitle;
        }

        if (format != null)
        {
            WindowTitle = string.Format(Se.Language.File.XProperties, format.Name);
        }

        LoadSettings();
    }
}
