namespace Nikse.SubtitleEdit.Logic.Config;

public class SeImproveTimeCodes
{
    /// <summary>A <c>ForcedAlignerOption</c> choice; empty means "pick one for the subtitle's language".</summary>
    public string Aligner { get; set; }
    public double MaxShiftSeconds { get; set; }
    public bool AdjustStart { get; set; }
    public bool AdjustEnd { get; set; }
    public bool IsolateSpeech { get; set; }

    public SeImproveTimeCodes()
    {
        Aligner = string.Empty;
        MaxShiftSeconds = 0.5;
        AdjustStart = true;
        AdjustEnd = true;
    }
}
