using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Video.EmbeddedSubtitlesEdit;

public partial class EditEmbeddedTrackViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _titleOrlanguage;
    [ObservableProperty] private bool _isForced;
    [ObservableProperty] private bool _isDefault;
    
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public EditEmbeddedTrackViewModel()
    {
        Name = string.Empty;
        TitleOrlanguage = string.Empty;
    }

    internal void Initialize(EmbeddedTrack selectedTrack)
    {
        Name = selectedTrack.Name;
        TitleOrlanguage = selectedTrack.LanguageOrTitle;
        IsForced = selectedTrack.Forced;
        IsDefault = selectedTrack.Default;
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

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            Ok();
        }
    }

    internal void EditEmbeddedTrackWindowLoaded(RoutedEventArgs e)
    {
        Window?.Focus();
    }
}