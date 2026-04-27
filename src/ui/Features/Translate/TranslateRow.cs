using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class TranslateRow : ObservableObject
{
    public int Number { get; set; }
    public TimeSpan Show { get; set; }
    public TimeSpan Hide { get; set; }
    public string Duration { get; set; }
    public string Text { get; set; }
    [ObservableProperty] private string _translatedText;

    public double DurationTotalMilliseconds => (Hide - Show).TotalMilliseconds;

    public TranslateRow()
    {
        Duration = string.Empty;
        Text = string.Empty;
        TranslatedText = string.Empty;
    }
}