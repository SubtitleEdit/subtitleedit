using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.JoinSubtitles;

public partial class JoinDisplayItem : ObservableObject
{
    [ObservableProperty] private int _lines;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _fullFileName;

    public JoinDisplayItem()
    {
        FileName = string.Empty;
        FullFileName = string.Empty;
    }
}

