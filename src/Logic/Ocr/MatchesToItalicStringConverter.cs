using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Forms.Ocr;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public static class MatchesToItalicStringConverter
    {
        private static readonly string[] Separators = { "-", "—", ".", "'", "\"", " ", "!", "\r", "\n", "\r\n" };

        public static string GetStringWithItalicTags(List<VobSubOcr.CompareMatch> matches)
        {
            var sb = new StringBuilder();
            foreach (var lineMatches in SplitMatchesToLines(matches))
            {
                var numberOfLetters = GetNumberOfLetters(lineMatches);
                var numberOfItalicLetters = GetNumberOfItalicLetters(lineMatches);
                if (numberOfItalicLetters == numberOfLetters || numberOfItalicLetters > 2 && numberOfLetters - numberOfItalicLetters < 2)
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

        private static string GetStringWithItalicTagsMixed(List<VobSubOcr.CompareMatch> lineMatches)
        {
            var sb = new StringBuilder();
            int italicCount = 0;
            bool italicOn = false;
            var sbWord = new StringBuilder();
            string prevSpace = string.Empty;
            for (int i = 0; i < lineMatches.Count; i++)
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
                    else if (m.Text != " ")
                    {
                        sbWord.Append(m.Text);
                        if (m.Italic)
                        {
                            italicCount += m.Text.Length;
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
            text = text.Replace("<i>-</i>", "-")
                .Replace("<i>s</i>", "s")
                .Replace("</i>s<i>", "s")
                .Replace("<i>!</i>", "!")
                .Replace("</i>!<i>", "!")
                .Replace("<i>?</i>", "?")
                .Replace("</i>?<i>", "?")
                .Replace("<i>'</i>", "'")
                .Replace("<i>''</i>", "'")
                .Replace("</i>'<i>", "'")
                .Replace("</i>''<i>", "'")
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
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Text == Environment.NewLine)
                {
                    if (line.Count > 0)
                    {
                        result.Add(line);
                        line = new List<VobSubOcr.CompareMatch>();
                    }
                }
                else
                {
                    line.Add(matches[i]);
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
            for (int i = 0; i < matches.Count; i++)
            {
                string text = matches[i].Text;
                if (text != null)
                {
                    sb.Append(text);
                }
            }
            return sb.ToString().Trim();
        }

        private static int GetNumberOfLetters(List<VobSubOcr.CompareMatch> matches)
        {
            int count = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string text = matches[i].Text;
                if (text != null && !Separators.Contains(text))
                {
                    count++;
                }
            }
            return count;
        }

        private static int GetNumberOfItalicLetters(List<VobSubOcr.CompareMatch> matches)
        {
            int count = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string text = matches[i].Text;
                if (text != null && matches[i].Italic && !Separators.Contains(text))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
