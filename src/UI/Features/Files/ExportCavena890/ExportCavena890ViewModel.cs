using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Files.ExportCavena890;

public partial class ExportCavena890ViewModel : ObservableObject
{
    [ObservableProperty] private string _translatedTitle;
    [ObservableProperty] private string _originalTitle;
    [ObservableProperty] private string _translator;
    [ObservableProperty] private string _comment;
    [ObservableProperty] private string _language;
    [ObservableProperty] private TimeSpan _startOfProgramme;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public ExportCavena890ViewModel()
    {
        TranslatedTitle = string.Empty;
        OriginalTitle = string.Empty;
        Translator = string.Empty;
        Comment = string.Empty;
        Language = string.Empty;
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

    [RelayCommand]
    private void Importl()
    {
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