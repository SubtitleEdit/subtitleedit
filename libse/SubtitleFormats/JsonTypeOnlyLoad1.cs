using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class JsonTypeOnlyLoad1 : SubtitleFormat
    {
        public override string Extension => ".json";

        public override string Name => "JSON Type Only load 1";

        public override string ToText(Subtitle subtitle, string title)
        {
            return string.Empty;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            var sb = new StringBuilder();
            foreach (string s in lines)
            {
                sb.Append(s);
            }

            if (!sb.ToString().Contains("\"words\":"))
            {
                return;
            }

            var temp = sb.ToString();
            while (temp.Contains("  "))
            {
                temp = temp.Replace("  ", " ");
            }

            temp = temp.Replace("}, {", "},{");
            temp = temp.Replace("} , {", "},{");
            temp = temp.Replace("} ,{", "},{");
            temp = temp.Substring(temp.IndexOf("\"words\":", StringComparison.Ordinal) + 10);
            Paragraph p = null;

            foreach (string line in temp.Replace("},{", Environment.NewLine).SplitToLines())
            {
                string s = line.Trim() + "}";
                string start = Json.ReadTag(s, "time");
                string duration = Json.ReadTag(s, "duration");
                string text = Json.ReadTag(s, "name");
                if (start != null && duration != null && text != null)
                {
                    if (double.TryParse(start, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var startSeconds) &&
                        double.TryParse(duration, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out var durationSeconds))
                    {
                        if ((text == "." || text == "!" || text == "?" || text == ":") && p != null)
                        {
                            p.EndTime.TotalMilliseconds += durationSeconds * TimeCode.BaseUnit;
                            p.Text += text;
                        }
                        else
                        {
                            p = new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, (startSeconds + durationSeconds) * TimeCode.BaseUnit);
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

    }
}
