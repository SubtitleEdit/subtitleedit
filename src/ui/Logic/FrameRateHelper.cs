using System;

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
}
