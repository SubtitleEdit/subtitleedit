namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

/// <summary>
/// Where Binary Edit's frame rate came from (issue #15549).
/// </summary>
public enum BinaryEditFrameRateSource
{
    /// <summary>The global current frame rate - nothing more specific is known.</summary>
    Current,

    /// <summary>The frame rate a loaded Blu-ray sup declares, its times being on that grid.</summary>
    Declared,

    /// <summary>Found from a loaded Blu-ray sup's times, which are not on the declared grid.</summary>
    Detected,

    /// <summary>The opened video's frame rate.</summary>
    Video,

    /// <summary>Picked in the combo box or set by Change frame rate.</summary>
    Manual,
}
