using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeBridgeGaps
{
    public int BridgeGapsSmallerThanMs { get; set; }
    public int MinGapMs { get; set; }
    public int PercentForLeft { get; set; }

    /// <summary>
    /// The values typed in frame mode (#14959). Kept apart from the millisecond keys, so a number
    /// entered as frames is never read back as milliseconds (or the other way round) after the
    /// time format is switched. Null means "never entered as frames" - derived from the ms value.
    /// </summary>
    public int? BridgeGapsSmallerThanFrames { get; set; }
    public int? MinGapFrames { get; set; }

    public SeBridgeGaps()
    {
        BridgeGapsSmallerThanMs = 2000;
        MinGapMs = 24;
        PercentForLeft = 100;
    }

    public int GetBridgeGapsSmallerThan(bool frames)
    {
        return frames
            ? BridgeGapsSmallerThanFrames ?? SubtitleFormat.MillisecondsToFrames(BridgeGapsSmallerThanMs)
            : BridgeGapsSmallerThanMs;
    }

    public int GetMinGap(bool frames)
    {
        return frames
            ? MinGapFrames ?? SubtitleFormat.MillisecondsToFrames(MinGapMs)
            : MinGapMs;
    }

    /// <summary>
    /// Saves a value in the unit it was entered in. Frames also update the millisecond key, which
    /// is what Merge short lines reads as its gap threshold, so it follows the latest value.
    /// </summary>
    public void SetBridgeGapsSmallerThan(int value, bool frames)
    {
        if (frames)
        {
            BridgeGapsSmallerThanFrames = value;
            BridgeGapsSmallerThanMs = SubtitleFormat.FramesToMilliseconds(value);
        }
        else
        {
            BridgeGapsSmallerThanMs = value;
        }
    }

    public void SetMinGap(int value, bool frames)
    {
        if (frames)
        {
            MinGapFrames = value;
            MinGapMs = SubtitleFormat.FramesToMilliseconds(value);
        }
        else
        {
            MinGapMs = value;
        }
    }
}
