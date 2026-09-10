using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

public partial class PickAudioTrackViewModel : ObservableObject
{
    [ObservableProperty] private string _titleText = string.Empty;
    [ObservableProperty] private string _messageText = string.Empty;
    [ObservableProperty] private ObservableCollection<AudioTrackOption> _tracks = new();
    [ObservableProperty] private AudioTrackOption? _selectedTrack;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public void Initialize(string title, string message, List<AudioTrackOption> tracks, int defaultSelectedIndex = 0)
    {
        TitleText = title;
        MessageText = message;
        Tracks = new ObservableCollection<AudioTrackOption>(tracks);
        if (defaultSelectedIndex >= 0 && defaultSelectedIndex < Tracks.Count)
        {
            SelectedTrack = Tracks[defaultSelectedIndex];
        }
        else if (Tracks.Count > 0)
        {
            SelectedTrack = Tracks[0];
        }
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
        OkPressed = false;
        Window?.Close();
    }
}
