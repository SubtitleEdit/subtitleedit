using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class QuickTimeText : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\[\d\d:\d\d:\d\d.\d\d\]", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "QuickTime text"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines != null && lines.Count > 0 && lines[0].StartsWith("{\\rtf", StringComparison.Ordinal))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string Header = @"{QTtext} {font:Tahoma}
{plain} {size:20}
{timeScale:30}
{width:160} {height:32}
{timestamps:absolute} {language:0}";

            var sb = new StringBuilder();
            sb.AppendLine(Header);
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //[00:00:07.12]
                //d’être perdu dans un brouillard de pensées,
                //[00:00:17.06] (this line is optional!)
                //              (blank line optional too)
                //[00:00:26.26]
                //tout le temps,
                //[00:00:35.08]
                sb.AppendLine(string.Format("{0}{1}{2}", EncodeTimeCode(p.StartTime) + Environment.NewLine, HtmlUtil.RemoveHtmlTags(p.Text) + Environment.NewLine, EncodeTimeCode(p.EndTime) + Environment.NewLine));
                index++;
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //[00:00:07.12]
            return string.Format("[{0:00}:{1:00}:{2:00}.{3:00}]", time.Hours, time.Minutes, time.Seconds, MillisecondsToFramesMaxFrameRate(time.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //[00:00:07.12]
            //d’être perdu dans un brouillard de pensées,
            //[00:00:17.06] (this line is optional!)
            //              (blank line optional too)
            //[00:00:26.26]
            //tout le temps,
            //[00:00:35.08]
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string temp = line.Substring(0, RegexTimeCodes.Match(line).Length);
                    string[] parts = temp.Split(new[] { '.', ':', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        if (p == null || string.IsNullOrEmpty(p.Text))
                        {
                            try
                            {
                                string text = string.Empty;
                                int indexOfEndTime = line.IndexOf(']');
                                if (indexOfEndTime > 0 && indexOfEndTime + 1 < line.Length)
                                    text = line.Substring(indexOfEndTime + 1);
                                p = new Paragraph(DecodeTimeCode(parts), DecodeTimeCode(parts), text);
                            }
                            catch
                            {
                                _errorCount++;
                            }
                        }
                        else
                        {
                            p.EndTime = DecodeTimeCode(parts);
                            subtitle.Paragraphs.Add(p);

                            string text = string.Empty;
                            int indexOfEndTime = line.IndexOf(']');
                            if (indexOfEndTime > 0 && indexOfEndTime + 1 < line.Length)
                                text = line.Substring(indexOfEndTime + 1);
                            p = new Paragraph(DecodeTimeCode(parts), DecodeTimeCode(parts), text);
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip these lines
                }
                else if (p != null)
                {
                    if (string.IsNullOrEmpty(p.Text))
                        p.Text = line;
                    else
                        p.Text = p.Text + Environment.NewLine + line;
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
