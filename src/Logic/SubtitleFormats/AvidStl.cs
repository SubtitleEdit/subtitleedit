using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class AvidStl : SubtitleFormat
    {
        private const int TextLength = 112;

        private static Paragraph ReadSubtitleBlock(byte[] buffer, int index)
        {
            index += 5;
            Paragraph p = new Paragraph();
            p.StartTime = ReadTimeCode(buffer, ref index);
            p.EndTime = ReadTimeCode(buffer, ref index);
            index += 3;
            for (int i = index; i < index + TextLength; i++)
            {
                if (buffer[i] == 0x8f || buffer[i] == 0)
                    buffer[i] = 32;
            }
            p.Text = System.Text.Encoding.GetEncoding(1252).GetString(buffer, index, TextLength).Trim();
            return p;
        }

        private static TimeCode ReadTimeCode(byte[] buffer, ref int index)
        {
            int hours = buffer[index];
            int minutes = buffer[index+1];
            int seconds = buffer[index+2];
            int milliseconds = FramesToMillisecondsMax999(buffer[index+3]);
            index += 4;
            return new TimeCode(hours, minutes, seconds, milliseconds);
        }

        public static void WriteSubtitleBlock(FileStream fs, Paragraph p, int number)
        {
            fs.WriteByte(0);
            fs.WriteByte((byte)(number % 256)); // number - low byte
            fs.WriteByte((byte)(number / 256)); // number - high byte
            fs.WriteByte(0xff);
            fs.WriteByte(0);
            WriteTimeCode(fs, p.StartTime);
            WriteTimeCode(fs, p.EndTime);
            fs.WriteByte(1);
            fs.WriteByte(2);
            fs.WriteByte(0);
            var buffer = System.Text.Encoding.GetEncoding(1252).GetBytes(p.Text);
            if (buffer.Length <= 128)
            {
                fs.Write(buffer, 0, buffer.Length);
                for (int i = buffer.Length; i < TextLength; i++)
                {
                    fs.WriteByte(0x8f);
                }
            }
            else
            {
                for (int i = 0; i < TextLength; i++)
                {
                    fs.WriteByte(buffer[i]);
                }
            }
        }

        private static void WriteTimeCode(FileStream fs, TimeCode tc)
        {
            fs.WriteByte((byte)(tc.Hours));
            fs.WriteByte((byte)(tc.Minutes));
            fs.WriteByte((byte)(tc.Seconds));
            fs.WriteByte((byte)(MillisecondsToFramesMaxFrameRate(tc.Milliseconds)));
        }

        public override string Extension
        {
            get { return ".stl"; }
        }

        public override string Name
        {
            get { return "Avid stl"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public void Save(string fileName, Subtitle subtitle)
        {
            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[] { 0x38, 0x35, 0x30, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x30, 0x30, 0x30, 0x39 };
            fs.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < 0xde; i++)
                fs.WriteByte(0);
            buffer = new byte[] { 0x30, 0x30, 0x30, 0x31, 0x31, 0x30, 0x30, 0x30, 0x31, 0x31, 0x30, 0x30, 0x31 };
            fs.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < 0x13; i++)
                fs.WriteByte(0);
            buffer = System.Text.Encoding.ASCII.GetBytes(subtitle.Paragraphs.Count.ToString());
            if (buffer.Length < 4)
                fs.WriteByte(0);
            if (buffer.Length < 3)
                fs.WriteByte(0);
            if (buffer.Length < 2)
                fs.WriteByte(0);
            fs.Write(buffer, 0, buffer.Length);
            while (fs.Length < 1024)
                fs.WriteByte(0);

            int subtitleNumber = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                WriteSubtitleBlock(fs, p, subtitleNumber);
                subtitleNumber++;
            }
            fs.Close();
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length > 1150 && fi.Length < 1024000) // not too small or too big
                {
                    byte[] buffer = File.ReadAllBytes(fileName);
                    if (buffer[0] == 0x38 &&
                        buffer[1] == 0x35 &&
                        buffer[2] == 0x30 &&
                        buffer[1024] == 0 &&
                        buffer[1025] == 0 &&
                        buffer[1026] == 0 &&
                        buffer[1027] == 0xff)
                    {
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

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = File.ReadAllBytes(fileName);

            int index = 1024;
            while (index <= buffer.Length - 128)
            {
                Paragraph p = ReadSubtitleBlock(buffer, index);
                subtitle.Paragraphs.Add(p);
                index += 128;
            }
            subtitle.Renumber(1);
        }

    }
}