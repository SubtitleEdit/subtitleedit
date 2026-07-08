namespace Nikse.SubtitleEdit.Features.Video.VideoOcr;

/// <summary>
/// A run of consecutive video frames that look alike (same subtitle on screen, or all blank).
/// Only one representative frame per group is sent to the OCR engine.
/// </summary>
public class VideoOcrFrameGroup
{
    /// <summary>First frame index of the group (0-based, in sampled frames).</summary>
    public int StartFrame { get; set; }

    /// <summary>Last frame index of the group (inclusive).</summary>
    public int EndFrame { get; set; }

    /// <summary>File name of the frame image used for OCR (middle of the group).</summary>
    public string RepresentativeFileName { get; set; } = string.Empty;

    /// <summary>True when the group has no pixels above the brightness minimum - no OCR needed.</summary>
    public bool IsBlank { get; set; }

    /// <summary>OCR result for the representative frame.</summary>
    public string Text { get; set; } = string.Empty;

    public double GetStartMs(double framesPerSecond)
    {
        return StartFrame * 1000.0 / framesPerSecond;
    }

    public double GetEndMs(double framesPerSecond)
    {
        return (EndFrame + 1) * 1000.0 / framesPerSecond;
    }
}
