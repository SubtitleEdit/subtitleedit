using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class Formatting
    {
        public static readonly List<string> LanguagesAllowingLineMerging = new List<string>
        {
            "en", "da", "nl", "de", "sv", "nb", "fr", "it", "tr", "es", "pt", "sr", "ru", "lv", "lt", "et", "ro", "pl", "ar", "he", "no"
        };

        private bool Italic { get; set; }
        private string Font { get; set; }
        private bool ItalicTwoLines { get; set; }
        private string StartTags { get; set; }
        private bool AutoBreak { get; set; }
        private bool SquareBrackets { get; set; }
        private bool SquareBracketsUppercase { get; set; }
        private int BreakNumberOfLines { get; set; }
        private bool BreakSplitAtLineEnding { get; set; }
        private bool BreakIsDialog { get; set; }

        public string SetTagsAndReturnTrimmed(string input, string sourceLanguage)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var text = input.Trim();

            // SSA/ASS tags
            if (text.StartsWith("{\\", StringComparison.Ordinal))
            {
                var endIndex = text.IndexOf('}');
                if (endIndex > 0)
                {
                    StartTags = text.Substring(0, endIndex + 1);
                    text = text.Remove(0, endIndex + 1).Trim();
                }
            }

            // Italic tags
            if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && text.Contains("</i>" + Environment.NewLine + "<i>") && Utilities.GetNumberOfLines(text) == 2 && Utilities.CountTagInText(text, "<i>") == 2)
            {
                ItalicTwoLines = true;
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic);
            }
            else if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1)
            {
                Italic = true;
                text = text.Substring(3, text.Length - 7);
            }

            // font tags
            var idxOfGt = text.IndexOf('>');
            if (text.StartsWith("<font ", StringComparison.Ordinal) && text.EndsWith("</font>", StringComparison.Ordinal) &&
                Utilities.CountTagInText(text, "</font>") == 1 && idxOfGt < text.IndexOf("</font>", StringComparison.Ordinal))
            {
                Font = text.Substring(0, idxOfGt + 1);
                text = text.Remove(0, idxOfGt + 1);
                text = text.Remove(text.Length - "</font>".Length);
            }

            // Square brackets
            if (text.StartsWith("[", StringComparison.Ordinal) && text.EndsWith("]", StringComparison.Ordinal) &&
                Utilities.GetNumberOfLines(text) == 1 && Utilities.CountTagInText(text, "[") == 1 &&
                Utilities.GetNumberOfLines(text) == 1 && Utilities.CountTagInText(text, "]") == 1)
            {
                if (text == text.ToUpperInvariant())
                {
                    SquareBracketsUppercase = true;
                }
                else
                {
                    SquareBrackets = true;
                }

                text = text.Replace("[", string.Empty).Replace("]", string.Empty);
            }

            // Un-break line
            if (LanguagesAllowingLineMerging.Contains(sourceLanguage))
            {
                var lines = HtmlUtil.RemoveHtmlTags(text).SplitToLines();
                if (lines.Count == 2 && !string.IsNullOrEmpty(lines[0]) && !string.IsNullOrEmpty(lines[1]) &&
                    char.IsLetterOrDigit(lines[0][lines[0].Length - 1]) &&
                    char.IsLower(lines[1][0]))
                {
                    text = Utilities.UnbreakLine(text);
                    AutoBreak = true;
                }
            }

            return text.Trim();
        }

        public string ReAddFormatting(string input)
        {
            var text = input.Trim();

            // Auto-break line
            if (AutoBreak)
            {
                text = Utilities.AutoBreakLine(text);
            }

            // Square brackets
            if (SquareBracketsUppercase)
            {
                text = "[" + text.ToUpperInvariant().Trim() + "]";
            }
            else if (SquareBrackets)
            {
                text = "[" + text.Trim() + "]";
            }

            // Italic tags
            if (ItalicTwoLines)
            {
                var sb = new StringBuilder();
                foreach (var line in text.SplitToLines())
                {
                    sb.AppendLine("<i>" + line + "</i>");
                }
                text = sb.ToString().Trim();
            }
            else if (Italic)
            {
                text = "<i>" + text + "</i>";
            }

            // Font tag
            if (!string.IsNullOrEmpty(Font))
            {
                text = Font + text + "</font>";
            }

            // SSA/ASS tags
            text = StartTags + text;

            return text;
        }

        public string UnBreak(string text, string source)
        {
            var lines = source.SplitToLines();
            BreakNumberOfLines = lines.Count;
            BreakSplitAtLineEnding = lines.Count == 2 && lines[0].HasSentenceEnding();
            BreakIsDialog = lines.Count == 2 &&
                       (lines[0].StartsWith('-') || lines[0].StartsWith("<i>-", StringComparison.Ordinal)) &&
                       lines[1].StartsWith('-') &&
                       Utilities.CountTagInText(source, '-') == 2;
            return Utilities.UnbreakLine(text);
        }

        public string ReBreak(string text, string language)
        {
            if (BreakNumberOfLines == 1)
            {
                return text;
            }

            if (BreakIsDialog && Utilities.GetNumberOfLines(text) == 1 && Utilities.CountTagInText(text, '-') == 2)
            {
                return text
                    .Insert(text.LastIndexOf('-') - 1, Environment.NewLine)
                    .Replace(" " + Environment.NewLine, Environment.NewLine)
                    .Replace(Environment.NewLine + " ", Environment.NewLine);
            }

            return Utilities.AutoBreakLine(text, language, BreakSplitAtLineEnding);
        }
    }
}
