using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Oresme : SubtitleFormat
    {
        //00:00:00:00{BC}{W2710}
        //10:00:00:15{Bottom}{Open Caption}{Center}{White}{Font Arial GVP Bold}
        //10:00:17:06{Bottom}{Open Caption}{Center}{White}{Font Arial GVP Bold}We view
        //10:00:19:06{Bottom}{Open Caption}{Center}{White}{Font Arial GVP Bold}Lufa Farms as{N}an agrotechnology business
        private static readonly Regex RegexTimeCodes1 = new Regex(@"^\d\d:\d\d:\d\d:\d\d\{", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Oresme";

        public override string ToText(Subtitle subtitle, string title)
        {
            const string format = "{0}{1}{2}";
            const string tags = "{Bottom}{Open Caption}{Center}{White}{Font Arial GVP Bold}";
            var sb = new StringBuilder();
            sb.AppendLine("00:00:00:00{BC}{W2710}");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                text = text.Replace(Environment.NewLine, "{N}");
                sb.AppendLine(string.Format(format, EncodeTimeCode(p.StartTime), tags, text));
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            char[] splitChars = { '.', ':' };
            foreach (string line in lines)
            {
                string s = line.Trim();
                var match = RegexTimeCodes1.Match(s);
                if (match.Success)
                {
                    var p = new Paragraph();
                    try
                    {
                        p.StartTime = DecodeTimeCodeFrames(s.Substring(0, 11), splitChars);
                        p.Text = GetText(line.Remove(0, 11));
                        subtitle.Paragraphs.Add(p);
                    }
                    catch (Exception exception)
                    {
                        _errorCount++;
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                    }
                }
                else
                {
                    _errorCount++;
                }

                for (int i = 0; i < subtitle.Paragraphs.Count - 1; i++)
                {
                    var p2 = subtitle.Paragraphs[i];
                    var next = subtitle.Paragraphs[i + 1];
                    p2.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
                if (subtitle.Paragraphs.Count > 0)
                {
                    subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds = subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].StartTime.TotalMilliseconds +
                         Utilities.GetOptimalDisplayMilliseconds(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text);
                }
            }
            subtitle.RemoveEmptyLines();
            subtitle.Renumber();
        }

        private static string GetText(string s)
        {
            s = s.Replace("{N}", Environment.NewLine);
            var sb = new StringBuilder();
            bool tagOn = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '{')
                {
                    tagOn = true;
                }
                else if (s[i] == '}')
                {
                    tagOn = false;
                }
                else if (!tagOn)
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString().Trim();
        }
    }
}
