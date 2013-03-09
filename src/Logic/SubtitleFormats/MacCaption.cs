using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class MacCaption : SubtitleFormat
    {

        static readonly Regex RegexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".mcc"; }
        }

        public override string Name
        {
            get { return "MacCaption 1.0"; }
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

        private string FixMax4LinesAndMax32CharsPerLine(string text)
        {
            var lines = text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            bool allOk = true;
            foreach (string line in lines)
            {
                if (line.Length > 32)
                    allOk = false;
            }
            if (lines.Length > 4)
                allOk = false;

            if (allOk)
                return text;

            text = Utilities.AutoBreakLine(text, 1, 32, 4);
            lines = text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            allOk = true;
            foreach (string line in lines)
            {
                if (line.Length > 32)
                    allOk = false;
            }
            if (lines.Length > 4)
                allOk = false;

            if (allOk)
                return text;

            text = AutoBreakLineMax4Lines(text, 32);
            lines = text.Trim().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            allOk = true;
            foreach (string line in lines)
            {
                if (line.Length > 32)
                    allOk = false;
            }
            if (lines.Length > 4)
                allOk = false;

            if (allOk)
                return text;

            var sb = new StringBuilder();
            int count = 0;
            foreach (string line in lines)
            {
                if (count < 4)
                {
                    if (line.Length > 32)
                        sb.AppendLine(line.Substring(0, 32));
                    else
                        sb.AppendLine(line);
                }
                count++;
            }
            return sb.ToString().Trim();
        }

        private int GetLastIndexOfSpace(string s, int endCount)
        {
            int end = endCount;
            if (end >= s.Length)
                end = s.Length - 1;

            int i = end;
            while (i > 0)
            {
                if (s[i] == ' ')
                    return i;
                i--;
            }
            return -1;
        }

        private string AutoBreakLineMax4Lines(string text, int maxLength)
        {
            string s = text.Replace(Environment.NewLine, " ");
            s = s.Replace("  ", " ");
            var sb = new StringBuilder();
            int i = GetLastIndexOfSpace(s, maxLength);
            if (i > 0)
            {
                sb.AppendLine(s.Substring(0, i));
                s = s.Remove(0, i).Trim();
                if (s.Length <= maxLength)
                    i = s.Length;
                else
                    i = GetLastIndexOfSpace(s, maxLength);
                if (i > 0)
                {
                    sb.AppendLine(s.Substring(0, i));
                    s = s.Remove(0, i).Trim();
                    if (s.Length <= maxLength)
                        i = s.Length;
                    else
                        i = GetLastIndexOfSpace(s, maxLength);
                    if (i > 0)
                    {
                        sb.AppendLine(s.Substring(0, i));
                        s = s.Remove(0, i).Trim();
                        if (s.Length <= maxLength)
                            i = s.Length;
                        else
                            i = GetLastIndexOfSpace(s, maxLength);
                        if (i > 0)
                        {
                            sb.AppendLine(s.Substring(0, i));
                        }
                        else
                        {
                            sb.Append(s);
                        }
                    }
                    else
                    {
                        sb.Append(s);
                    }
                }
                return sb.ToString().Trim();
            }
            return text;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"File Format=MacCaption_MCC V1.0
///////////////////////////////////////////////////////////////////////////////////
// Computer Prompting and Captioning Company
// Ancillary Data Packet Transfer File
//
// Permission to generate this format is granted provided that
// 1. This ANC Transfer file format is used on an as-is basis and no warranty is given, and
// 2. This entire descriptive information text is included in a generated .mcc file.
//
// General file format:
// HH:MM:SS:FF(tab)[Hexadecimal ANC data in groups of 2 characters]
// Hexadecimal data starts with the Ancillary Data Packet DID (Data ID defined in S291M)
// and concludes with the Check Sum following the User Data Words.
// Each time code line must contain at most one complete ancillary data packet.
// To transfer additional ANC Data successive lines may contain identical time code.
// Time Code Rate=[24, 25, 30, 30DF, 50, 60]
//
// ANC data bytes may be represented by one ASCII character according to the following schema:
// G FAh 00h 00h
// H 2 x (FAh 00h 00h)
// I 3 x (FAh 00h 00h)
// J 4 x (FAh 00h 00h)
// K 5 x (FAh 00h 00h)
// L 6 x (FAh 00h 00h)
// M 7 x (FAh 00h 00h)
// N 8 x (FAh 00h 00h)
// O 9 x (FAh 00h 00h)
// P FBh 80h 80h
// Q FCh 80h 80h
// R FDh 80h 80h
// S 96h 69h
// T 61h 01h
// U E1h 00h 00h
// Z 00h
//
///////////////////////////////////////////////////////////////////////////////////");
            sb.AppendLine();
            sb.AppendLine("UUID=" + Guid.NewGuid().ToString().ToUpper());// UUID=9F6112F4-D9D0-4AAF-AA95-854710D3B57A
            sb.AppendLine("Creation Program=Subtitle Edit" );
            sb.AppendLine("Creation Date=" + DateTime.Now.ToLongDateString());
            sb.AppendLine("Creation Time=" + DateTime.Now.ToShortTimeString());
            sb.AppendLine();

            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                sb.AppendLine(string.Format("{0}\t{1}", ToTimeCode(p.StartTime.TotalMilliseconds), p.Text)); //TODO: Encode text - how???
                sb.AppendLine();

                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next == null || Math.Abs(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds) > 100)
                {
                    sb.AppendLine(string.Format("{0}\t???", ToTimeCode(p.EndTime.TotalMilliseconds))); //TODO: Some end text???
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private string ToTimeCode(double totalMilliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(totalMilliseconds);
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", ts.Hours, ts.Minutes, ts.Seconds, MillisecondsToFramesMaxFrameRate(ts.Milliseconds));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var header = new StringBuilder();
            foreach (string line in lines)
            {
                string s = line.Trim();

                if (s.StartsWith("//") || s.StartsWith("File Format=MacCaption_MCC") || s.StartsWith("UUID=") ||
                    s.StartsWith("Creation Program=") || s.StartsWith("Creation Date=") || s.StartsWith("Creation Time=") ||
                    s.StartsWith("Code Rate=") || s.StartsWith("Time Code Rate=") || string.IsNullOrEmpty(s))
                {
                    header.AppendLine(line);
                }
                else
                {
                    var match = RegexTimeCodes.Match(s);
                    if (match.Success)
                    {
                        TimeCode startTime = ParseTimeCode(s.Substring(0, match.Length - 1));
                        string text = GetSccText(s.Substring(match.Index), ref _errorCount);

                        if (text == "942c 942c" || text == "942c")
                        {
                            p.EndTime = new TimeCode(TimeSpan.FromMilliseconds(startTime.TotalMilliseconds));
                        }
                        else
                        {
                            p = new Paragraph(startTime, new TimeCode(TimeSpan.FromMilliseconds(startTime.TotalMilliseconds)), text);
                            subtitle.Paragraphs.Add(p);
                        }
                    }
                }
            }
            for (int i = subtitle.Paragraphs.Count - 2; i >= 0; i--)
            {
                p = subtitle.GetParagraphOrDefault(i);
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (p != null && next != null && p.EndTime.TotalMilliseconds == p.StartTime.TotalMilliseconds)
                    p.EndTime = new TimeCode(TimeSpan.FromMilliseconds(next.StartTime.TotalMilliseconds));
                if (next != null && string.IsNullOrEmpty(next.Text))
                    subtitle.Paragraphs.Remove(next);
            }
            p = subtitle.GetParagraphOrDefault(0);
            if (p != null && string.IsNullOrEmpty(p.Text))
                subtitle.Paragraphs.Remove(p);

            subtitle.Renumber(1);
        }

        public static string GetSccText(string s, ref int errorCount)
        {
            string[] parts = s.Split(" \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();
            foreach (string part in ExecuteReplacesAndGetParts(parts))
            {
                try
                {
                    //TODO: How to decode????
                    int num = int.Parse(part, System.Globalization.NumberStyles.HexNumber);
                    if (num >= 32 && num <= 255)
                    {
                        var encoding = Encoding.GetEncoding("ISO-8859-1");
                        byte[] bytes = new byte[1];
                        bytes[0] = (byte)num;                        
                        sb.Append(encoding.GetString(bytes));
                    }
                }
                catch
                { 
                }
            }
            string res = sb.ToString().Replace("<i></i>", string.Empty).Replace("</i><i>", string.Empty);
            res = res.Replace("♪♪", "♪");
            res = res.Replace("'''", "'");
            res = res.Replace("  ", " ").Replace("  ", " ").Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
            return Utilities.FixInvalidItalicTags(res);
        }

        private static List<string> ExecuteReplacesAndGetParts(string[] parts)
        {
            var list = new List<string>();
            if (parts.Length != 2)
            {
                return list;
            }
            string s = parts[1];
            s = s.Replace("G", "FA0000");
            s = s.Replace("H", "FA0000FA0000");
            s = s.Replace("I", "FA0000FA0000FA0000");
            s = s.Replace("J", "FA0000FA0000FA0000FA0000");
            s = s.Replace("K", "FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("L", "FA0000FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("M", "FA0000FA0000FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("N", "FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("O", "FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000FA0000");
            s = s.Replace("P", "FB8080");
            s = s.Replace("Q", "FC8080");
            s = s.Replace("R", "FD80h80");
            s = s.Replace("S", "9669");
            s = s.Replace("T", "6101");
            s = s.Replace("U", "E10000");
            s = s.Replace("Z", "00");
            for (int i = 0; i < s.Length; i+=4)
            {
                string sub = s.Substring(i);
                if (sub.Length >= 2)
                    list.Add(sub.Substring(0, 2));
            }
            return list;
        }

        private TimeCode ParseTimeCode(string start)
        {
            string[] arr = start.Split(":;,".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            int milliseconds = (int)((1000 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(arr[3]));
            if (milliseconds > 999)
                milliseconds = 999;

            var ts = new TimeSpan(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), milliseconds);
            return new TimeCode(ts);
        }

    }
}