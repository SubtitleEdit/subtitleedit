using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.ColumnPaste;

public partial class ColumnPasteViewModel : ObservableObject
{
    [ObservableProperty] private bool _columnsAll;
    [ObservableProperty] private bool _columnsTimeCodesOnly;
    [ObservableProperty] private bool _columnsTextOnly;

    [ObservableProperty] private bool _modeOverwrite;
    [ObservableProperty] private bool _modeTextDown;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    /// <summary>
    /// True when the clipboard held plain text with no time codes, so only the text column can
    /// be pasted. The window then disables the other column choices.
    /// </summary>
    public bool IsTextOnlySource { get; private set; }

    public ColumnPasteViewModel()
    {
        ColumnsAll = true;
        ModeOverwrite = true;
    }

    /// <summary>
    /// Locks the column choice to "text only" - SE4 opened the dialog the same way when the
    /// clipboard could not be parsed as a subtitle.
    /// </summary>
    internal void SetTextOnlySource()
    {
        IsTextOnlySource = true;
        ColumnsAll = false;
        ColumnsTimeCodesOnly = false;
        ColumnsTextOnly = true;
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
