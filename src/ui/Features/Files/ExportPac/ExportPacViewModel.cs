using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Files.ExportPac;

public partial class ExportPacViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _pacCodePages;
    [ObservableProperty] private string? _selectedPacCodePage;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public int? PacCodePage { get; private set; }

    public ExportPacViewModel()
    {
        PacCodePages = new ObservableCollection<string>
        {
            "Latin",
            "Greek",
            "Latin Czech",
            "Arabic",
            "Hebrew",
            "Thai",
            "Cyrillic",
            "Chinese Traditional (Big5)",
            "Chinese Simplified (gb2312)",
            "Korean",
            "Japanese",
            "Latin Turkish",
            "Portuguese",
        };

        SelectedPacCodePage = PacCodePages[0];
    }

    [RelayCommand]
    private void Ok()
    {
        if (string.IsNullOrEmpty(SelectedPacCodePage))
        {
            return;
        }

        OkPressed = true;
        // The list index is the code page number only because the list mirrors Pac's constants
        // exactly; it used to skip Turkish, so "Portuguese" selected CodePageLatinTurkish (11)
        // instead of CodePageLatinPortuguese (12) and Turkish was unreachable entirely.
        PacCodePage = PacCodePages.IndexOf(SelectedPacCodePage);
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}