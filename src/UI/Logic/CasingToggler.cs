using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public class CasingToggler : ICasingToggler
    {
        public string ToggleCasing(string input, SubtitleFormat format, string? overrideFromStringInit = null)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            var sb = new StringBuilder();
            var tags = RemoveAndSaveTags(input, sb, format);
            var text = sb.ToString();

            var containsLowercase = false;
            var containsUppercase = false;
            var stringInit = overrideFromStringInit != null ? HtmlUtil.RemoveHtmlTags(overrideFromStringInit, true) : text;
            for (var i = 0; i < stringInit.Length; i++)
            {
                var ch = stringInit[i];
                if (char.IsNumber(ch))
                {
                    continue;
                }

                if (!containsLowercase && char.IsLower(ch))
                {
                    containsLowercase = true;
                }
                else if (!containsUppercase && char.IsUpper(ch))
                {
                    containsUppercase = true;
                }
            }

            if (containsUppercase && containsLowercase)
            {
                return RestoreSavedAndRemovedTags(text.ToUpperInvariant(), tags);
            }

            if (containsUppercase)
            {
                return RestoreSavedAndRemovedTags(text.ToLowerInvariant(), tags);
            }

            return RestoreSavedAndRemovedTags(System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text), tags);
        }

        private static string RestoreSavedAndRemovedTags(string input, List<KeyValuePair<int, string>> tags)
        {
            var s = input;
            for (var index = tags.Count - 1; index >= 0; index--)
            {
                var keyValuePair = tags[index];
                if (keyValuePair.Key >= s.Length)
                {
                    s += keyValuePair.Value;
                }
                else
                {
                    s = s.Insert(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return s;
        }

        private static List<KeyValuePair<int, string>> RemoveAndSaveTags(string input, StringBuilder sb, SubtitleFormat format)
        {
            var sbTag = new StringBuilder();
            var tags = new List<KeyValuePair<int, string>>();
            var tagOn = false;
            var tagIndex = 0;
            var skipNext = false;
            var isAssa = format != null
                         && (format.GetType() == typeof(AdvancedSubStationAlpha) || format.GetType() == typeof(SubStationAlpha));
            for (var index = 0; index < input.Length; index++)
            {
                if (skipNext)
                {
                    skipNext = false;
                    continue;
                }

                var ch = input[index];

                if (!tagOn && isAssa && ch == '\\'
                           && (input.Substring(index).StartsWith("\\N")
                               || input.Substring(index).StartsWith("\\n")
                               || input.Substring(index).StartsWith("\\h")))
                {
                    tags.Add(new KeyValuePair<int, string>(index, input.Substring(index, 2)));
                    skipNext = true;
                    continue;
                }

                if (tagOn && (ch == '>' || ch == '}'))
                {
                    sbTag.Append(ch);
                    tagOn = false;
                    tags.Add(new KeyValuePair<int, string>(tagIndex, sbTag.ToString()));
                    sbTag.Clear();
                    continue;
                }

                if (!tagOn && ch == '<')
                {
                    var s = input.Substring(index);
                    if (
                        s.StartsWith("<i>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</i>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<b>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</b>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<u>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</u>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<box>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</box>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<font ", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</font>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<span", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</span>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<rt", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</rt", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<ruby", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</ruby>", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<c", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</c", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("<v", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("</v>", StringComparison.OrdinalIgnoreCase))
                    {
                        tagOn = true;
                        tagIndex = sb.Length;
                    }
                }
                else if (!tagOn && ch == '{')
                {
                    var s = input.Substring(index);
                    if (s.StartsWith("{\\", StringComparison.Ordinal))
                    {
                        tagOn = true;
                        tagIndex = sb.Length;
                    }
                }

                if (tagOn)
                {
                    sbTag.Append(ch);
                }
                else
                {
                    sb.Append(ch);
                }
            }

            return tags;
        }


    }
}
