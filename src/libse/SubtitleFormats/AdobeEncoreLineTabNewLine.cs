using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class AdobeEncoreLineTabNewLine : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d \d\d:\d\d:\d\d:\d\d \d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Adobe Encore (line#/tabs/n)";

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
            var language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                index++;
                //0002       00:01:48:22       00:01:52:17      - I need those samples, fast!
                //                                              - Yes, professor.
                string text = p.Text;
                text = text.Replace("<i>", "@Italic@");
                text = text.Replace("</i>", "@Italic@");
                text = HtmlUtil.RemoveHtmlTags(text, true);
                if (Utilities.CountTagInText(Environment.NewLine, text) > 1)
                {
                    text = Utilities.AutoBreakLineMoreThanTwoLines(text, Configuration.Settings.General.SubtitleLineMaximumLength, Configuration.Settings.General.MergeLinesShorterThan, language);
                }

                text = text.Replace(Environment.NewLine, Environment.NewLine + "\t\t\t\t");
                sb.AppendLine($"{index:0000} {EncodeTimeCode(p.StartTime)} {EncodeTimeCode(p.EndTime)}\t{text}");
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
            //0002       00:01:48:22       00:01:52:17      - I need those samples, fast!
            //                                              - Yes, professor.
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line;
                if (RegexTimeCodes.IsMatch(s))
                {
                    var temp = s.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
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
                else if (line == "\t\t\t" || line == "\t\t\t\t" || line == "\t\t\t\t\t")
                {
                    // skip empty lines
                }
                else if (line.StartsWith("\t\t\t\t", StringComparison.Ordinal) && p != null)
                {
                    if (p.Text.Length < 200)
                    {
                        p.Text = (p.Text + Environment.NewLine + line.Trim()).Trim();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line) && p != null)
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

    }
}
