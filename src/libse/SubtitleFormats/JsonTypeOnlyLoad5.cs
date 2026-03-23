using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// OOONA (https://www.ooona.net/) JSON
    /// </summary>
    public class JsonTypeOnlyLoad5 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type Only load 5";

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (var s in lines)
            {
                sb.Append(s);
            }

            var text = sb.ToString().TrimStart();
            if (text.IndexOf("\"Paragraphs\"", StringComparison.Ordinal) < 0)
            {
                return;
            }

            subtitle.Paragraphs.Clear();
            var parser = new SeJsonParser();
            var subtitleArray = parser.GetArrayElementsByName(text, "Paragraphs");
            foreach (var subtitleItem in subtitleArray)
            {
                var content = new StringBuilder();
                var paragraphLines = parser.GetArrayElementsByName(subtitleItem, "Lines");
                var start = parser.GetFirstObject(subtitleItem, "Start");
                var end = parser.GetFirstObject(subtitleItem, "End");
                var vAlign = parser.GetFirstObject(subtitleItem, "VAlign");
                foreach (var line in paragraphLines)
                {
                    if (vAlign == "Top")
                    {
                        content.Append("{\\an8}");
                    }

                    var textLine = parser.GetFirstObject(line, "Text");
                    if (!string.IsNullOrEmpty(textLine))
                    {
                        content.AppendLine(Json.DecodeJsonText(textLine));
                    }
                }

                if (content.Length > 0 &&
                    double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var startSeconds) &&
                    double.TryParse(end, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var endSeconds))
                {
                    var caption = Json.DecodeJsonText(content.ToString().Trim());
                    caption = caption.Replace("<br />", Environment.NewLine);
                    caption = FixItalic(caption);
                    var p = new Paragraph(TimeCode.FromSeconds(startSeconds), TimeCode.FromSeconds(endSeconds), caption);
                    subtitle.Paragraphs.Add(p);
                }
            }

            subtitle.Renumber();
        }

        private string FixItalic(string caption)
        {
            var sb = new StringBuilder(caption.Length);
            var skipCount = 0;
            var italicOn = false;
            var italicStartTag = "<span style=\"font-style:italic\">";
            var italicEndTag = "</span>";
            for (var i = 0; i < caption.Length; i++)
            {
                char ch = caption[i];
                if (skipCount > 0)
                {
                    skipCount--;
                    continue;
                }

                if (ch == '<')
                {
                    var rest = caption.Substring(i);
                    if (rest.StartsWith(italicStartTag, StringComparison.Ordinal))
                    {
                        sb.Append("<i>");
                        italicOn = true;
                        skipCount = italicStartTag.Length - 1;
                        continue;
                    }
                }

                if (ch == '<' && italicOn)
                {
                    var rest = caption.Substring(i);
                    if (rest.StartsWith(italicEndTag, StringComparison.Ordinal))
                    {
                        sb.Append("</i>");
                        italicOn = true;
                        skipCount = italicEndTag.Length - 1;
                        continue;
                    }
                }

                sb.Append(ch);
            }

            return sb.ToString().Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
        }
    }
}
