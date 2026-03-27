using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public partial class FixNameHitItem : ObservableObject
{
    [ObservableProperty] private bool _isEnabled;

    public string Name { get; set; }
    public int LineIndex { get; set; }
    public int LineIndexDisplay { get; set; }
    public string Before { get; set; }
    public string After { get; set; }

    public FixNameHitItem(string name, int lineIndex, string before, string after, bool isEnabled)
    {
        Name = name;
        LineIndex = lineIndex;
        LineIndexDisplay = lineIndex + 1;
        Before = before;
        After = after;
        IsEnabled = isEnabled;
    }

    public override string ToString()
    {
        return Name;
    }
}