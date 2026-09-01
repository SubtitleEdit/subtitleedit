using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Implementation of JacoSub. Specs: (http://unicorn.us.com/jacosub/jscripts.html)
    /// </summary>
    public class JacoSub : SubtitleFormat
    {
        // H:MM:SS.FF H:MM:SS.FF directive {comment} text {comment} more text...
        // 0:30:57.22 0:30:59.46 vm {opening credit} A Film By Akira Kurosawa
        private static readonly Regex RegexTimeCode = new Regex(@"^(\d:\d\d:\d\d\.\d\d) (\d:\d\d:\d\d\.\d\d)", RegexOptions.Compiled);

        public override string Extension => ".jss";

        public override string Name => "JACOsub";

        public override bool IsMine(List<string> lines, string fileName)
        {
            // only validate/check file extension if file exists
            if (File.Exists(fileName) && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return base.IsMine(lines, fileName);
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            // reset members
            subtitle.Paragraphs.Clear();
            _errorCount = 0;

            // must only be used with matched value of RegexTimeCode
            char[] timeSplitChar = { ':', '.', ' ' };

            int lineCount = lines.Count;
            int i = 0;
            while (i < lineCount)
            {
                string line = lines[i].Trim();
                Match match = null;
                if (line.Length >= 21 && !line.StartsWith('#'))
                {
                    match = RegexTimeCode.Match(line);
                }
                if (match?.Success == true)
                {
                    var text = line.Substring(match.Value.Length);
                    while (text.EndsWith('\\') && i < lineCount - 1)
                    {
                        i++;
                        text = text.TrimEnd('\\') + lines[i].Trim();
                    }
                    text = DecodeText(text.TrimEnd('\\').Trim());
                    if (!string.IsNullOrEmpty(text))
                    {
                        subtitle.Paragraphs.Add(new Paragraph
                        {
                            StartTime = DecodeTime(match.Groups[1].Value, timeSplitChar),
                            EndTime = DecodeTime(match.Groups[2].Value, timeSplitChar),
                            Text = text
                        });
                    }
                }
                else if (line.Length > 0 && !line.StartsWith('#'))
                {
                    _errorCount++;
                }
                i++;
            }
            subtitle.Renumber();
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            // 0:30:57.22 0:30:59.46 vm {opening credit} A Film By Akira Kurosawa
            string writeFormat = "{0} {1} D {2}" + Environment.NewLine;

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = EncodeTime(p.StartTime);
                string endTime = EncodeTime(p.EndTime);
                // The JACOsub escape codes are backslash codes ("\n", "\I", ...) and
                // RemoveHtmlTags(alsoSsaTags: true) treats those as ASSA tags - running it after
                // the substitutions turned the "\n" line break back into a real newline (which
                // the reader then dropped, losing the second line) and ate the style codes. Build
                // the codes via a placeholder so they are materialized only after all stripping.
                const char marker = '\u0001';
                string text = p.Text
                    .Replace("<i>", marker + "I")
                    .Replace("</i>", marker + "i")
                    .Replace("<b>", marker + "B")
                    .Replace("</b>", marker + "b")
                    .Replace("<u>", marker + "U")
                    .Replace("</u>", marker + "u");
                text = HtmlUtil.RemoveHtmlTags(text, true);
                text = text.Replace(Environment.NewLine, marker + "n").Replace(marker, '\\');
                sb.AppendFormat(writeFormat, startTime, endTime, text);
            }
            return sb.ToString();
        }

        private static TimeCode DecodeTime(string timestamp, char[] splitChars)
        {
            // H:MM:SS.FF H:MM:SS.FF
            string[] tokens = timestamp.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            // parse tokens
            int hours = int.Parse(tokens[0]);
            int minutes = int.Parse(tokens[1]);
            int seconds = int.Parse(tokens[2]);
            int milliseconds = FramesToMilliseconds(double.Parse(tokens[3], CultureInfo.InvariantCulture));

            return new TimeCode(hours, minutes, seconds, milliseconds);
        }

        /// <summary>
        /// Returns time code encoded in this format: H:MM:SS.FF H:MM:SS.FF
        /// </summary>
        private static string EncodeTime(TimeCode tc) => $"{tc.Hours:#0}:{tc.Minutes:00}:{tc.Seconds:00}.{MillisecondsToFrames(tc.Milliseconds):00}";

        private static string DecodeText(string input)
        {
            input = input.Trim();
            var sb = new StringBuilder(input.Length);
            bool directiveOn = true;
            int i = 0;
            string endTags = string.Empty;
            while (i < input.Length)
            {
                var ch = input[i];
                if (directiveOn)
                {
                    if (ch == ' ')
                    {
                        directiveOn = false;
                    }
                }
                else if (ch == '\\' && i < input.Length - 1)
                {
                    var next = input[i + 1];
                    switch (next)
                    {
                        case 'n':
                            {
                                sb.AppendLine();
                                i++;
                                break;
                            }
                        case '{':
                            {
                                sb.Append('{');
                                i++;
                                break;
                            }
                        case '~':
                            {
                                sb.Append('~');
                                i++;
                                break;
                            }
                        case '\\':
                            {
                                sb.Append('\\');
                                i++;
                                break;
                            }
                        case 'D':
                            {
                                sb.Append(DateTime.Now.ToString("dd MMM yyyy")); // DD MMM YYYY, as in 2 Apr 1996
                                i++;
                                break;
                            }
                        case 'T':
                            {
                                sb.Append(DateTime.Now.ToString("HH:mm")); // HH:MM (24-hour time)
                                i++;
                                break;
                            }
                        case 'N':
                            {
                                sb.Append(endTags);
                                endTags = string.Empty;
                                i++;
                                break;
                            }
                        case 'I':
                            {
                                endTags = "</i>" + endTags;
                                sb.Append("<i>");
                                i++;
                                break;
                            }
                        case 'i':
                            {
                                if (endTags.StartsWith("</i>", StringComparison.Ordinal))
                                {
                                    endTags = endTags.Remove(0, 4);
                                }
                                sb.Append("</i>");
                                i++;
                                break;
                            }
                        case 'B':
                            {
                                endTags = "</b>" + endTags;
                                sb.Append("<b>");
                                i++;
                                break;
                            }
                        case 'b':
                            {
                                if (endTags.StartsWith("</b>", StringComparison.Ordinal))
                                {
                                    endTags = endTags.Remove(0, 4);
                                }
                                sb.Append("</b>");
                                i++;
                                break;
                            }
                        case 'U':
                            {
                                endTags = "</u>" + endTags;
                                sb.Append("<u>");
                                i++;
                                break;
                            }
                        case 'u':
                            {
                                if (endTags.StartsWith("</u>", StringComparison.Ordinal))
                                {
                                    endTags = endTags.Remove(0, 4);
                                }
                                sb.Append("</u>");
                                i++;
                                break;
                            }
                    }
                }
                else if (ch == '{') // comment
                {
                    var endComment = input.IndexOf('}', i);
                    if (endComment < 0)
                    {
                        i = input.Length;
                    }
                    else
                    {
                        i = endComment;
                    }
                }
                else if (ch == '~') // hard space
                {
                    sb.Append(" ");
                }
                else
                {
                    sb.Append(ch);
                }
                i++;
            }
            return sb + endTags;
        }

    }
}
