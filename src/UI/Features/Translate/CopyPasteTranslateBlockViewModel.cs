using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class CopyPasteTranslateBlockViewModel : ObservableObject
{

    [ObservableProperty] private string _windowTitle;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string TranslatedText { get; private set; }

    private string _sourceText;

    public CopyPasteTranslateBlockViewModel()
    {
        WindowTitle = string.Empty;
        _sourceText = string.Empty;
        TranslatedText = string.Empty;
    }

    internal void Initialize(int blockNumber, int totalBlockNumbers, string text)
    {
        _sourceText = text;
        WindowTitle = string.Format(Se.Language.Translate.BlockXOfY, blockNumber, totalBlockNumbers);
    }

    [RelayCommand]
    private async Task CopyFromClipboard()
    {
        if (Window == null || Window.Clipboard == null)
        {
            return;
        }

        TranslatedText = await ClipboardHelper.GetTextAsync(Window) ?? string.Empty;

        if (TranslatedText == string.Empty)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Translate.NoTextInClipboard,
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        if (TranslatedText == _sourceText)
        {
            await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.Translate.TextInClipboardIsSameAsSourceText,
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            return;
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task CopyToClipboard()
    {
        if (Window == null || Window.Clipboard == null)
        {
            return;
        }

        await ClipboardHelper.SetTextAsync(Window, _sourceText);
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

    internal void Loaded(object? sender, RoutedEventArgs e)
    {
        _ = CopyToClipboard();
    }
}