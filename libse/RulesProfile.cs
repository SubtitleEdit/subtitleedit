using System;
using System.Xml.Serialization;

namespace Nikse.SubtitleEdit.Core
{
    public class RulesProfile
    {
        [XmlIgnore]
        public Guid Id { get; set; }
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

        public RulesProfile()
        {
            Id = Guid.NewGuid();
        }

        public RulesProfile(RulesProfile profile)
        {
            Id = profile.Id;
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
