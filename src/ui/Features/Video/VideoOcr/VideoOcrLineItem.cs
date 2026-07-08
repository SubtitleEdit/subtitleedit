using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

public partial class VideoOcrLineItem : ObservableObject
{
    [ObservableProperty] private int _number;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;
    [ObservableProperty] private string _text = string.Empty;

    public TimeSpan Duration => EndTime - StartTime;
}
