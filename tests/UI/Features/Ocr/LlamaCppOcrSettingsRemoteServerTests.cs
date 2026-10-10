using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Ocr;

namespace UITests.Features.Ocr;

/// <summary>
/// The llama.cpp OCR settings had a URL box that was never used: a curated model was always
/// selected, so OCR always started the bundled llama-server (#15854). The "use external server"
/// checkbox now decides whether the URL is used.
/// </summary>
public class LlamaCppOcrSettingsRemoteServerTests
{
    private static SettingsScope OcrScope() => new(
        "Ocr.LlamaCppUseRemoteServer",
        "Ocr.LlamaCppUrl",
        "Ocr.LlamaCppOcrPrompt",
        "Ocr.LlamaCppOcrTimeoutMinutes");

    [AvaloniaFact]
    public void UrlTextBox_IsEnabledOnlyWithRemoteServer()
    {
        using var _ = OcrScope();
        Se.Settings.Ocr.LlamaCppUseRemoteServer = false;
        Se.Settings.Ocr.LlamaCppUrl = "http://192.168.1.10:8080/v1/chat/completions";

        var vm = new LlamaCppOcrSettingsViewModel();
        var window = new LlamaCppOcrSettingsWindow(vm);
        try
        {
            var urlTextBox = Assert.Single(window.GetLogicalDescendants().OfType<TextBox>(),
                t => t.Text == Se.Settings.Ocr.LlamaCppUrl);
            Assert.False(urlTextBox.IsEnabled);

            vm.UseRemoteServer = true;
            Assert.True(urlTextBox.IsEnabled);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Ok_PersistsRemoteServerAndTrimmedUrl()
    {
        using var _ = OcrScope();
        Se.Settings.Ocr.LlamaCppUseRemoteServer = false;
        Se.Settings.Ocr.LlamaCppOcrPrompt = SeOcrDefaults.LlamaCppOcrPrompt;

        var vm = new LlamaCppOcrSettingsViewModel
        {
            UseRemoteServer = true,
            Url = "  http://192.168.1.10:8080/v1/chat/completions ",
        };
        await vm.OkCommand.ExecuteAsync(null);

        Assert.True(vm.OkPressed);
        Assert.True(Se.Settings.Ocr.LlamaCppUseRemoteServer);
        Assert.Equal("http://192.168.1.10:8080/v1/chat/completions", Se.Settings.Ocr.LlamaCppUrl);
    }

    [AvaloniaFact]
    public async Task Ok_WithRemoteServerAndEmptyUrl_IsRejected()
    {
        using var _ = OcrScope();
        Se.Settings.Ocr.LlamaCppUseRemoteServer = false;
        Se.Settings.Ocr.LlamaCppOcrPrompt = SeOcrDefaults.LlamaCppOcrPrompt;

        var vm = new LlamaCppOcrSettingsViewModel
        {
            UseRemoteServer = true,
            Url = "   ",
        };
        await vm.OkCommand.ExecuteAsync(null);

        Assert.False(vm.OkPressed);
        Assert.False(Se.Settings.Ocr.LlamaCppUseRemoteServer);
    }
}
