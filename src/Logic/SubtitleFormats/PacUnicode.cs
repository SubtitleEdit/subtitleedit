using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    /// <summary>
    /// UniPac
    /// </summary>
    public class PacUnicode : SubtitleFormat
    {

        public override string Extension
        {
            get { return ".fpc"; }
        }

        public override string Name
        {
            get { return "PAC Unicode (UniPac)"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    var fi = new FileInfo(fileName);
                    if (fi.Length > 100 && fi.Length < 1024000) // not too small or too big
                    {
                        byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

                        if (buffer[00] == 1 &&
                            buffer[01] == 0 &&
                            buffer[02] == 0 &&
                            buffer[03] == 0 &&
                            buffer[04] == 0 &&
                            buffer[05] == 0 &&
                            buffer[06] == 0 &&
                            buffer[07] == 0 &&
                            buffer[08] == 0 &&
                            buffer[09] == 0 &&
                            buffer[10] == 0 &&
                            buffer[11] == 0 &&
                            buffer[12] == 0 &&
                            buffer[13] == 0 &&
                            buffer[14] == 0 &&
                            buffer[15] == 0 &&
                            buffer[16] == 0 &&
                            buffer[17] == 0 &&
                            buffer[18] == 0 &&
                            buffer[19] == 0 &&
                            buffer[20] == 0 &&
                            fileName.EndsWith(".fpc", StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
                catch
                {
                    return false;
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
            byte[] buffer = FileUtil.ReadAllBytesShared(fileName);

            int index = 0;
            while (index < buffer.Length)
            {
                Paragraph p = GetPacParagraph(ref index, buffer);
                if (p != null)
                    subtitle.Paragraphs.Add(p);
            }
            if (subtitle.Paragraphs.Count > 2 && subtitle.Paragraphs[0].StartTime.TotalMilliseconds < 0.001 && subtitle.Paragraphs[1].StartTime.TotalMilliseconds < 0.001)
            {
                subtitle.Paragraphs.RemoveAt(0);
            }
            subtitle.Renumber();
        }

        private static Paragraph GetPacParagraph(ref int index, byte[] buffer)
        {
            while (index < 15)
            {
                index++;
            }
            bool con = true;
            while (con)
            {
                index++;
                if (index + 20 >= buffer.Length)
                    return null;

                if (buffer[index] == 0xFE && buffer[index - 1] == 0x80)
                    con = false;
            }

            int feIndex = index;
            byte alignment = buffer[feIndex + 1];
            byte verticalAlignment = buffer[feIndex - 1];
            var p = new Paragraph();

            int timeStartIndex = feIndex - 15;
            p.StartTime = Pac.GetTimeCode(timeStartIndex + 1, buffer);
            p.EndTime = Pac.GetTimeCode(timeStartIndex + 5, buffer);

            int textLength = buffer[timeStartIndex + 9] + buffer[timeStartIndex + 10] * 256;
            if (textLength > 500)
                return null; // probably not correct index
            int maxIndex = timeStartIndex + 10 + textLength;

            var sb = new StringBuilder();
            index = feIndex + 3;

            int textIndex = index;
            int textBegin = index;
            while (textIndex < buffer.Length && textIndex <= maxIndex)
            {
                if (buffer[textIndex] == 0xFE)
                {
                    if (textIndex > textBegin)
                    {
                        sb.AppendLine(Encoding.UTF8.GetString(buffer, textBegin, textIndex - textBegin - 1));
                        textBegin = textIndex + 7;
                        textIndex += 6;
                    }
                }
                else if (buffer[textIndex] == 0xFF)
                {
                    sb.Append(' ');
                }
                textIndex++;
            }
            if (textIndex > textBegin)
            {
                sb.Append(Encoding.UTF8.GetString(buffer, textBegin, textIndex - textBegin - 1));
            }
            p.Text = sb.ToString().Trim();
            for (int k = 0; k < p.Text.Length; k++)
            {
                if (p.Text[k] == 65533)
                {
                    p.Text = p.Text.Remove(k, 1).Insert(k, ".");
                }
            }

            index += textLength;
            if (index + 20 >= buffer.Length)
                return null;

            p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
            p.Text = p.Text.Replace(Environment.NewLine + " ", Environment.NewLine);
            p.Text = p.Text.Replace(Environment.NewLine + ">", Environment.NewLine);
            p.Text = p.Text.Replace("\0", string.Empty);

            if (verticalAlignment < 5)
            {
                if (alignment == 1) // left
                    p.Text = "{\\an7}" + p.Text;
                else if (alignment == 0) // right
                    p.Text = "{\\an9}" + p.Text;
                else
                    p.Text = "{\\an8}" + p.Text;
            }
            else if (verticalAlignment < 9)
            {
                if (alignment == 1) // left
                    p.Text = "{\\an4}" + p.Text;
                else if (alignment == 0) // right
                    p.Text = "{\\an6}" + p.Text;
                else
                    p.Text = "{\\an5}" + p.Text;
            }
            else
            {
                if (alignment == 1) // left
                    p.Text = "{\\an1}" + p.Text;
                else if (alignment == 0) // right
                    p.Text = "{\\an3}" + p.Text;
            }

            p.Text = p.Text.Replace(Convert.ToChar(0).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(1).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(2).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(3).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(4).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(5).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(6).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(7).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(8).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(11).ToString(CultureInfo.InvariantCulture), string.Empty);
            p.Text = p.Text.Replace(Convert.ToChar(12).ToString(CultureInfo.InvariantCulture), string.Empty);

            return p;
        }

    }
}
