using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    /// <summary>
    /// Check the newly-updated timing to Shot Changes rules.
    /// https://partnerhelp.netflixstudios.com/hc/en-us/articles/360051554394-Timed-Text-Style-Guide-Subtitle-Timing-Guidelines
    /// </summary>
    public class NetflixCheckSceneChange : INetflixQualityChecker
    {
        public void Check(Subtitle subtitle, NetflixQualityController controller)
        {
            if (!controller.VideoExists)
            {
                return;
            }

            var SceneChanges = SceneChangeHelper.FromDisk(controller.VideoFileName);
            if (SceneChanges == null || SceneChanges.Count == 0)
            {
                return;
            }

            int halfSecGapInFrames = (int)Math.Round(controller.FrameRate / 2);
            double twoFramesGap = 1000.0 / controller.FrameRate * 2.0;

            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var fixedParagraph = new Paragraph(p, false);
                string comment = string.Empty;

                List<double> previousStartSceneChanges = SceneChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) < SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds)).ToList();
                List<double> nextStartSceneChanges = SceneChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) > SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds)).ToList();
                List<double> previousEndSceneChanges = SceneChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) < SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds)).ToList();
                List<double> nextEndSceneChanges = SceneChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) > SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds)).ToList();
                var onSceneChange = SceneChanges.Where(x => SubtitleFormat.MillisecondsToFrames(x * 1000) == SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds)).FirstOrDefault();

                if (previousStartSceneChanges.Count > 0)
                {
                    double nearestStartPrevSceneChange = previousStartSceneChanges.Aggregate((x, y) => Math.Abs(x - p.StartTime.TotalSeconds) < Math.Abs(y - p.StartTime.TotalSeconds) ? x : y);
                    if (SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds - nearestStartPrevSceneChange * 1000) < halfSecGapInFrames)
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartPrevSceneChange * 1000;
                        comment = $"The in-cue is within {halfSecGapInFrames} frames after the shot change, snap the in-cue to the shot-change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (nextStartSceneChanges.Count > 0)
                {
                    double nearestStartNextSceneChange = nextStartSceneChanges.Aggregate((x, y) => Math.Abs(x - p.StartTime.TotalSeconds) < Math.Abs(y - p.StartTime.TotalSeconds) ? x : y);
                    var gapToSceneChange = SubtitleFormat.MillisecondsToFrames(nearestStartNextSceneChange * 1000 - p.StartTime.TotalMilliseconds);
                    var threshold = (int)Math.Round(halfSecGapInFrames * 0.75);
                    if (gapToSceneChange < halfSecGapInFrames)
                    {
                        if (gapToSceneChange < threshold)
                        {
                            fixedParagraph.StartTime.TotalMilliseconds = nearestStartNextSceneChange * 1000;
                            comment = $"The in-cue is 1-{threshold - 1} frames before the shot change, snap the in-cue to the shot change";
                        }
                        else
                        {
                            fixedParagraph.StartTime.TotalMilliseconds = nearestStartNextSceneChange * 1000 - (1000.0 / controller.FrameRate * halfSecGapInFrames);
                            comment = $"The in-cue is {threshold}-{halfSecGapInFrames - 1} frames before the shot change, pull the in-cue to half a second ({halfSecGapInFrames} frames) before the shot-change";
                        }

                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (previousEndSceneChanges.Count > 0)
                {
                    double nearestEndPrevSceneChange = previousEndSceneChanges.Aggregate((x, y) => Math.Abs(x - p.EndTime.TotalSeconds) < Math.Abs(y - p.EndTime.TotalSeconds) ? x : y);
                    if (SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds - nearestEndPrevSceneChange * 1000) < halfSecGapInFrames)
                    {
                        fixedParagraph.EndTime.TotalMilliseconds = nearestEndPrevSceneChange * 1000 - twoFramesGap;
                        comment = $"The out-cue is within {halfSecGapInFrames} frames after the shot change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (nextEndSceneChanges.Count > 0)
                {
                    double nearestEndNextSceneChange = nextEndSceneChanges.Aggregate((x, y) => Math.Abs(x - p.EndTime.TotalSeconds) < Math.Abs(y - p.EndTime.TotalSeconds) ? x : y);
                    if (SubtitleFormat.MillisecondsToFrames(nearestEndNextSceneChange * 1000 - p.EndTime.TotalMilliseconds) < halfSecGapInFrames &&
                        SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds) != SubtitleFormat.MillisecondsToFrames(nearestEndNextSceneChange * 1000 - twoFramesGap))
                    {
                        fixedParagraph.EndTime.TotalMilliseconds = nearestEndNextSceneChange * 1000 - twoFramesGap;
                        comment = $"The out-cue is within {halfSecGapInFrames} frames of the shot change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (onSceneChange > 0)
                {
                    fixedParagraph.EndTime.TotalMilliseconds = onSceneChange * 1000 - twoFramesGap;
                    comment = "The out-cue is on the shot change, respect the two-frame gap";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
