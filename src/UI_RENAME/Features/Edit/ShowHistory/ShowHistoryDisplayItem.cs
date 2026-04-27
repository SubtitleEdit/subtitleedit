using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Edit.ShowHistory;

public partial class ShowHistoryDisplayItem : ObservableObject
{
    [ObservableProperty] private string _time;
    [ObservableProperty] private string _description;

    public int Hash { get; set; }

    public ShowHistoryDisplayItem()
    {
        Time = string.Empty;
        Description = string.Empty;
    }
}
