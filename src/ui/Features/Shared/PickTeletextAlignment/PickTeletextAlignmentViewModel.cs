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

    /// <summary>
    /// Teletext row 1..23, as stored in the EBU STL vertical position field.
    /// </summary>
    [ObservableProperty] private int _teletextLine = DefaultTeletextLine;
    [ObservableProperty] private string _horizontalAlignment = string.Empty;
    [ObservableProperty] private bool _preview;
    [ObservableProperty] private bool _showTeletextColumn;

    // Each edit is opt-in, so a run that only shifts lines leaves the alignment alone.
    [ObservableProperty] private bool _applyTeletextLine;
    [ObservableProperty] private bool _applyHorizontalAlignment;
    [ObservableProperty] private bool _applyLineShift;
    [ObservableProperty] private int _lineShift = 1;
    [ObservableProperty] private bool _applyLineReplace;
    [ObservableProperty] private int _replaceFromLine = DefaultTeletextLine;
    [ObservableProperty] private int _replaceToLine = DefaultTeletextLine;

    // The bottom row - where subtitles normally sit - and the fallback for a line that has no
    // usable vertical position yet.
    private const int DefaultTeletextLine = 23;

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

    internal void Initialize(SubtitleLineViewModel? selectedSubtitle, bool preview, bool showTeletextColumn)
    {
        Preview = preview;
        ShowTeletextColumn = showTeletextColumn;

        if (selectedSubtitle == null)
        {
            TeletextLine = DefaultTeletextLine;
            HorizontalAlignment = Se.Language.General.Center;
            return;
        }

        TeletextLine = int.TryParse(selectedSubtitle.MarginV, out var line) && line >= 1 && line <= 23
            ? line
            : DefaultTeletextLine;

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
