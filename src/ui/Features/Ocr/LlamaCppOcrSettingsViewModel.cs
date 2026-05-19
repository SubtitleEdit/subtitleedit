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

namespace Nikse.SubtitleEdit.Features.Ocr;

public partial class LlamaCppOcrSettingsViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    [ObservableProperty] private string _url;
    [ObservableProperty] private string _prompt;
    [ObservableProperty] private int _timeoutMinutes;
    [ObservableProperty] private string _engineLabel = string.Empty;
    [ObservableProperty] private IBrush _engineBrush = Brushes.Gray;
    [ObservableProperty] private bool _isEngineInstalled;

    public LlamaCppOcrSettingsViewModel()
    {
        _url = Se.Settings.Ocr.LlamaCppUrl ?? string.Empty;
        _prompt = Se.Settings.Ocr.LlamaCppOcrPrompt ?? string.Empty;
        _timeoutMinutes = Math.Max(1, Se.Settings.Ocr.LlamaCppOcrTimeoutMinutes);
    }

    public void Initialize()
    {
        Refresh();
    }

    private void Refresh()
    {
        IsEngineInstalled = LlamaCppServerManager.IsEngineInstalled();
        if (!IsEngineInstalled)
        {
            EngineLabel = "Not installed";
            EngineBrush = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)); // grey
            return;
        }

        switch (LlamaCppServerManager.GetEngineUpdateStatus())
        {
            case DownloadHashManager.UpdateStatus.UpToDate:
                EngineLabel = "Installed (latest)";
                EngineBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // green
                break;
            case DownloadHashManager.UpdateStatus.UpdateAvailable:
                EngineLabel = "Update available";
                EngineBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // amber
                break;
            default:
                EngineLabel = "Installed (unknown version)";
                EngineBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // green
                break;
        }
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
