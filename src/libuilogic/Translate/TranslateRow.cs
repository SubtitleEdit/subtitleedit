using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.UiLogic.Translate;

public partial class TranslateRow : ObservableObject
{
    public int Number { get; set; }
    public TimeSpan Show { get; set; }
    public TimeSpan Hide { get; set; }
    public string Duration { get; set; }
    public string Text { get; set; }
    [ObservableProperty] private string _translatedText;

    /// <summary>
    /// The engine's own translation when the post-processing dictionary changed it, else null,
    /// so the grid can show the old text under the fixed one.
    /// </summary>
    [ObservableProperty] private string? _postProcessOriginal;

    public double DurationTotalMilliseconds => (Hide - Show).TotalMilliseconds;

    public TranslateRow()
    {
        Duration = string.Empty;
        Text = string.Empty;
        TranslatedText = string.Empty;
    }
}