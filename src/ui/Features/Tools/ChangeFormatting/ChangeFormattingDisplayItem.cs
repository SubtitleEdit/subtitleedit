using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeFormatting
{
    public partial class ChangeFormattingDisplayItem : ObservableObject
    {
        [ObservableProperty] private int _number;
        [ObservableProperty] private TimeSpan _startTime;
        [ObservableProperty] private TimeSpan _endTime;
        [ObservableProperty] private TimeSpan _duration;
        [ObservableProperty] private string _text;
        [ObservableProperty] private string _newText;

        public SubtitleLineViewModel SubtitleLineViewModel { get; set; }

        public ChangeFormattingDisplayItem()
        {
            Text = string.Empty;
            NewText = string.Empty;
            SubtitleLineViewModel = new SubtitleLineViewModel();
        }

        public ChangeFormattingDisplayItem(SubtitleLineViewModel p)
        {
            SubtitleLineViewModel = new SubtitleLineViewModel(p);
            Text = p.Text;
            StartTime = TimeSpan.FromMilliseconds(p.StartTime.TotalMilliseconds);
            EndTime = TimeSpan.FromMilliseconds(p.EndTime.TotalMilliseconds);
            Duration = TimeSpan.FromMilliseconds(p.Duration.TotalMilliseconds);
            Number = p.Number;
            NewText = string.Empty;
        }
    }
}
