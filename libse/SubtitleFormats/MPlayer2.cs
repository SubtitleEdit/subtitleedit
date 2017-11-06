﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class MPlayer2 : SubtitleFormat
    {
        private static readonly Regex RegexMPlayer2Line = new Regex(@"^\[-?\d+]\[-?\d+].*$", RegexOptions.Compiled);

        public override string Extension => ".mpl";

        public override string Name => "MPlayer2";

        public override bool IsMine(List<string> lines, string fileName)
        {
            int errors = 0;
            var trimmedLines = new List<string>();
            foreach (string line in lines)
            {
                int indexOfStartBracket = line.IndexOf('[');
                if (!string.IsNullOrWhiteSpace(line) && line.Length < 250 && indexOfStartBracket >= 0 && indexOfStartBracket < 10)
                {
                    string s = RemoveIllegalSpacesAndFixEmptyCodes(line);
                    if (RegexMPlayer2Line.IsMatch(s))
                        trimmedLines.Add(line);
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

        private static string RemoveIllegalSpacesAndFixEmptyCodes(string line)
        {
            int index = line.IndexOf(']');
            if (index >= 0 && index < line.Length)
            {
                index = line.IndexOf(']', index + 1);
                if (index >= 0 && index + 1 < line.Length)
                {
                    var indexOfBrackets = line.IndexOf("[]", StringComparison.Ordinal);
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

        public override string ToText(Subtitle subtitle, string title)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                sb.Append('[');
                sb.Append((int)(p.StartTime.TotalMilliseconds / 100));
                sb.Append("][");
                sb.Append(((int)(p.EndTime.TotalMilliseconds / 100)));
                sb.Append(']');

                var parts = p.Text.SplitToLines();
                int count = 0;
                bool italicOn = false;
                foreach (string line in parts)
                {
                    if (count > 0)
                        sb.Append('|');

                    if (line.StartsWith("<i>", StringComparison.Ordinal) || italicOn)
                    {
                        italicOn = true;
                        sb.Append('/');
                    }

                    if (line.Contains("</i>"))
                        italicOn = false;
                    sb.Append(HtmlUtil.RemoveHtmlTags(line));
                    count++;
                }
                sb.AppendLine();
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;

            foreach (string line in lines)
            {
                string s = RemoveIllegalSpacesAndFixEmptyCodes(line);
                if (RegexMPlayer2Line.IsMatch(s))
                {
                    try
                    {
                        int textIndex = s.IndexOf(']') + 1;
                        textIndex = s.IndexOf(']', textIndex) + 1;
                        if (textIndex < s.Length)
                        {
                            string text = s.Substring(textIndex);
                            if (text.StartsWith('/') && (Utilities.CountTagInText(text, '|') == 0 || text.Contains("|/")))
                                text = "<i>" + text.TrimStart('/').Replace("|/", Environment.NewLine) + "</i>";
                            else if (text.StartsWith('/') && text.Contains('|') && !text.Contains("|/"))
                                text = "<i>" + text.TrimStart('/').Replace("|", "</i>" + Environment.NewLine);
                            else if (text.Contains("|/"))
                                text = text.Replace("|/", Environment.NewLine + "<i>") + "</i>";
                            text = text.Replace("|", Environment.NewLine);
                            string temp = s.Substring(0, textIndex - 1);
                            string[] frames = temp.Replace("][", ":").Replace("[", string.Empty).Replace("]", string.Empty).Split(':');

                            double startSeconds = double.Parse(frames[0]) / 10;
                            double endSeconds = double.Parse(frames[1]) / 10;

                            if (startSeconds == 0 && subtitle.Paragraphs.Count > 0)
                                startSeconds = (subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].EndTime.TotalMilliseconds / 1000) + 0.1;
                            if (endSeconds == 0)
                                endSeconds = startSeconds;

                            subtitle.Paragraphs.Add(new Paragraph(text, startSeconds * 1000, endSeconds * 1000));
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
            subtitle.Renumber();
        }
    }
}
