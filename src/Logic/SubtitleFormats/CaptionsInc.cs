using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class CaptionsInc : SubtitleFormat
    {

        public override string Extension
        {
            get { return ".cin"; }
        }

        public override string Name
        {
            get { return "Caption Inc"; }
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

            string name = Path.GetFileNameWithoutExtension(fileName);
            byte[] buffer = Encoding.ASCII.GetBytes(name);
            for (int i = 0; i < buffer.Length && i < 8; i++)
                fs.WriteByte(buffer[i]);
            while (fs.Length < 8)
                fs.WriteByte(0x20);

            buffer = Encoding.ASCII.GetBytes("00000617");
            fs.Write(buffer, 0, buffer.Length);
            
            buffer = Encoding.ASCII.GetBytes("00011818");
            fs.Write(buffer, 0, buffer.Length);

            buffer = Encoding.ASCII.GetBytes("program description                                                                                                                                                                                             CPC CaptionMaker     D");
            fs.Write(buffer, 0, buffer.Length);


            // paragraphs
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                buffer = new byte[] { 0x0D, 0x0A, 0xFE, 0x30, 0x34, 0x31, 0x30, 0x30, 0x30, 0x30, 0x30, 0x36, 0x31, 0x37, 0x0D, 0x0A, 0x14, 0x20, 0x14, 0x2E, 0x14 };
                fs.Write(buffer, 0, buffer.Length);

                buffer = Encoding.GetEncoding(1252).GetBytes(p.Text);
                fs.Write(buffer, 0, buffer.Length);


                buffer = new byte[] { 0x14, 0x2C, 0x14, 0x2F };
                fs.Write(buffer, 0, buffer.Length);

                //WriteTime(fs, p.StartTime);
                //WriteTime(fs, p.EndTime);
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
                if (!fileName.ToLower().EndsWith(".cin"))
                    return false;

                var sub = new Subtitle();
                LoadSubtitle(sub, lines, fileName);
                return sub.Paragraphs.Count > 0;
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        private TimeCode DecodeTimeStamp(string timeCode)
        {
            try
            {
                return new TimeCode(int.Parse(timeCode.Substring(0, 2)), int.Parse(timeCode.Substring(2, 2)), int.Parse(timeCode.Substring(4, 2)), FramesToMilliseconds(int.Parse(timeCode.Substring(6, 2))));
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
                return new TimeCode(0, 0, 0, 0);
            }
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = File.ReadAllBytes(fileName);

            int i = 256;
            Paragraph last = null;
            while (i < buffer.Length - 20)
            {
                var p = new Paragraph();

                while (buffer[i] != 0xfe && i < buffer.Length - 20)
                {
                    i++;
                }
                if (buffer[i] == 0xfe)
                {
                    i += 4;
                    string startTime = Encoding.ASCII.GetString(buffer, i, 8);
                    i += 8;
                    if (Utilities.IsInteger(startTime))
                    {
                        p.StartTime = DecodeTimeStamp(startTime);
                    }
                }

                bool startFound = false;
                bool textEnd = false;
                while (!startFound && !textEnd && i < buffer.Length - 20)
                {
                    if (buffer[i] == 0x0d)
                        i++;
                    else if (buffer[i] == 0x0a)
                        ;
                    else if (buffer[i] == 0x14 && buffer[i + 1] == 0x2c) // text end
                        textEnd = true;
                    else if (buffer[i] <= 0x20) // text start
                        i++;
                    else
                        startFound = true;

                    i++;
                }
                i++;

                if (!textEnd)
                {
                    i-=2;
                    int start = i;
                    int textLength = 0;

                    while (!textEnd && i < buffer.Length - 20)
                    {
                        if (buffer[i] == 0x14 && buffer[i + 1] == 0x2c) // text end
                            textEnd = true;
                        else if (buffer[i] == 0xd && buffer[i + 1] == 0xa) // text end
                            textEnd = true;
                        textLength++;
                        i++;
                    }
                    i++;
                    if (start + textLength < buffer.Length - 10 && textLength > 1)
                    {
                        string text = Encoding.GetEncoding(1252).GetString(buffer, start, textLength - 1);
                        text = text.Replace(Encoding.GetEncoding(1252).GetString(new byte[] { 0x14, 0x70 }, 0, 2), Environment.NewLine);
                        p.Text = text;
                        subtitle.Paragraphs.Add(p);
                        last = p;
                    }
                }
                while (i < buffer.Length && buffer[i] != 0xa)
                    i++;                
                i++;
                
                if (buffer[i] == 0xfe)
                {
                    string endTime = Encoding.ASCII.GetString(buffer, i+ 4, 8);
                    if (Utilities.IsInteger(endTime))
                    {
                        p.EndTime = DecodeTimeStamp(endTime);
                    }
                }
            }
            if (last != null && last.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetDisplayMillisecondsFromText(last.Text);

            subtitle.Renumber(1);
        }

    }
}