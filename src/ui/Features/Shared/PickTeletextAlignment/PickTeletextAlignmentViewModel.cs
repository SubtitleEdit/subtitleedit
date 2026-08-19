using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickTeletextAlignment;

public partial class PickTeletextAlignmentViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    [ObservableProperty]
    private int teletextLine = 23;

    [ObservableProperty]
    private string horizontalAlignment = string.Empty;

    [ObservableProperty]
private bool preview;

[ObservableProperty]
private bool showTeletextColumn;

[ObservableProperty]
private bool applyTeletextLine = false;

[ObservableProperty]
private bool applyHorizontalAlignment = false;
[ObservableProperty]
private bool applyLineShift = false;

[ObservableProperty]
private int lineShift = 1;

[ObservableProperty]
private bool applyLineReplace = false;

[ObservableProperty]
private int replaceFromLine = 23;

[ObservableProperty]
private int replaceToLine = 23;

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
    internal void Initialize(
    SubtitleLineViewModel? selectedSubtitle,
    bool preview,
    bool showTeletextColumn)
{
     Preview = preview;
     ShowTeletextColumn = showTeletextColumn;
     
    if (selectedSubtitle == null)
    {
        TeletextLine = 23;
        HorizontalAlignment = Se.Language.General.Center;
        return;
    }

    if (int.TryParse(selectedSubtitle.MarginV, out var line) &&
        line >= 0 &&
        line <= 22)
    {
        TeletextLine = line + 1;
    }
    else
    {
        TeletextLine = 23;
    }

    var text = selectedSubtitle.Text ?? string.Empty;

    if (text.StartsWith("{\\an1}") ||
        text.StartsWith("{\\an4}") ||
        text.StartsWith("{\\an7}"))
    {
        HorizontalAlignment = Se.Language.General.Left;
    }
    else if (text.StartsWith("{\\an3}") ||
             text.StartsWith("{\\an6}") ||
             text.StartsWith("{\\an9}"))
    {
        HorizontalAlignment = Se.Language.General.Right;
    }
    else
    {
        HorizontalAlignment = Se.Language.General.Center;
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