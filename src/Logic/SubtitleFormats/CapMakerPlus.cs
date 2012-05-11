using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class CapMakerPlus : SubtitleFormat
    {

        static Regex regexTimeCodes = new Regex(@"^\d\d:\d\d:\d\d:\d\d$", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".cap"; }
        }

        public override string Name
        {
            get { return "CapMaker Plus"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public void Save(string fileName, Subtitle subtitle)
        {
            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);

            byte[] buffer = new byte[127];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = 0;
            buffer[01] = 0xEA;
            buffer[02] = 0x22;
            buffer[02] = 0x01;

            // paragraphs
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = p.Text.Replace(Environment.NewLine, "\0\0\0\0");
                int length = 20 + text.Length;
                fs.WriteByte((byte)length);
                fs.WriteByte(0x61); // ??
                WriteTime(fs, p.StartTime);
                WriteTime(fs, p.EndTime);

                fs.WriteByte(0x12); // ??
                fs.WriteByte(0x03); // ??
                fs.WriteByte(0x00); // ??
                fs.WriteByte(0x00); // ??
                fs.WriteByte(0x00); // ??
                fs.WriteByte(0x00); // ??
                fs.WriteByte(0x03); // ??
                fs.WriteByte(0x0F); // ??
                fs.WriteByte(0x10); // ??

                buffer = Encoding.ASCII.GetBytes(text);
                fs.Write(buffer, 0, buffer.Length); // Text starter på index 19 (0 baseret)
            }
            fs.Close();
        }

        private void WriteTime(FileStream fs, TimeCode timeCode)
        {
            fs.WriteByte((byte)timeCode.Hours);
            fs.WriteByte((byte)timeCode.Minutes);
            fs.WriteByte((byte)timeCode.Seconds);
            fs.WriteByte((byte)MillisecondsToFrames(timeCode.Milliseconds));
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                FileInfo fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.ToLower().EndsWith(".cap"))
                    {
                        byte[] buffer = File.ReadAllBytes(fileName);
                        if (buffer[0] == 0x2b) // "+"
                                return true;
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        private TimeCode DecodeTimeCode(string[] parts)
        {
            //00:00:07:12
            string hour = parts[0];
            string minutes = parts[1];
            string seconds = parts[2];
            string frames = parts[3];

            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * int.Parse(frames));
            if (milliseconds > 999)
                milliseconds = 999;

            TimeCode tc = new TimeCode(int.Parse(hour), int.Parse(minutes), int.Parse(seconds), milliseconds);
            return tc;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = File.ReadAllBytes(fileName);

            int i = 128;
            Paragraph last = null;
            while (i < buffer.Length - 20)
            {
                if (buffer[i] == 0x0b)
                {
                    string timeCode = Encoding.ASCII.GetString(buffer, i + 1, 11);
                    if (timeCode != "00:00:00:00" && regexTimeCodes.IsMatch(timeCode))
                    {
                        Paragraph p = new Paragraph();
                        p.StartTime = DecodeTimeCode(timeCode.Split(':'));
                        int textStart = i + 25; // text starts 25 chars after time code
                        int textLength = 0;
                        while (textStart + textLength < buffer.Length && buffer[textStart + textLength] != 0)
                        {
                            textLength++;
                        }
                        if (textLength > 0)
                        {
                            p.Text = Encoding.GetEncoding(1252).GetString(buffer, textStart, textLength);
                            int rtIndex = p.Text.IndexOf("{\\rtf1");
                            if (rtIndex >= 0 && rtIndex < 10)
                            {
                                var rtBox = new System.Windows.Forms.RichTextBox();
                                try
                                {
                                    rtBox.Rtf = p.Text.Substring(rtIndex);
                                    p.Text = rtBox.Text;
                                }
                                catch (Exception exception)
                                {
                                    System.Diagnostics.Debug.WriteLine(exception.Message);
                                }
                            }
                        }
                        else
                        {
                            p.Text = string.Empty;
                        }
                        last = p;
                        subtitle.Paragraphs.Add(p);
                    }
                }
                i++;
            }
            if (last != null)
                last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetDisplayMillisecondsFromText(last.Text);

            for (i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                subtitle.Paragraphs[i].EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds;
            }
            for (i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(subtitle.Paragraphs[i].Text))
                    subtitle.Paragraphs.RemoveAt(i);
            }

            var deletes = new List<int>();
            for (i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                if (subtitle.Paragraphs[i].StartTime.TotalMilliseconds == subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds)
                {
                    subtitle.Paragraphs[i].Text += Environment.NewLine + subtitle.Paragraphs[i+1].Text;
                    subtitle.Paragraphs[i].EndTime = subtitle.Paragraphs[i + 1].EndTime;
                    deletes.Add(i + 1);
                }
            }
            deletes.Reverse();
            foreach (int index in deletes)
            {
                subtitle.Paragraphs.RemoveAt(index);
            }

            for (i = 0; i < subtitle.Paragraphs.Count - 1; i++)
            {
                if (subtitle.Paragraphs[i].StartTime.TotalMilliseconds == subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds)
                {
                }
                else if (subtitle.Paragraphs[i].EndTime.TotalMilliseconds == subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds)
                {
                    subtitle.Paragraphs[i].EndTime.TotalMilliseconds = subtitle.Paragraphs[i + 1].StartTime.TotalMilliseconds - 1;
                }
            }


            subtitle.Renumber(1);
        }

    }
}