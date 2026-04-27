using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.ConvertActors;

public partial class ConvertActorsDisplayItem : ObservableObject
{
    [ObservableProperty] private int _number;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private TimeSpan _duration;
    [ObservableProperty] private string _text;
    [ObservableProperty] private string _newText;
    [ObservableProperty] private bool _isChecked;

    public bool IsNextParagraph { get; set; }
    public Guid OriginalId { get; set; }
    public SubtitleLineViewModel UpdatedViewModel { get; set; }

    public ConvertActorsDisplayItem()
    {
        Text = string.Empty;
        NewText = string.Empty;
        UpdatedViewModel = new SubtitleLineViewModel();
    }

    public ConvertActorsDisplayItem(SubtitleLineViewModel p)
    {
        UpdatedViewModel = new SubtitleLineViewModel(p);
        Text = p.Text;
        StartTime = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds);
        Duration = TimeSpan.FromMilliseconds(p.Duration.TotalMilliseconds);
        Number = p.Number;
        NewText = string.Empty;
        IsChecked = true;
        OriginalId = p.Id;
    }
}
