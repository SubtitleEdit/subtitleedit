using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class CsvNuendo : SubtitleFormat
    {
        private static readonly Regex RegexCsvLine = new Regex(@"^.+?,(?:[\d,:]+),(.+)?", RegexOptions.Compiled | RegexOptions.Singleline);
        private const string LineFormat = "{1}{0}{2}{0}{3}{0}{4}";
        private static string Header = string.Format(LineFormat, ",", "\"Character\"", "\"Timecode In\"", "\"Timecode Out\"", "\"Dialogue\"");

        private readonly char[] _slitChar = { ':' };

        public override string Extension => ".csv";

        public override string Name => "Csv Nuendo";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0)
            {
                if (IsValidHeader(lines[0]))
                {
                    return true;
                }
            }
            int matchCount = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                if (RegexCsvLine.IsMatch(lines[i]))
                {
                    matchCount++;
                }
            }
            return matchCount > 0;
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string text = '"' + p.Text + '"';
                sb.AppendLine(string.Format(LineFormat, ',', p.Actor, p.StartTime.ToHHMMSSFF(),
                    p.EndTime.ToHHMMSSFF(), NormalizeQuotation(text)));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            bool continuation = false;
            Paragraph p = null;

            // Token indices
            const int Actor = 0;
            const int StartTime = 1;
            const int EndTime = 2;

            foreach (string line in lines)
            {
                Match match = RegexCsvLine.Match(line);
                if (match.Success)
                {
                    // tokens: actor, start-time, end-time
                    string[] tokens = line.Substring(0, match.Groups[1].Index).Split(',');
                    try
                    {
                        var actor = Utilities.FixQuotes(tokens[Actor]);

                        var start = DecodeTime(tokens[StartTime]);
                        var end = DecodeTime(tokens[EndTime]);

                        string matchText = match.Groups[1].Value;
                        string text = Utilities.FixQuotes(matchText);

                        p = new Paragraph(start, end, text);
                        if (!string.IsNullOrEmpty(actor))
                        {
                            p.Actor = actor;
                        }

                        subtitle.Paragraphs.Add(p);
                        continuation = matchText.StartsWith('"') && !matchText.EndsWith('"');
                    }
                    catch
                    {
                        _errorCount++;
                    }
                }
                else
                {
                    if (continuation)
                    {
                        if (p.Text.Length < 300)
                        {
                            string tempLine = line.Trim();

                            // only treat one quote to be the pattern, recursive quotes are treated as inside text quotes
                            if (tempLine.EndsWith('"'))
                            {
                                // allow parsing text with quote => <"John "Foobar""> => <John "Foobar">
                                tempLine = tempLine.Substring(0, tempLine.Length - 1).TrimEnd();
                                continuation = false;
                            }
                            p.Text = (p.Text + Environment.NewLine + tempLine).TrimStart();
                        }
                        else
                        {
                            continuation = false;
                        }
                    }
                    else
                    {
                        _errorCount++;
                    }
                }
            }

            // header
            _errorCount--;
            subtitle.Renumber();
        }

        private TimeCode DecodeTime(string timeStamp) => DecodeTimeCodeFramesFourParts(timeStamp.Split(_slitChar));

        protected virtual bool IsValidHeader(string header)
        {
            // https://github.com/SubtitleEdit/subtitleedit/issues/3379
            string[] tokens = header.RemoveChar('"').Split(',');
            if (tokens.Length != 4)
            {
                return false;
            }
            return tokens[0].Equals("Character", StringComparison.OrdinalIgnoreCase) &&
                tokens[1].Equals("Timecode In", StringComparison.OrdinalIgnoreCase) &&
                tokens[2].Equals("Timecode Out", StringComparison.OrdinalIgnoreCase) &&
                tokens[3].Equals("Dialogue", StringComparison.OrdinalIgnoreCase);
        }

        protected static string NormalizeQuotation(string input)
        {
            char[] chars = new char[input.Length];
            int idxTrack = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                switch (ch)
                {
                    case '”':
                    case '“':
                    case '‟':
                    case '❝':
                    case '＂':
                    case '〝':
                    case '〞':
                        chars[idxTrack++] = '"';
                        break;

                    case '‘':
                    case '’':
                        chars[idxTrack++] = '"';
                        // skip next char if it complements captured char
                        if (i + 1 < input.Length && (input[i + 1] == '‘' || input[i + 1] == '’'))
                        {
                            i++;
                        }
                        break;

                    default:
                        chars[idxTrack++] = ch;
                        break;
                }
            }
            return new string(chars, 0, idxTrack);
        }
    }
}
