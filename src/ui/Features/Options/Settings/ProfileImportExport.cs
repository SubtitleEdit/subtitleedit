using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings
{
    public class ProfileImportExport
    {
        public class ProfileImportExportItem
        {
            public string Name { get; set; } = string.Empty;
            public string MaxNumberOfLines { get; set; } = string.Empty;
            public string CpsLineLengthStrategy { get; set; } = string.Empty;
            public string MergeLinesShorterThan { get; set; } = string.Empty;
            public string MinimumMillisecondsBetweenLines { get; set; } = string.Empty;
            public string SubtitleLineMaximumLength { get; set; } = string.Empty;
            public string SubtitleMaximumCharactersPerSeconds { get; set; } = string.Empty;
            public string SubtitleMaximumDisplayMilliseconds { get; set; } = string.Empty;
            public string SubtitleMaximumWordsPerMinute { get; set; } = string.Empty;
            public string SubtitleMinimumDisplayMilliseconds { get; set; } = string.Empty;
            public string SubtitleOptimalCharactersPerSeconds { get; set; } = string.Empty;
            public string DialogStyle { get; set; } = string.Empty;
            public string ContinuationStyle { get; set; } = string.Empty;
        }

        public ProfileImportExport()
        {
        }

        public ProfileImportExport(List<ProfileDisplay> toExport)
        {
            foreach (var profile in toExport)
            {
                var item = new ProfileImportExportItem
                {
                    Name = profile.Name,
                    MaxNumberOfLines = profile.MaxLines.HasValue ? profile.MaxLines.Value.ToString(CultureInfo.InvariantCulture) : "2",
                    CpsLineLengthStrategy = profile.CpsLineLengthStrategy?.Code ?? string.Empty,
                    MergeLinesShorterThan =  profile.UnbreakLinesShorterThan.HasValue ? profile.UnbreakLinesShorterThan.Value.ToString(CultureInfo.InvariantCulture) : "25",
                    MinimumMillisecondsBetweenLines = profile.MinGapMs.HasValue ? profile.MinGapMs.Value.ToString(CultureInfo.InvariantCulture) : "1",
                    SubtitleLineMaximumLength = profile.SingleLineMaxLength.HasValue ? profile.SingleLineMaxLength.Value.ToString(CultureInfo.InvariantCulture) : "43",
                    SubtitleMaximumCharactersPerSeconds = profile.MaxCharsPerSec.HasValue ? profile.MaxCharsPerSec.Value.ToString(CultureInfo.InvariantCulture) : "25",
                    SubtitleMaximumDisplayMilliseconds = profile.MaxDurationMs.HasValue ? profile.MaxDurationMs.Value.ToString(CultureInfo.InvariantCulture) : "10000",
                    SubtitleMaximumWordsPerMinute = profile.MaxWordsPerMin.HasValue ? profile.MaxWordsPerMin.Value.ToString(CultureInfo.InvariantCulture) : "400",
                    SubtitleMinimumDisplayMilliseconds = profile.MinDurationMs.HasValue ? profile.MinDurationMs.Value.ToString(CultureInfo.InvariantCulture) : "500",
                    SubtitleOptimalCharactersPerSeconds = profile.OptimalCharsPerSec.HasValue ? profile.OptimalCharsPerSec.Value.ToString(CultureInfo.InvariantCulture) : "20",
                    DialogStyle = profile.DialogStyle?.Code ?? string.Empty,
                    ContinuationStyle = profile.ContinuationStyle?.Code ?? string.Empty
                };
                Profiles.Add(item);
            }
        }

        public List<ProfileImportExportItem> Profiles { get; set; } = new List<ProfileImportExportItem>();

        internal List<ProfileDisplay>? ToProfileDisplayList()
        {
            var list = new List<ProfileDisplay>();
            foreach (var item in Profiles)
            {
                var profile = new ProfileDisplay
                {
                    Name = item.Name,
                    MaxLines = int.Parse(item.MaxNumberOfLines, CultureInfo.InvariantCulture),
                    CpsLineLengthStrategy = CpsLineLengthStrategyDisplay.List().FirstOrDefault(p=>p.Code == item.CpsLineLengthStrategy) ?? CpsLineLengthStrategyDisplay.List().First(),
                    UnbreakLinesShorterThan = int.Parse(item.MergeLinesShorterThan, CultureInfo.InvariantCulture),
                    MinGapMs = int.Parse(item.MinimumMillisecondsBetweenLines, CultureInfo.InvariantCulture),
                    SingleLineMaxLength = int.Parse(item.SubtitleLineMaximumLength, CultureInfo.InvariantCulture),
                    MaxCharsPerSec = int.Parse(item.SubtitleMaximumCharactersPerSeconds, CultureInfo.InvariantCulture),
                    MaxDurationMs = int.Parse(item.SubtitleMaximumDisplayMilliseconds, CultureInfo.InvariantCulture),
                    MaxWordsPerMin = int.Parse(item.SubtitleMaximumWordsPerMinute, CultureInfo.InvariantCulture),
                    MinDurationMs = int.Parse(item.SubtitleMinimumDisplayMilliseconds, CultureInfo.InvariantCulture),
                    OptimalCharsPerSec = int.Parse(item.SubtitleOptimalCharactersPerSeconds, CultureInfo.InvariantCulture),
                    DialogStyle = DialogStyleDisplay.List().FirstOrDefault(p => p.Code == item.DialogStyle) ?? DialogStyleDisplay.List().First(),
                    ContinuationStyle = ContinuationStyleDisplay.List().FirstOrDefault(p => p.Code == item.ContinuationStyle) ?? ContinuationStyleDisplay.List().First()
                };
                list.Add(profile);
            }
            return list;
        }
    }
}
