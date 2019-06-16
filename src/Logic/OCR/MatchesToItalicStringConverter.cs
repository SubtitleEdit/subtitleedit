using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Forms.Ocr;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public static class MatchesToItalicStringConverter
    {
        private static readonly string[] Seperators = { "-", "—", ".", "'", "\"", " ", "!", "\r", "\n", "\r\n" };

        public static string GetStringWithItalicTags(List<VobSubOcr.CompareMatch> matches)
        {
            var numberOfLetters = GetNumberOfLetters(matches);
            var numberOfItalicLetters = GetNumberOfItalicLetters(matches);
            if (numberOfItalicLetters == numberOfLetters || numberOfItalicLetters > 2 && numberOfLetters - numberOfItalicLetters < 2)
            {
                return "<i>" + GetRawString(matches) + "</i>";
            }
            if (numberOfItalicLetters == 0 || numberOfLetters > 2 && numberOfItalicLetters < 2)
            {
                return GetRawString(matches);
            }

            return GetStringWithItalicTagsMixed(matches);
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
                if (text != null && !Seperators.Contains(text))
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
                if (text != null && matches[i].Italic && !Seperators.Contains(text))
                {
                    count++;
                }
            }
            return count;
        }


        public static string GetStringWithItalicTagsMixed(List<VobSubOcr.CompareMatch> matches)
        {
            var paragraph = new StringBuilder();
            var line = new StringBuilder();
            var word = new StringBuilder();
            int lettersItalics = 0;
            int lettersNonItalics = 0;
            int lineLettersNonItalics = 0;
            int wordItalics = 0;
            int wordNonItalics = 0;
            bool isItalic = false;
            bool allItalic = true;

            for (int i = 0; i < matches.Count; i++)
            {
                string text = matches[i].Text;
                if (text != null)
                {
                    bool italic = matches[i].Italic;

                    if (Seperators.Contains(text) && word.Length > 0)
                    {
                        ItalicsWord(line, ref word, ref lettersItalics, ref lettersNonItalics, ref wordItalics, ref wordNonItalics, ref isItalic, text);
                    }
                    else if (text == Environment.NewLine)
                    {
                        ItalicsWord(line, ref word, ref lettersItalics, ref lettersNonItalics, ref wordItalics, ref wordNonItalics, ref isItalic, "");
                        ItalianLine(paragraph, ref line, ref allItalic, ref wordItalics, ref wordNonItalics, ref isItalic, Environment.NewLine, lineLettersNonItalics);
                        lineLettersNonItalics = 0;
                    }
                    else
                    {
                        bool isMixedCaseWithoutDashAndAlike = IsMixedCaseWithoutDashAndAlike(matches, i, out var italicOrNot);
                        if (Seperators.Contains(text) && !isMixedCaseWithoutDashAndAlike && word.Length > 0)
                        {
                            italic = italicOrNot;
                        }

                        if (italic)
                        {
                            word.Append(text);
                            lettersItalics += text.Length;
                        }
                        else
                        {
                            word.Append(text);
                            lettersNonItalics += text.Length;
                            lineLettersNonItalics += text.Length;
                        }
                    }
                }
            }

            if (word.Length > 0)
            {
                ItalicsWord(line, ref word, ref lettersItalics, ref lettersNonItalics, ref wordItalics, ref wordNonItalics, ref isItalic, "");
            }

            if (line.Length > 0)
            {
                ItalianLine(paragraph, ref line, ref allItalic, ref wordItalics, ref wordNonItalics, ref isItalic, "", lineLettersNonItalics);
            }

            if (allItalic && matches.Count > 0)
            {
                var temp = HtmlUtil.RemoveOpenCloseTags(paragraph.ToString(), HtmlUtil.TagItalic);
                paragraph.Clear();
                paragraph.Append("<i>");
                paragraph.Append(temp);
                paragraph.Append("</i>");
            }

            var result = paragraph.ToString();
            result = result.Replace("</i>'' <i>", "'' ");
            result = result.Replace("</i>' <i>", "' ");
            result = result.Replace("</i>\" <i>", "\" ");
            result = result.Replace("</i> \"<i>", " \"");
            result = result.Replace("<i>-</i>", "-");
            result = result.Replace("<i>.</i>", ".");
            if (result.Contains("'</i>") || result.Contains("\"</i>"))
            {
                result = result.Replace(" '<i>'", " <i>''");
            }
            else if (result.Contains("</i>'") || result.Contains("</i>\""))
            {
                result = result.Replace(" '<i>'", " ''<i>");
            }
            if (result.StartsWith("'<i>'", StringComparison.Ordinal))
            {
                if (result.Contains("'</i>") || result.Contains("\"</i>"))
                {
                    result = "<i>''" + result.Remove(0, 5);
                }
                else if (result.Contains("</i>'") || result.Contains("</i>\""))
                {
                    result = "''<i>" + result.Remove(0, 5);
                }
            }
            if (result.EndsWith("'</i>'", StringComparison.Ordinal))
            {
                if (result.Contains("<i>'") || result.Contains("<i>\""))
                {
                    result = result.Substring(0, result.Length - 6) + "''</i>";
                }
                else if (result.Contains("'<i>") || result.Contains("\"<i>"))
                {
                    result = result.Substring(0, result.Length - 6) + "</i>''";
                }
            }
            result = result.Replace("  ", " ");

            return result;
        }

        private static bool IsMixedCaseWithoutDashAndAlike(List<VobSubOcr.CompareMatch> matches, int startIndex, out bool italicOrNot)
        {
            while (startIndex > 0 && (matches[startIndex - 1].Text == " " || matches[startIndex - 1].Text == Environment.NewLine))
            {
                startIndex--;
            }

            int italicCount = 0;
            int nonItalicCount = 0;
            for (int i = startIndex; i < matches.Count; i++)
            {
                var m = matches[i];
                if (m.Text != null)
                {
                    if (m.Text == "-" || m.Text == "—" || m.Text == "." || m.Text == "'")
                    {
                    }
                    else if (m.Italic)
                    {
                        italicCount++;
                    }
                    else
                    {
                        nonItalicCount++;
                    }
                    if (m.Text == " " || m.Text == Environment.NewLine)
                    {
                        break;
                    }
                }
            }
            italicOrNot = italicCount > 0;
            return italicCount > 0 && nonItalicCount > 0;
        }

        private static void ItalianLine(StringBuilder paragraph, ref StringBuilder line, ref bool allItalic, ref int wordItalics, ref int wordNonItalics, ref bool isItalic, string appendString, int lineLettersNonItalics)
        {
            if (isItalic)
            {
                line.Append("</i>");
                isItalic = false;
            }

            if (wordItalics > 0
                && (wordNonItalics == 0 || wordNonItalics < 2 && lineLettersNonItalics < 3 && line.ToString().TrimStart().StartsWith('-')))
            {
                paragraph.Append("<i>");
                paragraph.Append(HtmlUtil.RemoveOpenCloseTags(line.ToString(), HtmlUtil.TagItalic));
                paragraph.Append("</i>");
                paragraph.Append(appendString);
            }
            else
            {
                allItalic = false;

                if (wordItalics > 0)
                {
                    string temp = line.ToString().Replace(" </i>", "</i> ");
                    line.Clear();
                    line.Append(temp);
                }

                paragraph.Append(line);
                paragraph.Append(appendString);
            }
            line.Clear();
            wordItalics = 0;
            wordNonItalics = 0;
        }

        private static void ItalicsWord(StringBuilder line, ref StringBuilder word, ref int lettersItalics, ref int lettersNonItalics, ref int wordItalics, ref int wordNonItalics, ref bool isItalic, string appendString)
        {
            if (line.Length == 0 && !isItalic && lettersItalics == 0 && lettersNonItalics == 1 && word.ToString() == "-")
            {
                line.Append(word);
                word.Clear();
                word.Append(appendString);
                lettersItalics = 0;
                lettersNonItalics = 0;
                return;
            }
            else if (lettersItalics >= lettersNonItalics && lettersItalics > 0)
            {
                if (!isItalic)
                {
                    line.Append("<i>");
                }

                line.Append(word + appendString);
                wordItalics++;
                isItalic = true;
            }
            else
            {
                if (appendString == "\"" && isItalic && !line.ToString().Contains("\"<i>"))
                {
                    line.Append(appendString);
                    line.Append("</i>");
                    isItalic = false;
                }
                else
                {
                    if (isItalic)
                    {
                        line.Append("</i>");
                        isItalic = false;
                    }
                    line.Append(word);
                    line.Append(appendString);
                }
                wordNonItalics++;
            }
            word.Clear();
            lettersItalics = 0;
            lettersNonItalics = 0;
        }

    }
}
