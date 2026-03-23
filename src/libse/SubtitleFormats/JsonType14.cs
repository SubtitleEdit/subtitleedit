using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonType14 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type 14";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder("{ \"captionGroups\": [");
            int count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (count > 0)
                {
                    sb.Append(", ");
                }

                sb.Append("{ \"t\":\"");
                sb.Append(Json.EncodeJsonText(p.Text.Replace(Environment.NewLine, "\\n")));
                sb.Append("\", \"s\":");
                sb.Append(p.StartTime.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                sb.Append(", \"e\":");
                sb.Append(p.EndTime.TotalSeconds.ToString(CultureInfo.InvariantCulture));
                sb.Append(" }");
                count++;
            }
            sb.Append("]}");
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            string allText = sb.ToString().Trim();
            if (!(allText.StartsWith("{", StringComparison.Ordinal) || allText.Contains("captionGroups", StringComparison.Ordinal)))
            {
                return;
            }

            var parser = new JsonParser();
            Dictionary<string, object> dictionary;
            try
            {
                dictionary = (Dictionary<string, object>)parser.Parse(allText);
            }
            catch (ParserException)
            {
                return;
            }

            foreach (var k in dictionary.Keys)
            {
                if (k != "captionGroups" || !(dictionary[k] is List<object> captionGroups))
                {
                    continue;
                }

                foreach (var item in captionGroups)
                {
                    if (!(item is Dictionary<string, object> line))
                    {
                        continue;
                    }

                    var text = string.Empty;
                    var start = -1d;
                    var end = -1d;
                    foreach (var lineKey in line.Keys)
                    {
                        if (lineKey == "t")
                        {
                            text = line[lineKey].ToString().Replace("\\n", Environment.NewLine);
                        }
                        else if (lineKey == "s")
                        {
                            if (double.TryParse(line[lineKey].ToString().Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var s))
                            {
                                start = s;
                            }
                        }
                        else if (lineKey == "e")
                        {
                            if (double.TryParse(line[lineKey].ToString().Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var s))
                            {
                                end = s;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(text) && end > 0)
                    {
                        subtitle.Paragraphs.Add(new Paragraph(text, start * TimeCode.BaseUnit, end * TimeCode.BaseUnit));
                    }
                }
            }

            subtitle.Renumber();
        }

    }
}
