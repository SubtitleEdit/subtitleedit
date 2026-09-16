using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Assa.AssaProgressBar;

/// <summary>
/// Represents a chapter marker in the progress bar.
/// </summary>
public partial class ProgressBarChapter : ObservableObject
{
    [ObservableProperty] private string _text = string.Empty;
    [ObservableProperty] private double _startTimeMs;

    public string StartTimeDisplay => TimeSpan.FromMilliseconds(StartTimeMs).ToString(@"hh\:mm\:ss\.fff");

    partial void OnStartTimeMsChanged(double value)
    {
        OnPropertyChanged(nameof(StartTimeDisplay));
    }

    // A list row or combo box value is announced by ToString() unless its template is a bare
    // text block - without this a screen reader reads the class name (#12087).
    public override string ToString() => string.IsNullOrEmpty(Text) ? StartTimeDisplay : $"{Text}, {StartTimeDisplay}";
}
