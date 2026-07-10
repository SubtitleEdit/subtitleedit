using System;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit;

public enum PositionMonitorZone
{
    ActivePicture,
    TopBar,
    BottomBar,
}

/// <summary>
/// Letterbox / zone math for the position monitor (discussion #12318):
/// classifies where an image subtitle sits on the video canvas so QC can spot
/// forced titles in the top letterbox bar and dialogue intruding into the bars.
/// </summary>
public static class PositionMonitorLogic
{
    /// <summary>
    /// Computes the letterbox bar height in pixels for content with the given aspect
    /// ratio inside a screenWidth x screenHeight frame. Returns 0 when the content
    /// fills the frame (or on invalid input).
    /// </summary>
    public static int CalculateBarHeight(int screenWidth, int screenHeight, double contentAspectRatio)
    {
        if (screenWidth <= 0 || screenHeight <= 0 || contentAspectRatio <= 0)
        {
            return 0;
        }

        var contentHeight = screenWidth / contentAspectRatio;
        var bar = (screenHeight - contentHeight) / 2.0;
        return bar > 0 ? (int)Math.Round(bar) : 0;
    }

    /// <summary>
    /// Classifies a subtitle rectangle: anything touching the top bar is TopBar (the
    /// red flag - forced/narrative titles), anything touching the bottom bar is
    /// BottomBar, everything else is inside the active picture.
    /// </summary>
    public static PositionMonitorZone Classify(int y, int height, int screenHeight, int barHeight)
    {
        if (barHeight <= 0 || screenHeight <= 0)
        {
            return PositionMonitorZone.ActivePicture;
        }

        if (y < barHeight)
        {
            return PositionMonitorZone.TopBar;
        }

        if (y + height > screenHeight - barHeight)
        {
            return PositionMonitorZone.BottomBar;
        }

        return PositionMonitorZone.ActivePicture;
    }
}

/// <summary>
/// Item for the letterbox aspect-ratio dropdown. Ratio is null for "Off" and "Custom".
/// </summary>
public class LetterboxRatioItem
{
    public string Name { get; }
    public double? Ratio { get; }
    public bool IsCustom { get; }

    /// <summary>Invariant key persisted in settings ("off", "custom", or the ratio like "2.39").</summary>
    public string SettingsKey { get; }

    public LetterboxRatioItem(string name, double? ratio, bool isCustom, string settingsKey)
    {
        Name = name;
        Ratio = ratio;
        IsCustom = isCustom;
        SettingsKey = settingsKey;
    }

    public override string ToString()
    {
        return Name;
    }
}
