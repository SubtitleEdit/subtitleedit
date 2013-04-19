using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.SubtitleFormats
{
    public class MicroDvd : SubtitleFormat
    {
        static Regex _regexMicroDvdLine = new Regex(@"^\{-?\d+}\{-?\d+}.*$", RegexOptions.Compiled);
        public string Errors { get; private set; }
        private StringBuilder _errors;
        private int _lineNumber;

        public override string Extension
        {
            get { return ".sub"; }
        }

        public override string Name
        {
            get { return "MicroDVD"; }
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
                if (line.Trim().Length > 0)
                {
                    if (line.Contains("{"))
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

                //{y:b} is italics for single line
                //{Y:b} is italics for both lines

                string[] parts = p.Text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                bool italicOn = false;
                bool boldOn = false;
                bool underlineOn = false;
                var lineSb = new StringBuilder();
                foreach (string line in parts)
                {
                    if (count > 0)
                        lineSb.Append("|");

                    var pre = new StringBuilder();
                    string s = line;
                    for (int i = 0; i < 5; i++)
                    {

                        if (s.StartsWith("<i>") || italicOn)
                        {
                            italicOn = true;
                            boldOn = false;
                            underlineOn = false;
                            pre.Append("{y:i}"); // italic single line
                            s = s.Remove(0, 3);
                        }
                        else if (s.StartsWith("<b>") || boldOn)
                        {
                            italicOn = false;
                            boldOn = true;
                            underlineOn = false;
                            pre.Append("{y:b}"); // bold single line
                            s = s.Remove(0, 3);
                        }
                        else if (s.StartsWith("<u>") || underlineOn)
                        {
                            italicOn = false;
                            boldOn = true;
                            underlineOn = false;
                            pre.Append("{y:u}"); // underline single line
                            s = s.Remove(0, 3);
                        }

                        if (s.Contains("</i>"))
                            italicOn = false;

                        if (s.Contains("</b>"))
                            boldOn = false;

                        if (s.Contains("</u>"))
                            underlineOn = false;

                        if (s.StartsWith("<font "))
                        {
                            int start = s.IndexOf("<font ");
                            int end = s.IndexOf(">", start);
                            if (end > start)
                            {
                                string tag = s.Substring(start, end - start);
                                if (tag.Contains(" color="))
                                {
                                    int colorStart = tag.IndexOf(" color=");
                                    int colorEnd = tag.IndexOf("\"", colorStart + " color=".Length + 1);
                                    if (colorEnd > 0)
                                    {
                                        string color = tag.Substring(colorStart, colorEnd - colorStart);
                                        color = color.Remove(0, " color=".Length);
                                        color = color.Trim('"');
                                        color = color.Trim('\'');
                                        color = color.TrimStart('#');
                                        if (color.Length == 6)
                                        {
                                            if (s.Contains(Environment.NewLine) && s.Contains("</font>" + Environment.NewLine))
                                                pre.Append("{c:$" + color.Substring(4, 2) + color.Substring(2, 2) + color.Substring(0, 2) + "}");
                                            else
                                                pre.Append("{C:$" + color.Substring(4, 2) + color.Substring(2, 2) + color.Substring(0, 2) + "}");
                                        }
                                    }
                                }
                                if (tag.Contains(" face="))
                                {
                                    int colorStart = tag.IndexOf(" face=");
                                    int colorEnd = tag.IndexOf("\"", colorStart + " face=".Length + 1);
                                    if (colorEnd > 0)
                                    {
                                        string fontName = tag.Substring(colorStart, colorEnd - colorStart);
                                        fontName = fontName.Remove(0, " face=".Length).Trim();
                                        fontName = fontName.Trim('"');
                                        fontName = fontName.Trim('\'');
                                        if (fontName.Length > 0)
                                        {
                                            if (s.Contains(Environment.NewLine) && s.Contains("</font>" + Environment.NewLine))
                                                pre.Append("{f:" + fontName + "}");
                                            else
                                                pre.Append("{F:" + fontName + "}");
                                        }
                                    }
                                }
                                if (tag.Contains(" size="))
                                {
                                    int colorStart = tag.IndexOf(" size=");
                                    int colorEnd = tag.IndexOf("\"", colorStart + " size=".Length + 1);
                                    if (colorEnd > 0)
                                    {
                                        string fontSize = tag.Substring(colorStart, colorEnd - colorStart);
                                        fontSize = fontSize.Remove(0, " size=".Length).Trim();
                                        fontSize = fontSize.Trim('"');
                                        fontSize = fontSize.Trim('\'');
                                        if (fontSize.Length > 0)
                                        {
                                            if (s.Contains(Environment.NewLine) && s.Contains("</font>" + Environment.NewLine))
                                                pre.Append("{s:" + fontSize + "}");
                                            else
                                                pre.Append("{S:" + fontSize + "}");
                                        }
                                    }
                                }
                                s = s.Remove(0, end + 1);
                            }
                        }
                    }


                    lineSb.Append(Utilities.RemoveHtmlTags(pre + line));
                    count++;
                }
                string text = lineSb.ToString();
                int noOfLines = Utilities.CountTagInText(text,"|") +1;
                if (Utilities.CountTagInText(text, "{y:i}") == noOfLines)
                    text = "{Y:i}" + text.Replace("{y:i}", string.Empty);
                else if (noOfLines > 1 && Utilities.CountTagInText(text, "{y:b}") == noOfLines)
                    text = "{Y:b}" + text.Replace("{y:b}", string.Empty);
                else if (noOfLines > 1 && Utilities.CountTagInText(text, "{y:u}") == noOfLines)
                    text = "{Y:u}" + text.Replace("{y:u}", string.Empty);
                sb.AppendLine(Utilities.RemoveHtmlTags(text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            _errors = new StringBuilder();
            _lineNumber = 0;

            foreach (string line in lines)
            {
                _lineNumber++;
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

                            string post = string.Empty;
                            string[] parts = text.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            int count = 0;
                            var lineSb = new StringBuilder();

                            foreach (string s2 in parts)
                            {

                                if (count > 0)
                                    lineSb.AppendLine();

                                s = s2.Trim();
                                var pre = new StringBuilder();
                                for (int i = 0; i < 5; i++)
                                {
                                    if (s.StartsWith("{Y:i}"))
                                    {
                                        s = s.Remove(0, 5);
                                        pre.Append("<i>");
                                        post += "</i>";
                                    }
                                    else if (s.StartsWith("{Y:b}"))
                                    {
                                        s = s.Remove(0, 5);
                                        pre.Append("<b>");
                                        post += "</b>";
                                    }
                                    else if (s.StartsWith("{Y:u}"))
                                    {
                                        s = s.Remove(0, 5);
                                        pre.Append("<u>");
                                        post += "</u>";
                                    }
                                    else if (s.StartsWith("{y:i}"))
                                    {
                                        if (parts.Length > 1)
                                        {
                                            s = s.Remove(0, 5) + "</i>";
                                            pre.Append("<i>");
                                        }
                                        else
                                        {
                                            s = s.Remove(0, 5);
                                            pre.Append("<i>");
                                            post += "</i>";
                                        }
                                    }
                                    else if (s.StartsWith("{y:b}"))
                                    {
                                        if (parts.Length > 1)
                                        {

                                            s = s.Remove(0, 5) + "</b>";
                                            pre.Append("<b>");
                                        }
                                        else
                                        {
                                            s = s.Remove(0, 5);
                                            pre.Append("<b>");
                                            post += "</b>";
                                        }
                                    }
                                    else if (s.StartsWith("{y:u}"))
                                    {
                                        if (parts.Length > 1)
                                        {
                                            s = s.Remove(0, 5) + "</u>";
                                            pre.Append("<u>");
                                        }
                                        else
                                        {
                                            s = s.Remove(0, 5);
                                            pre.Append("<u>");
                                            post += "</u>";
                                        }
                                    }
                                    else if (s.StartsWith("{y:b,u}") || s.StartsWith("{y:u,b}"))
                                    {
                                        if (parts.Length > 1)
                                        {

                                            s = s.Remove(0, 7) + "</u></b>";
                                            pre.Append("<b><u>");
                                        }
                                        else
                                        {
                                            s = s.Remove(0, 5);
                                            pre.Append("<b><u>");
                                            post += "</u></b>";
                                        }
                                    }
                                    else if (s.StartsWith("{y:b,i}") || s.StartsWith("{y:i,b}"))
                                    {
                                        if (parts.Length > 1)
                                        {

                                            s = s.Remove(0, 7) + "</i></b>";
                                            pre.Append("<b><i>");
                                        }
                                        else
                                        {
                                            s = s.Remove(0, 5);
                                            pre.Append("<b><i>");
                                            post += "</i></b>";
                                        }
                                    }
                                    else if (s.StartsWith("{y:i,u}") || s.StartsWith("{y:u,i}"))
                                    {
                                        if (parts.Length > 1)
                                        {

                                            s = s.Remove(0, 7) + "</i></u>";
                                            pre.Append("<u><i>");
                                        }
                                        else
                                        {
                                            s = s.Remove(0, 5);
                                            pre.Append("<u><i>");
                                            post += "</i></u>";
                                        }
                                    }
                                    else if (s.StartsWith("{Y:b,u}") || s.StartsWith("{Y:u,b}"))
                                    {
                                        s = s.Remove(0, 5);
                                        pre.Append("<b><u>");
                                        post += "</u></b>";
                                    }
                                    else if (s.StartsWith("{Y:b,i}") || s.StartsWith("{Y:i,b}"))
                                    {
                                        s = s.Remove(0, 5);
                                        pre.Append("<b><i>");
                                        post += "</i></b>";
                                    }
                                    else if (s.StartsWith("{Y:i,u}") || s.StartsWith("{Y:u,i}"))
                                    {
                                        s = s.Remove(0, 5);
                                        pre.Append("<i><u>");
                                        post += "</u></i>";
                                    }
                                    else if (s.Contains("{c:$"))
                                    {
                                        int start = s.IndexOf("{c:$");
                                        int end = s.IndexOf("}", start);
                                        if (end > start)
                                        {
                                            string tag = s.Substring(start, end - start);
                                            tag = tag.Remove(0, 4);
                                            if (tag.Length == 6)
                                            {
                                                pre.Append("<font color=\"#" + tag.Substring(4, 2) + tag.Substring(2, 2) + tag.Substring(0, 2) + "\">");
                                                s = s.Remove(start, end - start + 1) + "</font>";
                                            }
                                        }
                                    }
                                    else if (s.Contains("{C:$")) // uppercase=all lines
                                    {
                                        int start = s.IndexOf("{C:$");
                                        int end = s.IndexOf("}", start);
                                        if (end > start)
                                        {
                                            string tag = s.Substring(start, end - start);
                                            tag = tag.Remove(0, 4);
                                            if (tag.Length == 6)
                                            {
                                                pre.Append("<font color=\"#" + tag.Substring(4, 2) + tag.Substring(2, 2) + tag.Substring(0, 2) + "\">");
                                                s = s.Remove(start, end - start + 1);
                                                post += "</font>";
                                            }
                                        }
                                    }
                                    else if (s.Contains("{f:"))
                                    {
                                        int start = s.IndexOf("{f:");
                                        int end = s.IndexOf("}", start);
                                        if (end > start)
                                        {
                                            string tag = s.Substring(start, end - start);
                                            tag = tag.Remove(0, 3).Trim();
                                            if (tag.Length > 0)
                                            {
                                                pre.Append( "<font face=\"" + tag + "\">");
                                                s = s.Remove(start, end - start + 1) + "</font>";
                                            }
                                        }
                                    }
                                    else if (s.Contains("{F:")) // uppercase=all lines
                                    {
                                        int start = s.IndexOf("{F:");
                                        int end = s.IndexOf("}", start);
                                        if (end > start)
                                        {
                                            string tag = s.Substring(start, end - start);
                                            tag = tag.Remove(0, 3).Trim();
                                            if (tag.Length > 0)
                                            {
                                                pre.Append("<font face=\"" + tag + "\">");
                                                s = s.Remove(start, end - start + 1);
                                                post += "</font>";
                                            }
                                        }
                                    }
                                    else if (s.Contains("{s:"))
                                    {
                                        int start = s.IndexOf("{s:");
                                        int end = s.IndexOf("}", start);
                                        if (end > start)
                                        {
                                            string tag = s.Substring(start, end - start);
                                            tag = tag.Remove(0, 3).Trim();
                                            if (tag.Length > 0)
                                            {
                                                pre.Append("<font size=\"" + tag + "\">");
                                                s = s.Remove(start, end - start + 1) + "</font>";
                                            }
                                        }
                                    }
                                    else if (s.Contains("{S:")) // uppercase=all lines
                                    {
                                        int start = s.IndexOf("{S:");
                                        int end = s.IndexOf("}", start);
                                        if (end > start)
                                        {
                                            string tag = s.Substring(start, end - start);
                                            tag = tag.Remove(0, 3).Trim();
                                            if (tag.Length > 0)
                                            {
                                                pre.Append("<font size=\"" + tag + "\">");
                                                s = s.Remove(start, end - start + 1);
                                                post += "</font>";
                                            }
                                        }
                                    }
                                }

                                s = s.Replace("{Y:i}", string.Empty).Replace("{y:i}", string.Empty);
                                s = s.Replace("{Y:b}", string.Empty).Replace("{y:b}", string.Empty);
                                s = s.Replace("{Y:u}", string.Empty).Replace("{y:u}", string.Empty);
                                lineSb.Append(pre + s);
                                count++;
                            }
                            text = lineSb.ToString() + post;
                            subtitle.Paragraphs.Add(new Paragraph(startFrame, endFrame, text));
                        }
                    }
                    catch
                    {
                        _errorCount++;
                        if (_errors.Length < 2000)
                            _errors.AppendLine(string.Format(Configuration.Settings.Language.Main.LineNumberXErrorReadingFromSourceLineY, _lineNumber, line));
                    }
                }
                else
                {
                    _errorCount++;
                    if (_errors.Length < 2000)
                        _errors.AppendLine(string.Format(Configuration.Settings.Language.Main.LineNumberXErrorReadingFromSourceLineY, _lineNumber, line));
                }
            }

            int j = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                Paragraph previous = subtitle.GetParagraphOrDefault(j - 1);
                if (p.StartFrame == 0 && previous != null)
                {
                    p.StartFrame = previous.EndFrame + 1;
                }
                if (p.EndFrame == 0)
                {
                    p.EndFrame = p.StartFrame;
                }
                j++;
            }

            subtitle.Renumber(1);
            Errors = _errors.ToString();
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
