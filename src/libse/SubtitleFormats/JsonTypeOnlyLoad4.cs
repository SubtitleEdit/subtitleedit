using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// GTS Pro PMW JSON
    /// </summary>
    public class JsonTypeOnlyLoad4 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type Only load 4";

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
            if (text.IndexOf("\"subtitles\"", StringComparison.Ordinal) < 0)
            {
                return;
            }

            subtitle.Paragraphs.Clear();
            var parser = new SeJsonParser();
            var subtitleArray = parser.GetArrayElementsByName(text, "subtitles");
            foreach (var subtitleItem in subtitleArray)
            {
                var contentItems = parser.GetArrayElementsByName(subtitleItem, "content");
                sb = new StringBuilder();
                foreach (var content in contentItems)
                {
                    foreach (var textElement in parser.GetArrayElementsByName(content, "text"))
                    {
                        if (!string.IsNullOrEmpty(textElement))
                        {
                            sb.Append(textElement);
                            sb.Append(" ");
                        }
                    }
                }

                var startElement = parser.GetFirstObject(subtitleItem, "tc_in");
                var start = parser.GetFirstObject(startElement, "value");
                var endElement = parser.GetFirstObject(subtitleItem, "tc_out");
                var end = parser.GetFirstObject(endElement, "value");
                if (sb.Length > 0 && !string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
                {
                    var sa = start.Split(':');
                    var ea = end.Split(':');
                    if (sa.Length == 4 && ea.Length == 4 &&
                        int.TryParse(sa[0], out _) &&
                        int.TryParse(sa[1], out _) &&
                        int.TryParse(sa[2], out _) &&
                        int.TryParse(sa[3], out _) &&
                        int.TryParse(sa[0], out _) &&
                        int.TryParse(sa[1], out _) &&
                        int.TryParse(sa[2], out _) &&
                        int.TryParse(sa[3], out _))
                    {
                        var caption = Json.DecodeJsonText(sb.ToString());
                        caption = caption.Replace("<br />", Environment.NewLine);
                        var p = new Paragraph(DecodeTimeCodeFramesFourParts(sa), DecodeTimeCodeFramesFourParts(ea), caption);
                        subtitle.Paragraphs.Add(p);
                    }
                }
            }

            subtitle.Renumber();
        }
    }
}
