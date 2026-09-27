using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Files.ExportPac;

public partial class ExportPacViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _pacCodePages;
    [ObservableProperty] private string? _selectedPacCodePage;
    [ObservableProperty] private ObservableCollection<string> _secondaryPacCodePages;
    [ObservableProperty] private string? _selectedSecondaryPacCodePage;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public int? PacCodePage { get; private set; }

    /// <summary>
    /// Code page for lines in another script (e.g. the Russian lines of a Hebrew subtitle) - they
    /// are written flagged as "secondary code page". -1 = none.
    /// </summary>
    public int SecondaryPacCodePage { get; private set; } = -1;

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

        // "(none)" first, then the same code pages - index - 1 is the code page number
        SecondaryPacCodePages = new ObservableCollection<string>(new[] { "(none)" }.Concat(PacCodePages));
        SelectedSecondaryPacCodePage = SecondaryPacCodePages[0];
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
        SecondaryPacCodePage = string.IsNullOrEmpty(SelectedSecondaryPacCodePage)
            ? -1
            : SecondaryPacCodePages.IndexOf(SelectedSecondaryPacCodePage) - 1;
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/file", "export-to-pac");
        }
    }
}