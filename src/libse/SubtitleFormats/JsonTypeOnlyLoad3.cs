using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// https://www.seriencamp-watchroom.tv
    /// </summary>
    public class JsonTypeOnlyLoad3 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type Only load 3";

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
            if (text.IndexOf("captiondata", StringComparison.Ordinal) < 0)
            {
                return;
            }

            subtitle.Paragraphs.Clear();
            var parser = new SeJsonParser();
            var captionDataAr = parser.GetArrayElementsByName(text, "captiondata");
            foreach (var captionData in captionDataAr)
            {
                var texts = parser.GetArrayElementsByName(captionData, "data");
                foreach (var textElement in texts)
                {
                    var start = parser.GetFirstObject(textElement, "fromms");
                    var end = parser.GetFirstObject(textElement, "toms");
                    var caption = parser.GetFirstObject(textElement, "caption");
                    if (!string.IsNullOrEmpty(caption) &&
                        double.TryParse(start, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var startMs) &&
                        double.TryParse(end, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var endMs))
                    {
                        caption = Json.DecodeJsonText(caption);
                        caption = caption.Replace("<br />", Environment.NewLine);
                        var p = new Paragraph(caption, startMs, endMs);
                        subtitle.Paragraphs.Add(p);
                    }
                }
            }

            subtitle.Renumber();
        }
    }
}
