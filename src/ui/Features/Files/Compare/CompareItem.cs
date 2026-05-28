using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public partial class CompareItem : ObservableObject
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NumberDisplay))] private int _number;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(StartTimeDisplay))] private TimeSpan _startTime;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(EndTimeDisplay))] private TimeSpan _endTime;
    [ObservableProperty] private TimeSpan _duration;
    [ObservableProperty] private string _text;
    [ObservableProperty] private StackPanel _textPanel;
    [ObservableProperty] private string _originalText;
    [ObservableProperty] private IBrush _backgroundBrush;
    [ObservableProperty] private IBrush _numberBackgroundBrush;
    [ObservableProperty] private IBrush _startTimeBackgroundBrush;
    [ObservableProperty] private IBrush _endTimeBackgroundBrush;
    [ObservableProperty] private IBrush _textBackgroundBrush;
    public bool HasDifference { get; set; }
    public bool IsDefault => Text == string.Empty && Number == 0 && Duration == TimeSpan.Zero && StartTime == TimeSpan.Zero;
    public string NumberDisplay => IsDefault ? string.Empty : Number.ToString();
    public string StartTimeDisplay => IsDefault ? string.Empty : new TimeCode(StartTime).ToDisplayString();
    public string EndTimeDisplay => IsDefault ? string.Empty : new TimeCode(EndTime).ToDisplayString();

    public CompareItem()
    {
        Text = string.Empty;
        OriginalText = string.Empty;
        StartTime = TimeSpan.Zero;
        EndTime = TimeSpan.Zero;
        Duration = TimeSpan.Zero;
        BackgroundBrush = new SolidColorBrush(Colors.Transparent);
        NumberBackgroundBrush = new SolidColorBrush(Colors.Transparent);
        StartTimeBackgroundBrush = new SolidColorBrush(Colors.Transparent);
        EndTimeBackgroundBrush = new SolidColorBrush(Colors.Transparent);
        TextBackgroundBrush = new SolidColorBrush(Colors.Transparent);
        TextPanel = new StackPanel();
    }

    public CompareItem(SubtitleLineViewModel line)
    {
        Text = line.Text;
        OriginalText = line.OriginalText;
        StartTime = line.StartTime;
        EndTime = line.EndTime;
        Duration = line.Duration;
        Number = line.Number;
        BackgroundBrush = new SolidColorBrush(Colors.Transparent);
        NumberBackgroundBrush = new SolidColorBrush(Colors.Transparent);
        StartTimeBackgroundBrush = new SolidColorBrush(Colors.Transparent);
        EndTimeBackgroundBrush = new SolidColorBrush(Colors.Transparent);
        TextBackgroundBrush = new SolidColorBrush(Colors.Transparent);
        TextPanel = new StackPanel
        {
            Children = { UiUtil.MakeLabel(line.Text) }
        };
    }
}