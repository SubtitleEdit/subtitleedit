using System;

namespace Nikse.SubtitleEdit.Core
{
    public class RulesProfile
    {
        public string Name { get; set; }
        public decimal SubtitleLineMaximumLength { get; set; }
        public decimal SubtitleOptimalCharactersPerSeconds { get; set; }
        public decimal SubtitleMaximumWordsPerMinute { get; set; }
        public decimal SubtitleMaximumCharactersPerSeconds { get; set; }
        public int SubtitleMinimumDisplayMilliseconds { get; set; }
        public int SubtitleMaximumDisplayMilliseconds { get; set; }
        public int MinimumMillisecondsBetweenLines { get; set; }
        public bool CpsIncludesSpace { get; set; }
        public int MaxNumberOfLines { get; set; }
        public int MergeLinesShorterThan { get; set; }
        private Guid _id;

        public RulesProfile()
        {
            ResetId();
        }

        public Guid GetId()
        {
            return _id;
        }

        public void ResetId()
        {
            _id = Guid.NewGuid();
        }

        public RulesProfile(RulesProfile profile)
        {
            _id = profile.GetId();
            Name = profile.Name;
            SubtitleLineMaximumLength = profile.SubtitleLineMaximumLength;
            SubtitleOptimalCharactersPerSeconds = profile.SubtitleOptimalCharactersPerSeconds;
            SubtitleMaximumWordsPerMinute = profile.SubtitleMaximumWordsPerMinute;
            SubtitleMaximumCharactersPerSeconds = profile.SubtitleMaximumCharactersPerSeconds;
            SubtitleMinimumDisplayMilliseconds = profile.SubtitleMinimumDisplayMilliseconds;
            SubtitleMaximumDisplayMilliseconds = profile.SubtitleMaximumDisplayMilliseconds;
            MinimumMillisecondsBetweenLines = profile.MinimumMillisecondsBetweenLines;
            CpsIncludesSpace = profile.CpsIncludesSpace;
            MaxNumberOfLines = profile.MaxNumberOfLines;
            MergeLinesShorterThan = profile.MergeLinesShorterThan;
        }
    }
}
