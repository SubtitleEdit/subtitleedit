using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public static class SplitLongLinesHelper
    {
        public static bool QualifiesForSplit(string text, int singleLineMaxCharacters, int totalLineMaxCharacters)
        {
            string noTagText = HtmlUtil.RemoveHtmlTags(text.Trim(), true);
            if (noTagText.Length > totalLineMaxCharacters)
            {
                return true;
            }

            foreach (string noTagLine in noTagText.SplitToLines())
            {
                if (noTagLine.Length > singleLineMaxCharacters)
                {
                    return true;
                }
            }

            if (!noTagText.StartsWith('-') || Utilities.CountTagInText(text, "- ") < 2)
            {
                return false;
            }

            string singleLineNoTag = Utilities.UnbreakLine(noTagText);

            string[] patterns = { ". -", "! -", "? -", "] -", ") -" };
            int patternIdx = singleLineNoTag.IndexOfAny(patterns, StringComparison.OrdinalIgnoreCase);
            if (patternIdx < 0)
            {
                return false;
            }

            int maxLineLen = Math.Max(patternIdx + 1, singleLineNoTag.Length - (patternIdx + 2));

            return maxLineLen > singleLineMaxCharacters;
        }

        public static Subtitle SplitLongLinesInSubtitle(Subtitle subtitle, int totalLineMaxCharacters, int singleLineMaxCharacters)
        {
            var splittedIndexes = new List<int>();
            var autoBreakedIndexes = new List<int>();
            var splittedSubtitle = new Subtitle(subtitle);
            splittedSubtitle.Paragraphs.Clear();
            string language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                bool added = false;
                var p = subtitle.GetParagraphOrDefault(i);
                if (p?.Text != null)
                {
                    if (QualifiesForSplit(p.Text, singleLineMaxCharacters, totalLineMaxCharacters))
                    {
                        var text = Utilities.AutoBreakLine(p.Text, language);
                        if (!QualifiesForSplit(text, singleLineMaxCharacters, totalLineMaxCharacters))
                        {
                            var newParagraph = new Paragraph(p) { Text = text };
                            autoBreakedIndexes.Add(splittedSubtitle.Paragraphs.Count);
                            splittedSubtitle.Paragraphs.Add(newParagraph);
                            added = true;
                        }
                        else
                        {
                            if (text.Contains(Environment.NewLine))
                            {
                                var arr = text.SplitToLines();
                                if (arr.Count == 2)
                                {
                                    var minMsBtwnLnBy2 = Configuration.Settings.General.MinimumMillisecondsBetweenLines / 2;
                                    int spacing1 = minMsBtwnLnBy2;
                                    int spacing2 = minMsBtwnLnBy2;
                                    if (Configuration.Settings.General.MinimumMillisecondsBetweenLines % 2 == 1)
                                    {
                                        spacing2++;
                                    }

                                    double duration = p.Duration.TotalMilliseconds / 2.0;
                                    var newParagraph1 = new Paragraph(p);
                                    var newParagraph2 = new Paragraph(p);
                                    newParagraph1.Text = Utilities.AutoBreakLine(arr[0], language);
                                    newParagraph1.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration - spacing1;
                                    newParagraph2.Text = Utilities.AutoBreakLine(arr[1], language);
                                    newParagraph2.StartTime.TotalMilliseconds = newParagraph1.EndTime.TotalMilliseconds + spacing2;

                                    splittedIndexes.Add(splittedSubtitle.Paragraphs.Count);
                                    splittedIndexes.Add(splittedSubtitle.Paragraphs.Count + 1);

                                    string p1 = HtmlUtil.RemoveHtmlTags(newParagraph1.Text);
                                    var len = p1.Length - 1;
                                    if (p1.Length > 0 && (p1[len] == '.' || p1[len] == '!' || p1[len] == '?' || p1[len] == ':' || p1[len] == ')' || p1[len] == ']' || p1[len] == '♪'))
                                    {
                                        if (newParagraph1.Text.StartsWith('-') && newParagraph2.Text.StartsWith('-'))
                                        {
                                            newParagraph1.Text = newParagraph1.Text.Remove(0, 1).Trim();
                                            newParagraph2.Text = newParagraph2.Text.Remove(0, 1).Trim();
                                        }
                                        else if (newParagraph1.Text.StartsWith("<i>-", StringComparison.Ordinal) && newParagraph2.Text.StartsWith('-'))
                                        {
                                            newParagraph1.Text = newParagraph1.Text.Remove(3, 1).Trim();
                                            if (newParagraph1.Text.StartsWith("<i> ", StringComparison.Ordinal))
                                            {
                                                newParagraph1.Text = newParagraph1.Text.Remove(3, 1).Trim();
                                            }

                                            newParagraph2.Text = newParagraph2.Text.Remove(0, 1).Trim();
                                        }
                                    }
                                    else
                                    {
                                        if (newParagraph1.Text.EndsWith("</i>", StringComparison.Ordinal))
                                        {
                                            const string post = "</i>";
                                            newParagraph1.Text = newParagraph1.Text.Remove(newParagraph1.Text.Length - post.Length);
                                        }

                                        if (newParagraph2.Text.StartsWith("<i>", StringComparison.Ordinal))
                                        {
                                            const string pre = "<i>";
                                            newParagraph2.Text = newParagraph2.Text.Remove(0, pre.Length);
                                        }
                                    }

                                    var indexOfItalicOpen1 = newParagraph1.Text.IndexOf("<i>", StringComparison.Ordinal);
                                    if (indexOfItalicOpen1 >= 0 && indexOfItalicOpen1 < 10 && newParagraph1.Text.IndexOf("</i>", StringComparison.Ordinal) < 0 &&
                                        newParagraph2.Text.Contains("</i>") && newParagraph2.Text.IndexOf("<i>", StringComparison.Ordinal) < 0)
                                    {
                                        newParagraph1.Text += "</i>";
                                        newParagraph2.Text = "<i>" + newParagraph2.Text;
                                    }

                                    splittedSubtitle.Paragraphs.Add(newParagraph1);
                                    splittedSubtitle.Paragraphs.Add(newParagraph2);
                                    added = true;
                                }
                            }
                        }
                    }
                }
                if (!added)
                {
                    splittedSubtitle.Paragraphs.Add(new Paragraph(p));
                }
            }
            splittedSubtitle.Renumber();
            return splittedSubtitle;
        }

    }
}
