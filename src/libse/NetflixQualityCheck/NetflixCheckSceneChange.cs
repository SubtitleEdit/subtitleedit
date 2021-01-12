using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

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

            const int twelveFramesGap = 12;
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
                    if (SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds - nearestStartPrevSceneChange * 1000) < twelveFramesGap)
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartPrevSceneChange * 1000;
                        comment = "The in-cue is within 12 frames after the shot change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (nextStartSceneChanges.Count > 0)
                {
                    double nearestStartNextSceneChange = nextStartSceneChanges.Aggregate((x, y) => Math.Abs(x - p.StartTime.TotalSeconds) < Math.Abs(y - p.StartTime.TotalSeconds) ? x : y);
                    if (SubtitleFormat.MillisecondsToFrames(nearestStartNextSceneChange * 1000 - p.StartTime.TotalMilliseconds) < twelveFramesGap)
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartNextSceneChange * 1000;
                        comment = "The in-cue is within 12 frames before the shot change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (previousEndSceneChanges.Count > 0)
                {
                    double nearestEndPrevSceneChange = previousEndSceneChanges.Aggregate((x, y) => Math.Abs(x - p.EndTime.TotalSeconds) < Math.Abs(y - p.EndTime.TotalSeconds) ? x : y);
                    if (SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds - nearestEndPrevSceneChange * 1000) < twelveFramesGap)
                    {
                        fixedParagraph.EndTime.TotalMilliseconds = nearestEndPrevSceneChange * 1000 - twoFramesGap;
                        comment = "The out-cue is within 12 frames after the shot change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (nextEndSceneChanges.Count > 0)
                {
                    double nearestEndNextSceneChange = nextEndSceneChanges.Aggregate((x, y) => Math.Abs(x - p.EndTime.TotalSeconds) < Math.Abs(y - p.EndTime.TotalSeconds) ? x : y);
                    if (SubtitleFormat.MillisecondsToFrames(nearestEndNextSceneChange * 1000 - p.EndTime.TotalMilliseconds) - 1 < twelveFramesGap &&
                        SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds) != SubtitleFormat.MillisecondsToFrames(nearestEndNextSceneChange * 1000 - twoFramesGap))
                    {
                        fixedParagraph.EndTime.TotalMilliseconds = nearestEndNextSceneChange * 1000 - twoFramesGap;
                        comment = "The out-cue is within 12 frames of the last frame before the shot change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (onSceneChange > 0)
                {
                    fixedParagraph.EndTime.TotalMilliseconds = onSceneChange * 1000 - twoFramesGap;
                    comment = "The out-cue is on the shot change (respect the two-frame gap)";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
