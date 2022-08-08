using Nikse.SubtitleEdit.Forms.Ocr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public static class MatchesToItalicStringConverter
    {
        private static readonly string[] Separators = { "-", "—", ".", "'", "\"", " ", "!", "\r", "\n", "\r\n" };

        public static string GetStringWithItalicTags(List<VobSubOcr.CompareMatch> matches)
        {
            if (matches == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var lineMatches in SplitMatchesToLines(matches))
            {
                var numberOfLetters = GetNumberOfLetters(lineMatches);
                var numberOfItalicLetters = GetNumberOfItalicLetters(lineMatches);
                if (numberOfItalicLetters == numberOfLetters || numberOfItalicLetters > 3 && numberOfLetters - numberOfItalicLetters < 2 && ItalicIsInsideWord(matches))
                {
                    sb.AppendLine("<i>" + GetRawString(lineMatches) + "</i>");
                }
                else if (numberOfItalicLetters == 0 || numberOfLetters > 2 && numberOfItalicLetters < 2)
                {
                    sb.AppendLine(GetRawString(lineMatches));
                }
                else
                {
                    sb.AppendLine(GetStringWithItalicTagsMixed(lineMatches));
                }
            }
            return sb.ToString().TrimEnd().Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
        }

        private static bool ItalicIsInsideWord(List<VobSubOcr.CompareMatch> matches)
        {
            for (var i = 0; i < matches.Count; i++)
            {
                var m = matches[i];
                if (m.Italic || Separators.Contains(m.Text))
                {
                    continue;
                }

                var beforeHasLetter = i > 0 && !Separators.Contains(matches[i - 1].Text);
                var afterHasLetter = i < matches.Count -1 && !Separators.Contains(matches[i + 1].Text);
                if (beforeHasLetter || afterHasLetter)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static string GetStringWithItalicTagsMixed(List<VobSubOcr.CompareMatch> lineMatches)
        {
            var sb = new StringBuilder();
            var italicCount = 0;
            var italicOn = false;
            var sbWord = new StringBuilder();
            var prevSpace = string.Empty;

            for (var i = 0; i < lineMatches.Count; i++)
            {
                var m = lineMatches[i];
                if (m.Text == " " || m.Text == "-" || m.Text == "'") // chars that allow change of italic
                {
                    if (sbWord.Length > 0)
                    {
                        italicOn = AddWord(sb, italicCount, ref italicOn, sbWord, prevSpace);
                        prevSpace = m.Text;
                        sbWord = new StringBuilder();
                        italicCount = 0;
                    }
                    else
                    {
                        sbWord.Append(m.Text);
                        if (m.Italic)
                        {
                            var skipItalic = m.Text == "-" && i < lineMatches.Count - 1 && !lineMatches[i + 1].Italic;

                            if (!skipItalic)
                            {
                                italicCount += m.Text.Length;
                            }
                        }
                    }
                }
                else if (m.Text != null)
                {
                    sbWord.Append(m.Text);
                    if (m.Italic)
                    {
                        italicCount += m.Text.Length;
                    }
                }
            }

            italicOn = AddWord(sb, italicCount, ref italicOn, sbWord, prevSpace);
            if (italicOn)
            {
                sb.Append("</i>");
            }

            var text = sb.ToString().Trim();
            text = text
                .Replace("<i>-</i>", "-")
                .Replace("</i>-<i>", "-")
                .Replace("<i>s</i>", "s")
                .Replace("</i>s<i>", "s")
                .Replace("<i>!</i>", "!")
                .Replace("</i>!<i>", "!")
                .Replace("<i>?</i>", "?")
                .Replace("</i>?<i>", "?")
                .Replace("<i>'</i>", "'")
                .Replace("</i>'<i>", "'")
                .Replace("<i>''</i>", "''")
                .Replace("</i>''<i>", "''")
                .Replace("<i>\"</i>", "\"")
                .Replace("</i>\"<i>", "\"")
                .Replace("<i>:</i>", ":")
                .Replace("</i>:<i>", ":")
                .Replace("<i>.</i>", ".")
                .Replace("</i>.<i>", ".")
                .Replace("<i>...</i>", "...")
                .Replace("</i>...<i>", "...");
            return text;
        }

        private static bool AddWord(StringBuilder sb, int italicCount, ref bool italicOn, StringBuilder sbWord, string prevSpace)
        {
            var w = sbWord.ToString();
            var wordIsItalic = italicCount > w.Length / 2.0;
            if (!wordIsItalic && Math.Abs(italicCount - w.Length / 2.0) < 0.3 && italicOn)
            {
                wordIsItalic = true;
            }

            if (wordIsItalic && italicOn)
            {
                sb.Append(prevSpace + sbWord);
            }
            else if (wordIsItalic)
            {
                sb.Append(prevSpace + "<i>" + sbWord);
                italicOn = true;
            }
            else if (italicOn)
            {
                sb.Append("</i>" + prevSpace + sbWord);
                italicOn = false;
            }
            else
            {
                sb.Append(prevSpace + sbWord);
            }

            return italicOn;
        }

        private static List<List<VobSubOcr.CompareMatch>> SplitMatchesToLines(List<VobSubOcr.CompareMatch> matches)
        {
            var result = new List<List<VobSubOcr.CompareMatch>>();
            var line = new List<VobSubOcr.CompareMatch>();
            foreach (var t in matches)
            {
                if (t.Text == Environment.NewLine)
                {
                    if (line.Count > 0)
                    {
                        result.Add(line);
                        line = new List<VobSubOcr.CompareMatch>();
                    }
                }
                else
                {
                    line.Add(t);
                }
            }
            if (line.Count > 0)
            {
                result.Add(line);
            }
            return result;
        }

        private static string GetRawString(List<VobSubOcr.CompareMatch> matches)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < matches.Count; i++)
            {
                var text = matches[i].Text;
                if (text != null)
                {
                    sb.Append(text);
                }
            }

            return sb.ToString().Trim();
        }

        private static int GetNumberOfLetters(List<VobSubOcr.CompareMatch> matches)
        {
            var count = 0;
            for (var i = 0; i < matches.Count; i++)
            {
                var text = matches[i].Text;
                if (text != null && !Separators.Contains(text))
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetNumberOfItalicLetters(List<VobSubOcr.CompareMatch> matches)
        {
            var count = 0;
            foreach (var t in matches)
            {
                var text = t.Text;
                if (text != null && t.Italic && !Separators.Contains(text))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
