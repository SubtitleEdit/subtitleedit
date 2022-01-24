using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace Nikse.SubtitleEdit.Core.AudioToText
{
    public class AudioToTextPostProcessor
    {
        private List<ResultText> _resultTexts;

        /// <summary>
        /// Set period if distance to next subtitle is more than this value in milliseconds.
        /// </summary>
        public double SetPeriodIfDistanceToNextIsMoreThan { get; set; } = 300;
        public double SetPeriodIfDistanceToNextIsMoreThanAlways { get; set; } = 500;

        public int ParagraphMaxChars { get; set; } = 86;

        public string TwoLetterLanguageCode { get; private set; }


        public AudioToTextPostProcessor(string twoLetterLanguageCode)
        {
            TwoLetterLanguageCode = twoLetterLanguageCode;
        }

        public Subtitle Generate(List<ResultText> resultTexts, bool usePostProcessing)
        {
            _resultTexts = resultTexts;
            var subtitle = new Subtitle();
            foreach (var resultText in _resultTexts)
            {
                if (usePostProcessing && TwoLetterLanguageCode == "en" && resultText.Text == "the" && resultText.End - resultText.Start > 1)
                {
                    continue;
                }

                subtitle.Paragraphs.Add(new Paragraph(resultText.Text, (double)resultText.Start * 1000.0, (double)resultText.End * 1000.0));
            }

            if (usePostProcessing)
            {
                subtitle = AddPeriods(subtitle, TwoLetterLanguageCode);
                subtitle = MergeShortLines(subtitle, TwoLetterLanguageCode);
                subtitle = FixCasing(subtitle, TwoLetterLanguageCode);
                subtitle = FixShortDuration(subtitle);
            }

            subtitle.Renumber();
            return subtitle;
        }

        private Subtitle AddPeriods(Subtitle inputSubtitle, string language)
        {
            var englishSkipLastWords = new string[] { "with", "however" };
            var englishSkipFirstWords = new string[] { "to", "and", "but" };

            var subtitle = new Subtitle(inputSubtitle);
            for (var index = 0; index < subtitle.Paragraphs.Count - 1; index++)
            {
                var paragraph = subtitle.Paragraphs[index];
                var next = subtitle.Paragraphs[index + 1];
                if (next.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds > SetPeriodIfDistanceToNextIsMoreThan &&
                    !paragraph.Text.EndsWith('.') &&
                    !paragraph.Text.EndsWith('!') &&
                    !paragraph.Text.EndsWith('?') &&
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

            if (subtitle.Paragraphs.Count > 0 &&
                !subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text.EndsWith('.') &&
                !subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text.EndsWith('!') &&
                !subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text.EndsWith('?') &&
                !subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text.EndsWith(':'))
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

        private Subtitle MergeShortLines(Subtitle subtitle, string language)
        {
            var maxMillisecondsBetweenLines = 100;
            const bool onlyContinuousLines = true;

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
                if (next != null)
                {
                    if (Utilities.QualifiesForMerge(p, next, maxMillisecondsBetweenLines, ParagraphMaxChars, onlyContinuousLines))
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

                        lastMerged = true;
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
                mergedSubtitle.Paragraphs.Add(new Paragraph(subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1)));
            }

            return mergedSubtitle;
        }

        private static Subtitle FixCasing(Subtitle inputSubtitle, string language)
        {
            var subtitle = new Subtitle(inputSubtitle);

            // fix casing normal
            var fixCasing = new FixCasing(language);
            fixCasing.Fix(subtitle);

            // fix casing for names
            var nameList = new NameList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            var nameListInclMulti = nameList.GetAllNames();
            foreach (var paragraph in subtitle.Paragraphs)
            {
                var text = paragraph.Text;
                var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
                if (textNoTags != textNoTags.ToUpperInvariant() && !string.IsNullOrEmpty(text))
                {
                    var st = new StrippableText(text);
                    st.FixCasing(nameListInclMulti, true, false, false, string.Empty);
                    paragraph.Text = st.MergedString;
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
