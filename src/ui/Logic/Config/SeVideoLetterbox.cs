namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// Settings for the Letterboxing ribbon (Video menu > Letterboxing..., #14845): virtual black
/// bars drawn over the video preview, independent of whatever the source video already has.
///
/// Heights are stored as a percentage of the video's own height (not pixels), the same way
/// VideoOcr's crop selection is persisted, so the bars scale correctly across videos of
/// different resolutions instead of being tied to whatever video was open when they were set.
/// </summary>
public class SeVideoLetterbox
{
    /// <summary>Reapply the last bar heights automatically whenever a video is (re)loaded.</summary>
    public bool Enabled { get; set; }

    public double TopHeightPercent { get; set; }
    public double BottomHeightPercent { get; set; }

    public string Color { get; set; }

    public SeVideoLetterbox()
    {
        Enabled = false;
        TopHeightPercent = 0;
        BottomHeightPercent = 0;
        Color = Avalonia.Media.Color.FromRgb(0, 0, 0).FromColorToHex(includeAlpha: false);
    }
}
