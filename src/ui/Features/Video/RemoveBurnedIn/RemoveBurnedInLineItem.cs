using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

/// <summary>
/// One OCR line in the review step. <see cref="Line"/> is what the timeline shows: dragging a
/// box edits its StartTime/EndTime in place, and the run uses those edited times.
/// </summary>
public partial class RemoveBurnedInLineItem : ObservableObject
{
    [ObservableProperty] private bool _isIncluded = true;

    public SubtitleLineViewModel Line { get; }

    public string Text => Line.Text;

    public int Number => Line.Number;

    public RemoveBurnedInLineItem(int number, TimeSpan start, TimeSpan end, string text)
    {
        Line = new SubtitleLineViewModel
        {
            Id = Guid.NewGuid(),
            Number = number,
            StartTime = start,
            EndTime = end,
            Text = text,
        };
    }

    public override string ToString() => Text;
}
