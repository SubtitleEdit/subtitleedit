using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformThemes;

public partial class WaveformThemesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<WaveformThemeDisplay> _themes;
    [ObservableProperty] private WaveformThemeDisplay? _selectedTheme;
    [ObservableProperty] private Color _textColor;
    [ObservableProperty] private Color _waveformColor;
    [ObservableProperty] private Color _backgroundColor;
    [ObservableProperty] private Color _selectedColor;
    [ObservableProperty] private Color _cursorColor;
    [ObservableProperty] private Color _shotChangeColor;
    [ObservableProperty] private Color _paragraphBackgroundColor;
    [ObservableProperty] private Color _paragraphSelectedBackgroundColor;
    [ObservableProperty] private Color _paragraphLeftColor;
    [ObservableProperty] private Color _paragraphRightColor;
    [ObservableProperty] private Color _fancyHighColor;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private bool _applyingTheme;
    private int _customThemeCount;

    public WaveformThemesViewModel()
    {
        _themes = new ObservableCollection<WaveformThemeDisplay>();
    }

    public void Initialize(
        Color textColor, Color waveformColor, Color backgroundColor,
        Color selectedColor, Color cursorColor, Color shotChangeColor,
        Color paragraphBackgroundColor, Color paragraphSelectedBackgroundColor,
        Color paragraphLeftColor, Color paragraphRightColor, Color fancyHighColor)
    {
        Themes.Clear();
        Themes.Add(MakeDarkTheme());
        Themes.Add(MakeLightTheme());
        Themes.Add(MakeHighContrastTheme());
        Themes.Add(MakeOceanBlueTheme());
        Themes.Add(MakeWarmSunsetTheme());
        Themes.Add(MakeForestTheme());
        Themes.Add(MakeMidnightPurpleTheme());
        Themes.Add(MakeRetroAmberTheme());
        Themes.Add(MakeIceTheme());
        Themes.Add(MakeSolarizedDarkTheme());
        Themes.Add(new WaveformThemeDisplay
        {
            Name = "Custom",
            TextColor = textColor,
            WaveformColor = waveformColor,
            BackgroundColor = backgroundColor,
            SelectedColor = selectedColor,
            CursorColor = cursorColor,
            ShotChangeColor = shotChangeColor,
            ParagraphBackgroundColor = paragraphBackgroundColor,
            ParagraphSelectedBackgroundColor = paragraphSelectedBackgroundColor,
            ParagraphLeftColor = paragraphLeftColor,
            ParagraphRightColor = paragraphRightColor,
            FancyHighColor = fancyHighColor,
        });

        _applyingTheme = true;
        try
        {
            TextColor = textColor;
            WaveformColor = waveformColor;
            BackgroundColor = backgroundColor;
            SelectedColor = selectedColor;
            CursorColor = cursorColor;
            ShotChangeColor = shotChangeColor;
            ParagraphBackgroundColor = paragraphBackgroundColor;
            ParagraphSelectedBackgroundColor = paragraphSelectedBackgroundColor;
            ParagraphLeftColor = paragraphLeftColor;
            ParagraphRightColor = paragraphRightColor;
            FancyHighColor = fancyHighColor;
        }
        finally
        {
            _applyingTheme = false;
        }

        SelectedTheme = Themes.Last();
    }

    partial void OnSelectedThemeChanged(WaveformThemeDisplay? value)
    {
        if (value == null) return;

        _applyingTheme = true;
        try
        {
            TextColor = value.TextColor;
            WaveformColor = value.WaveformColor;
            BackgroundColor = value.BackgroundColor;
            SelectedColor = value.SelectedColor;
            CursorColor = value.CursorColor;
            ShotChangeColor = value.ShotChangeColor;
            ParagraphBackgroundColor = value.ParagraphBackgroundColor;
            ParagraphSelectedBackgroundColor = value.ParagraphSelectedBackgroundColor;
            ParagraphLeftColor = value.ParagraphLeftColor;
            ParagraphRightColor = value.ParagraphRightColor;
            FancyHighColor = value.FancyHighColor;
        }
        finally
        {
            _applyingTheme = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsCustomTheme()
    {
        if (Window == null) return;

        _customThemeCount++;
        var vm = new PromptTextBoxViewModel();
        vm.Initialize("Save custom theme", $"Custom {_customThemeCount}", 200, 30, returnSubmits: true);
        var promptWindow = new PromptTextBoxWindow(vm);
        await promptWindow.ShowDialog(Window);

        if (!vm.OkPressed || string.IsNullOrWhiteSpace(vm.Text))
        {
            _customThemeCount--;
            return;
        }

        var theme = new WaveformThemeDisplay
        {
            Name = vm.Text.Trim(),
            TextColor = TextColor,
            WaveformColor = WaveformColor,
            BackgroundColor = BackgroundColor,
            SelectedColor = SelectedColor,
            CursorColor = CursorColor,
            ShotChangeColor = ShotChangeColor,
            ParagraphBackgroundColor = ParagraphBackgroundColor,
            ParagraphSelectedBackgroundColor = ParagraphSelectedBackgroundColor,
            ParagraphLeftColor = ParagraphLeftColor,
            ParagraphRightColor = ParagraphRightColor,
            FancyHighColor = FancyHighColor,
        };

        Themes.Add(theme);
        SelectedTheme = theme;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task LoadTheme()
    {
        if (Window == null) return;

        var topLevel = TopLevel.GetTopLevel(Window);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Se.Language.Options.Settings.WaveformLoadThemeDotDotDot,
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Subtitle Edit theme (*.seWaveformTheme)")
                {
                    Patterns = new List<string> { "*.seWaveformTheme" }
                },
            },
        });

        if (files.Count == 0) return;

        var path = files[0].Path.LocalPath;
        if (!File.Exists(path)) return;

        try
        {
            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<WaveformThemeDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });
            if (dto == null) return;

            var theme = dto.ToThemeDisplay(Path.GetFileNameWithoutExtension(path));
            Themes.Add(theme);
            SelectedTheme = theme;
        }
        catch
        {
            // Silently ignore malformed files
        }
    }

    [RelayCommand]
    private async Task ExportTheme()
    {
        if (Window == null) return;

        var topLevel = TopLevel.GetTopLevel(Window);
        if (topLevel == null) return;

        var suggestedName = (SelectedTheme?.Name ?? "custom").Replace(' ', '_');

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Se.Language.Options.Settings.WaveformExportThemeDotDotDot,
            SuggestedFileName = suggestedName,
            DefaultExtension = "seTheme",
            FileTypeChoices = new List<FilePickerFileType>
            {
                new FilePickerFileType("Subtitle Edit theme (*.seWaveformTheme)")
                {
                    Patterns = new List<string> { "*.seWaveformTheme" }
                },
            },
        });

        if (file == null) return;

        var dto = WaveformThemeDto.FromColors(
            name: SelectedTheme?.Name ?? suggestedName,
            textColor: TextColor,
            waveformColor: WaveformColor,
            backgroundColor: BackgroundColor,
            selectedColor: SelectedColor,
            cursorColor: CursorColor,
            shotChangeColor: ShotChangeColor,
            paragraphBackgroundColor: ParagraphBackgroundColor,
            paragraphSelectedBackgroundColor: ParagraphSelectedBackgroundColor,
            paragraphLeftColor: ParagraphLeftColor,
            paragraphRightColor: ParagraphRightColor,
            fancyHighColor: FancyHighColor);

        try
        {
            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file.Path.LocalPath, json);
        }
        catch
        {
            // Silently ignore write errors
        }
    }

    private static WaveformThemeDisplay MakeDarkTheme() => new()
    {
        Name = "Dark",
        TextColor = Colors.White,
        WaveformColor = Color.FromArgb(255, 0, 70, 0),
        BackgroundColor = Color.FromArgb(255, 0, 0, 0),
        SelectedColor = Color.FromArgb(150, 0, 120, 255),
        CursorColor = Colors.Cyan,
        ShotChangeColor = Colors.AntiqueWhite,
        ParagraphBackgroundColor = Color.FromArgb(90, 70, 70, 70),
        ParagraphSelectedBackgroundColor = Color.FromArgb(90, 70, 70, 120),
        ParagraphLeftColor = Color.FromArgb(90, 0, 255, 0),
        ParagraphRightColor = Color.FromArgb(90, 255, 0, 0),
        FancyHighColor = Colors.Orange,
    };

    private static WaveformThemeDisplay MakeLightTheme() => new()
    {
        Name = "Light",
        TextColor = Colors.Black,
        WaveformColor = Color.FromArgb(255, 0, 80, 160),
        BackgroundColor = Color.FromArgb(255, 240, 240, 240),
        SelectedColor = Color.FromArgb(120, 0, 100, 220),
        CursorColor = Colors.DarkBlue,
        ShotChangeColor = Colors.DarkSlateGray,
        ParagraphBackgroundColor = Color.FromArgb(90, 180, 180, 180),
        ParagraphSelectedBackgroundColor = Color.FromArgb(90, 140, 140, 200),
        ParagraphLeftColor = Color.FromArgb(90, 0, 160, 0),
        ParagraphRightColor = Color.FromArgb(90, 200, 0, 0),
        FancyHighColor = Colors.DarkOrange,
    };

    private static WaveformThemeDisplay MakeHighContrastTheme() => new()
    {
        Name = "High Contrast",
        TextColor = Colors.Yellow,
        WaveformColor = Color.FromArgb(255, 0, 255, 0),
        BackgroundColor = Color.FromArgb(255, 0, 0, 0),
        SelectedColor = Color.FromArgb(150, 255, 255, 0),
        CursorColor = Colors.White,
        ShotChangeColor = Colors.Red,
        ParagraphBackgroundColor = Color.FromArgb(90, 80, 80, 0),
        ParagraphSelectedBackgroundColor = Color.FromArgb(90, 0, 80, 80),
        ParagraphLeftColor = Color.FromArgb(90, 0, 255, 255),
        ParagraphRightColor = Color.FromArgb(90, 255, 0, 255),
        FancyHighColor = Colors.Yellow,
    };

    private static WaveformThemeDisplay MakeOceanBlueTheme() => new()
    {
        Name = "Ocean Blue",
        TextColor = Colors.White,
        WaveformColor = Color.FromArgb(255, 0, 120, 200),
        BackgroundColor = Color.FromArgb(255, 0, 20, 50),
        SelectedColor = Color.FromArgb(150, 0, 200, 255),
        CursorColor = Colors.LightCyan,
        ShotChangeColor = Colors.Yellow,
        ParagraphBackgroundColor = Color.FromArgb(90, 0, 60, 120),
        ParagraphSelectedBackgroundColor = Color.FromArgb(90, 0, 80, 160),
        ParagraphLeftColor = Color.FromArgb(90, 0, 200, 0),
        ParagraphRightColor = Color.FromArgb(90, 200, 100, 0),
        FancyHighColor = Colors.Cyan,
    };

    private static WaveformThemeDisplay MakeWarmSunsetTheme() => new()
    {
        Name = "Warm Sunset",
        TextColor = Colors.White,
        WaveformColor = Color.FromArgb(255, 200, 100, 0),
        BackgroundColor = Color.FromArgb(255, 30, 10, 0),
        SelectedColor = Color.FromArgb(150, 255, 150, 0),
        CursorColor = Colors.Yellow,
        ShotChangeColor = Colors.White,
        ParagraphBackgroundColor = Color.FromArgb(90, 80, 40, 0),
        ParagraphSelectedBackgroundColor = Color.FromArgb(90, 120, 60, 0),
        ParagraphLeftColor = Color.FromArgb(90, 255, 200, 0),
        ParagraphRightColor = Color.FromArgb(90, 200, 50, 0),
        FancyHighColor = Colors.Orange,
    };

    private static WaveformThemeDisplay MakeForestTheme() => new()
    {
        Name = "Forest",
        TextColor = Color.FromArgb(255, 200, 255, 200),
        WaveformColor = Color.FromArgb(255, 50, 160, 50),
        BackgroundColor = Color.FromArgb(255, 10, 30, 10),
        SelectedColor = Color.FromArgb(140, 100, 220, 100),
        CursorColor = Color.FromArgb(255, 180, 255, 100),
        ShotChangeColor = Color.FromArgb(255, 255, 220, 100),
        ParagraphBackgroundColor = Color.FromArgb(80, 30, 80, 30),
        ParagraphSelectedBackgroundColor = Color.FromArgb(80, 60, 120, 60),
        ParagraphLeftColor = Color.FromArgb(100, 0, 200, 80),
        ParagraphRightColor = Color.FromArgb(100, 180, 120, 0),
        FancyHighColor = Color.FromArgb(255, 120, 255, 80),
    };

    private static WaveformThemeDisplay MakeMidnightPurpleTheme() => new()
    {
        Name = "Midnight Purple",
        TextColor = Color.FromArgb(255, 220, 200, 255),
        WaveformColor = Color.FromArgb(255, 140, 80, 220),
        BackgroundColor = Color.FromArgb(255, 12, 8, 30),
        SelectedColor = Color.FromArgb(140, 180, 100, 255),
        CursorColor = Color.FromArgb(255, 200, 160, 255),
        ShotChangeColor = Color.FromArgb(255, 255, 180, 80),
        ParagraphBackgroundColor = Color.FromArgb(80, 60, 30, 100),
        ParagraphSelectedBackgroundColor = Color.FromArgb(80, 90, 50, 150),
        ParagraphLeftColor = Color.FromArgb(100, 100, 60, 200),
        ParagraphRightColor = Color.FromArgb(100, 200, 60, 120),
        FancyHighColor = Color.FromArgb(255, 220, 120, 255),
    };

    private static WaveformThemeDisplay MakeRetroAmberTheme() => new()
    {
        Name = "Retro Amber",
        TextColor = Color.FromArgb(255, 255, 200, 80),
        WaveformColor = Color.FromArgb(255, 200, 140, 20),
        BackgroundColor = Color.FromArgb(255, 18, 12, 0),
        SelectedColor = Color.FromArgb(140, 255, 180, 40),
        CursorColor = Color.FromArgb(255, 255, 230, 120),
        ShotChangeColor = Color.FromArgb(255, 255, 80, 40),
        ParagraphBackgroundColor = Color.FromArgb(80, 80, 50, 0),
        ParagraphSelectedBackgroundColor = Color.FromArgb(80, 120, 80, 10),
        ParagraphLeftColor = Color.FromArgb(100, 200, 160, 0),
        ParagraphRightColor = Color.FromArgb(100, 200, 80, 0),
        FancyHighColor = Color.FromArgb(255, 255, 220, 60),
    };

    private static WaveformThemeDisplay MakeIceTheme() => new()
    {
        Name = "Ice",
        TextColor = Color.FromArgb(255, 200, 230, 255),
        WaveformColor = Color.FromArgb(255, 100, 180, 240),
        BackgroundColor = Color.FromArgb(255, 10, 20, 40),
        SelectedColor = Color.FromArgb(130, 160, 220, 255),
        CursorColor = Colors.White,
        ShotChangeColor = Color.FromArgb(255, 255, 240, 150),
        ParagraphBackgroundColor = Color.FromArgb(70, 60, 120, 180),
        ParagraphSelectedBackgroundColor = Color.FromArgb(70, 80, 150, 220),
        ParagraphLeftColor = Color.FromArgb(100, 100, 200, 255),
        ParagraphRightColor = Color.FromArgb(100, 200, 100, 180),
        FancyHighColor = Color.FromArgb(255, 180, 240, 255),
    };

    private static WaveformThemeDisplay MakeSolarizedDarkTheme() => new()
    {
        Name = "Solarized Dark",
        TextColor = Color.FromArgb(255, 131, 148, 150),
        WaveformColor = Color.FromArgb(255, 38, 139, 210),
        BackgroundColor = Color.FromArgb(255, 0, 43, 54),
        SelectedColor = Color.FromArgb(130, 42, 161, 152),
        CursorColor = Color.FromArgb(255, 181, 137, 0),
        ShotChangeColor = Color.FromArgb(255, 220, 50, 47),
        ParagraphBackgroundColor = Color.FromArgb(70, 7, 54, 66),
        ParagraphSelectedBackgroundColor = Color.FromArgb(70, 0, 70, 80),
        ParagraphLeftColor = Color.FromArgb(100, 133, 153, 0),
        ParagraphRightColor = Color.FromArgb(100, 211, 54, 130),
        FancyHighColor = Color.FromArgb(255, 203, 75, 22),
    };
}

/// <summary>
/// JSON-serializable DTO for a waveform theme. Stores colors as ARGB hex strings
/// (e.g. "#FF004600") to work around Avalonia.Media.Color having read-only properties.
/// </summary>
internal class WaveformThemeDto
{
    public string Name { get; set; } = string.Empty;
    public string TextColor { get; set; } = "#FFFFFFFF";
    public string WaveformColor { get; set; } = "#FF004600";
    public string BackgroundColor { get; set; } = "#FF000000";
    public string SelectedColor { get; set; } = "#960078FF";
    public string CursorColor { get; set; } = "#FF00FFFF";
    public string ShotChangeColor { get; set; } = "#FFFAEBD7";
    public string ParagraphBackgroundColor { get; set; } = "#5A464646";
    public string ParagraphSelectedBackgroundColor { get; set; } = "#5A464678";
    public string ParagraphLeftColor { get; set; } = "#5A00FF00";
    public string ParagraphRightColor { get; set; } = "#5AFF0000";
    public string FancyHighColor { get; set; } = "#FFFFA500";

    public static WaveformThemeDto FromColors(
        string name,
        Color textColor, Color waveformColor, Color backgroundColor,
        Color selectedColor, Color cursorColor, Color shotChangeColor,
        Color paragraphBackgroundColor, Color paragraphSelectedBackgroundColor,
        Color paragraphLeftColor, Color paragraphRightColor, Color fancyHighColor) => new()
    {
        Name = name,
        TextColor = textColor.FromColorToHex(),
        WaveformColor = waveformColor.FromColorToHex(),
        BackgroundColor = backgroundColor.FromColorToHex(),
        SelectedColor = selectedColor.FromColorToHex(),
        CursorColor = cursorColor.FromColorToHex(),
        ShotChangeColor = shotChangeColor.FromColorToHex(),
        ParagraphBackgroundColor = paragraphBackgroundColor.FromColorToHex(),
        ParagraphSelectedBackgroundColor = paragraphSelectedBackgroundColor.FromColorToHex(),
        ParagraphLeftColor = paragraphLeftColor.FromColorToHex(),
        ParagraphRightColor = paragraphRightColor.FromColorToHex(),
        FancyHighColor = fancyHighColor.FromColorToHex(),
    };

    public WaveformThemeDisplay ToThemeDisplay(string fallbackName) => new()
    {
        Name = string.IsNullOrWhiteSpace(Name) ? fallbackName : Name,
        TextColor = ParseColor(TextColor),
        WaveformColor = ParseColor(WaveformColor),
        BackgroundColor = ParseColor(BackgroundColor),
        SelectedColor = ParseColor(SelectedColor),
        CursorColor = ParseColor(CursorColor),
        ShotChangeColor = ParseColor(ShotChangeColor),
        ParagraphBackgroundColor = ParseColor(ParagraphBackgroundColor),
        ParagraphSelectedBackgroundColor = ParseColor(ParagraphSelectedBackgroundColor),
        ParagraphLeftColor = ParseColor(ParagraphLeftColor),
        ParagraphRightColor = ParseColor(ParagraphRightColor),
        FancyHighColor = ParseColor(FancyHighColor),
    };

    private static Color ParseColor(string hex)
    {
        try { return hex.FromHexToColor(); }
        catch { return Colors.White; }
    }
}
