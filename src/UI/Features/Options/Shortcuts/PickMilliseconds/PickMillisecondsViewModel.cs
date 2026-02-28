using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.PickMilliseconds;

public partial class PickMillisecondsViewModel : ObservableObject
{
    [ObservableProperty] private int _milliseconds;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public PickMillisecondsViewModel()
    {
    }

    public void Initialize(int milliseconds)
    {
        Milliseconds = milliseconds;
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