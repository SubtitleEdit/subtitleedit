using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.ContainerFormats.Chapters;
using System;

namespace Nikse.SubtitleEdit.Features.Video.Chapters;

public partial class ChapterItem : ObservableObject
{
    [ObservableProperty] private int _number;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private double _startMilliseconds;

    public ChapterItem()
    {
    }

    public ChapterItem(double startMilliseconds, string title)
    {
        _startMilliseconds = startMilliseconds;
        _title = title ?? string.Empty;
    }

    public ChapterItem(Chapter chapter) : this(chapter.StartMilliseconds, chapter.Title)
    {
    }

    public string StartTimeDisplay => new TimeCode(StartMilliseconds).ToDisplayString();

    public double StartSeconds => StartMilliseconds / TimeCode.BaseUnit;

    /// <summary>
    /// Two-way target for the time code editor.
    /// </summary>
    public TimeSpan StartTimeSpan
    {
        get => TimeSpan.FromMilliseconds(StartMilliseconds);
        set => StartMilliseconds = Math.Max(0, value.TotalMilliseconds);
    }

    public Chapter ToChapter() => new Chapter(StartMilliseconds, Title);

    partial void OnStartMillisecondsChanged(double value)
    {
        OnPropertyChanged(nameof(StartTimeDisplay));
        OnPropertyChanged(nameof(StartSeconds));
        OnPropertyChanged(nameof(StartTimeSpan));
    }

    public override string ToString() => $"{StartTimeDisplay} {Title}";
}
