using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class RulesProfile
    {
        //public Guid Id { get; set; }
        public string Name { get; set; }
        public int SubtitleLineMaximumLength { get; set; }
        public decimal SubtitleOptimalCharactersPerSeconds { get; set; }
        public decimal SubtitleMaximumWordsPerMinute { get; set; }
        public decimal SubtitleMaximumCharactersPerSeconds { get; set; }
        public int SubtitleMinimumDisplayMilliseconds { get; set; }
        public int SubtitleMaximumDisplayMilliseconds { get; set; }
        public int MinimumMillisecondsBetweenLines { get; set; }
        public string CpsLineLengthStrategy { get; set; }
        public int MaxNumberOfLines { get; set; }
        public int MergeLinesShorterThan { get; set; }
        public DialogType DialogStyle { get; set; }
        public ContinuationStyle ContinuationStyle { get; set; }
        public CustomContinuationStyle CustomContinuationStyle { get; set; }

        public RulesProfile()
        {
            //Id = Guid.NewGuid();
            DialogStyle = DialogType.DashBothLinesWithSpace;
            ContinuationStyle = ContinuationStyle.NoneLeadingTrailingDots;
            CustomContinuationStyle = new CustomContinuationStyle();
        }

        public RulesProfile(RulesProfile profile)
        {
            //Id = profile.Id;
            Name = profile.Name;
            SubtitleLineMaximumLength = profile.SubtitleLineMaximumLength;
            SubtitleOptimalCharactersPerSeconds = profile.SubtitleOptimalCharactersPerSeconds;
            SubtitleMaximumWordsPerMinute = profile.SubtitleMaximumWordsPerMinute;
            SubtitleMaximumCharactersPerSeconds = profile.SubtitleMaximumCharactersPerSeconds;
            SubtitleMinimumDisplayMilliseconds = profile.SubtitleMinimumDisplayMilliseconds;
            SubtitleMaximumDisplayMilliseconds = profile.SubtitleMaximumDisplayMilliseconds;
            MinimumMillisecondsBetweenLines = profile.MinimumMillisecondsBetweenLines;
            CpsLineLengthStrategy = profile.CpsLineLengthStrategy;
            MaxNumberOfLines = profile.MaxNumberOfLines;
            MergeLinesShorterThan = profile.MergeLinesShorterThan;
            DialogStyle = profile.DialogStyle;
            ContinuationStyle = profile.ContinuationStyle;
            CustomContinuationStyle = new CustomContinuationStyle(profile.CustomContinuationStyle);
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
                var ccs = p.CustomContinuationStyle ?? new CustomContinuationStyle();
                sb.Append("{\"name\":\"" + Encode(p.Name) + "\", " +
                          "\"maxNumberOfLines\":\"" + p.MaxNumberOfLines.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"cpsLineLengthStrategy\":\"" + p.CpsLineLengthStrategy + "\"," +
                          "\"mergeLinesShorterThan\":\"" + p.MergeLinesShorterThan.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"minimumMillisecondsBetweenLines\":\"" + p.MinimumMillisecondsBetweenLines.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleLineMaximumLength\":\"" + p.SubtitleLineMaximumLength.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleMaximumCharactersPerSeconds\":\"" + p.SubtitleMaximumCharactersPerSeconds.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleMaximumDisplayMilliseconds\":\"" + p.SubtitleMaximumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleMaximumWordsPerMinute\":\"" + p.SubtitleMaximumWordsPerMinute.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleMinimumDisplayMilliseconds\":\"" + p.SubtitleMinimumDisplayMilliseconds.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"subtitleOptimalCharactersPerSeconds\":\"" + p.SubtitleOptimalCharactersPerSeconds.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"dialogStyle\":\"" + p.DialogStyle + "\"," +
                          "\"continuationStyle\":\"" + p.ContinuationStyle + "\"," +
                          // Flat fields rather than a nested object: Json.ReadTag is a plain string
                          // scanner and would read a nested object value only up to its first comma.
                          "\"customContinuationStylePause\":\"" + ccs.Pause.ToString(CultureInfo.InvariantCulture) + "\"," +
                          "\"customContinuationStyleSuffix\":\"" + Encode(ccs.Suffix) + "\"," +
                          "\"customContinuationStyleSuffixApplyIfComma\":\"" + ccs.SuffixApplyIfComma + "\"," +
                          "\"customContinuationStyleSuffixAddSpace\":\"" + ccs.SuffixAddSpace + "\"," +
                          "\"customContinuationStyleSuffixReplaceComma\":\"" + ccs.SuffixReplaceComma + "\"," +
                          "\"customContinuationStylePrefix\":\"" + Encode(ccs.Prefix) + "\"," +
                          "\"customContinuationStylePrefixAddSpace\":\"" + ccs.PrefixAddSpace + "\"," +
                          "\"customContinuationStyleUseDifferentStyleGap\":\"" + ccs.UseDifferentStyleGap + "\"," +
                          "\"customContinuationStyleGapSuffix\":\"" + Encode(ccs.GapSuffix) + "\"," +
                          "\"customContinuationStyleGapSuffixApplyIfComma\":\"" + ccs.GapSuffixApplyIfComma + "\"," +
                          "\"customContinuationStyleGapSuffixAddSpace\":\"" + ccs.GapSuffixAddSpace + "\"," +
                          "\"customContinuationStyleGapSuffixReplaceComma\":\"" + ccs.GapSuffixReplaceComma + "\"," +
                          "\"customContinuationStyleGapPrefix\":\"" + Encode(ccs.GapPrefix) + "\"," +
                          "\"customContinuationStyleGapPrefixAddSpace\":\"" + ccs.GapPrefixAddSpace + "\"" +
                          "}");
            }
            sb.AppendLine("]}");
            return sb.ToString();
        }

        public static List<RulesProfile> Deserialize(string input)
        {
            var list = new List<RulesProfile>();
            var s = (input ?? string.Empty).Trim();
            var arrayStart = s.IndexOf('[');
            if (arrayStart < 0)
            {
                return list;
            }

            var profiles = Json.ReadObjectArray(s.Substring(arrayStart).TrimEnd('}'));
            if (profiles == null || profiles.Count == 0)
            {
                return list;
            }

            // A profile written by an older version can be missing any of these tags. Fall back to
            // the built-in defaults rather than letting Convert.ToInt32(null) silently yield 0 - a
            // zero maximum duration would flag every line - or letting Enum.Parse throw on a
            // missing dialogStyle and take the whole profile list down with it.
            var defaults = new GeneralSettings();

            foreach (var p in profiles)
            {
                list.Add(new RulesProfile
                {
                    Name = ReadString(p, "name", string.Empty),
                    MaxNumberOfLines = ReadInt(p, "maxNumberOfLines", defaults.MaxNumberOfLines),
                    CpsLineLengthStrategy = Json.ReadTag(p, "cpsLineLengthStrategy") ?? string.Empty,
                    MergeLinesShorterThan = ReadInt(p, "mergeLinesShorterThan", defaults.MergeLinesShorterThan),
                    MinimumMillisecondsBetweenLines = ReadInt(p, "minimumMillisecondsBetweenLines", defaults.MinimumMillisecondsBetweenLines),
                    SubtitleLineMaximumLength = ReadInt(p, "subtitleLineMaximumLength", defaults.SubtitleLineMaximumLength),
                    SubtitleMaximumCharactersPerSeconds = ReadDecimal(p, "subtitleMaximumCharactersPerSeconds", (decimal)defaults.SubtitleMaximumCharactersPerSeconds),
                    SubtitleMaximumWordsPerMinute = ReadDecimal(p, "subtitleMaximumWordsPerMinute", (decimal)defaults.SubtitleMaximumWordsPerMinute),
                    SubtitleMaximumDisplayMilliseconds = ReadInt(p, "subtitleMaximumDisplayMilliseconds", defaults.SubtitleMaximumDisplayMilliseconds),
                    SubtitleMinimumDisplayMilliseconds = ReadInt(p, "subtitleMinimumDisplayMilliseconds", defaults.SubtitleMinimumDisplayMilliseconds),
                    SubtitleOptimalCharactersPerSeconds = ReadDecimal(p, "subtitleOptimalCharactersPerSeconds", (decimal)defaults.SubtitleOptimalCharactersPerSeconds),
                    DialogStyle = ReadEnum(p, "dialogStyle", defaults.DialogStyle),
                    ContinuationStyle = ReadEnum(p, "continuationStyle", defaults.ContinuationStyle),
                    CustomContinuationStyle = ReadCustomContinuationStyle(p),
                });
            }

            return list;
        }

        private static CustomContinuationStyle ReadCustomContinuationStyle(string json)
        {
            var defaults = new CustomContinuationStyle();
            return new CustomContinuationStyle
            {
                Pause = ReadInt(json, "customContinuationStylePause", defaults.Pause),
                Suffix = ReadString(json, "customContinuationStyleSuffix", defaults.Suffix),
                SuffixApplyIfComma = ReadBool(json, "customContinuationStyleSuffixApplyIfComma", defaults.SuffixApplyIfComma),
                SuffixAddSpace = ReadBool(json, "customContinuationStyleSuffixAddSpace", defaults.SuffixAddSpace),
                SuffixReplaceComma = ReadBool(json, "customContinuationStyleSuffixReplaceComma", defaults.SuffixReplaceComma),
                Prefix = ReadString(json, "customContinuationStylePrefix", defaults.Prefix),
                PrefixAddSpace = ReadBool(json, "customContinuationStylePrefixAddSpace", defaults.PrefixAddSpace),
                UseDifferentStyleGap = ReadBool(json, "customContinuationStyleUseDifferentStyleGap", defaults.UseDifferentStyleGap),
                GapSuffix = ReadString(json, "customContinuationStyleGapSuffix", defaults.GapSuffix),
                GapSuffixApplyIfComma = ReadBool(json, "customContinuationStyleGapSuffixApplyIfComma", defaults.GapSuffixApplyIfComma),
                GapSuffixAddSpace = ReadBool(json, "customContinuationStyleGapSuffixAddSpace", defaults.GapSuffixAddSpace),
                GapSuffixReplaceComma = ReadBool(json, "customContinuationStyleGapSuffixReplaceComma", defaults.GapSuffixReplaceComma),
                GapPrefix = ReadString(json, "customContinuationStyleGapPrefix", defaults.GapPrefix),
                GapPrefixAddSpace = ReadBool(json, "customContinuationStyleGapPrefixAddSpace", defaults.GapPrefixAddSpace),
            };
        }

        private static string Encode(string text)
        {
            return Json.EncodeJsonText(text ?? string.Empty);
        }

        private static string ReadString(string json, string tag, string defaultValue)
        {
            var value = Json.ReadTag(json, tag);
            return value == null ? defaultValue : Json.DecodeJsonText(value);
        }

        private static int ReadInt(string json, string tag, int defaultValue)
        {
            return int.TryParse(Json.ReadTag(json, tag), NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : defaultValue;
        }

        private static decimal ReadDecimal(string json, string tag, decimal defaultValue)
        {
            return decimal.TryParse(Json.ReadTag(json, tag), NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                ? result
                : defaultValue;
        }

        private static bool ReadBool(string json, string tag, bool defaultValue)
        {
            return bool.TryParse(Json.ReadTag(json, tag), out var result) ? result : defaultValue;
        }

        private static T ReadEnum<T>(string json, string tag, T defaultValue) where T : struct
        {
            return Enum.TryParse<T>(Json.ReadTag(json, tag), out var result) ? result : defaultValue;
        }
    }
}
