using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Logic.Forms
{
    public static class SplitLongLinesHelper
    {

        public static bool QualifiesForSplit(string text, int singleLineMaxCharacters, int totalLineMaxCharacters)
        {
            string s = Utilities.RemoveHtmlTags(text.Trim());
            if (s.Length > totalLineMaxCharacters)
                return true;

            string[] arr = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in arr)
            {
                if (line.Length > singleLineMaxCharacters)
                    return true;
            }
            return false;
        }

        public static Subtitle SplitLongLinesInSubtitle(Subtitle subtitle, int totalLineMaxCharacters, int singleLineMaxCharacters)
        {
            List<int> splittedIndexes = new List<int>();
            List<int> autoBreakedIndexes = new List<int>();
            int numberOfSplits = 0;
            Subtitle splittedSubtitle = new Subtitle();
            string language = Utilities.AutoDetectGoogleLanguage(subtitle);
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                bool added = false;
                var p = subtitle.GetParagraphOrDefault(i);
                if (p != null && p.Text != null)
                {
                    string oldText = Utilities.RemoveHtmlTags(p.Text);
                    if (QualifiesForSplit(p.Text, singleLineMaxCharacters, totalLineMaxCharacters))
                    {
                        if (!QualifiesForSplit(Utilities.AutoBreakLine(p.Text, language), singleLineMaxCharacters, totalLineMaxCharacters))
                        {
                            Paragraph newParagraph = new Paragraph(p);
                            newParagraph.Text = Utilities.AutoBreakLine(p.Text, language);
                            autoBreakedIndexes.Add(splittedSubtitle.Paragraphs.Count);
                            splittedSubtitle.Paragraphs.Add(newParagraph);
                            added = true;
                            numberOfSplits++;
                        }
                        else
                        {
                            string text = Utilities.AutoBreakLine(p.Text, language);
                            if (text.Contains(Environment.NewLine))
                            {
                                string[] arr = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                                if (arr.Length == 2)
                                {
                                    int spacing1 = Configuration.Settings.General.MininumMillisecondsBetweenLines / 2;
                                    int spacing2 = Configuration.Settings.General.MininumMillisecondsBetweenLines / 2;
                                    if (Configuration.Settings.General.MininumMillisecondsBetweenLines % 2 == 1)
                                        spacing2++;

                                    double duration = p.Duration.TotalMilliseconds / 2.0;
                                    Paragraph newParagraph1 = new Paragraph(p);
                                    Paragraph newParagraph2 = new Paragraph(p);
                                    newParagraph1.Text = Utilities.AutoBreakLine(arr[0], language);
                                    newParagraph1.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration - spacing1;
                                    newParagraph2.Text = Utilities.AutoBreakLine(arr[1], language);
                                    newParagraph2.StartTime.TotalMilliseconds = newParagraph1.EndTime.TotalMilliseconds + spacing2;

                                    splittedIndexes.Add(splittedSubtitle.Paragraphs.Count);
                                    splittedIndexes.Add(splittedSubtitle.Paragraphs.Count + 1);

                                    string p1 = Utilities.RemoveHtmlTags(newParagraph1.Text);
                                    if (p1.EndsWith('.') || p1.EndsWith('!') || p1.EndsWith('?') || p1.EndsWith(':') || p1.EndsWith(')') || p1.EndsWith(']') || p1.EndsWith('♪'))
                                    {
                                        if (newParagraph1.Text.StartsWith('-') && newParagraph2.Text.StartsWith('-'))
                                        {
                                            newParagraph1.Text = newParagraph1.Text.Remove(0, 1).Trim();
                                            newParagraph2.Text = newParagraph2.Text.Remove(0, 1).Trim();
                                        }
                                        else if (newParagraph1.Text.StartsWith("<i>-") && newParagraph2.Text.StartsWith('-'))
                                        {
                                            newParagraph1.Text = newParagraph1.Text.Remove(3, 1).Trim();
                                            if (newParagraph1.Text.StartsWith("<i> "))
                                                newParagraph1.Text = newParagraph1.Text.Remove(3, 1).Trim();
                                            newParagraph2.Text = newParagraph2.Text.Remove(0, 1).Trim();
                                        }
                                    }
                                    else
                                    {
                                        string post = string.Empty;
                                        if (newParagraph1.Text.EndsWith("</i>"))
                                        {
                                            post = "</i>";
                                            newParagraph1.Text = newParagraph1.Text.Remove(newParagraph1.Text.Length - post.Length);
                                        }
                                        //newParagraph1.Text += comboBoxLineContinuationEnd.Text.TrimEnd() + post;

                                        string pre = string.Empty;
                                        if (newParagraph2.Text.StartsWith("<i>"))
                                        {
                                            pre = "<i>";
                                            newParagraph2.Text = newParagraph2.Text.Remove(0, pre.Length);
                                        }
                                        //newParagraph2.Text = pre + comboBoxLineContinuationBegin.Text + newParagraph2.Text;
                                    }

                                    var indexOfItalicOpen1 = newParagraph1.Text.IndexOf("<i>", StringComparison.Ordinal);
                                    if (indexOfItalicOpen1 >= 0 && indexOfItalicOpen1 < 10 & newParagraph1.Text.IndexOf("</i>", StringComparison.Ordinal) < 0 &&
                                        newParagraph2.Text.Contains("</i>") && newParagraph2.Text.IndexOf("<i>", StringComparison.Ordinal) < 0)
                                    {
                                        newParagraph1.Text += "</i>";
                                        newParagraph2.Text = "<i>" + newParagraph2.Text;
                                    }

                                    splittedSubtitle.Paragraphs.Add(newParagraph1);
                                    splittedSubtitle.Paragraphs.Add(newParagraph2);
                                    added = true;
                                    numberOfSplits++;
                                }
                            }
                        }
                    }
                }
                if (!added)
                    splittedSubtitle.Paragraphs.Add(new Paragraph(p));
            }
            splittedSubtitle.Renumber(1);
            return splittedSubtitle;
        }

    }
}
