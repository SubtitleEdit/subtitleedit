using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Chk  : SubtitleFormat
    {
        private Encoding _codePage = Encoding.GetEncoding(850);
        private string _languageId = "EN";

        public override string Extension
        {
            get { return ".chk"; }
        }

        public override string Name
        {
            get { return "CHK"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName.ToLower().EndsWith(".chk"))
            {
                //TODO: something like checking the first bytes...
                return true;
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var buffer = Utilities.ReadAllBytes(fileName);
            int index = 256;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            while (index < buffer.Length)
            {
                Paragraph p = ReadParagraph(buffer, index);
                if (p != null)
                    subtitle.Paragraphs.Add(p);
                index += 128;
            }
        }

        private Queue<Paragraph> _stack = new Queue<Paragraph>();

        private Paragraph ReadParagraph(byte[] buffer, int index)
        {
            if (buffer[index] != 0x0a && buffer[index] != 1)
            {
                _stack = new Queue<Paragraph>();
                for (int i = 0; i < 15; i++)
                {
                    int start = index + 2 + (i*8);
                    int startBig = buffer[start + 3];
                    int startFrame = buffer[start + 4];
                    int endBig = buffer[start + 5];
                    int endFrame = buffer[start + 6];
                    Paragraph p = new Paragraph();
                    //p.StartTime.TotalMilliseconds = startFrame * Configuration.Settings.General.CurrentFrameRate + startBig * 256;
                    //p.EndTime.TotalMilliseconds = endFrame * Configuration.Settings.General.CurrentFrameRate + endBig * 256;
                    p.StartTime.TotalMilliseconds = startFrame * Configuration.Settings.General.CurrentFrameRate + startBig * new TimeSpan(0, 43, 41, 600).TotalMilliseconds;
                    p.EndTime.TotalMilliseconds = endFrame * Configuration.Settings.General.CurrentFrameRate + endBig * new TimeSpan(0, 43, 41, 600).TotalMilliseconds;
                    _stack.Enqueue(p);
                }

            }
            else if (buffer[index] != 1)
            {
                // time codes or what?

            }
            else // 01 = text
            {
                var sb = new StringBuilder();
                int skipCount = 0;
                int textLength = buffer[index + 2] - 11;
                for (int i = 0; i <= textLength; i++)
                {
                    if (skipCount > 0)
                    {
                        skipCount--;
                    }
                    else if (buffer[index + 13 + i] == 0xFE)
                    {
                        skipCount = 2;
                        sb.AppendLine();
                    }
                    else if (buffer[index + 13 + i] == 0)
                    {
                        break;
                    }
                    else
                    {
                        sb.Append(_codePage.GetString(buffer, index + 13 + i, 1));
                    }
                }
                Paragraph p;
                if (_stack.Count > 0)
                    p = _stack.Dequeue();
                else
                    p = new Paragraph();
                p.Number = buffer[index + 3] * 256 + buffer[index + 4]; // Subtitle number
                p.Text = sb.ToString();
                if (p.Number == 0 && p.Text.StartsWith("LANG:D", StringComparison.Ordinal) && p.Text.Length > 8)
                {
                    _languageId = p.Text.Substring(6, 2);
                }
                else
                {
                    if (_languageId == "SP")
                    {
                        p.Text = p.Text.Replace("ÔA", "Á");
                        p.Text = p.Text.Replace("ÔE", "É");
                        p.Text = p.Text.Replace("ÔI", "Í");
                        p.Text = p.Text.Replace("ÓN", "Ñ");
                        p.Text = p.Text.Replace("ÔO", "Ó");
                        p.Text = p.Text.Replace("ÔU", "Ú");

                        p.Text = p.Text.Replace("Ôa", "á");
                        p.Text = p.Text.Replace("Ôe", "é");
                        p.Text = p.Text.Replace("Ôi", "í");
                        p.Text = p.Text.Replace("Ón", "ñ");
                        p.Text = p.Text.Replace("Ôo", "ó");
                        p.Text = p.Text.Replace("Ôu", "ú");
                    }
                }
                return p;
            }
            return null;
        }

    }
}
