using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AvidDvd : SubtitleFormat
    {
        //25    10:03:20:23 10:03:23:05 some text
        //I see, on my way.|New line also.
        //
        //26    10:03:31:18 10:03:34:00 even more text
        //Panessa, why didn't they give them
        //an escape route ?

        private static readonly Regex RegexTimeCode = new Regex(@"^\d+\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t.+$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Avid DVD";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null)
            {
                if (fileName.EndsWith(".dost", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (fileName.EndsWith(".sst", StringComparison.OrdinalIgnoreCase) && new SonicScenaristBitmaps().IsMine(lines, fileName))
                {
                    return false;
                }
            }

            return base.IsMine(lines, fileName);
        }

        private static string MakeTimeCode(TimeCode tc)
        {
            return tc.ToHHMMSSFF();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int count = 1;
            bool italic = false;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = p.Text;
                if (text.StartsWith('{') && text.Length > 6 && text[6] == '}')
                {
                    text = text.Remove(0, 6);
                }

                if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal))
                {
                    if (!italic)
                    {
                        italic = true;
                        sb.AppendLine("$Italic = TRUE");
                    }
                }
                else if (italic)
                {
                    italic = false;
                    sb.AppendLine("$Italic = FALSE");
                }

                text = HtmlUtil.RemoveHtmlTags(text, true);
                sb.AppendLine($"{count}\t{MakeTimeCode(p.StartTime)}\t{MakeTimeCode(p.EndTime)}\t{text.Replace(Environment.NewLine, "|")}");
                sb.AppendLine();
                count++;
            }

            return sb.ToString();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            Paragraph p = null;
            var sb = new StringBuilder();
            bool italic = false;
            foreach (string line in lines)
            {
                string s = line.TrimEnd();
                if (RegexTimeCode.IsMatch(s))
                {
                    try
                    {
                        if (p != null)
                        {
                            p.Text = sb.ToString().Replace("|", Environment.NewLine).Trim();
                            subtitle.Paragraphs.Add(p);
                        }
                        sb.Clear();
                        string[] arr = s.Split('\t');
                        if (arr.Length >= 3)
                        {
                            string text = s.Remove(0, arr[0].Length + arr[1].Length + arr[2].Length + 2).Trim();

                            if (string.IsNullOrWhiteSpace(text
                                .RemoveChar('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ':', ',')))
                            {
                                _errorCount++;
                            }

                            if (italic)
                            {
                                text = "<i>" + text + "</i>";
                            }

                            sb.AppendLine(text);
                            char[] splitChars = { ',', '.', ':' };
                            p = new Paragraph(DecodeTimeCodeFrames(arr[1], splitChars), DecodeTimeCodeFrames(arr[2], splitChars), string.Empty);
                        }
                    }
                    catch
                    {
                        _errorCount++;
                        p = null;
                    }
                }
                else if (s.StartsWith('$'))
                {
                    if (s.RemoveChar(' ').Equals("$italic=true", StringComparison.OrdinalIgnoreCase))
                    {
                        italic = true;
                    }
                    else if (s.RemoveChar(' ').Equals("$italic=false", StringComparison.OrdinalIgnoreCase))
                    {
                        italic = false;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(s))
                {
                    sb.AppendLine(s);
                }
            }
            if (p != null)
            {
                p.Text = sb.ToString().Replace("|", Environment.NewLine).Trim();
                subtitle.Paragraphs.Add(p);
            }
            subtitle.Renumber();
        }

    }
}
