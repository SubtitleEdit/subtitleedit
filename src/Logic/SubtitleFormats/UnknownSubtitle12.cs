using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    ///4.01		5.12
    ///Dit is de dag.
    /// </summary>
    public class UnknownSubtitle12 : SubtitleFormat
    {
        static Regex _regexTimeCode = new Regex(@"^\d+.\d\d\t\t\d+.\d\d\t*$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".txt"; }
        }

        public override string Name
        {
            get { return "Unknown 12"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
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
       
        private string MakeTimeCode(TimeCode tc)
        {
            return string.Format("{0:0.00}", tc.TotalSeconds);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append(MakeTimeCode(p.StartTime));
                sb.Append("\t\t");
                sb.Append(MakeTimeCode(p.EndTime));
                sb.Append("\t\t\n");
                sb.Append(p.Text.Replace(Environment.NewLine, "\n") + "\n\n");
            }
            return sb.ToString().Trim();
        }

        private TimeCode DecodeTimeCode(string timeCode)
        {            
            return new TimeCode(TimeSpan.FromSeconds(double.Parse(timeCode.Trim())));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var text = new StringBuilder();
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (_regexTimeCode.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            p.Text = text.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        string[] arr = s.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        text = new StringBuilder();
                        p = new Paragraph(DecodeTimeCode(arr[0]), DecodeTimeCode(arr[1]), "");
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (p != null)
                {
                    text.AppendLine(s);
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber(1);
        }
        
    }
}
