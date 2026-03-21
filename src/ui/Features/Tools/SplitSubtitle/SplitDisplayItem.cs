using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;

public partial class SplitDisplayItem : ObservableObject
{
    [ObservableProperty] private int _lines;
    [ObservableProperty] private int _characters;
    [ObservableProperty] private string _fileName;

    public SplitDisplayItem()
    {
        FileName = string.Empty;
    }
}

