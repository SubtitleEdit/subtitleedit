using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public partial class SubtitleDisplayItem : ObservableObject
{
    [ObservableProperty] private string _text;
    public SubtitleLineViewModel Subtitle { get; set; }

    public SubtitleDisplayItem(SubtitleLineViewModel subtitle)
    {
        Text = string.Empty;
        Subtitle = subtitle;
        UpdateText();
    }

    public void UpdateText()
    {
        var startTime = new TimeCode(Subtitle.StartTime);
        Text = $"{startTime.ToDisplayString()}  {Subtitle.Text}";
    }

    public override string ToString()
    {
        return Text;
    }
}