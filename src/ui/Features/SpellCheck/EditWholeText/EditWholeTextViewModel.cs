using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.SpellCheck.EditWholeText;

public partial class EditWholeTextViewModel : ObservableObject
{
    [ObservableProperty] private string _lineInfo;
    [ObservableProperty] private string _wholeText;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public EditWholeTextViewModel()
    {
        WholeText = string.Empty;
        LineInfo = string.Empty;
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}