using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class Chk  : SubtitleFormat
    {
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
                //try to load subtitle
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
            int index = 512;
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

        private Paragraph ReadParagraph(byte[] buffer, int index)
        {
            var sb = new StringBuilder();
            int skipCount = 0;
            for (int i = 0; i < 46; i++)
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
                    sb.Append(System.Text.Encoding.Default.GetString(buffer, index + 13 + i, 1));
                }
            }
            Paragraph p = new Paragraph();
            p.Text = sb.ToString();
            return p;
        }

    }
}
