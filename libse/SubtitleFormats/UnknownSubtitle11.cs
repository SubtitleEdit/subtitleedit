using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// MicroDVD with time codes...?
    /// </summary>
    public class UnknownSubtitle11 : SubtitleFormat
    {
        private static readonly Regex RegexMicroDvdLine = new Regex(@"^\{-?\d+:\d+:\d+}\{-?\d+:\d+:\d+}.*$", RegexOptions.Compiled);

        public override string Extension => ".sub";

        public override string Name => "Unknown 11";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var trimmedLines = new List<string>();
            int errors = 0;
            foreach (string line in lines)
            {
                if (line.Contains('{'))
                {
                    string s = RemoveIllegalSpacesAndFixEmptyCodes(line);
                    if (RegexMicroDvdLine.IsMatch(s))
                    {
                        trimmedLines.Add(s);
                    }
                    else
                    {
                        errors++;
                    }
                }
                else
                {
                    errors++;
                }
            }

            return trimmedLines.Count > errors;
        }

        private static string RemoveIllegalSpacesAndFixEmptyCodes(string line)
        {
            int index = line.IndexOf('}');
            if (index >= 0 && index < line.Length)
            {
                index = line.IndexOf('}', index + 1);
                if (index >= 0 && index + 1 < line.Length)
                {
                    var indexOfBrackets = line.IndexOf("{}", StringComparison.Ordinal);
                    if (indexOfBrackets >= 0 && indexOfBrackets < index)
                    {
                        line = line.Insert(indexOfBrackets + 1, "0"); // set empty time codes to zero
                        index++;
                    }

                    while (line.Contains(' ') && line.IndexOf(' ') < index)
                    {
                        line = line.Remove(line.IndexOf(' '), 1);
                        index--;
                    }
                }
            }
            return line;
        }

        private static string MakeTimeCode(TimeCode tc)
        {
            return string.Format("{0}:{1:00}:{2:00}", tc.Hours, tc.Minutes, tc.Seconds);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append('{');
                sb.Append(MakeTimeCode(p.StartTime));
                sb.Append("}{");
                sb.Append(MakeTimeCode(p.EndTime));
                sb.Append('}');

                //{y:b} is italics for single line
                //{Y:b} is italics for both lines

                var parts = p.Text.SplitToLines();
                int count = 0;
                bool italicOn = false;
                bool boldOn = false;
                bool underlineOn = false;
                var lineSb = new StringBuilder();
                foreach (string line in parts)
                {
                    if (count > 0)
                    {
                        lineSb.Append('|');
                    }

                    if (line.StartsWith("<i>") || italicOn)
                    {
                        italicOn = true;
                        boldOn = false;
                        underlineOn = false;
                        lineSb.Append("{y:i}"); // italic single line
                    }
                    else if (line.StartsWith("<b>") || boldOn)
                    {
                        italicOn = false;
                        boldOn = true;
                        underlineOn = false;
                        lineSb.Append("{y:b}"); // bold single line
                    }
                    else if (line.StartsWith("<u>") || underlineOn)
                    {
                        italicOn = false;
                        boldOn = false;
                        underlineOn = true;
                        lineSb.Append("{y:u}"); // underline single line
                    }

                    if (line.Contains("</i>"))
                    {
                        italicOn = false;
                    }

                    if (line.Contains("</b>"))
                    {
                        boldOn = false;
                    }

                    if (line.Contains("</u>"))
                    {
                        underlineOn = false;
                    }

                    lineSb.Append(HtmlUtil.RemoveHtmlTags(line));
                    count++;
                }
                string text = lineSb.ToString();
                int noOfLines = Utilities.CountTagInText(text, '|') + 1;
                if (noOfLines > 1 && Utilities.CountTagInText(text, "{y:i}") == noOfLines)
                {
                    text = "{Y:i}" + text.Replace("{y:i}", string.Empty);
                }
                else if (noOfLines > 1 && Utilities.CountTagInText(text, "{y:b}") == noOfLines)
                {
                    text = "{Y:b}" + text.Replace("{y:b}", string.Empty);
                }
                else if (noOfLines > 1 && Utilities.CountTagInText(text, "{y:u}") == noOfLines)
                {
                    text = "{Y:u}" + text.Replace("{y:u}", string.Empty);
                }

                sb.AppendLine(HtmlUtil.RemoveHtmlTags(text));
            }
            return sb.ToString().Trim();
        }

        private static TimeCode DecodeTimeCode(string timeCode)
        {
            string[] arr = timeCode.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
            return new TimeCode(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), 0);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            foreach (string line in lines)
            {
                string s = RemoveIllegalSpacesAndFixEmptyCodes(line);
                if (RegexMicroDvdLine.IsMatch(s))
                {
                    try
                    {
                        int textIndex = GetTextStartIndex(s);
                        if (textIndex < s.Length)
                        {
                            string text = s.Substring(textIndex);
                            string temp = s.Substring(0, textIndex - 1);
                            string[] frames = temp.Replace("}{", ";").Replace("{", string.Empty).Replace("}", string.Empty).Split(';');

                            TimeCode startTime = DecodeTimeCode(frames[0]);
                            TimeCode endTime = DecodeTimeCode(frames[1]);

                            string post = string.Empty;
                            string[] parts = text.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            int count = 0;
                            var lineSb = new StringBuilder();
                            foreach (string s2 in parts)
                            {
                                if (count > 0)
                                {
                                    lineSb.AppendLine();
                                }

                                s = s2.Trim();
                                if (s.StartsWith("{Y:i}"))
                                {
                                    s = "<i>" + s.Replace("{Y:i}", string.Empty);
                                    post += "</i>";
                                }
                                else if (s.StartsWith("{Y:b}"))
                                {
                                    s = "<b>" + s.Replace("{Y:b}", string.Empty);
                                    post += "</b>";
                                }
                                else if (s.StartsWith("{Y:u}"))
                                {
                                    s = "<u>" + s.Replace("{Y:u}", string.Empty);
                                    post += "</u>";
                                }
                                else if (s.StartsWith("{y:i}"))
                                {
                                    s = "<i>" + s.Replace("{y:i}", string.Empty) + "</i>";
                                }
                                else if (s.StartsWith("{y:b}"))
                                {
                                    s = "<b>" + s.Replace("{y:b}", string.Empty) + "</b>";
                                }
                                else if (s.StartsWith("{y:u}"))
                                {
                                    s = "<u>" + s.Replace("{y:u}", string.Empty) + "</u>";
                                }
                                s = s.Replace("{Y:i}", string.Empty).Replace("{y:i}", string.Empty);
                                s = s.Replace("{Y:b}", string.Empty).Replace("{y:b}", string.Empty);
                                s = s.Replace("{Y:u}", string.Empty).Replace("{y:u}", string.Empty);
                                lineSb.Append(s);
                                count++;
                            }
                            text = lineSb + post;
                            subtitle.Paragraphs.Add(new Paragraph(startTime, endTime, text));
                        }
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    _errorCount++;
                }
            }

            int i = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                Paragraph previous = subtitle.GetParagraphOrDefault(i - 1);
                if (p.StartTime.TotalMilliseconds == 0 && previous != null)
                {
                    p.StartTime.TotalMilliseconds = previous.EndTime.TotalMilliseconds + 1;
                }
                if (p.EndTime.TotalMilliseconds == 0)
                {
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
                }
                i++;
            }

            subtitle.Renumber();
        }

        private static int GetTextStartIndex(string line)
        {
            int i = 0;
            int tagCount = 0;
            while (i < line.Length && tagCount < 4)
            {
                if (line[i] == '{' || line[i] == '}')
                {
                    tagCount++;
                }
                i++;
            }
            return i;
        }
    }
}
