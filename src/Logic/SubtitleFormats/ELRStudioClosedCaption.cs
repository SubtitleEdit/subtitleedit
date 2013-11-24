using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class ELRStudioClosedCaption : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".elr"; }
        }

        public override string Name
        {
            get { return "ELRStudio Closed Caption"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public void Save(string fileName, Subtitle subtitle)
        {
            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            fs.Close();
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.ToLower().EndsWith(".elr"))
                    {
                        byte[] buffer = File.ReadAllBytes(fileName);
                        byte[] compareBuffer = new byte[] { 0x05, 0x01, 0x0D, 0x15, 0x11, 0x00, 0xA9, 0x00, 0x45, 0x00, 0x6C, 0x00, 0x72, 0x00, 0x6F, 0x00, 0x6D, 0x00, 0x20, 0x00, 0x53, 0x00, 0x74, 0x00, 0x75, 0x00, 0x64, 0x00, 0x69, 0x00, 0x6F, 0x00 };

                        for (int i = 6; i < compareBuffer.Length; i++)
                            if (buffer[i] != compareBuffer[i])
                                return false;

                        var sub = new Subtitle();
                        LoadSubtitle(sub, lines, fileName);
                        return sub.Paragraphs.Count > 0;
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        private TimeCode DecodeTimeCode(byte[] buffer, int index)
        {
            int hour = buffer[index];
            int minutes = buffer[index + 1];
            int seconds = buffer[index + 2];
            int frames = buffer[index + 3];

            int milliseconds = (int)((1000.0 / Configuration.Settings.General.CurrentFrameRate) * frames);
            if (milliseconds > 999)
                milliseconds = 999;

            TimeCode tc = new TimeCode(hour, minutes, seconds, milliseconds);
            return tc;
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = File.ReadAllBytes(fileName);


            int i = 128;
            while (i < buffer.Length - 40)
            {
                try
                {
                    if (buffer[i] == 0xc4 && buffer[i + 1] == 9 && buffer[i + 2] == 0 && buffer[i + 3] == 0x10) // start time (hopefully)
                    {
                        var p = new Paragraph();
                        p.StartTime = GetTimeCode(buffer, i + 4);
                        i += 7;

                        // seek to endtime
                        while (i < buffer.Length - 10 && !(buffer[i] == 0xc4 && buffer[i + 1] == 9 && buffer[i + 2] == 0 && buffer[i + 3] == 0x10))
                        {
                            i++;
                        }
                        if (buffer[i] == 0xc4 && buffer[i + 1] == 9 && buffer[i + 2] == 0 && buffer[i + 3] == 0x10)
                        {
                            p.EndTime = GetTimeCode(buffer, i + 4);
                            i += 7;
                        }
                        if (p.EndTime.TotalMilliseconds == 0)
                        {
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 2000;
                        }

                        // seek to text
                        int length = 0;
                        var sb = new StringBuilder();
                        while (i < buffer.Length - 10 && !(buffer[i] == 0xc4 && buffer[i + 1] == 9 && buffer[i + 2] == 0 && buffer[i + 3] == 0x10))
                        {
                            if (buffer[i] == 9 && buffer[i + 1] == 0 && buffer[i + 2] == 0x44)
                            {
                                length = buffer[i - 1];
                                i += 12;
                                for (int j = i; j < i + (length*4); j+=4)
                                {
                                    sb.Append(Encoding.GetEncoding(1252).GetString(buffer, j, 1));
                                }
                                sb.AppendLine();
                            }
                            else
                            {
                                i++;
                            }
                        }
                        p.Text = p.Text + " " + sb.ToString().TrimEnd();
                        subtitle.Paragraphs.Add(p);
                    }
                    else
                    {
                        i++;
                    }
                }
                catch
                {
                    i += 5;
                }
            }
            subtitle.Renumber(1);
        }

        private static TimeCode GetTimeCode(byte[] buffer, int idx)
        {
            try
            {
                int frames = int.Parse(buffer[idx].ToString("X4"));
                int seconds = int.Parse(buffer[idx + 1].ToString("X4"));
                int minutes = int.Parse(buffer[idx + 2].ToString("X4"));
                int hours = int.Parse(buffer[idx + 3].ToString("X4"));
                return new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));
            }
            catch
            {
                return new TimeCode(0, 0, 0, 0);
            }
        }

    }
}