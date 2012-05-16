using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class Ultech130 : SubtitleFormat
    {

        private const string UltechId = "ULTECH\01.30";

        public override string Extension
        {
            get { return ".ult"; }
        }

        public override string Name
        {
            get { return "Ultech 1.30 Caption"; }
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

            byte[] buffer = buffer = Encoding.ASCII.GetBytes(UltechId);
            fs.Write(buffer, 0, buffer.Length);

            buffer = new byte[] { 0, 0, 2, 0x1D, 0 }; // ?
            fs.Write(buffer, 0, buffer.Length);

            int numberOfLines = subtitle.Paragraphs.Count;
            fs.WriteByte((byte)(numberOfLines % 256)); // paragraphs - low byte
            fs.WriteByte((byte)(numberOfLines / 256)); // paragraphs - high byte

            buffer = new byte[] { 0, 0, 0, 0, 0x1, 0, 0xF, 0x15, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0xE, 0x15, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0xD, 0x15, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0xC, 0x15, 0, 0, 0, 0, 0, 0, 0, 0x1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; // ?
            fs.Write(buffer, 0, buffer.Length);

            buffer = buffer = Encoding.ASCII.GetBytes("Subtitle Edit");
            fs.Write(buffer, 0, buffer.Length);

            while (fs.Length < 512)
                fs.WriteByte(0);

            // paragraphs
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                // convert line breaks
                var sb = new StringBuilder();
                var line = new StringBuilder();
                int skipCount = 0;
                int numberOfNewLines = Utilities.CountTagInText(p.Text, Environment.NewLine);
                bool italic = p.Text.StartsWith("<i>") && p.Text.EndsWith("</i>");
                string text = Utilities.RemoveHtmlTags(p.Text);
                if (italic)
                {
                    sb.Append(Convert.ToChar(0x11).ToString());
                    sb.Append(Convert.ToChar(0x2E).ToString());
                }
                int y = 0x74 - (numberOfNewLines * 0x20);
                for (int j=0; j<text.Length; j++)
                {
                    if (text.Substring(j).StartsWith(Environment.NewLine))
                    {
                        y += 0x20;
                        if (line.Length > 0)
                            sb.Append(line);
                        line = new StringBuilder();
                        skipCount = Environment.NewLine.Length - 1;
                        sb.Append(Convert.ToChar(0x14).ToString());
                        sb.Append(Convert.ToChar((byte)(y)).ToString());
                        if (italic)
                        {
                            sb.Append(Convert.ToChar(0x11).ToString());
                            sb.Append(Convert.ToChar(0x2E).ToString());
                        }
                    }
                    else if (skipCount == 0)
                    {
                        line.Append(text.Substring(j, 1));
                    }
                    else
                    {
                        skipCount--;
                    }
                }
                if (line.Length > 0)
                    sb.Append(line);
                text = sb.ToString();


                // codes?
                buffer = new byte[] {
                    0x14,
                    0x20,
                    0x14,
                    0x2E,
                    0x14,
                    (byte)(0x74 - (numberOfNewLines * 0x20)),
                    0x17,
                    0x21,
                };

                //if (text.StartsWith("{\\a6}"))
                //{
                //    text = p.Text.Remove(0, 5);
                //    buffer[7] = 1; // align top
                //}
                //else if (text.StartsWith("{\\a1}"))
                //{
                //    text = p.Text.Remove(0, 5);
                //    buffer[8] = 0x0A; // align left
                //}
                //else if (text.StartsWith("{\\a3}"))
                //{
                //    text = p.Text.Remove(0, 5);
                //    buffer[8] = 0x1E; // align right
                //}
                //else if (text.StartsWith("{\\a5}"))
                //{
                //    text = p.Text.Remove(0, 5);
                //    buffer[7] = 1; // align top
                //    buffer[8] = 05; // align left
                //}
                //else if (text.StartsWith("{\\a7}"))
                //{
                //    text = p.Text.Remove(0, 5);
                //    buffer[7] = 1; // align top
                //    buffer[8] = 0xc; // align right
                //}

                fs.WriteByte(0xF1); //ID of start record

                // length
                int length = text.Length + 15;
                fs.WriteByte((byte)(length));
                fs.WriteByte(0);

                // start time
                WriteTime(fs, p.StartTime);
                fs.Write(buffer, 0, buffer.Length);

                // text
                buffer = Encoding.ASCII.GetBytes(text);
                fs.Write(buffer, 0, buffer.Length); // Text starter på index 19 (0 baseret)
                fs.WriteByte(0x14);
                fs.WriteByte(0x2F);
                fs.WriteByte(0);

                // end time
                fs.WriteByte(0xF1); // id of start record
                fs.WriteByte(7); // length of end time
                fs.WriteByte(0);
                WriteTime(fs, p.EndTime);
                fs.WriteByte(0x14);
                fs.WriteByte(0x2c);
                fs.WriteByte(0);
            }

            buffer = new byte[] { 0xF1, 0x0B, 0x00, 0x00, 0x00, 0x1B, 0x18, 0x14, 0x20, 0x14, 0x2E, 0x14, 0x2F, 0x00 }; // footer
            fs.Write(buffer, 0, buffer.Length);

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
                var fi = new FileInfo(fileName);
                if (fi.Length >= 200 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.ToLower().EndsWith(".ult") || fileName.ToLower().EndsWith(".uld")) //  drop frame is often named uld, and ult for non-drop
                    {
                        byte[] buffer = File.ReadAllBytes(fileName);
                        string id = Encoding.ASCII.GetString(buffer, 0, UltechId.Length);
                        return id == UltechId;
                    }
                }
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not supported!";
        }

        private TimeCode DecodeTimeStamp(byte[] buffer, int index)
        {
            return new TimeCode(buffer[index], buffer[index + 1], buffer[index + 2], FramesToMilliseconds(buffer[index + 3]));
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = File.ReadAllBytes(fileName);

            int i = 512;
            Paragraph last = null;
            while (i < buffer.Length - 25)
            {
                var p = new Paragraph();
                int length = buffer[i+1];

                p.StartTime = DecodeTimeStamp(buffer, i + 3);
                if (last != null && last.EndTime.TotalMilliseconds == 0)
                    last.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds-1;

                if (length > 22)
                {
                    int start = i + 7;
                    var sb = new StringBuilder();
                    int skipCount = 0;
                    for (int k = start; k < length + i; k++)
                    {
                        byte b = buffer[k];
                        if (skipCount > 0)
                        {
                            skipCount--;
                        }
                        else if (b < 0x1F)
                        {
                            skipCount = 1;
                            if (sb.Length > 0)
                                sb.AppendLine();
                        }
                        else if (b == 0x80)
                        {
                            break;
                        }
                        else
                        {
                            sb.Append(Convert.ToChar(b).ToString());
                        }
                    }

                    p.Text = sb.ToString();
                    p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                    p.Text = p.Text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);

                    subtitle.Paragraphs.Add(p);
                }
                else if (last != null)
                {
                    last.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
                }
                last = p;

                i += length + 3;
            }
            if (last != null)
            {
                if (last.EndTime.TotalMilliseconds == 0)
                    last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + 2500;
                if (last != null && last.Duration.TotalMilliseconds > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                    last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetDisplayMillisecondsFromText(last.Text);
            }

            subtitle.Renumber(1);
        }

    }
}