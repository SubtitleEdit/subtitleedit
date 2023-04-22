using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class AudioToTextPostProcessor
    {
        public enum Engine
        {
            Whisper,
            Vosk,
        }

        /// <summary>
        /// Set period if distance to next subtitle is more than this value in milliseconds.
        /// </summary>
        public double SetPeriodIfDistanceToNextIsMoreThan { get; set; } = 300;
        public double SetPeriodIfDistanceToNextIsMoreThanAlways { get; set; } = 500;

        public int ParagraphMaxChars { get; set; }

        public string TwoLetterLanguageCode { get; }

        public AudioToTextPostProcessor(string twoLetterLanguageCode)
        {
            TwoLetterLanguageCode = twoLetterLanguageCode;

            ParagraphMaxChars = Configuration.Settings.Tools.AudioToTextLineMaxChars;
            if (ParagraphMaxChars < 10 || ParagraphMaxChars > 1_000_000)
            {
                ParagraphMaxChars = 86;
            }
        }

        public Subtitle Fix(Engine engine, List<ResultText> input, bool usePostProcessing, bool addPeriods, bool mergeLines, bool fixCasing, bool fixShortDuration, bool splitLines)
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.AddRange(input.Select(p => new Paragraph(p.Text, (double)p.Start * 1000.0, (double)p.End * 1000.0)).ToList());

            return Fix(engine, subtitle, usePostProcessing, addPeriods, mergeLines, fixCasing, fixShortDuration, splitLines);
        }

        public Subtitle Fix(Engine engine, Subtitle input, bool usePostProcessing, bool addPeriods, bool mergeLines, bool fixCasing, bool fixShortDuration, bool splitLines)
        {
            var subtitle = new Subtitle();

            for (var index = 0; index < input.Paragraphs.Count; index++)
            {
                var paragraph = input.Paragraphs[index];
                if (usePostProcessing && engine == Engine.Vosk && TwoLetterLanguageCode == "en" && paragraph.Text == "the" && paragraph.EndTime.TotalSeconds - paragraph.StartTime.TotalSeconds > 1)
                {
                    continue;
                }

                if (usePostProcessing && engine == Engine.Whisper)
                {
                    if (new[] { "(.", "(" }.Contains(paragraph.Text))
                    {
                        continue;
                    }

                    if (TwoLetterLanguageCode == "en")
                    {
                        // anything?
                    }
                    else if (TwoLetterLanguageCode == "da")
                    {
                        if (paragraph.Text.Contains("Danske tekster af nicolai winther", StringComparison.OrdinalIgnoreCase))
                        {
                            continue; // from some training data...
                        }
                    }

                    paragraph.Text = Utilities.RemoveUnneededSpaces(paragraph.Text, TwoLetterLanguageCode);

                    if (paragraph.Text.StartsWith('.') && paragraph.Text.EndsWith(").", StringComparison.Ordinal))
                    {
                        paragraph.Text = paragraph.Text.TrimEnd('.');
                    }

                    var next = input.GetParagraphOrDefault(index + 1);
                    if (next != null && Math.Abs(paragraph.EndTime.TotalMilliseconds - next.StartTime.TotalMilliseconds) < 0.01)
                    {
                        next.StartTime.TotalMilliseconds++;
                    }
                }

                subtitle.Paragraphs.Add(paragraph);
            }

            if (usePostProcessing && engine == Engine.Whisper && TwoLetterLanguageCode == "da")
            {
                new FixDanishLetterI().Fix(subtitle, new EmptyFixCallback());
            }

            return Fix(subtitle, usePostProcessing, addPeriods, mergeLines, fixCasing, fixShortDuration, splitLines, engine);
        }

        public Subtitle Fix(Subtitle subtitle, bool usePostProcessing, bool addPeriods, bool mergeLines, bool fixCasing, bool fixShortDuration, bool splitLines, Engine engine)
        {
            if (usePostProcessing)
            {
                if (addPeriods)
                {
                    subtitle = AddPeriods(subtitle, TwoLetterLanguageCode);
                }

                if (fixCasing)
                {
                    subtitle = FixCasing(subtitle, TwoLetterLanguageCode, engine);
                }

                if (fixShortDuration)
                {
                    subtitle = FixShortDuration(subtitle);
                }

                if (splitLines && !new[] { "jp", "cn" }.Contains(TwoLetterLanguageCode))
                {
                    var totalMaxChars = Configuration.Settings.General.SubtitleLineMaximumLength * Configuration.Settings.General.MaxNumberOfLines;
                    subtitle = SplitLongLinesHelper.SplitLongLinesInSubtitle(subtitle, totalMaxChars, Configuration.Settings.General.SubtitleLineMaximumLength);
                }

                if (mergeLines)
                {
                    subtitle = MergeShortLines(subtitle, TwoLetterLanguageCode);
                    subtitle = AutoBalanceLines(subtitle, TwoLetterLanguageCode);
                }
            }

            subtitle.Renumber();
            return subtitle;
        }

        public Subtitle AddPeriods(Subtitle inputSubtitle, string language)
        {
            if (language == "jp" || language == "cn")
            {
                return new Subtitle(inputSubtitle);
            }

            var englishSkipLastWords = new[] { "with", "however" };
            var englishSkipFirstWords = new[] { "to", "and", "but" };

            var subtitle = new Subtitle(inputSubtitle);
            for (var index = 0; index < subtitle.Paragraphs.Count - 1; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                var next = subtitle.Paragraphs[index + 1];
                if (next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds > SetPeriodIfDistanceToNextIsMoreThan &&
                    !paragraph.Text.EndsWith('.') &&
                    !paragraph.Text.EndsWith('!') &&
                    !paragraph.Text.EndsWith('?') &&
                    !paragraph.Text.EndsWith(',') &&
                    !paragraph.Text.EndsWith(':'))
                {
                    if (next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds > SetPeriodIfDistanceToNextIsMoreThanAlways)
                    {
                        paragraph.Text += ".";
                    }
                    else
                    {
                        var lastWord = GetLastWord(paragraph.Text);
                        var nextFirstWord = GetFirstWord(next.Text);
                        if (TwoLetterLanguageCode == "en" && (englishSkipLastWords.Contains(lastWord) || englishSkipFirstWords.Contains(nextFirstWord)))
                        {
                            continue;
                        }

                        paragraph.Text += ".";
                    }
                }
            }

            var last = subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1);
            if (last != null &&
                !last.Text.EndsWith('.') &&
                !last.Text.EndsWith('!') &&
                !last.Text.EndsWith('?') &&
                !last.Text.EndsWith(',') &&
                !last.Text.EndsWith(':'))
            {
                subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text += ".";
            }

            return subtitle;
        }

        private static string GetFirstWord(string text)
        {
            var arr = text.Split(' ', '\r', '\n', '\t', ',');
            return arr.Length == 0 ? string.Empty : arr[0];
        }

        private static string GetLastWord(string text)
        {
            var arr = text.Split(' ', '\r', '\n', '\t', ',');
            return arr.Length == 0 ? string.Empty : arr[arr.Length - 1];
        }

        public Subtitle MergeShortLines(Subtitle subtitle, string language)
        {
            const int maxMillisecondsBetweenLines = 100;
            const bool onlyContinuousLines = true;

            if (language == "jp")
            {
                ParagraphMaxChars = Configuration.Settings.Tools.AudioToTextLineMaxCharsJp;
            }

            if (language == "cn")
            {
                ParagraphMaxChars = Configuration.Settings.Tools.AudioToTextLineMaxCharsCn;
            }

            var mergedSubtitle = new Subtitle();
            var lastMerged = false;
            Paragraph p = null;
            for (var i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (!lastMerged)
                {
                    p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                    mergedSubtitle.Paragraphs.Add(p);
                }

                var next = subtitle.GetParagraphOrDefault(i);
                var nextNext = subtitle.GetParagraphOrDefault(i + 1);
                if (next != null)
                {
                    if (Utilities.QualifiesForMerge(p, next, maxMillisecondsBetweenLines, ParagraphMaxChars, onlyContinuousLines))
                    {
                        MergeNextIntoP(language, p, next);
                        lastMerged = true;
                    }
                    else if (IsNextCloseAndAlone(p, next, nextNext, maxMillisecondsBetweenLines, onlyContinuousLines))
                    {
                        var splitDone = false;
                        if (language != "jp" && language != "cn")
                        {
                            var pNew = new Paragraph(p);
                            MergeNextIntoP(language, pNew, next);
                            var textNoHtml = HtmlUtil.RemoveHtmlTags(pNew.Text, true);
                            var arr = textNoHtml.SplitToLines();
                            foreach (var line in arr)
                            {
                                if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                                {
                                    var text = Utilities.AutoBreakLine(pNew.Text, language);
                                    arr = text.SplitToLines();
                                    if (arr.Count == 2)
                                    {
                                        p.Text = Utilities.AutoBreakLine(arr[0], language);
                                        next.Text = Utilities.AutoBreakLine(arr[1], language);
                                        //TODO: calc time
                                        splitDone = true;
                                    }

                                    break;
                                }
                            }
                        }

                        if (splitDone)
                        {
                            lastMerged = false;
                        }
                        else
                        {
                            MergeNextIntoP(language, p, next);
                            lastMerged = true;
                        }
                    }
                    else
                    {
                        lastMerged = false;
                    }
                }
                else
                {
                    lastMerged = false;
                }
            }

            if (!lastMerged)
            {
                var last = subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1);
                if (last != null && !string.IsNullOrWhiteSpace(last.Text))
                {
                    mergedSubtitle.Paragraphs.Add(new Paragraph(last));
                }
            }

            return mergedSubtitle;
        }

        public Subtitle AutoBalanceLines(Subtitle subtitle, string language)
        {
            if (language == "jp" || language == "cn")
            {
                return subtitle;
            }

            const int maxMillisecondsBetweenLines = 100;
            var shortLineCharsCount = Configuration.Settings.General.SubtitleLineMaximumLength;
            var deleteItems = new List<Paragraph>();

            var s = new Subtitle(subtitle);
            for (var i = 0; i < s.Paragraphs.Count-1; i++)
            {
                var p = s.GetParagraphOrDefault(i);
                var next = s.GetParagraphOrDefault(i + 1);

                if (p == null || next == null || deleteItems.Contains(p))
                {
                    continue;
                }

                var gapToNext = next.StartTime.TotalMilliseconds- p.EndTime.TotalMilliseconds;
                if (gapToNext > maxMillisecondsBetweenLines || p.Text.HasSentenceEnding(language))
                {
                    continue;
                }

                if (next.Text.Contains(Environment.NewLine) || next.Text.Length > shortLineCharsCount)
                {
                    continue;
                }

                var text = Utilities.UnbreakLine(p.Text + " " + next.Text);
                var newText = Utilities.AutoBreakLine(text, language);
                var arr = newText.SplitToLines();
                if (arr.Count == 2)
                {
                    if (arr[0].CountCharacters(false) < Configuration.Settings.General.SubtitleLineMaximumLength &&
                        arr[1].CountCharacters(false) < Configuration.Settings.General.SubtitleLineMaximumLength)
                    {
                        p.Text = newText;
                        deleteItems.Add(next);
                    }
                    else
                    {
                        p.Text = Utilities.AutoBreakLine(arr[0], language);
                        next.Text = Utilities.AutoBreakLine(arr[1], language);
                    }
                }
            }

            foreach (var deleteItem in deleteItems)
            {
                s.Paragraphs.Remove(deleteItem);
            }

            s.Renumber();
            return s;
        }

        private bool IsNextCloseAndAlone(Paragraph p, Paragraph next, Paragraph nextNext, int maxMillisecondsBetweenLines, bool onlyContinuousLines)
        {
            if (nextNext == null || next.Text.Length > 12)
            {
                return false;
            }

            if (!Utilities.QualifiesForMerge(p, next, maxMillisecondsBetweenLines, ParagraphMaxChars + 5, onlyContinuousLines))
            {
                return false;
            }

            if (nextNext.StartTime.TotalMilliseconds - next.EndTime.TotalMilliseconds < maxMillisecondsBetweenLines + 100)
            {
                return false;
            }

            return true;
        }

        private static void MergeNextIntoP(string language, Paragraph p, Paragraph next)
        {
            if (GetStartTag(p.Text) == GetStartTag(next.Text) &&
                GetEndTag(p.Text) == GetEndTag(next.Text))
            {
                var s1 = p.Text.Trim();
                s1 = s1.Substring(0, s1.Length - GetEndTag(s1).Length);
                var s2 = next.Text.Trim();
                s2 = s2.Substring(GetStartTag(s2).Length);
                p.Text = Utilities.AutoBreakLine(s1 + Environment.NewLine + s2, language);
            }
            else
            {
                p.Text = Utilities.AutoBreakLine(p.Text + Environment.NewLine + next.Text, language);
            }

            p.EndTime = next.EndTime;

            if (language == "jp" || language == "cn")
            {
                p.Text = p.Text.RemoveChar('\r').RemoveChar('\n').RemoveChar(' ');
            }
        }

        public static Subtitle FixCasing(Subtitle inputSubtitle, string language, Engine engine)
        {
            var subtitle = new Subtitle(inputSubtitle);

            // fix casing for names
            var nameList = new NameList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            var nameListInclMulti = nameList.GetAllNames();
            if (nameListInclMulti.Contains("US") && language == "en")
            {
                nameListInclMulti.Remove("US");
            }

            var lastLine = string.Empty;
            foreach (var paragraph in subtitle.Paragraphs)
            {
                var text = paragraph.Text;
                var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
                if (textNoTags != textNoTags.ToUpperInvariant() && !string.IsNullOrEmpty(text))
                {
                    var st = new StrippableText(text);
                    st.FixCasing(nameListInclMulti, true, engine == Engine.Vosk, engine == Engine.Vosk, lastLine);
                    paragraph.Text = st.MergedString;
                }

                lastLine = text;
            }

            // fix german nouns
            if (language == "de")
            {
                var germanNouns = new GermanNouns();
                foreach (var paragraph in subtitle.Paragraphs)
                {
                    paragraph.Text = germanNouns.UppercaseNouns(paragraph.Text);
                }
            }

            return subtitle;
        }

        private static string GetEndTag(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            text = text.Trim();
            if (!text.EndsWith('>'))
            {
                return string.Empty;
            }

            var endTag = string.Empty;
            var start = text.LastIndexOf("</", StringComparison.Ordinal);
            if (start > 0 && start >= text.Length - 8)
            {
                endTag = text.Substring(start);
            }
            return endTag;
        }

        private static string GetStartTag(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            text = text.Trim();
            if (!text.StartsWith('<'))
            {
                return string.Empty;
            }

            var startTag = string.Empty;
            var end = text.IndexOf('>');
            if (end > 0 && end < 25)
            {
                startTag = text.Substring(0, end + 1);
            }

            return startTag;
        }

        private static Subtitle FixShortDuration(Subtitle inputSubtitle)
        {
            var subtitle = new Subtitle(inputSubtitle);
            new FixShortDisplayTimes().Fix(subtitle, new EmptyFixCallback());
            return subtitle;
        }
    }
}
