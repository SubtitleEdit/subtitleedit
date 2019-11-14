using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class ProjectionSubtitleList : SubtitleFormat
    {
        public override string Extension => ".psl";
        public override string Name => "Projection Subtitle List";

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (var p in subtitle.Paragraphs)
            {
                sb.Append($"{EncodeTime(p.StartTime)} {EncodeTime(p.EndTime)} {{\\uc0\\pard\\qc ");
                var text = Utilities.RemoveSsaTags(p.Text);
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagBold, HtmlUtil.TagUnderline, HtmlUtil.TagFont);
                foreach (var line in text.SplitToLines())
                {
                    sb.Append(RftEncode(line));
                    sb.Append("\\par ");
                }
                sb.AppendLine("}");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private static string RftEncode(string line)
        {
            var sb = new StringBuilder();
            int index = 0;
            while (index < line.Length)
            {
                var ch = line[index];
                var number = (int)ch;
                if (ch == '\\' || ch == '{' || ch == '}')
                {
                    sb.Append($"\\{ch}");
                }
                else if (ch == '<' && line.Substring(index).StartsWith("<i>"))
                {
                    sb.Append("{\\i ");
                    index += 2;
                }
                else if (ch == '<' && line.Substring(index).StartsWith("</i>"))
                {
                    sb.Append(" }");
                    index += 3;
                }
                else if (number <= 127)
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append($"\\u{GetUnicodeNumber(ch).ToString(CultureInfo.InvariantCulture).PadLeft(3, '0')} ");
                }
                index++;
            }

            return sb.ToString();
        }

        private static int GetUnicodeNumber(char character)
        {
            var encoding = new UTF32Encoding();
            var bytes = encoding.GetBytes(character.ToString().ToCharArray());
            return BitConverter.ToInt32(bytes, 0);
        }

        private static string EncodeTime(TimeCode time)
        {
            return MillisecondsToFrames(time.TotalMilliseconds).ToString(CultureInfo.InvariantCulture).PadLeft(8, ' ');
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                int startText = line.IndexOf("{\\", StringComparison.Ordinal);
                if (startText < 3)
                {
                    _errorCount++;
                    if (_errorCount > 20)
                    {
                        break;
                    }
                    continue;
                }

                var frameArray = line.Substring(0, startText).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (frameArray.Length != 2)
                {
                    _errorCount++;
                    if (_errorCount > 20)
                    {
                        break;
                    }
                    continue;
                }

                if (long.TryParse(frameArray[0], NumberStyles.None, CultureInfo.InvariantCulture, out var startFrame) &&
                    long.TryParse(frameArray[1], NumberStyles.None, CultureInfo.InvariantCulture, out var endFrame))
                {
                    var rtf = line.Remove(0, startText);
                    var text = RtfDecode(rtf);
                    subtitle.Paragraphs.Add(new Paragraph(text, FramesToMilliseconds(startFrame), FramesToMilliseconds(endFrame)));
                }
            }
            subtitle.Renumber();
        }

        private static string RtfDecode(string text)
        {
            var codeOn = false;
            var italicOn = false;
            var sb = new StringBuilder();
            char last = ' ';
            var i = 0;
            while (i < text.Length)
            {
                var ch = text[i];
                if (codeOn && ch == '{')
                {
                    codeOn = false;
                }
                else if (codeOn && !char.IsWhiteSpace(ch))
                {
                    // just code - wait for whitespace
                }
                else if (codeOn && char.IsWhiteSpace(ch))
                {
                    codeOn = false;
                    sb.Append(ch);
                }
                else if (last == '\\')
                {
                    if (ch == 'u' && i + 6 < text.Length && char.IsNumber(text[i + 1]) && char.IsNumber(text[i + 2]) && char.IsNumber(text[i + 3]) && char.IsNumber(text[i + 4]) && char.IsNumber(text[i + 5]))
                    {
                        var unicodeNumber = text.Substring(i + 1, 4);
                        var unescaped = char.ConvertFromUtf32(int.Parse(unicodeNumber));
                        sb.Append(unescaped);
                        if (i + 7 < text.Length && text[i + 6] == ' ')
                        {
                            i++;
                        }
                        i += 5;
                    }
                    else if (ch == 'u' && i + 5 < text.Length && char.IsNumber(text[i + 1]) && char.IsNumber(text[i + 2]) && char.IsNumber(text[i + 3]) && char.IsNumber(text[i + 4]))
                    {
                        var unicodeNumber = text.Substring(i + 1, 4);
                        var unescaped = char.ConvertFromUtf32(int.Parse(unicodeNumber));
                        sb.Append(unescaped);
                        if (i + 6 < text.Length && text[i + 5] == ' ')
                        {
                            i++;
                        }
                        i += 4;
                    }
                    else if (ch == 'u' && i + 4 < text.Length && char.IsNumber(text[i + 1]) && char.IsNumber(text[i + 2]) && char.IsNumber(text[i + 3]))
                    {
                        var unicodeNumber = text.Substring(i + 1, 3);
                        var unescaped = char.ConvertFromUtf32(int.Parse(unicodeNumber));
                        sb.Append(unescaped);
                        if (i + 5 < text.Length && text[i + 4] == ' ')
                        {
                            i++;
                        }
                        i += 3;
                    }
                    else if (ch == 'i' && i + 3 < text.Length && " \r\n\\{}".Contains(text[i + 1]))
                    {
                        sb.Append("<i>");
                        italicOn = true;
                        i++;
                    }
                    else if (ch == 'p' && i + 5 < text.Length && text[i + 1] == 'a' && text[i + 2] == 'r' && " \r\n\\{}".Contains(text[i + 3]))
                    {
                        if (italicOn)
                        {
                            sb.Append("</i>");
                        }
                        italicOn = false;
                        sb.AppendLine();
                        i += 2;
                    }
                    else if (ch >= 'a' && ch <= 'z' || ch == '*')
                    {
                        codeOn = true;
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
                else if (ch == '\\')
                {
                    // start of code
                }
                else if (ch == '{' || ch == '}')
                {
                    // start/end of block
                    codeOn = false;
                }
                else if (!"\r\n".Contains(ch))
                {
                    sb.Append(ch);
                }
                i++;
                last = ch;
            }
            if (italicOn)
            {
                sb.Append("</i>");
            }
            return Utilities.RemoveUnneededSpaces(sb.ToString(), string.Empty);
        }
    }
}
