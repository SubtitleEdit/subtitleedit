using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.AudioToText.PhocketSphinx;
using Nikse.SubtitleEdit.Core.Dictionaries;

namespace Nikse.SubtitleEdit.Core.AudioToText.PocketSphinx
{
    public class SubtitleGenerator
    {
        private readonly List<ResultText> _resultTexts;

        /// <summary>
        /// Set period if distance to next subtitle is more than this value in milliseconds
        /// </summary>
        public double SetPeriodIfDistanceToNextIsMoreThan { get; set; }

        public SubtitleGenerator(List<ResultText> resultTexts)
        {
            _resultTexts = resultTexts;
            SetPeriodIfDistanceToNextIsMoreThan = 250;
        }

        public Subtitle Generate(string language)
        {
            var subtitle = new Subtitle();
            foreach (var resultText in _resultTexts)
            {
                subtitle.Paragraphs.Add(new Paragraph(resultText.Text, resultText.Start * 1000.0, resultText.End * 1000.0));
            }

            AddPeriods(subtitle, language);

            //  subtitle = MergeShortLines(subtitle, language);

            FixCasing(subtitle, language);

            return subtitle;
        }

        private void AddPeriods(Subtitle subtitle, string language)
        {
            //TODO: check of English non-break words

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
                    paragraph.Text += ".";
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
        }

        private Subtitle MergeShortLines(Subtitle subtitle, string language)
        {
            int maxMillisecondsBetweenLines = 100;
            int maxCharacters = 90;
            const bool onlyContinuousLines = true;

            var mergedSubtitle = new Subtitle();
            bool lastMerged = false;
            Paragraph p = null;
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                if (!lastMerged)
                {
                    p = new Paragraph(subtitle.GetParagraphOrDefault(i - 1));
                    mergedSubtitle.Paragraphs.Add(p);
                }
                var next = subtitle.GetParagraphOrDefault(i);
                if (next != null)
                {
                    if (Utilities.QualifiesForMerge(p, next, maxMillisecondsBetweenLines, maxCharacters, onlyContinuousLines))
                    {
                        if (GetStartTag(p.Text) == GetStartTag(next.Text) &&
                            GetEndTag(p.Text) == GetEndTag(next.Text))
                        {
                            string s1 = p.Text.Trim();
                            s1 = s1.Substring(0, s1.Length - GetEndTag(s1).Length);
                            string s2 = next.Text.Trim();
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

        private static void FixCasing(Subtitle subtitle, string language)
        {
            // fix casing normal
            var fixCasing = new FixCasing(language);
            fixCasing.Fix(subtitle);

            // fix casing for names
            var nameList = new NameList(Configuration.DictionariesDirectory, language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);
            var nameListInclMulti = nameList.GetAllNames();
            foreach (var paragraph in subtitle.Paragraphs)
            {
                string text = paragraph.Text;
                string textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
                if (textNoTags != textNoTags.ToUpperInvariant())
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        var st = new StrippableText(text);
                        st.FixCasing(nameListInclMulti, true, false, false, string.Empty);
                        paragraph.Text = st.MergedString;
                    }
                }
            }
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

            string endTag = string.Empty;
            int start = text.LastIndexOf("</", StringComparison.Ordinal);
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

            string startTag = string.Empty;
            int end = text.IndexOf('>');
            if (end > 0 && end < 25)
            {
                startTag = text.Substring(0, end + 1);
            }
            return startTag;
        }

    }
}
