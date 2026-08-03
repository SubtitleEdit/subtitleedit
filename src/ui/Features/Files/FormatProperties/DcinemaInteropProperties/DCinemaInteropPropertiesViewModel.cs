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

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.DCinemaInteropProperties;

public partial class DCinemaInteropPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private string _windowTitle = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _languages = null!;
    [ObservableProperty] private ObservableCollection<string> _fontEffects = null!;

    [ObservableProperty] private bool _generateIdAuto;
    [ObservableProperty] private string _subtitleId = string.Empty;
    [ObservableProperty] private string _movieTitle = string.Empty;
    [ObservableProperty] private int _reelNumber;
    [ObservableProperty] private string _selectedLanguage = string.Empty;

    [ObservableProperty] private string _fontId = string.Empty;
    [ObservableProperty] private string _fontUri = string.Empty;
    [ObservableProperty] private Color _fontColor;
    [ObservableProperty] private string _selectedFontEffect = string.Empty;
    [ObservableProperty] private Color _fontEffectColor;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private int _topBottomMargin;
    [ObservableProperty] private decimal _zPosition;
    [ObservableProperty] private int _fadeUpTime;
    [ObservableProperty] private int _fadeDownTime;

    public Window? Window { get; set; }

    public Subtitle Subtitle { get; set; } = new();

    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    public DCinemaInteropPropertiesViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        // Interop's <Language> element holds the English culture name, not an ISO code
        var languages = CultureInfo.GetCultures(CultureTypes.NeutralCultures)
            .Select(x => x.EnglishName)
            .OrderBy(l => l)
            .ToList();
        _languages = new ObservableCollection<string>(languages);

        _fontEffects = new ObservableCollection<string> { "None", "Border", "Shadow" };
    }

    private void LoadSettings()
    {
        var ss = Se.Settings.File.DCinemaSmpte;

        GenerateIdAuto = ss.DCinemaAutoGenerateSubtitleId;

        SubtitleId = !string.IsNullOrEmpty(ss.CurrentDCinemaSubtitleId)
            ? ss.CurrentDCinemaSubtitleId
            : DCinemaInterop.GenerateId();

        ReelNumber = int.TryParse(ss.CurrentDCinemaReelNumber, out int reelNumber) && reelNumber > 0
            ? reelNumber
            : 1;

        MovieTitle = ss.CurrentDCinemaMovieTitle ?? string.Empty;
        SelectedLanguage = !string.IsNullOrEmpty(ss.CurrentDCinemaLanguage)
            ? ss.CurrentDCinemaLanguage
            : "English";

        FontId = !string.IsNullOrEmpty(ss.CurrentDCinemaFontId)
            ? ss.CurrentDCinemaFontId
            : "Font1";

        FontUri = !string.IsNullOrEmpty(ss.CurrentDCinemaFontUri)
            ? ss.CurrentDCinemaFontUri
            : ss.DCinemaFontFile;

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
            : ss.DCinemaFontSize > 0 ? ss.DCinemaFontSize : 42;

        TopBottomMargin = ss.DCinemaBottomMargin > 0 ? ss.DCinemaBottomMargin : 8;
        ZPosition = (decimal)ss.DCinemaZPosition is >= -10 and <= 10 ? (decimal)ss.DCinemaZPosition : 0;
        FadeUpTime = ss.DCinemaFadeUpTime;
        FadeDownTime = ss.DCinemaFadeDownTime;
    }

    private void SaveSettings()
    {
        var ss = Se.Settings.File.DCinemaSmpte;

        ss.DCinemaAutoGenerateSubtitleId = GenerateIdAuto;

        ss.CurrentDCinemaSubtitleId = SubtitleId;
        ss.CurrentDCinemaMovieTitle = MovieTitle;
        ss.CurrentDCinemaReelNumber = ReelNumber.ToString();
        ss.CurrentDCinemaLanguage = SelectedLanguage;
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

        ss.DCinemaFontSize = FontSize;
        ss.DCinemaBottomMargin = TopBottomMargin;
        ss.DCinemaZPosition = (double)ZPosition;
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

    [RelayCommand]
    private void GenerateSubtitleId()
    {
        SubtitleId = DCinemaInterop.GenerateId();
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
            var importer = new DcPropertiesInterop();
            if (importer.Load(fileName))
            {
                GenerateIdAuto = Convert.ToBoolean(importer.GenerateIdAuto, CultureInfo.InvariantCulture);

                if (int.TryParse(importer.ReelNumber, out var reelNumber))
                {
                    ReelNumber = reelNumber;
                }

                SelectedLanguage = importer.Language ?? "English";
                FontId = importer.FontId ?? "Font1";
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

                if (decimal.TryParse(importer.ZPosition, NumberStyles.Any, CultureInfo.InvariantCulture, out var zPosition) &&
                    zPosition is >= -10 and <= 10)
                {
                    ZPosition = zPosition;
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
            var exporter = new DcPropertiesInterop
            {
                GenerateIdAuto = GenerateIdAuto.ToString(CultureInfo.InvariantCulture),
                ReelNumber = ReelNumber.ToString(CultureInfo.InvariantCulture),
                Language = SelectedLanguage,
                FontId = FontId,
                FontUri = FontUri,
                FontColor = ColorToString(FontColor),
                Effect = SelectedFontEffect,
                EffectColor = ColorToString(FontEffectColor),
                FontSize = FontSize.ToString(CultureInfo.InvariantCulture),
                TopBottomMargin = TopBottomMargin.ToString(CultureInfo.InvariantCulture),
                FadeUpTime = FadeUpTime.ToString(CultureInfo.InvariantCulture),
                FadeDownTime = FadeDownTime.ToString(CultureInfo.InvariantCulture),
                ZPosition = ZPosition.ToString(CultureInfo.InvariantCulture),
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

    public void Initialize(Subtitle subtitle, SubtitleFormat format)
    {
        Subtitle = subtitle;
        WindowTitle = string.Format(Se.Language.File.XProperties, format.Name);
        LoadSettings();
    }
}
