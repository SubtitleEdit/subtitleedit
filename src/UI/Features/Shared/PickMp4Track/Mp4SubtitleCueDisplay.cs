using Avalonia.Controls;
using System;

namespace Nikse.SubtitleEdit.Features.Shared.PickMp4Track;

public class Mp4SubtitleCueDisplay
{
    public int Number { get; set; }
    public TimeSpan Show { get; set; }
    public TimeSpan Hide { get; set; }
    public TimeSpan Duration { get; set; }
    public string Text { get; set; }
    public Image Image { get; set; }

    public Mp4SubtitleCueDisplay()
    {
        Text = string.Empty;
        Image = new Image();
    }
}
