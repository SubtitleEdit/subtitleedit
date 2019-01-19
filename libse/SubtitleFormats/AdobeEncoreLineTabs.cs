using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdobeEncoreLineTabs : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Adobe Encore (line/tabs)";

        public override bool IsMine(List<string> lines, string fileName)
        {
            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            if (sb.ToString().Contains(Environment.NewLine + "SP_NUMBER\tSTART\tEND\tFILE_NAME"))
            {
                return false; // SON
            }

            if (sb.ToString().Contains(Environment.NewLine + "SP_NUMBER     START        END       FILE_NAME"))
            {
                return false; // SON
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                //0002       00:01:48:22       00:01:52:17       - I need those samples, fast!//- Yes, professor.
                string text = p.Text;
                text = text.Replace("<i>", "@Italic@");
                text = text.Replace("</i>", "@Italic@");
                text = text.Replace(Environment.NewLine, "//");
                sb.AppendLine($"{index:0000}\t{EncodeTimeCode(p.StartTime)}\t{EncodeTimeCode(p.EndTime)}\t{HtmlUtil.RemoveHtmlTags(text, true)}");
            }
            return sb.ToString();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //00:03:15:22 (last is frame)
            return time.ToHHMMSSFF();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0002       00:01:48:22       00:01:52:17       - I need those samples, fast!//- Yes, professor.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line.Replace("       ", "\t");
                if (RegexTimeCodes.IsMatch(s))
                {
                    var temp = s.Split('\t');
                    if (temp.Length > 1)
                    {
                        string start = temp[1];
                        string end = temp[2];

                        string[] startParts = start.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                        string[] endParts = end.Split(SplitCharColon, StringSplitOptions.RemoveEmptyEntries);
                        if (startParts.Length == 4 && endParts.Length == 4)
                        {
                            try
                            {
                                string text = s.Remove(0, RegexTimeCodes.Match(s).Length - 1).Trim();
                                if (!text.Contains(Environment.NewLine))
                                {
                                    text = text.Replace("//", Environment.NewLine);
                                }

                                if (text.Contains("@Italic@"))
                                {
                                    bool italicOn = false;
                                    while (text.Contains("@Italic@"))
                                    {
                                        var index = text.IndexOf("@Italic@", StringComparison.Ordinal);
                                        string italicTag = "<i>";
                                        if (italicOn)
                                        {
                                            italicTag = "</i>";
                                        }

                                        text = text.Remove(index, "@Italic@".Length).Insert(index, italicTag);
                                        italicOn = !italicOn;
                                    }
                                    text = HtmlUtil.FixInvalidItalicTags(text);
                                }
                                p = new Paragraph(DecodeTimeCodeFramesFourParts(startParts), DecodeTimeCodeFramesFourParts(endParts), text);
                                subtitle.Paragraphs.Add(p);
                            }
                            catch (Exception exception)
                            {
                                _errorCount++;
                                System.Diagnostics.Debug.WriteLine(exception.Message);
                            }
                        }
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // skip empty lines
                }
                else if (p != null)
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

    }
}
