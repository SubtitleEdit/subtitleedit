using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
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
    public void Check(Subtitle subtitle, NetflixQualityController controller)
    {
        if (!controller.VideoExists)
        {
            return;
        }

        var shotChanges = ShotChangeHelper.FromDisk(controller.VideoFileName);
        if (shotChanges == null || shotChanges.Count == 0)
        {
            return;
        }

        if (Configuration.Settings.General.CurrentVideoIsSmpte)
        {
            shotChanges = shotChanges.Select(sc => Math.Round(sc /= 1.001, 3, MidpointRounding.AwayFromZero)).ToList();
        }

        int halfSecGapInFrames = (int)Math.Round(controller.FrameRate / 2, MidpointRounding.AwayFromZero);
        double twoFramesGap = 1000.0 / controller.FrameRate * 2.0;

        foreach (Paragraph p in subtitle.Paragraphs)
        {
            var fixedParagraph = new Paragraph(p, false);
            string comment = string.Empty;

            List<double> previousStartShotChanges = shotChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) < SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds)).ToList();
            List<double> nextStartShotChanges = shotChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) > SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds)).ToList();
            List<double> previousEndShotChanges = shotChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) < SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds)).ToList();
            List<double> nextEndShotChanges = shotChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) > SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds)).ToList();
            var onShotChange = shotChanges.FirstOrDefault(x => SubtitleFormat.MillisecondsToFrames(x * 1000) == SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds));

            if (previousStartShotChanges.Count > 0)
            {
                double nearestStartPrevShotChange = previousStartShotChanges.Aggregate((x, y) => Math.Abs(x - p.StartTime.TotalSeconds) < Math.Abs(y - p.StartTime.TotalSeconds) ? x : y);
                var gapToShotChange = SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds - nearestStartPrevShotChange * 1000);
                if (gapToShotChange != 0 && gapToShotChange < halfSecGapInFrames)
                {
                    fixedParagraph.StartTime.TotalMilliseconds = nearestStartPrevShotChange * 1000;
                    comment = $"The in-cue is within {halfSecGapInFrames} frames after the shot change, snap the in-cue to the shot-change";
                    controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
                }
            }

            if (nextStartShotChanges.Count > 0)
            {
                double nearestStartNextShotChange = nextStartShotChanges.Aggregate((x, y) => Math.Abs(x - p.StartTime.TotalSeconds) < Math.Abs(y - p.StartTime.TotalSeconds) ? x : y);
                var gapToShotChange = SubtitleFormat.MillisecondsToFrames(nearestStartNextShotChange * 1000 - p.StartTime.TotalMilliseconds);
                var threshold = (int)Math.Round(halfSecGapInFrames * 0.75, MidpointRounding.AwayFromZero);
                if (gapToShotChange != 0 && gapToShotChange < halfSecGapInFrames)
                {
                    var canBeFixed = false;  
                    if (gapToShotChange < threshold)
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartNextShotChange * 1000;
                        comment = $"The in-cue is 1-{threshold - 1} frames before the shot change, snap the in-cue to the shot change";
                        canBeFixed = true;
                    }
                    else
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartNextShotChange * 1000 - (1000.0 / controller.FrameRate * halfSecGapInFrames);
                        comment = $"The in-cue is {threshold}-{halfSecGapInFrames - 1} frames before the shot change, pull the in-cue to half a second ({halfSecGapInFrames} frames) before the shot-change";
                    }

                    controller.AddRecord(p, fixedParagraph, comment, string.Empty, canBeFixed);
                }
            }

            if (previousEndShotChanges.Count > 0)
            {
                double nearestEndPrevShotChange = previousEndShotChanges.Aggregate((x, y) => Math.Abs(x - p.EndTime.TotalSeconds) < Math.Abs(y - p.EndTime.TotalSeconds) ? x : y);
                if (SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds - nearestEndPrevShotChange * 1000) < halfSecGapInFrames)
                {
                    fixedParagraph.EndTime.TotalMilliseconds = nearestEndPrevShotChange * 1000 - twoFramesGap;
                    comment = $"The out-cue is within {halfSecGapInFrames} frames after the shot change";
                    controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
                }
            }

            if (nextEndShotChanges.Count > 0)
            {
                double nearestEndNextShotChange = nextEndShotChanges.Aggregate((x, y) => Math.Abs(x - p.EndTime.TotalSeconds) < Math.Abs(y - p.EndTime.TotalSeconds) ? x : y);
                if (SubtitleFormat.MillisecondsToFrames(nearestEndNextShotChange * 1000 - p.EndTime.TotalMilliseconds) < halfSecGapInFrames &&
                    SubtitleFormat.MillisecondsToFrames(nearestEndNextShotChange * 1000 - p.EndTime.TotalMilliseconds) < 2)
                {
                    fixedParagraph.EndTime.TotalMilliseconds = nearestEndNextShotChange * 1000 - twoFramesGap;
                    comment = $"The out-cue is within {halfSecGapInFrames} frames of the shot change";
                    controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
                }
            }

            if (onShotChange > 0)
            {
                fixedParagraph.EndTime.TotalMilliseconds = onShotChange * 1000 - twoFramesGap;
                comment = "The out-cue is on the shot change, respect the two-frame gap";
                controller.AddRecord(p, fixedParagraph, comment, string.Empty, true);
            }
        }
    }
}
