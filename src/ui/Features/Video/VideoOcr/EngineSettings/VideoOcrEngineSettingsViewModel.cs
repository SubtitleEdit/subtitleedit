using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Ocr.Download;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr.EngineSettings;

/// <summary>
/// Engine info/settings dialog for the video OCR engines that have no dialog of their own
/// (CrispEmbed and llama.cpp open their existing dialogs instead): what the engine is, whether
/// it is installed, where it lives, its request timeout, plus re-download and open-folder.
/// </summary>
public partial class VideoOcrEngineSettingsViewModel : ObservableObject
{
    private readonly IFolderHelper _folderHelper;

    private VideoOcrEngineItem? _engine;
    private Func<Task>? _redownloadAsync;

    [ObservableProperty] private string _titleText = string.Empty;
    [ObservableProperty] private string _subtitleText = string.Empty;
    [ObservableProperty] private string _backendLabel = string.Empty;
    [ObservableProperty] private string _statusLabel = string.Empty;
    [ObservableProperty] private IBrush _statusBrush = Brushes.Gray;
    [ObservableProperty] private string _installFolder = string.Empty;
    [ObservableProperty] private bool _hasInstallFolder;
    [ObservableProperty] private bool _canRedownload;
    [ObservableProperty] private bool _hasTimeout;
    [ObservableProperty] private int _timeoutMinutes = 5;
    [ObservableProperty] private string _websiteUrl = string.Empty;
    [ObservableProperty] private bool _hasWebsite;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonLabel))]
    private bool _isInstalled;

    public string DownloadButtonLabel => IsInstalled ? Se.Language.General.Redownload : Se.Language.General.Download;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public VideoOcrEngineSettingsViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
    }

    public void Initialize(VideoOcrEngineItem engine, Func<Task>? redownloadAsync)
    {
        _engine = engine;
        _redownloadAsync = redownloadAsync;

        TitleText = engine.Name;
        SubtitleText = engine.Description;

        switch (engine.EngineType)
        {
            case OcrEngineType.PaddleOcrStandalone:
                HasInstallFolder = true;
                InstallFolder = Se.PaddleOcrFolder;
                CanRedownload = redownloadAsync != null;
                HasWebsite = true;
                WebsiteUrl = "https://github.com/PaddlePaddle/PaddleOCR";
                BackendLabel = Se.Settings.Ocr.PaddleOcrMode == "server"
                    ? "PaddleOCR (server models)"
                    : "PaddleOCR (mobile models)";
                break;
            case OcrEngineType.Ollama:
                HasTimeout = true;
                TimeoutMinutes = Math.Max(1, Se.Settings.Ocr.OllamaOcrTimeoutMinutes);
                HasWebsite = true;
                WebsiteUrl = "https://ollama.com";
                BackendLabel = Se.Language.Video.VideoOcr.EngineExternalServer;
                break;
            case OcrEngineType.Glm:
                HasWebsite = true;
                WebsiteUrl = "https://docs.z.ai";
                BackendLabel = Se.Language.Video.VideoOcr.EngineCloudApi;
                break;
            case OcrEngineType.AppleVision:
                HasWebsite = true;
                WebsiteUrl = "https://developer.apple.com/documentation/vision";
                BackendLabel = Se.Language.Video.VideoOcr.EngineBuiltIn;
                break;
            default:
                BackendLabel = engine.Name;
                break;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (_engine == null)
        {
            return;
        }

        switch (_engine.EngineType)
        {
            case OcrEngineType.PaddleOcrStandalone:
                // Engine binary plus the models folder - the same test the combo's status dot uses.
                IsInstalled = PaddleOcrInstallHelper.IsStandaloneInstalled();
                if (IsInstalled)
                {
                    SetStatus(Se.Language.General.Installed, 0x4C, 0xAF, 0x50); // green
                }
                else if (PaddleOcrInstallHelper.IsStandaloneEngineInstalled() && !Directory.Exists(Se.PaddleOcrModelsFolder))
                {
                    SetStatus(Se.Language.Video.VideoOcr.EngineModelsMissing, 0xFF, 0x98, 0x00); // amber
                }
                else
                {
                    SetStatus(Se.Language.General.NotInstalled, 0xF4, 0x43, 0x36); // red
                }

                break;

            case OcrEngineType.AppleVision:
                IsInstalled = true;
                SetStatus(Se.Language.General.Installed, 0x4C, 0xAF, 0x50);
                break;

            default:
                // Ollama and the GLM API: nothing SE installs, so there is no install state to
                // report - only whether the request would go to a server we can name.
                IsInstalled = true;
                SetStatus(Se.Language.Video.VideoOcr.EngineNothingToInstall, 0x9E, 0x9E, 0x9E); // grey
                break;
        }
    }

    private void SetStatus(string label, byte r, byte g, byte b)
    {
        StatusLabel = label;
        StatusBrush = new SolidColorBrush(Color.FromRgb(r, g, b));
    }

    [RelayCommand]
    private async Task Redownload()
    {
        if (_redownloadAsync == null)
        {
            return;
        }

        await _redownloadAsync();
        Refresh();
    }

    [RelayCommand]
    private async Task OpenFolder()
    {
        if (Window == null || string.IsNullOrEmpty(InstallFolder))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(InstallFolder);
            await _folderHelper.OpenFolder(Window, InstallFolder);
        }
        catch
        {
            // best-effort
        }
    }

    [RelayCommand]
    private void OpenWebsite()
    {
        if (!string.IsNullOrEmpty(WebsiteUrl))
        {
            UiUtil.OpenUrl(WebsiteUrl);
        }
    }

    [RelayCommand]
    private void Ok()
    {
        if (HasTimeout && _engine?.EngineType == OcrEngineType.Ollama)
        {
            Se.Settings.Ocr.OllamaOcrTimeoutMinutes = Math.Max(1, TimeoutMinutes);
        }

        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/video-ocr");
        }
    }
}
