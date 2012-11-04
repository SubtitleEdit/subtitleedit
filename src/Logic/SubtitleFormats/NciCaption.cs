using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    class NciCaption : SubtitleFormat
    {
        public override string Extension
        {
            get { return ".cap"; }
        }

        public override string Name
        {
            get { return "NCI Caption"; }
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

        private void WriteTime(FileStream fs, TimeCode timeCode)
        {
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                var fi = new FileInfo(fileName);
                if (fi.Length >= 640 && fi.Length < 1024000) // not too small or too big
                {
                    if (fileName.ToLower().EndsWith(".cap"))
                    {
                        byte[] buffer = File.ReadAllBytes(fileName);
                        return (buffer[0] == 0x43 &&
                                buffer[1] == 0x41 &&
                                buffer[2] == 0x50 &&
                                buffer[3] == 0x54 &&
                                buffer[4] == 0x00 &&
                                buffer[5] == 0x32 &&
                                buffer[6] == 0x2e &&
                                buffer[7] == 0x30);
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
            int minutes = buffer[index+1];
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
            subtitle.Paragraphs.Clear();
            subtitle.Header = null;
            byte[] buffer = File.ReadAllBytes(fileName);

            string title = Encoding.ASCII.GetString(buffer, 82, 66);

            int i = 128;
            while (i < buffer.Length - 66)
            {
                if (buffer[i] == 0xff && buffer[i + 1] == 0xff && buffer[i + 3] != 0xff && buffer[i - 1] != 0xff && buffer[i + 64] == 0xff && buffer[i + 65] == 0xff)
                {
                    var p = new Paragraph();
                    var sb = new StringBuilder();
                    int j = i + 4;
                    while (j < i + 64)
                    {
                        if (buffer[j] == 0)
                        {
                            break;
                        }
                        else if (buffer[j] == 0xd)
                        {
                            sb.AppendLine();
                            j += 3;
                        }
                        else if (buffer[j] == 0x87)
                        {
                            sb.Append('♪');
                            j++;
                        }
                        else
                        {
                            sb.Append(Encoding.GetEncoding(1252).GetString(buffer, j, 1));
                            j++;
                        }
                    }
                    p.Text = sb.ToString();
                    subtitle.Paragraphs.Add(p);
                    i += 62;
                }
                else
                {
                    i++;
                }
            }
            subtitle.Renumber(1);

            i = 230;
            int countTimecodes = 0;
            int start = i;
            int lastNumber = -1;
            while (i < buffer.Length - 66)
            {
                if (buffer[i] == 0xff && buffer[i + 1] == 0xff && buffer[i + 2] == 0xff && buffer[i + 3] == 0xff)
                {
                    int length = i - start - 2;
                    if (length >= 10)
                    {
                        int count = length / 14;
                        if (length % 14 == 10)
                        {
                            count++;
                        }
                        else
                        {
                            //System.Windows.Forms.MessageBox.Show("Problem at with a length of " + length.ToString() + " at file position " + (i + 2) + " which gives remainer: " + (length % 14));
                            if (length % 14 == 8)
                                count++;
                        }
                        for (int k = 0; k < count; k++)
                        {
                            int index = start + 2 + (14 * k);
                            int number = buffer[index] + buffer[index + 1] * 256;
                            if (number != lastNumber + 1)
                            {
                                int tempNumber = buffer[index-2] + buffer[index -1] * 256;
                                if (tempNumber == lastNumber + 1)
                                {
                                    index -= 2;
                                    number = tempNumber;
                                }
                            }
                            if (number > lastNumber)
                            {
                                lastNumber = number;
                                Paragraph p = subtitle.GetParagraphOrDefault(number);
                                if (p != null)
                                {
                                    if (k < count - 1)
                                    {
                                        p.StartTime = DecodeTimeCode(buffer, index + 6);
                                        p.EndTime = DecodeTimeCode(buffer, index + 10);
                                    }
                                    else
                                    {
                                        p.StartTime = DecodeTimeCode(buffer, index + 6);
                                    }
                                    countTimecodes++;
                                }
                            }
                        }
                    }
                    start = i + 2;
                    i += 5;
                }
                i++;
            }

            for (i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.GetParagraphOrDefault(i);
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null && p.EndTime.TotalMilliseconds == 0)
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
            }

            for (i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.GetParagraphOrDefault(i);
                Paragraph next = subtitle.GetParagraphOrDefault(i + 1);
                if (p.Duration.TotalMilliseconds <= 0 && next != null)
                    p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
            }

            subtitle.RemoveEmptyLines();
            Paragraph last = subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1);
            if (last != null)
                last.EndTime.TotalMilliseconds = last.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(last.Text);
            subtitle.Renumber(1);
        }

    }
}