using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SettingsImportExport;

public partial class SettingsImportExportViewModel : ObservableObject
{
    public string TitleText { get; set; }
    [ObservableProperty] private bool _exportImportAll;
    [ObservableProperty] private bool _exportImportRecentFiles;
    [ObservableProperty] private bool _exportImportRules;
    [ObservableProperty] private bool _exportImportAppearance;
    [ObservableProperty] private bool _exportImportAutoTranslate;
    [ObservableProperty] private bool _exportImportWaveform;
    [ObservableProperty] private bool _exportImportSyntaxColoring;
    [ObservableProperty] private bool _exportImportShortcuts;

    private bool _isRulesEnabled = true;
    public bool IsRulesEnabled
    {
        get => _isRulesEnabled;
        set
        {
            if (SetProperty(ref _isRulesEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditRules));
            }
        }
    }

    private bool _isAppearanceEnabled = true;
    public bool IsAppearanceEnabled
    {
        get => _isAppearanceEnabled;
        set
        {
            if (SetProperty(ref _isAppearanceEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditAppearance));
            }
        }
    }

    private bool _isAutoTranslateEnabled = true;
    public bool IsAutoTranslateEnabled
    {
        get => _isAutoTranslateEnabled;
        set
        {
            if (SetProperty(ref _isAutoTranslateEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditAutoTranslate));
            }
        }
    }

    private bool _isWaveformEnabled = true;
    public bool IsWaveformEnabled
    {
        get => _isWaveformEnabled;
        set
        {
            if (SetProperty(ref _isWaveformEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditWaveform));
            }
        }
    }

    private bool _isShortcutsEnabled = true;
    public bool IsShortcutsEnabled
    {
        get => _isShortcutsEnabled;
        set
        {
            if (SetProperty(ref _isShortcutsEnabled, value))
            {
                OnPropertyChanged(nameof(CanEditShortcuts));
            }
        }
    }

    public bool CanEditRules => !ExportImportAll && _isRulesEnabled;
    public bool CanEditAppearance => !ExportImportAll && _isAppearanceEnabled;
    public bool CanEditAutoTranslate => !ExportImportAll && _isAutoTranslateEnabled;
    public bool CanEditWaveform => !ExportImportAll && _isWaveformEnabled;
    public bool CanEditShortcuts => !ExportImportAll && _isShortcutsEnabled;

    private bool _isExport;
    private string _importFilePath = string.Empty;
    private Se? _importData;
    public bool OkPressed { get; set; }
    public Window? Window { get; set; }
    private readonly IFileHelper _fileHelper;

    public SettingsImportExportViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        TitleText = Se.Language.General.ImportDotDotDot;
        ExportImportAll = true;
    }

    partial void OnExportImportAllChanged(bool value)
    {
        OnPropertyChanged(nameof(CanEditRules));
        OnPropertyChanged(nameof(CanEditAppearance));
        OnPropertyChanged(nameof(CanEditAutoTranslate));
        OnPropertyChanged(nameof(CanEditWaveform));
        OnPropertyChanged(nameof(CanEditShortcuts));
    }

    public void SetIsExport(bool isExport)
    {
        _isExport = isExport;
        TitleText = isExport ? Se.Language.General.ExportDotDotDot : Se.Language.General.ImportDotDotDot;
    }


    public async Task<bool> PromptAndLoadImportFile()
    {
        if (Window == null)
        {
            return false;
        }

        var fileName = await _fileHelper.PickOpenFile(
            Window,
            Se.Language.General.ImportDotDotDot,
            "JSON files",
            ".json");

        if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
        {
            return false;
        }

        _importFilePath = fileName;

        try
        {
            var json = File.ReadAllText(_importFilePath);
            _importData = JsonSerializer.Deserialize<Se>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            });

            if (_importData == null)
            {
                return false;
            }

            IsRulesEnabled = _importData.General != null;
            IsAppearanceEnabled = _importData.Appearance != null;
            IsAutoTranslateEnabled = _importData.AutoTranslate != null;
            IsWaveformEnabled = _importData.Waveform != null;
            IsShortcutsEnabled = _importData.Shortcuts != null;

            if (!IsRulesEnabled)
            {
                ExportImportRules = false;
            }

            if (!IsAppearanceEnabled)
            {
                ExportImportAppearance = false;
            }

            if (!IsAutoTranslateEnabled)
            {
                ExportImportAutoTranslate = false;
            }

            if (!IsWaveformEnabled)
            {
                ExportImportWaveform = false;
            }

            if (!IsShortcutsEnabled)
            {
                ExportImportShortcuts = false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (_isExport)
        {
            await ExportSettings();
        }
        else
        {
            ImportSettings();
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private async Task ExportSettings()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(
            Window,
            ".json",
            "Settings.json",
            Se.Language.General.ExportDotDotDot);

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var exportData = new Se();
        var currentSettings = Se.Settings;

        if (ExportImportAll || ExportImportRules)
        {
            exportData.General = currentSettings.General;
        }

        if (ExportImportAll || ExportImportWaveform)
        {
            exportData.Waveform = currentSettings.Waveform;
        }

        if (ExportImportAll)
        {
            exportData.Tools = currentSettings.Tools;
        }

        if (ExportImportAll || ExportImportAppearance)
        {
            exportData.Appearance = currentSettings.Appearance;
        }

        if (ExportImportAll)
        {
            exportData.Options = currentSettings.Options;
        }

        if (ExportImportAll || ExportImportShortcuts)
        {
            exportData.Shortcuts = currentSettings.Shortcuts;
        }

        if (ExportImportAll || ExportImportAutoTranslate)
        {
            exportData.AutoTranslate = currentSettings.AutoTranslate;
        }

        if (ExportImportAll)
        {
            exportData.SpellCheck = currentSettings.SpellCheck;
        }

        var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fileName, json);
    }

    private void ImportSettings()
    {
        if (_importData == null)
        {
            return;
        }

        var importData = _importData;

        if (ExportImportAll || ExportImportRules)
        {
            if (importData.General != null)
            {
                Se.Settings.General = importData.General;
            }
        }

        if (ExportImportAll || ExportImportWaveform)
        {
            if (importData.Waveform != null)
            {
                Se.Settings.Waveform = importData.Waveform;
            }
        }

        if (ExportImportAll)
        {
            if (importData.Video != null)
            {
                Se.Settings.Video = importData.Video;
            }

            if (importData.Tools != null)
            {
                Se.Settings.Tools = importData.Tools;
            }

            if (importData.Options != null)
            {
                Se.Settings.Options = importData.Options;
            }

            if (importData.SpellCheck != null)
            {
                Se.Settings.SpellCheck = importData.SpellCheck;
            }
        }

        if (ExportImportAll || ExportImportAppearance)
        {
            if (importData.Appearance != null)
            {
                Se.Settings.Appearance = importData.Appearance;
            }
        }

        if (ExportImportAll || ExportImportShortcuts)
        {
            if (importData.Shortcuts != null)
            {
                Se.Settings.Shortcuts = importData.Shortcuts;
            }
        }

        if (ExportImportAll || ExportImportAutoTranslate)
        {
            if (importData.AutoTranslate != null)
            {
                Se.Settings.AutoTranslate = importData.AutoTranslate;
            }
        }

        Se.SaveSettings();
    }

    public async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (Window != null)
        {
            UiUtil.RestoreWindowPosition(Window);
        }

        if (!_isExport)
        {
            var loaded = await PromptAndLoadImportFile();
            if (!loaded)
            {
                Window?.Close();
            }
        }
    }
}

