using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.NetflixQualityCheck;

/// <summary>
/// Check the newly-updated timing to Shot Changes rules.
/// https://partnerhelp.netflixstudios.com/hc/en-us/articles/360051554394-Timed-Text-Style-Guide-Subtitle-Timing-Guidelines
/// </summary>
public class NetflixCheckShotChange : INetflixQualityChecker
{
    public static string ShotChangeDirectory = string.Empty;

    public string Name { get; set; }

    public NetflixCheckShotChange(string name)
    {
        Name = name;
    }

    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        if (!controller.VideoExists)
        {
            return;
        }

        var shotChanges = ShotChangeHelper.FromDisk(controller.VideoFileName, ShotChangeDirectory);
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return;
        }

        if (Configuration.Settings.General.CurrentVideoIsSmpte)
        {
            shotChanges = shotChanges.Select(sc => Math.Round(sc /= 1.001, 3, MidpointRounding.AwayFromZero)).ToList();
        }

        // Every frame conversion below uses controller.FrameRate - the video's rate, the same
        // one these two bounds come from. They used the app's CurrentFrameRate, so absolute
        // times were converted in a different frame space than the bounds they were compared
        // against, and the error grew with the time code (at ten minutes, 25 vs 23.976 is a
        // difference of about 600 frames).
        int halfSecGapInFrames = (int)Math.Round(controller.FrameRate / 2, MidpointRounding.AwayFromZero);
        double twoFramesGap = 1000.0 / controller.FrameRate * 2.0;

        // Frame conversion is monotonic, so sorted shot changes have non-decreasing frames: the
        // shot changes before a frame are a prefix and those after it a suffix. The nearest one
        // before is the last of the prefix, the nearest one after the first of the suffix - found
        // by binary search instead of five full scans (each converting every shot change) per line.
        var sortedShotChanges = shotChanges.ToArray();
        Array.Sort(sortedShotChanges);
        var shotChangeFrames = new int[sortedShotChanges.Length];
        for (var i = 0; i < sortedShotChanges.Length; i++)
        {
            shotChangeFrames[i] = SubtitleFormat.MillisecondsToFrames(sortedShotChanges[i] * 1000, controller.FrameRate);
        }

        foreach (Paragraph p in subtitle.Paragraphs)
        {
            var startFrame = SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds, controller.FrameRate);
            var endFrame = SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds, controller.FrameRate);

            var startLower = FirstAtOrAbove(shotChangeFrames, startFrame);
            var startUpper = FirstAbove(shotChangeFrames, startFrame);
            var endLower = FirstAtOrAbove(shotChangeFrames, endFrame);
            var endUpper = FirstAbove(shotChangeFrames, endFrame);
            var onShotChange = endLower < endUpper ? sortedShotChanges[endLower] : 0;

            if (startLower > 0)
            {
                double nearestStartPrevShotChange = sortedShotChanges[startLower - 1];
                var gapToShotChange = SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds - nearestStartPrevShotChange * 1000, controller.FrameRate);
                if (gapToShotChange != 0 && gapToShotChange < halfSecGapInFrames)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.StartTime.TotalMilliseconds = nearestStartPrevShotChange * 1000;
                    var comment = string.Format(Se.Language.Tools.NetflixCheckAndFix.ShotChangeInCueWithinXFramesAfterSnap, halfSecGapInFrames);
                    controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
                }
            }

            if (startUpper < sortedShotChanges.Length)
            {
                double nearestStartNextShotChange = sortedShotChanges[startUpper];
                var gapToShotChange = SubtitleFormat.MillisecondsToFrames(nearestStartNextShotChange * 1000 - p.StartTime.TotalMilliseconds, controller.FrameRate);
                var threshold = (int)Math.Round(halfSecGapInFrames * 0.75, MidpointRounding.AwayFromZero);
                if (gapToShotChange != 0 && gapToShotChange < halfSecGapInFrames)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    string comment;
                    var canBeFixed = false;
                    if (gapToShotChange < threshold)
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartNextShotChange * 1000;
                        comment = string.Format(Se.Language.Tools.NetflixCheckAndFix.ShotChangeInCue1ToXFramesBeforeSnap, threshold - 1);
                        canBeFixed = true;
                    }
                    else
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartNextShotChange * 1000 - (1000.0 / controller.FrameRate * halfSecGapInFrames);
                        comment = string.Format(Se.Language.Tools.NetflixCheckAndFix.ShotChangeInCueXToYFramesBeforePull, threshold, halfSecGapInFrames - 1, halfSecGapInFrames);
                    }

                    controller.AddRecord(p, fixedParagraph, comment, string.Empty, canBeFixed);
                }
            }

            if (endLower > 0)
            {
                double nearestEndPrevShotChange = sortedShotChanges[endLower - 1];
                if (SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds - nearestEndPrevShotChange * 1000, controller.FrameRate) < halfSecGapInFrames)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.EndTime.TotalMilliseconds = nearestEndPrevShotChange * 1000 - twoFramesGap;
                    var comment = string.Format(Se.Language.Tools.NetflixCheckAndFix.ShotChangeOutCueWithinXFramesAfterChange, halfSecGapInFrames);
                    controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
                }
            }

            if (endUpper < sortedShotChanges.Length)
            {
                double nearestEndNextShotChange = sortedShotChanges[endUpper];
                // "If an out-time is within half a second of the last frame before the shot change,
                // extend the out-time to the shot change, respecting the two-frame gap from the shot
                // change." An out-cue already sitting on the two-frame gap is what we would move it
                // to, so it is not an issue.
                var framesToShotChange = SubtitleFormat.MillisecondsToFrames(nearestEndNextShotChange * 1000 - p.EndTime.TotalMilliseconds, controller.FrameRate);
                if (framesToShotChange < halfSecGapInFrames && framesToShotChange != 2)
                {
                    var fixedParagraph = new Paragraph(p, false);
                    fixedParagraph.EndTime.TotalMilliseconds = nearestEndNextShotChange * 1000 - twoFramesGap;
                    var comment = string.Format(Se.Language.Tools.NetflixCheckAndFix.ShotChangeOutCueWithinXFramesOfChange, halfSecGapInFrames);
                    controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
                }
            }

            if (onShotChange > 0)
            {
                var fixedParagraph = new Paragraph(p, false);
                fixedParagraph.EndTime.TotalMilliseconds = onShotChange * 1000 - twoFramesGap;
                var comment = Se.Language.Tools.NetflixCheckAndFix.ShotChangeOutCueOnShotChange;
                controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
            }
        }
    }

    /// <summary>Index of the first frame that is at or above <paramref name="frame"/> (length if none).</summary>
    private static int FirstAtOrAbove(int[] frames, int frame)
    {
        var low = 0;
        var high = frames.Length;
        while (low < high)
        {
            var middle = (low + high) >>> 1;
            if (frames[middle] < frame)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    /// <summary>Index of the first frame that is above <paramref name="frame"/> (length if none).</summary>
    private static int FirstAbove(int[] frames, int frame)
    {
        var low = 0;
        var high = frames.Length;
        while (low < high)
        {
            var middle = (low + high) >>> 1;
            if (frames[middle] <= frame)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }
}
