using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Edl : SubtitleFormat
    {
        private static readonly Regex Regex = new Regex(@"^\d\d\d\s+[A-Z]{2}\s+[A-Z]\s+[A-Z]\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d\s+\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".edl"; }
        }

        public override string Name
        {
            get { return "EDL"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("TITLE: " + title);
            if (Configuration.Settings.General.CurrentFrameRate % 1.0 > 0.01)
                sb.AppendLine("FCM: DROP FRAME");
            const string writeFormat = "{0:000}  {1}       {2}     {3}        {4} {5} {6} {7}";
            for (int index = 0; index < subtitle.Paragraphs.Count; index++)
            {
                int no = index++;
                var p = subtitle.Paragraphs[index];
                var next = subtitle.GetParagraphOrDefault(no);
                var text = HtmlUtil.RemoveHtmlTags(p.Text, true);
                sb.AppendLine(string.Format(writeFormat, no, "AX", "V", "C", EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime)));
                sb.AppendLine();
            }
            return sb.ToString().Trim() + Environment.NewLine;
        }

        private static string EncodeTimeCode(TimeCode timeCode)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", timeCode.Hours, timeCode.Minutes, timeCode.Seconds, MillisecondsToFramesMaxFrameRate(timeCode.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {   //002  AX       V     C        01:00:01:15 01:00:04:18 00:00:01:15 00:00:04:18
            _errorCount = 0;
            Paragraph lastParagraph = null;
            int count = 0;
            foreach (string line in lines)
            {
                bool isTimeCode = false;
                if (line.Length > 0)
                {
                    bool success = false;
                    if (line.Length > 65 && line.Length < 75 && line.IndexOf(':') > 20)
                    {
                        var match = Regex.Match(line);
                        if (match.Success)
                        {
                            isTimeCode = true;
                            if (lastParagraph != null)
                                subtitle.Paragraphs.Add(lastParagraph);

                            var arr = line.Split(' ');
                            TimeCode start = DecodeTimeCodeFrames(arr[1], SplitCharColon);
                            TimeCode end = DecodeTimeCodeFrames(arr[2], SplitCharColon);
                            lastParagraph = new Paragraph(start, end, string.Empty);
                            success = true;
                        }
                    }
                    if (!isTimeCode && !string.IsNullOrWhiteSpace(line) && lastParagraph != null && Utilities.GetNumberOfLines(lastParagraph.Text) < 5)
                    {
                        lastParagraph.Text = (lastParagraph.Text + Environment.NewLine + line).Trim();
                        success = true;
                    }
                    if (!success && count > 9)
                        _errorCount++;
                }
                count++;
            }
            if (lastParagraph != null)
                subtitle.Paragraphs.Add(lastParagraph);
            subtitle.Renumber();
        }

    }
}