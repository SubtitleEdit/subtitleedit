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
                sb.Append($"{EncodeTime(p.StartTime)} {EncodeTime(p.EndTime)} {{\\uc0\\pard\\qc");
                var text = Utilities.RemoveSsaTags(p.Text);
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagBold, HtmlUtil.TagUnderline, HtmlUtil.TagFont);
                foreach (var line in text.SplitToLines())
                {
                    sb.Append("{");
                    sb.Append(RftEncode(line)); 
                    sb.Append("}\\par");
                }
                sb.AppendLine("}");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        private string RftEncode(string line)
        {
            var sb = new StringBuilder();
            for (var index = 0; index < line.Length; index++)
            {
                char ch = line[index];
                var number = (int)ch;
                if (ch == '\\' || ch == '{' || ch == '}')
                {
                    sb.Append($"\\{ch}");
                }
                else if (number <= byte.MaxValue)
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append($"\\u{number.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0')}");
                }
            }
            return sb.ToString();
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
                    var text = RtfDecode(rtf, "en");
                    subtitle.Paragraphs.Add(new Paragraph(text, FramesToMilliseconds(startFrame), FramesToMilliseconds(endFrame)));
                }
            }
            subtitle.Renumber();
        }

        private string RtfDecode(string text, string language)
        {
            var codeOn = false;
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
                else if (codeOn && ch != ' ')
                {

                }
                else if (codeOn && ch == ' ')
                {
                    codeOn = false;
                    sb.Append(ch);
                }
                else if (last == '\\')
                {
                    if (ch == 'u' && i + 5 < text.Length && char.IsNumber(text[i + 1]) && char.IsNumber(text[i + 2]) && char.IsNumber(text[i + 3]) && char.IsNumber(text[i + 4]))
                    {
                        var unicodeNumber = text.Substring(i + 1, 4);
                        var unescaped = char.ConvertFromUtf32(int.Parse(unicodeNumber));
                        sb.Append(unescaped);
                        i += 4;
                    }
                    if (ch == 'p' && i + 5 < text.Length && text[i + 1] == 'a' && text[i + 2] == 'r' && " \r\n\\{}".Contains(text[i + 3]))
                    {
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
                else
                {
                    sb.Append(ch);
                }
                i++;
                last = ch;
            }
            return Utilities.RemoveUnneededSpaces(sb.ToString(), language);
        }
    }
}
