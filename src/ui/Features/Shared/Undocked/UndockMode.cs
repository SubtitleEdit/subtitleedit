using System;

namespace Nikse.SubtitleEdit.Features.Shared.Undocked;

/// <summary>
/// Which of the video player and waveform live in their own windows (#17218).
/// </summary>
public enum UndockMode
{
    None,
    Both,
    VideoOnly,
    WaveformOnly,
}

public static class UndockModeHelper
{
    public static bool IsVideoUndocked(UndockMode mode) => mode is UndockMode.Both or UndockMode.VideoOnly;

    public static bool IsWaveformUndocked(UndockMode mode) => mode is UndockMode.Both or UndockMode.WaveformOnly;

    /// <summary>
    /// The undock mode to restore on start-up. Settings from before #17218 only stored
    /// <c>UndockVideoControls</c>, which meant both windows, so an unknown or empty mode is
    /// treated as <see cref="UndockMode.Both"/>.
    /// </summary>
    public static UndockMode FromSettings(bool undocked, string? mode)
    {
        if (!undocked)
        {
            return UndockMode.None;
        }

        if (Enum.TryParse<UndockMode>(mode, ignoreCase: true, out var parsed) && parsed != UndockMode.None)
        {
            return parsed;
        }

        return UndockMode.Both;
    }
}
