using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

public partial class OpenFromUrlViewModel : ObservableObject
{
    [ObservableProperty] private string _url;
    
    public Window? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    public OpenFromUrlViewModel()
    {
        Url = string.Empty;
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
        }
    }
}