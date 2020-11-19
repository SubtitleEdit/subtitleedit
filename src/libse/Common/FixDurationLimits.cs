namespace Nikse.SubtitleEdit.Core.Common
{
    public class FixDurationLimits
    {
        private readonly int _minDurationMs;
        private readonly int _maxDurationMs;

        public FixDurationLimits(int minDurationMs, int maxDurationMs)
        {
            _minDurationMs = minDurationMs;
            _maxDurationMs = maxDurationMs;
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
                var displayTime = p.Duration.TotalMilliseconds;
                if (displayTime < _minDurationMs)
                {
                    var next = subtitle.GetParagraphOrDefault(i + 1);
                    var wantedEndMs = p.StartTime.TotalMilliseconds + _minDurationMs;
                    if (next == null || wantedEndMs < next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines)
                    {
                        p.EndTime.TotalMilliseconds = wantedEndMs;
                    }
                }
            }
        }

        private void FixLongDisplayTimes(Subtitle subtitle)
        {
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var displayTime = p.Duration.TotalMilliseconds;
                if (displayTime > _maxDurationMs)
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + _maxDurationMs;
                }
            }
        }
    }
}
