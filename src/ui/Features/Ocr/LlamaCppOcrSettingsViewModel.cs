using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
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

    public LlamaCppOcrSettingsViewModel()
    {
        _url = Se.Settings.Ocr.LlamaCppUrl ?? string.Empty;
        _prompt = Se.Settings.Ocr.LlamaCppOcrPrompt ?? string.Empty;
        _timeoutMinutes = Math.Max(1, Se.Settings.Ocr.LlamaCppOcrTimeoutMinutes);
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
