using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class GooglePlayJson : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "Google Play json";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder("{" + Environment.NewLine + "  \"events\": [ ");
            int count = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                sb.AppendLine(count > 0 ? ", {" : "  {");
                sb.AppendLine("    \"tStartMs\": " + p.StartTime.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + ",");
                sb.AppendLine("    \"dDurationMs\": " + p.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + ",");
                sb.AppendLine("    \"segs\": [ {");
                sb.AppendLine("      \"utf8\": \"" + Json.EncodeJsonText(p.Text).Replace("<br />", "\\n") + "\"");
                sb.AppendLine("    } ]");
                sb.Append("  }");
                count++;
            }
            sb.AppendLine("]");
            sb.AppendLine("}");
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
            if (!(allText.StartsWith("{", StringComparison.Ordinal) || allText.Contains("dDurationMs", StringComparison.Ordinal)))
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
                if (k != "events" || !(dictionary[k] is List<object> captionGroups))
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
                    var dur = -1d;
                    foreach (var lineKey in line.Keys)
                    {
                        if (lineKey == "segs")
                        {
                            if (line[lineKey] is List<object> l)
                            {
                                foreach (var o in l)
                                {
                                    if (o is Dictionary<string, object> textDic)
                                    {
                                        foreach (var tk in textDic.Keys)
                                        {
                                            if (tk == "utf8")
                                            {
                                                text = string.Join(Environment.NewLine, textDic[tk].ToString().SplitToLines());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (lineKey == "tStartMs")
                        {
                            if (double.TryParse(line[lineKey].ToString().Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var s))
                            {
                                start = s;
                            }
                        }
                        else if (lineKey == "dDurationMs")
                        {
                            if (double.TryParse(line[lineKey].ToString().Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var s))
                            {
                                dur = s;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(text) && dur > 0)
                    {
                        subtitle.Paragraphs.Add(new Paragraph(text, start, start + dur));
                    }
                }
            }

            subtitle.Renumber();
        }

    }
}
