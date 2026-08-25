using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using System;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class LlamaCppOcrSettingsViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private Func<Task>? _redownloadAsync;

    [ObservableProperty] private string _url;
    [ObservableProperty] private string _prompt;
    [ObservableProperty] private int _timeoutMinutes;
    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Brushes.Gray;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonLabel))]
    private bool _isEngineInstalled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonLabel))]
    private DownloadHashManager.UpdateStatus _engineUpdateStatus;

    // "Download" when not installed, "Update" when a newer engine release is available,
    // otherwise "Re-download".
    public string DownloadButtonLabel
    {
        get
        {
            if (!IsEngineInstalled)
            {
                return Se.Language.General.Download;
            }

            return EngineUpdateStatus == DownloadHashManager.UpdateStatus.UpdateAvailable
                ? Se.Language.General.Update
                : Se.Language.General.Redownload;
        }
    }

    public LlamaCppOcrSettingsViewModel()
    {
        _url = Se.Settings.Ocr.LlamaCppUrl ?? string.Empty;
        _prompt = Se.Settings.Ocr.LlamaCppOcrPrompt ?? string.Empty;
        _timeoutMinutes = Math.Max(1, Se.Settings.Ocr.LlamaCppOcrTimeoutMinutes);
    }

    /// <summary>
    /// <paramref name="redownloadAsync"/> is supplied by the caller so the download runs through the
    /// same flow the caller already owns (stopping the server first, refreshing its model list and
    /// status dots afterwards) instead of this dialog duplicating it.
    /// </summary>
    public void Initialize(Func<Task> redownloadAsync)
    {
        _redownloadAsync = redownloadAsync;
        Refresh();
    }

    private void Refresh()
    {
        IsEngineInstalled = LlamaCppServerManager.IsEngineInstalled();
        if (!IsEngineInstalled)
        {
            EngineUpdateStatus = DownloadHashManager.UpdateStatus.Unknown;
            EngineLabel = Se.Language.General.NotInstalled;
            EngineBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)); // red
            return;
        }

        EngineUpdateStatus = LlamaCppUpdateStatus.GetEngineUpdateStatus();
        switch (EngineUpdateStatus)
        {
            case DownloadHashManager.UpdateStatus.UpToDate:
                EngineLabel = Se.Language.General.UpToDate;
                EngineBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // green
                break;
            case DownloadHashManager.UpdateStatus.UpdateAvailable:
                EngineLabel = Se.Language.General.UpdateAvailable;
                EngineBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // amber
                break;
            default:
                // Installed, but nothing identifies the build: an install predating the
                // .installed.sha256 sidecar, a manual install, or a build older than the ones
                // SE has hashes for. It still works, so say "Installed" rather than something
                // that reads as a failed install (#14057).
                EngineLabel = Se.Language.General.Installed;
                EngineBrush = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)); // grey
                break;
        }
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
    private async Task Ok()
    {
        if (string.IsNullOrWhiteSpace(Prompt))
        {
            await ShowPromptError(Se.Language.Ocr.LlamaCppOcrPromptEmpty);
            return;
        }

        if (!Prompt.Contains("{language}"))
        {
            await ShowPromptError(Se.Language.Ocr.LlamaCppOcrPromptMissingLanguagePlaceholder);
            return;
        }

        Se.Settings.Ocr.LlamaCppUrl = Url ?? string.Empty;
        Se.Settings.Ocr.LlamaCppOcrPrompt = Prompt;
        Se.Settings.Ocr.LlamaCppOcrTimeoutMinutes = Math.Max(1, TimeoutMinutes);
        OkPressed = true;
        Close();
    }

    private async Task ShowPromptError(string message)
    {
        if (Window == null)
        {
            return;
        }

        await MessageBox.Show(
            Window,
            Se.Language.General.Error,
            message,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
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
            Close();
        }
    }
}
