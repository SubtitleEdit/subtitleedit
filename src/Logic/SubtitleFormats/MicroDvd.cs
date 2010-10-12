using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class MicroDvd : SubtitleFormat
    {
        readonly Regex _regexMicroDvdLine = new Regex(@"^\{-?\d+}\{-?\d+}.*$", RegexOptions.Compiled);            

        public override string Extension
        {
            get { return ".sub"; }
        }

        public override string Name
        {
            get { return "MicroDVD"; }
        }

        public override bool HasLineNumber
        {
            get { return false; }
        }

        public override bool IsTimeBased
        {
            get { return false; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var trimmedLines = new List<string>();
            int errors = 0;
            foreach (string line in lines)
            {
                if (line.Trim().Length > 0 && line.Contains("{"))
                {
                    string s = RemoveIllegalSpacesAndFixEmptyCodes(line);
                    if (_regexMicroDvdLine.IsMatch(s))
                        trimmedLines.Add(s);
                    else
                        errors++;
                }
                else
                {
                    errors++;
                }
            }

            return trimmedLines.Count > errors;
        }

        private string RemoveIllegalSpacesAndFixEmptyCodes(string line)
        {
            int index = line.IndexOf("}");
            if (index >= 0 && index < line.Length)
            {
                index = line.IndexOf("}", index+1);
                if (index >= 0 && index +1 < line.Length)
                {
                    if (line.IndexOf("{}") >= 0 && line.IndexOf("{}") < index)
                    {
                        line = line.Insert(line.IndexOf("{}") +1, "0"); // set empty time codes to zero
                        index++;
                    }

                    while (line.IndexOf(" ")  >= 0 && line.IndexOf(" ") < index)
                    {
                        line = line.Remove(line.IndexOf(" "), 1);
                        index--;
                    }
                }
            }
            return line;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append("{");
                sb.Append(p.StartFrame.ToString());
                sb.Append("}{");
                sb.Append(p.EndFrame.ToString());
                sb.Append("}");

                string text = p.Text.Replace(Environment.NewLine, "|");
                text = text.Replace("<b>","{Y:b}");
                text = text.Replace("</b>", string.Empty);
                text = text.Replace("<i>","{Y:i}");
                text = text.Replace("</i>", string.Empty);
                text = text.Replace("<u>","{Y:u}");
                text = text.Replace("</u>", string.Empty);

                sb.AppendLine(text);
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            foreach (string line in lines)
            {
                string s = RemoveIllegalSpacesAndFixEmptyCodes(line);
                if (_regexMicroDvdLine.IsMatch(s))
                {
                    try
                    {
                        int textIndex = GetTextStartIndex(s);
                        if (textIndex < s.Length)
                        {
                            string text = s.Substring(textIndex);
                            string temp = s.Substring(0, textIndex - 1);
                            string[] frames = temp.Replace("}{", ":").Replace("{", string.Empty).Replace("}", string.Empty).Split(':');

                            int startFrame = int.Parse(frames[0]);
                            int endFrame = int.Parse(frames[1]);

                            subtitle.Paragraphs.Add(new Paragraph(startFrame, endFrame, text.Replace("|", Environment.NewLine)));
                            
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
                if (p.StartFrame == 0 && previous != null)
                {
                    p.StartFrame = previous.EndFrame + 1;
                }
                if (p.EndFrame == 0)
                {
                    p.EndFrame = p.StartFrame;
                }
                i++;
            }

            subtitle.Renumber(1);
        }

        private static int GetTextStartIndex(string line)
        {
            int i = 0;
            int tagCount = 0;
            while (i < line.Length && tagCount < 4)
            {
                if (line[i] == '{' || line[i] == '}')
                    tagCount++;

                i++;
            }
            return i;
        }
    }
}
