using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyMinGap;

public partial class ApplyMinGapItem : ObservableObject
{
    [ObservableProperty] private int _number;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _endTime;
    [ObservableProperty] private TimeSpan _duration;
    [ObservableProperty] private string _text;
    [ObservableProperty] private string _infoText;

    public SubtitleLineViewModel SubtitleLineViewModel { get; set; }

    public ApplyMinGapItem()
    {
        Text = string.Empty;
        InfoText = string.Empty;
        SubtitleLineViewModel = new SubtitleLineViewModel();
    }

    public ApplyMinGapItem(SubtitleLineViewModel p)
    {
        SubtitleLineViewModel = new SubtitleLineViewModel(p);
        Text = p.Text;
        StartTime = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds);
        EndTime = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds);
        Duration = TimeSpan.FromMilliseconds(p.Duration.TotalMilliseconds);
        Number = p.Number;
        InfoText = string.Empty;
    }
}

