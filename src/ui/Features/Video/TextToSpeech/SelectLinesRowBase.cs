using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// A checkable line in a select-lines dialog (detect speakers, skip noise lines):
/// a checkbox plus the #/show/text columns every such dialog shares.
/// </summary>
public abstract partial class SelectLinesRowBase : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public int Number { get; }
    public string Show { get; }
    public string Text { get; }

    protected SelectLinesRowBase(bool isSelected, int number, string show, string text)
    {
        IsSelected = isSelected;
        Number = number;
        Show = show;
        Text = text;
    }
}
