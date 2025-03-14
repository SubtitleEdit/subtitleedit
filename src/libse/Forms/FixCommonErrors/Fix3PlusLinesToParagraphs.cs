using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class Fix3PlusLinesToParagraphs : IFixCommonError
    {
        public static class Language
        {
            public static string Split3PlusLinesIntoMultiParagraphs { get; set; } = "Split 3+ lines into multiple paragraphs";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var fixAction = Language.Split3PlusLinesIntoMultiParagraphs;
            int fixCount = 0;

            for (int i = subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                var p = subtitle.Paragraphs[i];
                if (Utilities.GetNumberOfLines(p.Text) > 2 && callbacks.AllowFix(p, fixAction))
                {
                    var splitParagraphs = new List<Paragraph>();

                    var oldText = p.Text;
                    var text = Utilities.UnbreakLine(p.Text);
                    var singleLineMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength;

                    double charsPerSecond = (double)p.Text.CountCharacters(true) / p.Duration.TotalSeconds;

                    int l = 0;
                    while (l < text.Length)
                    {
                        // calculate the amount of string to take from 'l'
                        int r = l + Math.Min(singleLineMaxChars, text.Length - l);

                        r = r != text.Length && text[r] != ' '
                            ? text.LastIndexOf(' ', r - 1, (r - 1) - l)
                            : r;

                        string s = text.Substring(l, r - l).Trim();

                        // We reach the end, but the amount of string is too small for a new paragraph, so we add it to the previous paragraph
                        if (s.Length < singleLineMaxChars && r == text.Length)
                        {
                            var lastParagraph = splitParagraphs.Last();
                            lastParagraph.Text = Utilities.AutoBreakLine(lastParagraph.Text + Environment.NewLine + s);
                            lastParagraph.EndTime = new TimeCode(p.EndTime.TotalMilliseconds);
                        }
                        else
                        {
                            // calculate start time in milliseconds for the new paragraph
                            double startTimeMilliseconds = splitParagraphs.Count == 0
                                ? p.StartTime.TotalMilliseconds
                                : splitParagraphs.Last().EndTime.TotalMilliseconds + 1; //+ Configuration.Settings.General.MinimumMillisecondsBetweenLines;

                            double endTime = startTimeMilliseconds + (s.Length / charsPerSecond * TimeCode.BaseUnit);
                            splitParagraphs.Add(new Paragraph(Utilities.AutoBreakLine(s), startTimeMilliseconds, endTime)
                            {
                                Number = p.Number + splitParagraphs.Count
                            });
                        }

                        l = r;
                    }

                    subtitle.Paragraphs.RemoveAt(i);

                    for (int j = splitParagraphs.Count - 1; j >= 0; j--)
                    {
                        subtitle.Paragraphs.Insert(i, splitParagraphs[j]);
                    }

                    fixCount++;
                    callbacks.AddFixToListView(p, fixAction, oldText, string.Join("</b>", splitParagraphs));
                }
            }

            subtitle.Renumber();
            callbacks.UpdateFixStatus(fixCount, Language.Split3PlusLinesIntoMultiParagraphs);
        }
    }
}