using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.SurroundWith;

public partial class SurroundWithViewModel : ObservableObject
{
    [ObservableProperty] private string _before;
    [ObservableProperty] private string _after;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public SurroundWithViewModel()
    {
        Before = string.Empty;
        After = string.Empty;
    }

    public void Initialize(string before, string after)
    {
        Before = before;
        After = after;
    }

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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
            return;
        }
    }
}