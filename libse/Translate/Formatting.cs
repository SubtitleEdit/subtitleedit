using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Translate
{
    public class Formatting
    {
        public bool Italic { get; set; }
        public bool ItalicTwoLines { get; set; }
        public string StartTags { get; set; }
        public bool AutoBreak { get; set; }

        public string SetTagsAndReturnTrimmed(string text, string source)
        {
            text = text.Trim();

            // SSA/ASS tags
            if (text.StartsWith("{\\"))
            {
                var endIndex = text.IndexOf('}');
                if (endIndex > 0)
                {
                    StartTags = text.Substring(0, endIndex + 1);
                    text = text.Remove(0, endIndex + 1).Trim();
                }
            }

            // Italic tags
            if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && text.Contains("</i>" + Environment.NewLine + "<i>") && Utilities.GetNumberOfLines(text) == 2 && Utilities.CountTagInText(text, "<i>") == 1)
            {
                ItalicTwoLines = true;
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic);
            }
            else if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1)
            {
                Italic = true;
                text = text.Substring(3, text.Length - 7);
            }

            // Un-break line
            var allowedLanguages = new List<string> { "en", "da", "nl", "de", "sv", "nb", "fr", "it" };
            if (allowedLanguages.Contains(source))
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

        public string ReAddFormatting(string text)
        {
            // Auto-break line
            if (AutoBreak)
            {
                text = Utilities.AutoBreakLine(text);
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

            // SSA/ASS tags
            text = StartTags + text;

            return text;
        }

    }
}
