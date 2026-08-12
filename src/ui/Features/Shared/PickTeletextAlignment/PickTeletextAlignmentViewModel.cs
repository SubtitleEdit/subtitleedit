using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Shared.PickTeletextAlignment;

public partial class PickTeletextAlignmentViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    [ObservableProperty]
    private int teletextLine = 23;

    [ObservableProperty]
    private string horizontalAlignment = "Center";

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }


// NEU: aktuelle Teletext-Zeile des Untertitels einlesen
internal void Initialize(SubtitleLineViewModel? selectedSubtitle)
{
    if (selectedSubtitle == null)
    {
        TeletextLine = 23;
        return;
    }

    if (int.TryParse(selectedSubtitle.MarginV, out var line) &&
        line >= 0 &&
        line <= 22)
    {
        // EBU STL zählt die Zeilen von 0 bis 22.
        // Für den Benutzer zeigen wir 1 bis 23 an.
        TeletextLine = line + 1;
    }
    else
    {
        TeletextLine = 23;
    }
}

internal void OnKeyDown(KeyEventArgs e)
{
    if (e.Key == Key.Escape)
    {
        e.Handled = true;
        Window?.Close();
    }
}
}