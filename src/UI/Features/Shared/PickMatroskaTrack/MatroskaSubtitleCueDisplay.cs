using Avalonia.Controls;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;

public class MatroskaSubtitleCueDisplay
{
    public int Number { get; set; }
    public TimeSpan Show { get; set; }
    public TimeSpan Duration { get; set; }
    public string Text { get; set; }
    public Image? Image { get; set; }

    public MatroskaSubtitleCueDisplay()
    {
        Text = string.Empty;
    }
}
