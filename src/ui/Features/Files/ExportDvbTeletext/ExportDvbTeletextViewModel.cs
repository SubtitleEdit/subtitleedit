using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Files.ExportDvbTeletext;

public partial class ExportDvbTeletextViewModel : ObservableObject
{
    // 888 is the page subtitles are transmitted on in most of Europe.
    [ObservableProperty] private int _pageNumber = 888;
    [ObservableProperty] private string _languageCode;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public ExportDvbTeletextViewModel()
    {
        LanguageCode = "eng";
    }

    public void Initialize(int pageNumber, string languageCode)
    {
        PageNumber = pageNumber;
        LanguageCode = languageCode;
    }

    [RelayCommand]
    private void Ok()
    {
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
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}
