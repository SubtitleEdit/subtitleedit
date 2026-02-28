using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class TimeCodesBeautifier
    {
        private readonly List<Paragraph> _paragraphs;
        private readonly double _frameRate;
        private readonly List<double> _shotChanges;
        private readonly BeautifySettings _settings;

        public TimeCodesBeautifier(List<Paragraph> paragraphs, double frameRate, List<double> shotChanges, BeautifySettings settings)
        {
            _paragraphs = paragraphs;
            _frameRate = frameRate;
            _shotChanges = shotChanges ?? new List<double>();
            _settings = settings;
        }

        public List<Paragraph> Beautify()
        {
            var result = new List<Paragraph>();

            for (int i = 0; i < _paragraphs.Count; i++)
            {
                var p = new Paragraph(_paragraphs[i]);

                // Frame alignment
                if (_settings.SnapToFrames)
                {
                    p.StartTime = AlignToFrame(p.StartTime, _frameRate);
                    p.EndTime = AlignToFrame(p.EndTime, _frameRate);
                }

                // Shot change snapping
                if (_shotChanges.Count > 0)
                {
                    p.StartTime = SnapStartToShotChange(p.StartTime, _settings.ShotChangeThresholdMs, _settings.ShotChangeOffsetFrames, _frameRate);
                    p.EndTime = SnapEndToShotChange(p.EndTime, _settings.ShotChangeThresholdMs, _settings.ShotChangeOffsetFrames, _frameRate);
                }

                // Gap management with previous paragraph
                if (i > 0)
                {
                    var previousEnd = result[i - 1].EndTime.TotalMilliseconds;
                    var frameGapMs = FramesToMilliseconds(_settings.FrameGap, _frameRate);
                    var minStart = previousEnd + frameGapMs;

                    if (p.StartTime.TotalMilliseconds < minStart)
                    {
                        p.StartTime = new TimeCode(minStart);
                    }
                }

                // Validation: Ensure End > Start
                if (p.EndTime.TotalMilliseconds <= p.StartTime.TotalMilliseconds)
                {
                    p.EndTime = new TimeCode(p.StartTime.TotalMilliseconds + _settings.MinDurationMs);
                }

                // Validation: Ensure minimum duration
                var duration = p.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds;
                if (duration < _settings.MinDurationMs)
                {
                    p.EndTime = new TimeCode(p.StartTime.TotalMilliseconds + _settings.MinDurationMs);
                }

                result.Add(p);
            }

            return result;
        }

        private TimeCode AlignToFrame(TimeCode timeCode, double fps)
        {
            var totalMilliseconds = timeCode.TotalMilliseconds;
            var frameNumber = Math.Round(totalMilliseconds * fps / 1000.0);
            var alignedMilliseconds = frameNumber * 1000.0 / fps;
            return new TimeCode(alignedMilliseconds);
        }

        private TimeCode SnapStartToShotChange(TimeCode timeCode, int thresholdMs, int offsetFrames, double fps)
        {
            var timeSeconds = timeCode.TotalMilliseconds / 1000.0;
            var thresholdSeconds = thresholdMs / 1000.0;

            foreach (var shotChange in _shotChanges)
            {
                var diff = Math.Abs(timeSeconds - shotChange);
                if (diff < thresholdSeconds)
                {
                    // Snap to offsetFrames AFTER the shot change
                    var offsetSeconds = offsetFrames / fps;
                    var snappedSeconds = shotChange + offsetSeconds;
                    return new TimeCode(snappedSeconds * 1000.0);
                }
            }

            return timeCode;
        }

        private TimeCode SnapEndToShotChange(TimeCode timeCode, int thresholdMs, int offsetFrames, double fps)
        {
            var timeSeconds = timeCode.TotalMilliseconds / 1000.0;
            var thresholdSeconds = thresholdMs / 1000.0;

            foreach (var shotChange in _shotChanges)
            {
                var diff = Math.Abs(timeSeconds - shotChange);
                if (diff < thresholdSeconds)
                {
                    // Snap to offsetFrames BEFORE the shot change
                    var offsetSeconds = offsetFrames / fps;
                    var snappedSeconds = shotChange - offsetSeconds;
                    return new TimeCode(Math.Max(0, snappedSeconds * 1000.0));
                }
            }

            return timeCode;
        }

        private double FramesToMilliseconds(int frames, double fps)
        {
            return frames * 1000.0 / fps;
        }
    }

    public class BeautifySettings
    {
        public bool SnapToFrames { get; set; } = true;
        public int FrameGap { get; set; } = 2;
        public int ShotChangeThresholdMs { get; set; } = 250;
        public int ShotChangeOffsetFrames { get; set; } = 2;
        public double MinDurationMs { get; set; } = 800;
    }
}
