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

    public ColumnPasteViewModel()
    {
        ColumnsAll = true;
        ModeOverwrite = true;
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
