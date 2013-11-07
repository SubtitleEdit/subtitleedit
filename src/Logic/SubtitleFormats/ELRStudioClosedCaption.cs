using System;
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
            while (i < buffer.Length - 66)
            {
                try
                {

                    if (buffer[i] == 0xfe && buffer[i + 1] == 0xff && buffer[i + 2] == 0xff && buffer[i + 3] == 0xff && buffer[i + 4] != 0xff &&
                        buffer[i + 14] == 0xff && buffer[i + 15] == 0xff && buffer[i + 16] == 0xff && buffer[i + 17] == 0xff && buffer[i + 18] != 0xff)
                    {
                        var p = new Paragraph();

                        int frames = buffer[i - 14];
                        int seconds = buffer[i - 13];
                        int minutes = buffer[i - 12];
                        int hours = buffer[i - 11];
                        p.StartTime = new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));

                        frames = buffer[i - 6];
                        seconds = buffer[i - 5];
                        minutes = buffer[i - 4];
                        hours = buffer[i - 3];
                        p.EndTime = new TimeCode(hours, minutes, seconds, FramesToMillisecondsMax999(frames));

                        p.StartTime = GetTimeCode(buffer, i - 14);
                        p.EndTime = GetTimeCode(buffer, i - 6);

                        int length = buffer[i + 27];
                        var sb = new StringBuilder();
                        int j = i + 40;
                        int charsInCurrentLine = 0;
                        while (j + 4 < buffer.Length)
                        {
                            if (buffer[j] == 0 && buffer[j + 1] == 0 && buffer[j + 2] == 0 && buffer[j + 3] == 0)
                            {
                                break;
                            }
                            else if (buffer[j] == 0 && buffer[j + 3] == 0x96)
                            {
                                sb.Append(Environment.NewLine);
                                j += 4;
                                length = buffer[j];
                                j += 9;
                                charsInCurrentLine = 0;
                            }
                            else if (charsInCurrentLine <= length)
                            {
                                sb.Append(Encoding.GetEncoding(1252).GetString(buffer, j, 1));
                                charsInCurrentLine++;
                            }
                            else
                            {
                                break;
                            }
                            j += 4;
                        }

                        p.Text = sb.ToString();
                        subtitle.Paragraphs.Add(p);
                        i += 20;
                    }
                    else
                    {
                        i++;
                    }

                }
                catch
                {
                    i += 20;
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