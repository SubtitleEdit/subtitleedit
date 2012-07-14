using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class UnknownSubtitle16 : SubtitleFormat
    {
        static readonly Regex RegexTimeCode = new Regex(@" \d\d:\d\d:\d\d.\d\d\\tab \d\d:\d\d:\d\d.\d\d", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".cip"; }
        }

        public override string Name
        {
            get { return "Unknown 16"; }
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
            //10:01:37.23
            return string.Format("{0:00}:{1:00}:{2:00}.{3:00}", tc.Hours, tc.Minutes, tc.Seconds, MillisecondsToFrames(tc.Milliseconds));
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Tahoma;}{\f1\fnil\fcharset0 Courier New;}{\f2\fmodern\fprq6\fcharset0 SimSun;}{\f3\fmodern\fprq1\fcharset0 Gulim;}}");
            sb.AppendLine(@"{\colortbl ;\red0\green0\blue0;\red230\green230\blue230;\red0\green0\blue220;\red255\green255\blue255;}");
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append(@"\protect0\pard\protect\tx1320\tx2933\tx4546\tx6160\tx8066\cf2\highlight2\fs22 #0\cf3 1026\tab ");
                sb.Append(MakeTimeCode(p.StartTime));
                sb.Append(@"\tab ");
                sb.Append(MakeTimeCode(p.EndTime));
                sb.AppendLine(@"\tab\cf2 00:00:0\cf3 2.10\tab #F CC\cf2 00000D0\cf3\tab\cf2 #C\cf3  \highlight0\fs15\par");
                sb.AppendLine(p.Text.Trim().Replace(Environment.NewLine, @"\par" + Environment.NewLine)+ @"\par");
            }
            sb.AppendLine("}");
            return sb.ToString().Trim();
        }

        private TimeCode DecodeTimeCode(string timeCode)
        {
            string[] arr = timeCode.Split(".,:".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int hour = int.Parse(arr[0]);
            int minutes = int.Parse(arr[1]);
            int seconds = int.Parse(arr[2]);
            int frames = int.Parse(arr[3]);

            int milliseconds = (int) Math.Round(((1000.0 / Configuration.Settings.General.CurrentFrameRate) * frames));
            if (milliseconds > 999)
                milliseconds = 999;

            return new TimeCode(hour, minutes, seconds, milliseconds);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var text = new StringBuilder();

            if (lines.Count == 0 || !lines[0].Trim().StartsWith("{\\rtf1"))
                return;

            foreach (string line in lines)
            {
                string s = line.Trim();
                Match match = RegexTimeCode.Match(s);
                if (match.Success)
                {
                    try
                    {
                        if (p != null)
                        {
                            p.Text = text.ToString().Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        string[] arr = s.Substring(match.Index, match.Length).Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        text = new StringBuilder();
                        p = new Paragraph(DecodeTimeCode(arr[0]), DecodeTimeCode(arr[1].Replace("tab ", string.Empty)), "");
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (p != null)
                {
                    if (s.StartsWith("\\"))
                    {
                        int index = s.IndexOf(" ");
                        if (index > 0 && index < s.Length - 1)
                        {
                            s = s.Substring(index);
                            index = s.IndexOf("\\");
                            if (index > 0)
                                s = s.Substring(0, index);
                            text.AppendLine(s.Trim());
                        }
                    }
                    else
                    {
                        int index = s.IndexOf("\\");
                        if (index > 0)
                            s = s.Substring(0, index);
                        text.AppendLine(s.Trim());
                    }
                }
                else
                {
                    _errorCount++;
                }
            }

            if (p != null && p.Text.Length > 0)
            {
                p.Text = text.ToString().Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber(1);
        }

    }
}
