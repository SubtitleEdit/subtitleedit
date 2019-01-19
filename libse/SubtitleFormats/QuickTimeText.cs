using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class QuickTimeText : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\[\d\d:\d\d:\d\d.\d\d\]", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "QuickTime text";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines != null && lines.Count > 0 && lines[0].StartsWith("{\\rtf", StringComparison.Ordinal))
            {
                return false;
            }

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            bool isMine = subtitle.Paragraphs.Count > _errorCount;

            if (isMine && new UnknownSubtitle80().IsMine(lines, fileName))
            {
                return false;
            }

            return isMine;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string header = @"{QTtext} {font:Tahoma}
{plain} {size:20}
{timeScale:30}
{width:160} {height:32}
{timestamps:absolute} {language:0}";

            var sb = new StringBuilder();
            sb.AppendLine(header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                //[00:00:07.12]
                //d’être perdu dans un brouillard de pensées,
                //[00:00:17.06] (this line is optional!)
                //              (blank line optional too)
                //[00:00:26.26]
                //tout le temps,
                //[00:00:35.08]
                sb.AppendLine($"{EncodeTimeCode(p.StartTime) + Environment.NewLine}{HtmlUtil.RemoveHtmlTags(p.Text) + Environment.NewLine}{EncodeTimeCode(p.EndTime) + Environment.NewLine}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //[00:00:07.12]
            return $"[{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{MillisecondsToFramesMaxFrameRate(time.Milliseconds):00}]";
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
                                {
                                    text = line.Substring(indexOfEndTime + 1);
                                }

                                p = new Paragraph(DecodeTimeCodeFramesFourParts(parts), DecodeTimeCodeFramesFourParts(parts), text);
                            }
                            catch
                            {
                                _errorCount++;
                            }
                        }
                        else
                        {
                            p.EndTime = DecodeTimeCodeFramesFourParts(parts);
                            subtitle.Paragraphs.Add(p);

                            string text = string.Empty;
                            int indexOfEndTime = line.IndexOf(']');
                            if (indexOfEndTime > 0 && indexOfEndTime + 1 < line.Length)
                            {
                                text = line.Substring(indexOfEndTime + 1);
                            }

                            p = new Paragraph(DecodeTimeCodeFramesFourParts(parts), DecodeTimeCodeFramesFourParts(parts), text);
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
                    {
                        p.Text = line;
                    }
                    else
                    {
                        p.Text = p.Text + Environment.NewLine + line;
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
