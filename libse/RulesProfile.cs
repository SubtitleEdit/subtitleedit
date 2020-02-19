using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core
{
    public class RulesProfile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int SubtitleLineMaximumLength { get; set; }
        public decimal SubtitleOptimalCharactersPerSeconds { get; set; }
        public decimal SubtitleMaximumWordsPerMinute { get; set; }
        public decimal SubtitleMaximumCharactersPerSeconds { get; set; }
        public int SubtitleMinimumDisplayMilliseconds { get; set; }
        public int SubtitleMaximumDisplayMilliseconds { get; set; }
        public int MinimumMillisecondsBetweenLines { get; set; }
        public bool CpsIncludesSpace { get; set; }
        public int MaxNumberOfLines { get; set; }
        public int MergeLinesShorterThan { get; set; }
        public DialogType DialogStyle { get; set; }

        public RulesProfile()
        {
            Id = Guid.NewGuid();
            DialogStyle = DialogType.DashBothLinesWithSpace;
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
            DialogStyle = profile.DialogStyle;
        }

        public static string Serialize(List<RulesProfile> profiles)
        {
            int count = 0;
            var sb = new StringBuilder();
            sb.AppendLine("{\"profiles\":[");
            for (int i = 0; i < profiles.Count; i++)
            {
                var p = profiles[i];
                count++;
                if (count > 1)
                {
                    sb.Append(",");
                }
                sb.Append("{\"name\":\"" + Json.EncodeJsonText(p.Name) + "\", " +
                          "\"maxNumberOfLines\":\"" + p.MaxNumberOfLines.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"cpsIncludesSpace\":\"" + p.CpsIncludesSpace.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"mergeLinesShorterThan\":\"" + p.MergeLinesShorterThan.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"minimumMillisecondsBetweenLines\":\"" + p.MinimumMillisecondsBetweenLines.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleLineMaximumLength\":\"" + p.SubtitleLineMaximumLength.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleMaximumCharactersPerSeconds\":\"" + p.SubtitleMaximumCharactersPerSeconds.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleMaximumDisplayMilliseconds\":\"" + p.SubtitleMaximumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleMaximumWordsPerMinute\":\"" + p.SubtitleMaximumWordsPerMinute.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleMinimumDisplayMilliseconds\":\"" + p.SubtitleMinimumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleOptimalCharactersPerSeconds\":\"" + p.SubtitleOptimalCharactersPerSeconds.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"dialogStyle\":\"" + p.DialogStyle + "\"" +
                          "}");
            }
            sb.AppendLine("]}");
            return sb.ToString();
        }

        public static List<RulesProfile> Deserialize(string input)
        {
            var list = new List<RulesProfile>();
            var s = input.Trim();
            var profiles = Json.ReadObjectArray(s.Substring(s.IndexOf('[')).TrimEnd('}'));
            if (profiles == null || profiles.Count == 0)
            {
                return list;
            }

            foreach (var p in profiles)
            {
                var name = Json.DecodeJsonText(Json.ReadTag(p, "name"));
                var maxNumberOfLines = Convert.ToInt32(Json.ReadTag(p, "maxNumberOfLines"), CultureInfo.InvariantCulture);
                var cpsIncludesSpace = Convert.ToBoolean(Json.ReadTag(p, "cpsIncludesSpace"), CultureInfo.InvariantCulture);
                var mergeLinesShorterThan = Convert.ToInt32(Json.ReadTag(p, "mergeLinesShorterThan"), CultureInfo.InvariantCulture);
                var minimumMillisecondsBetweenLines = Convert.ToInt32(Json.ReadTag(p, "minimumMillisecondsBetweenLines"), CultureInfo.InvariantCulture);
                var subtitleLineMaximumLength = Convert.ToInt32(Json.ReadTag(p, "subtitleLineMaximumLength"), CultureInfo.InvariantCulture);
                var subtitleMaximumCharactersPerSeconds = Convert.ToDecimal(Json.ReadTag(p, "subtitleMaximumCharactersPerSeconds"), CultureInfo.InvariantCulture);
                var subtitleMaximumWordsPerMinute = Convert.ToDecimal(Json.ReadTag(p, "subtitleMaximumWordsPerMinute"), CultureInfo.InvariantCulture);
                var subtitleMaximumDisplayMilliseconds = Convert.ToInt32(Json.ReadTag(p, "subtitleMaximumDisplayMilliseconds"), CultureInfo.InvariantCulture);
                var subtitleMinimumDisplayMilliseconds = Convert.ToInt32(Json.ReadTag(p, "subtitleMinimumDisplayMilliseconds"), CultureInfo.InvariantCulture);
                var subtitleOptimalCharactersPerSeconds = Convert.ToDecimal(Json.ReadTag(p, "subtitleOptimalCharactersPerSeconds"), CultureInfo.InvariantCulture);
                var dialogStyle = (DialogType)Enum.Parse(typeof(DialogType), Json.ReadTag(p, "dialogStyle"));
                list.Add(new RulesProfile
                {
                    Name = name,
                    MaxNumberOfLines = maxNumberOfLines,
                    CpsIncludesSpace = cpsIncludesSpace,
                    MergeLinesShorterThan = mergeLinesShorterThan,
                    MinimumMillisecondsBetweenLines = minimumMillisecondsBetweenLines,
                    SubtitleLineMaximumLength = subtitleLineMaximumLength,
                    SubtitleMaximumCharactersPerSeconds = subtitleMaximumCharactersPerSeconds,
                    SubtitleMaximumWordsPerMinute = subtitleMaximumWordsPerMinute,
                    SubtitleMaximumDisplayMilliseconds = subtitleMaximumDisplayMilliseconds,
                    SubtitleMinimumDisplayMilliseconds = subtitleMinimumDisplayMilliseconds,
                    SubtitleOptimalCharactersPerSeconds = subtitleOptimalCharactersPerSeconds,
                    DialogStyle = dialogStyle
                });
            }
            return list;
        }
    }
}
