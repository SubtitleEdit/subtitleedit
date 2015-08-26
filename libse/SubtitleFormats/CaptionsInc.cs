using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CaptionsInc : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".cin"; }
        }

        public override string Name
        {
            get { return "Caption Inc"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public static void Save(string fileName, Subtitle subtitle)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                string name = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
                byte[] buffer = Encoding.ASCII.GetBytes(name);
                for (int i = 0; i < buffer.Length && i < 8; i++)
                    fs.WriteByte(buffer[i]);
                while (fs.Length < 8)
                    fs.WriteByte(0x20);

                WriteTime(fs, subtitle.Paragraphs[0].StartTime, false); // first start time
                WriteTime(fs, subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime, false); // last end time

                buffer = Encoding.ASCII.GetBytes("Generic Unknown Unknown \"\" Unknown Unknown Unknown                                                                                                                                                                                    ");
                fs.Write(buffer, 0, buffer.Length);

                // paragraphs
                foreach (Paragraph p in subtitle.Paragraphs)
                {
                    buffer = new byte[] { 0x0D, 0x0A, 0xFE }; // header
                    fs.Write(buffer, 0, buffer.Length);

                    // styles
                    var text = new List<byte> { 0x14, 0x20, 0x14, 0x2E, 0x14, 0x54, 0x17 };
                    int noOfLines = Utilities.GetNumberOfLines(p.Text);
                    if (noOfLines == 1)
                        text.Add(0x22); // 1 line?
                    else
                        text.Add(0x21); // 2 lines?

                    var lines = p.Text.Split(Utilities.NewLineChars, StringSplitOptions.None);
                    foreach (string line in lines)
                    {
                        foreach (char ch in line)
                            text.Add(Encoding.GetEncoding(1252).GetBytes(new[] { ch })[0]);

                        // new line
                        //text.Add(0x14); // y? 0x14 was lower!? 0x17 is higher??? 12=little top 11=top, 13=most buttom?, 15=little over middle
                        //text.Add(0x72);

                        text.Add(0x14);
                        text.Add(0x74);
                        //text.Add(0x17);
                        //text.Add(0x21);
                    }

                    // codes+text length
                    buffer = Encoding.ASCII.GetBytes(string.Format("{0:000}", text.Count));
                    fs.Write(buffer, 0, buffer.Length);

                    WriteTime(fs, p.StartTime, true);

                    // write codes + text
                    foreach (byte b in text)
                        fs.WriteByte(b);

                    buffer = new byte[] { 0x14, 0x2F, 0x0D, 0x0A, 0xFE, 0x30, 0x30, 0x32, 0x30 };
                    fs.Write(buffer, 0, buffer.Length);
                    WriteTime(fs, p.EndTime, true);
                    //buffer = new byte[] { 0x14, 0x2C };
                }
            }
        }

        private static void WriteTime(FileStream fs, TimeCode timeCode, bool addEndBytes)
        {
            var time = timeCode.ToHHMMSSFF();
            var buffer = Encoding.ASCII.GetBytes(time);
            fs.Write(buffer, 0, buffer.Length);
            if (addEndBytes)
            {
                fs.WriteByte(0xd);
                fs.WriteByte(0xa);
            }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                if (!fileName.EndsWith(".cin", StringComparison.OrdinalIgnoreCase))
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

        private static TimeCode DecodeTimestamp(string timeCode)
        {
            try
            {
                return new TimeCode(int.Parse(timeCode.Substring(0, 2)), int.Parse(timeCode.Substring(2, 2)), int.Parse(timeCode.Substring(4, 2)), FramesToMillisecondsMax999(int.Parse(timeCode.Substring(6, 2))));
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
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

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
                        p.StartTime = DecodeTimestamp(startTime);
                    }
                }

                bool startFound = false;
                bool textEnd = false;
                while (!startFound && !textEnd && i < buffer.Length - 20)
                {
                    bool skip = false;
                    if (buffer[i] == 0x0d)
                        i++;
                    else if (buffer[i] == 0x0a)
                        skip = true;
                    else if (buffer[i] == 0x14 && buffer[i + 1] == 0x2c) // text end
                        textEnd = true;
                    else if (buffer[i] <= 0x20) // text start
                        i++;
                    else
                        startFound = true;

                    if (!skip)
                        i++;
                }
                i++;

                if (!textEnd)
                {
                    i -= 2;
                    var sb = new StringBuilder();
                    while (!textEnd && i < buffer.Length - 20)
                    {
                        if (buffer[i] == 0x14 && buffer[i + 1] == 0x2c) // text end
                            textEnd = true;
                        else if (buffer[i] == 0xd && buffer[i + 1] == 0xa) // text end
                            textEnd = true;
                        else if (buffer[i] <= 0x17)
                        {
                            if (!sb.ToString().EndsWith(Environment.NewLine))
                                sb.Append(Environment.NewLine);
                            i++;
                        }
                        else
                            sb.Append(Encoding.GetEncoding(1252).GetString(buffer, i, 1));
                        i++;
                    }
                    i++;
                    if (sb.Length > 0)
                    {
                        string text = sb.ToString().Trim();
                        p.Text = text;
                        subtitle.Paragraphs.Add(p);
                        last = p;
                    }
                }

                if (buffer[i] == 0xFE)
                {
                    string endTime = Encoding.ASCII.GetString(buffer, i + 4, 8);
                    if (Utilities.IsInteger(endTime))
                    {
                        p.EndTime = DecodeTimestamp(endTime);
                    }
                    while (i < buffer.Length && buffer[i] != 0xa)
                        i++;
                    i++;
                }
                else
                {
                    while (i < buffer.Length && buffer[i] != 0xa)
                        i++;
                    i++;

                    if (buffer[i] == 0xfe)
                    {
                        string endTime = Encoding.ASCII.GetString(buffer, i + 4, 8);
                        if (Utilities.IsInteger(endTime))
                        {
                            p.EndTime = DecodeTimestamp(endTime);
                        }
                    }
                }
            }
            if (last != null && last.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(last.Text);

            subtitle.Renumber();
        }

    }
}