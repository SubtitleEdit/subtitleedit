using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class UknownFormatImporterJson
    {

        public Subtitle AutoGuessImport(string[] lines)
        {

            var sb = new StringBuilder();
            foreach (string s in lines)
                sb.Append(s);
            var allText = sb.ToString().Trim();
            if (!allText.Contains("{", StringComparison.Ordinal))
                return new Subtitle();

            var subtitle1 = new Subtitle();
            try
            {
                int count = 0;
                foreach (string line in allText.Split('{', '}', '[', ']'))
                {
                    count++;
                    string s = line.Trim();
                    if (s.Length > 6)
                    {
                        ReadParagraph(s, subtitle1);
                    }
                    if (count > 20 && subtitle1.Paragraphs.Count == 0)
                        break;
                }
            }
            catch (Exception)
            {
                // ignored                
            }

            var subtitle2 = new Subtitle();
            try
            {
                int count = 0;
                foreach (string line in allText.Split('{', '}'))
                {
                    count++;
                    string s = line.Trim();
                    if (s.Length > 6)
                    {
                        ReadParagraph(s, subtitle2);
                    }
                    if (count > 20 && subtitle2.Paragraphs.Count == 0)
                        break;
                }
            }
            catch (Exception)
            {
                // ignored                
            }

            var subtitle3 = new Subtitle();
            try
            {
                int count = 0;
                foreach (var line in Json.ReadObjectArray(allText))
                {
                    count++;
                    var s = line.Trim();
                    if (s.Length > 6)
                    {
                        ReadParagraph(s, subtitle3);
                    }
                    if (count > 20 && subtitle3.Paragraphs.Count == 0)
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (subtitle1.Paragraphs.Count > subtitle2.Paragraphs.Count && subtitle1.Paragraphs.Count > subtitle3.Paragraphs.Count)
            {
                subtitle1.Renumber();
                return subtitle1;
            }
            if (subtitle2.Paragraphs.Count > subtitle1.Paragraphs.Count && subtitle2.Paragraphs.Count > subtitle3.Paragraphs.Count)
            {
                subtitle2.Renumber();
                return subtitle2;
            }
            subtitle3.Renumber();
            return subtitle3;
        }

        private void ReadParagraph(string s, Subtitle subtitle)
        {
            var start = ReadStartTag(s);
            var end = ReadEndTag(s);
            var duration = ReadDurationTag(s);
            var text = ReadTextTag(s);

            if (start != null && start.Contains(":"))
            {
                start = DecodeFormatToSeconds(start);
            }

            if (end != null && end.Contains(":"))
            {
                end = DecodeFormatToSeconds(end);
            }

            if (start != null && end != null && text != null)
            {
                double startSeconds;
                double endSeconds;
                if (double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out startSeconds) &&
                    double.TryParse(end, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out endSeconds))
                {
                    subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, endSeconds * TimeCode.BaseUnit));
                }
            }
            else if (start != null && duration != null && text != null)
            {
                double startSeconds;
                double durationSeconds;
                if (double.TryParse(start, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out startSeconds) &&
                    double.TryParse(duration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out durationSeconds))
                {
                    subtitle.Paragraphs.Add(new Paragraph(Json.DecodeJsonText(text), startSeconds * TimeCode.BaseUnit, (startSeconds + durationSeconds) * TimeCode.BaseUnit));
                }
            }
        }

        private string DecodeFormatToSeconds(string s)
        {
            var ms = s.Length == 11 && s[8] == ':' ? TimeCode.ParseHHMMSSFFToMilliseconds(s) : TimeCode.ParseToMilliseconds(s);
            return (ms / TimeCode.BaseUnit).ToString(CultureInfo.InvariantCulture);
        }

        private string ReadStartTag(string s)
        {
            return ReadFirstMultiTag(s, new[]
            {
                "start",
                "startTime", "start_time", "starttime",
                "startMilis", "start_Milis", "startmilis",
                "startMs", "start_ms", "startms",
                "startMiliseconds", "start_Milisesonds", "startmiliseconds",
            });
        }

        private string ReadEndTag(string s)
        {
            return ReadFirstMultiTag(s, new[]
            {
                "end",
                "endTime", "end_time", "endtime",
                "endMilis", "end_Milis", "endmilis",
                "endMs", "end_ms", "startms",
                "endMiliseconds", "end_Milisesonds", "endmiliseconds",
            });
        }

        private string ReadDurationTag(string s)
        {
            return ReadFirstMultiTag(s, new[]
            {
                "duration",
                "dur",
            });
        }

        private string ReadTextTag(string s)
        {
            var textLines = Json.ReadArray(s, "text");
            if (textLines != null && textLines.Count > 0)
            {
                return string.Join(Environment.NewLine, textLines);
            }

            return ReadFirstMultiTag(s, new[] { "text", "content" });
        }

        private string ReadFirstMultiTag(string s, string[] tags)
        {
            foreach (var tag in tags)
            {
                var res = Json.ReadTag(s, tag);
                if (!string.IsNullOrEmpty(res))
                    return res;
            }
            return null;
        }

    }
}
