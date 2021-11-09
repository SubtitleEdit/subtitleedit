using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// www.arctime.cn
    /// </summary>
    public class JsonArchtime : SubtitleFormat
    {
        public override string Extension => ".atpj";

        public override string Name => "JSON Archtime project";

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
            if (text.IndexOf("atpjfile_info", StringComparison.Ordinal) < 0)
            {
                return;
            }

            subtitle.Paragraphs.Clear();
            var parser = new SeJsonParser();
            var elements = parser.GetArrayElementsByName(text, "BLOCKS");
            foreach (var element in elements)
            {
                var startTimeObject = parser.GetFirstObject(element, "time_start");
                var endTimeObject = parser.GetFirstObject(element, "time_end");
                var textObject = parser.GetFirstObject(element, "text");
                if (!string.IsNullOrEmpty(textObject) && !string.IsNullOrEmpty(startTimeObject) && !string.IsNullOrEmpty(endTimeObject))
                {
                    if (double.TryParse(startTimeObject, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var startMs) &&
                        double.TryParse(endTimeObject, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var endMs))
                    {
                        var s = Utilities.UrlDecode(Json.DecodeJsonText(textObject));
                        var p = new Paragraph(s, startMs, endMs);
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }
            subtitle.Renumber();
        }
    }
}
