namespace Nikse.SubtitleEdit.Controls.AudioVisualizerControl;

/// <summary>
/// A chapter mark as the waveform needs it: a time and the title to write on the flag.
/// </summary>
public class WaveformChapter
{
    public double Seconds { get; set; }

    public string Title { get; set; } = string.Empty;

    public WaveformChapter()
    {
    }

    public WaveformChapter(double seconds, string title)
    {
        Seconds = seconds;
        Title = title ?? string.Empty;
    }
}
