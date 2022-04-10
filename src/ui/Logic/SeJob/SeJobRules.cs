using System;

namespace Nikse.SubtitleEdit.Logic.SeJob
{
    [Serializable]
    public class SeJobRules
    {
        public int SubtitleLineMaximumLength { get; set; }
        public decimal SubtitleOptimalCharactersPerSeconds { get; set; }
        public decimal SubtitleMaximumWordsPerMinute { get; set; }
        public decimal SubtitleMaximumCharactersPerSeconds { get; set; }
        public int SubtitleMinimumDisplayMilliseconds { get; set; }
        public int SubtitleMaximumDisplayMilliseconds { get; set; }
        public int MinimumMillisecondsBetweenLines { get; set; }
        public string CpsLineLengthStrategy { get; set; }
        public int MaxNumberOfLines { get; set; }
    }
}
