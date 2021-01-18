using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// .CHK subtitle file format - 128 bytes blocks, first byte in block is id (01==text)
    /// </summary>
    public class Chk : SubtitleFormat
    {
        private readonly Encoding _codePage = Encoding.GetEncoding(850);
        // private string _languageId = "DEN"; // English

        public override string Extension => ".chk";

        public const string NameOfFormat = "CHK";

        public override string Name => NameOfFormat;

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName.EndsWith(".chk", StringComparison.OrdinalIgnoreCase))
            {
                var buffer = FileUtil.ReadAllBytesShared(fileName);
                return buffer.Length > 0 && buffer[0] == 0x1d;
            }
            return false;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            return "Not implemented!";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            var buffer = FileUtil.ReadAllBytesShared(fileName);
            int index = 256;
            _errorCount = 0;
            subtitle.Paragraphs.Clear();
            while (index < buffer.Length)
            {
                Paragraph p = ReadParagraph(buffer, index);
                if (p != null)
                {
                    subtitle.Paragraphs.Add(p);
                }

                index += 128;
            }

            if (subtitle.Paragraphs.Count > 1)
            {
                if (string.IsNullOrWhiteSpace(subtitle.Paragraphs[0].Text))
                {
                    subtitle.Paragraphs.RemoveAt(0);
                }

                if (string.IsNullOrWhiteSpace(subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text))
                {
                    subtitle.Paragraphs.RemoveAt(subtitle.Paragraphs.Count - 1);
                }
            }

            subtitle.Renumber();
        }

        private Queue<Paragraph> _timeCodeQueue = new Queue<Paragraph>();

        private Paragraph ReadParagraph(byte[] buffer, int index)
        {
            if (buffer[index] == 1 && _timeCodeQueue.Count > 0) // text
            {
                var sb = new StringBuilder();
                int skipCount = 0;
                int textLength = buffer[index + 2] - 11;
                int start = index + 13;

                for (int i = 0; i <= textLength; i++)
                {
                    if (skipCount > 0)
                    {
                        skipCount--;
                    }
                    else if (buffer[index + 13 + i] == 0xFE)
                    {
                        sb.Append(GetText(buffer, start, index + i + 13));
                        start = index + 13 + i + 3;
                        skipCount = 2;
                        sb.AppendLine();
                    }
                    else if (buffer[index + 13 + i] == 0)
                    {
                        sb.Append(GetText(buffer, start, index + i + 13));
                        break;
                    }
                    if (i == textLength)
                    {
                        sb.Append(GetText(buffer, start, index + i + 13 + 1));
                    }
                }

                Paragraph p;
                if (_timeCodeQueue.Count > 0)
                {
                    p = _timeCodeQueue.Dequeue();
                }
                else
                {
                    p = new Paragraph();
                }

                p.Number = buffer[index + 3] * 256 + buffer[index + 4]; // Subtitle number
                p.Text = sb.ToString();
                //if (p.Number == 0 && p.Text.StartsWith("LANG:", StringComparison.Ordinal) && p.Text.Length > 8)
                //{
                //    _languageId = p.Text.Substring(5, 3);
                //}
                return p;
            }

            if (buffer[index] == 0x0a && _timeCodeQueue.Count > 0)
            {
                // ?
            }
            else if (buffer[index] == 0x09 && _timeCodeQueue.Count > 0)
            {
                // ?
            }
            else // time codes
            {
                var newTimeCodes = new List<Paragraph>();
                for (int i = 0; i < 15; i++)
                {
                    int start = index + 2 + (i * 8);
                    int totalFrameNumber = (buffer[start + 3] << 16) + (buffer[start + 5] << 8) + buffer[start + 4];
                    int durationInFrames = buffer[start + 6];
                    var p = new Paragraph(string.Empty, FramesToMilliseconds(totalFrameNumber), FramesToMilliseconds(totalFrameNumber + durationInFrames));
                    newTimeCodes.Add(p);
                }

                if (_timeCodeQueue.Count != 15 || IsSequential(newTimeCodes))
                {
                    _timeCodeQueue = new Queue<Paragraph>(newTimeCodes);
                }
            }
            return null;
        }

        private static bool IsSequential(List<Paragraph> newTimeCodes)
        {
            var lastMs = 0.0;
            foreach (var p in newTimeCodes)
            {
                if (p.StartTime.TotalMilliseconds < lastMs)
                {
                    return false;
                }
                lastMs = p.StartTime.TotalMilliseconds;
            }
            return true;
        }

        private string GetText(byte[] buffer, int start, int end)
        {
            string text = string.Empty;
            if (buffer[start] == 0x1f && buffer[start + 1] == 0x57 && buffer[start + 2] == 0x31 && buffer[start + 3] == 0x36) // W16
            {
                if (end - start > 4)
                {
                    text = Encoding.GetEncoding(950).GetString(buffer, start + 4, end - start - 4);
                }
            }
            else
            {
                if (end - start > 0)
                {
                    text = _codePage.GetString(buffer, start, end - start);
                }
            }
            if (text.Length > 4 && text[0] == 0x1f && text[1] == 'R' && text[4] == '.' && CharUtils.IsDigit(text[2]) && CharUtils.IsDigit(text[3]))
            {
                text = text.Remove(0, 5);
            }

            // special language codes...
            text = text.Replace("ÔA", "Á");
            text = text.Replace("ÔE", "É");
            text = text.Replace("ÔI", "Í");
            text = text.Replace("ÓN", "Ñ");
            text = text.Replace("ÔO", "Ó");
            text = text.Replace("ÔU", "Ú");
            text = text.Replace("Ôa", "á");
            text = text.Replace("Ôe", "é");
            text = text.Replace("Ôi", "í");
            text = text.Replace("Ón", "ñ");
            text = text.Replace("Ôo", "ó");
            text = text.Replace("Ôu", "ú");

            text = text.Replace("ÒA", "À");
            text = text.Replace("ÒE", "È");
            text = text.Replace("ÒU", "Ù");
            text = text.Replace("Òa", "à");
            text = text.Replace("Òe", "è");
            text = text.Replace("Òu", "ù");

            text = text.Replace("ÕU", "Ü");
            text = text.Replace("ÕA", "Ä");
            text = text.Replace("ÕO", "Ö");
            text = text.Replace("Õu", "ü");
            text = text.Replace("Õa", "ä");
            text = text.Replace("Õo", "ö");

            text = text.Replace("õa", "â");
            text = text.Replace("õe", "ê");
            text = text.Replace("õi", "î");
            text = text.Replace("õu", "û");
            text = text.Replace("õA", "Â");
            text = text.Replace("õE", "Ê");
            text = text.Replace("õI", "Î");
            text = text.Replace("õU", "Û");

            return ApplyFont(text);
        }

        private static string ApplyFont(string text)
        {
            var sb = new StringBuilder();
            string post = string.Empty;
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == 01 && i < text.Length - 4 && text[i + 1] == 0x1D && text[i + 2] == 07)
                {
                    if (post != string.Empty)
                    {
                        sb.Append("</font>");
                    }

                    sb.Append("<font color=\"Red\">");
                    post = "<font>";
                    i += 2;
                }
                else if (text[i] == 01 && i < text.Length - 4 && text[i + 1] == 06)
                {
                    if (post != string.Empty)
                    {
                        sb.Append("</font>");
                    }

                    sb.Append("<font color=\"Cyan\">");
                    post = "<font>";
                    i++;
                }
                else if (text[i] == 2)
                {
                    if (post != string.Empty)
                    {
                        sb.Append("</font>");
                    }

                    sb.Append("<font color=\"Green\">");
                    post = "<font>";
                }
                else if (text[i] == 3)
                {
                    if (post != string.Empty)
                    {
                        sb.Append("</font>");
                    }

                    sb.Append("<font color=\"Yellow\">");
                    post = "<font>";
                }
                else if (text[i] == 6)
                {
                    if (post != string.Empty)
                    {
                        sb.Append("</font>");
                    }

                    sb.Append("<font color=\"Cyan\">");
                    post = "<font>";
                }
                else if (text[i] == 7)
                {
                    if (post != string.Empty)
                    {
                        sb.Append("</font>");
                    }

                    sb.Append("<font color=\"Red\">");
                    post = "<font>";
                }
                else
                {
                    sb.Append(text[i]);
                }
                i++;
            }

            text = sb + post;
            if (string.IsNullOrWhiteSpace(HtmlUtil.RemoveHtmlTags(text)))
            {
                return string.Empty;
            }

            return text;
        }
    }
}
