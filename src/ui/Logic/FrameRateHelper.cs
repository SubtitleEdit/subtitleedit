using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic;

public static class FrameRateHelper
{
    public static double[] StandardRates =
    {
        23.976, // 24000/1001
        24.0,
        25.0,
        29.97,  // 30000/1001
        30.0,
        48.0,
        50.0,
        59.94,  // 60000/1001
        60.0
    };

    public static double RoundToNearestCinematicFrameRate(double fps)
    {
        var closest = StandardRates[0];
        var smallestDiff = Math.Abs(fps - closest);

        foreach (var rate in StandardRates)
        {
            var diff = Math.Abs(fps - rate);
            if (diff < smallestDiff)
            {
                smallestDiff = diff;
                closest = rate;
            }
        }

        return closest;
    }

    /// <summary>
    /// Highest frame rate accepted when the user types one into a frame rate combo box.
    /// </summary>
    public const double MaxTypedFrameRate = 1000;

    /// <summary>
    /// Parses a frame rate the user typed into an editable frame rate combo box (#15806): SE 4
    /// accepted any value, e.g. 1, 2, 5, 10 or 15 fps for slide shows and animations. Both "." and
    /// "," work as the decimal separator, whatever the OS culture. The result is rounded to three
    /// decimals, like the "0.###" the combo boxes show, and must be above zero and at most
    /// <see cref="MaxTypedFrameRate"/>.
    /// </summary>
    public static bool TryParseTypedFrameRate(string? text, out double frameRate)
    {
        frameRate = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = text.Trim().Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        value = Math.Round(value, 3);
        if (value <= 0 || value > MaxTypedFrameRate)
        {
            return false;
        }

        frameRate = value;
        return true;
    }

    /// <summary>
    /// Returns the item to select in a frame rate combo box, first adding <paramref name="frameRate"/>
    /// to <paramref name="frameRates"/> when it is not one of the presets: the rate in use can be
    /// anything (restored from settings, read from a video file) and an item missing from the list
    /// would just leave the combo box blank. A rate of zero or less - an empty or corrupt setting -
    /// falls back to the first preset.
    /// </summary>
    public static string SelectInList(IList<string> frameRates, double frameRate)
    {
        if (frameRate <= 0)
        {
            return frameRates[0];
        }

        // Invariant culture: the list holds "23.976"-style items and the combo box round-trips
        // the selected item back through double.Parse with the invariant culture too.
        var text = frameRate.ToString(CultureInfo.InvariantCulture);
        if (!frameRates.Contains(text))
        {
            frameRates.Insert(0, text);
        }

        return text;
    }
}
