using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core.NetflixQualityCheck
{
    public class NetflixCheckSceneChange : INetflixQualityChecker
    {
        /// <summary>
        /// Check the newly-updated timing to Shot Changes rules.
        /// https://partnerhelp.netflixstudios.com/hc/en-us/articles/360051554394-Timed-Text-Style-Guide-Subtitle-Timing-Guidelines
        /// </summary>
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

            double twoFramesGap = 1000.0 / controller.FrameRate * 2.0;
            var twelveFrames = SubtitleFormat.MillisecondsToFrames(1000.0 / controller.FrameRate * 12.0);

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
                    if (SubtitleFormat.MillisecondsToFrames(p.StartTime.TotalMilliseconds - nearestStartPrevSceneChange * 1000) < twelveFrames)
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartPrevSceneChange * 1000;
                        comment = "The in-cue is 1-11 frames after the Shot Change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (nextEndSceneChanges.Count > 0)
                {
                    double nearestStartNextSceneChange = nextStartSceneChanges.Aggregate((x, y) => Math.Abs(x - p.StartTime.TotalSeconds) < Math.Abs(y - p.StartTime.TotalSeconds) ? x : y);
                    if (SubtitleFormat.MillisecondsToFrames(nearestStartNextSceneChange * 1000 - p.StartTime.TotalMilliseconds) < twelveFrames)
                    {
                        fixedParagraph.StartTime.TotalMilliseconds = nearestStartNextSceneChange * 1000;
                        comment = "The in-cue is 1-11 frames before the Shot Change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (previousEndSceneChanges.Count > 0)
                {
                    double nearestEndPrevSceneChange = previousEndSceneChanges.Aggregate((x, y) => Math.Abs(x - p.EndTime.TotalSeconds) < Math.Abs(y - p.EndTime.TotalSeconds) ? x : y);
                    if (SubtitleFormat.MillisecondsToFrames(p.EndTime.TotalMilliseconds - nearestEndPrevSceneChange * 1000) < twelveFrames)
                    {
                        fixedParagraph.EndTime.TotalMilliseconds = nearestEndPrevSceneChange * 1000 - twoFramesGap;
                        comment = "The out-cue is 1-11 frames after the Shot Change";
                        controller.AddRecord(p, fixedParagraph, comment);
                    }
                }

                if (onSceneChange > 0)
                {
                    fixedParagraph.EndTime.TotalMilliseconds = onSceneChange * 1000 - twoFramesGap;
                    comment = "The out-cue is on the Shot Change";
                    controller.AddRecord(p, fixedParagraph, comment);
                }
            }
        }
    }
}
