using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomSearch;

public partial class CustomSearchViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _url;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public CustomSearchViewModel()
    {
        Name = string.Empty;
        Url = string.Empty;
    }

    public void Initialize(string name, string url)
    {
        Name = name;
        Url = url;
    }

    [RelayCommand]
    private void Ok()
    {
        Name = Name.Trim();
        Url = Url.Trim();
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
