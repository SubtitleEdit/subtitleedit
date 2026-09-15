namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Frame packing of a stereoscopic (3D) video: how the two eye views share one frame, and so
/// how <see cref="Stereo3DImage"/> has to draw the subtitle to show up in both.
/// </summary>
public enum Export3DMode
{
    None,

    /// <summary>Left eye in the left half of the frame, right eye in the right half, each squeezed to half width.</summary>
    HalfSideBySide,

    /// <summary>Left eye in the top half of the frame, right eye in the bottom half, each squeezed to half height.</summary>
    HalfTopBottom,
}
