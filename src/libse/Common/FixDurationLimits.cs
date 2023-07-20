using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class FixDurationLimits
    {
        private readonly int _minDurationMs;
        private readonly int _maxDurationMs;
        private readonly List<double> _shotChanges;

        public FixDurationLimits(int minDurationMs, int maxDurationMs, List<double> shotChanges)
        {
            _minDurationMs = minDurationMs;
            _maxDurationMs = maxDurationMs;
            _shotChanges = shotChanges;
        }

        public Subtitle Fix(Subtitle subtitle)
        {
            var s = new Subtitle(subtitle, false);
            FixLongDisplayTimes(s);
            FixShortDisplayTimes(s);
            return s;
        }

        private void FixShortDisplayTimes(Subtitle subtitle)
        {
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var displayTime = p.DurationTotalMilliseconds;
                if (displayTime < _minDurationMs)
                {
                    var next = subtitle.GetParagraphOrDefault(i + 1);
                    var wantedEndMs = p.StartTime.TotalMilliseconds + _minDurationMs;
                    var bestEndMs = double.MaxValue;

                    // First check for next subtitle
                    if (next != null)
                    {
                        bestEndMs = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    }

                    // Then check for next shot change (if option is checked, and if any are supplied) -- keeping earliest time
                    if (_shotChanges.Count > 0)
                    {
                        bestEndMs = Math.Min(bestEndMs, ShotChangeHelper.GetNextShotChangeMinusGapInMs(_shotChanges, p.EndTime) ?? double.MaxValue);
                    }

                    if (wantedEndMs <= bestEndMs)
                    {
                        p.EndTime.TotalMilliseconds = wantedEndMs;
                    }
                    else if (bestEndMs > p.EndTime.TotalMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = bestEndMs;
                    }
                }
            }
        }

        private void FixLongDisplayTimes(Subtitle subtitle)
        {
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var displayTime = p.DurationTotalMilliseconds;
                if (displayTime > _maxDurationMs)
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + _maxDurationMs;
                }
            }
        }
    }
}
