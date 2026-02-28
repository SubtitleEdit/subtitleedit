using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
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

            if (ss.CurrentDCinemaFontEffect == "border")
            {
                SelectedFontEffect = FontEffects[1];
            }
            else if (ss.CurrentDCinemaFontEffect == "shadow")
            {
                SelectedFontEffect = FontEffects[2];
            }
            else
            {
                SelectedFontEffect = FontEffects[0];
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

            if (ss.CurrentDCinemaFontEffect == "border")
            {
                SelectedFontEffect = FontEffects[1];
            }
            else if (ss.CurrentDCinemaFontEffect == "shadow")
            {
                SelectedFontEffect = FontEffects[2];
            }
            else
            {
                SelectedFontEffect = FontEffects[0];
            }

            FontEffectColor = ColorFromString(ss.CurrentDCinemaFontEffectColor, Colors.Black);

            FontSize = ss.DCinemaFontSize > 0 ? ss.DCinemaFontSize : 42;
            TopBottomMargin = ss.DCinemaBottomMargin > 0 ? ss.DCinemaBottomMargin : 8;
            FadeUpTime = ss.DCinemaFadeUpTime;
            FadeDownTime = ss.DCinemaFadeDownTime;
        }
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

        if (SelectedFontEffect == FontEffects[1])
        {
            ss.CurrentDCinemaFontEffect = "border";
        }
        else if (SelectedFontEffect == FontEffects[2])
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
        Subtitle = subtitle;
        LoadSettings();
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
            Window, viewModel => { viewModel.SelectedColor = FontColor; });

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
            Window, viewModel => { viewModel.SelectedColor = FontEffectColor; });

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

                if (double.TryParse(importer.StartTime, out var startTimeMs))
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

        var fileName = await _fileHelper.PickSaveFile(Window, "Export D-Cinema properties", "D-Cinema profile", ".DCinema-interop-profile");
        if (fileName == null)
        {
            return;
        }

        try
        {
            var tc = TimeCode.ParseToMilliseconds(StartTime);

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
    private void Ok()
    {
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

    internal void Initialize(SubtitleFormat format)
    {
        WindowTitle = string.Format(Se.Language.File.XProperties, format.Name);
        LoadSettings();
    }
}