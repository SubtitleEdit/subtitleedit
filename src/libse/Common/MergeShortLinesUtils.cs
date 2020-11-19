using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    public static class MergeShortLinesUtils
    {
        public static Subtitle MergeShortLinesInSubtitle(Subtitle subtitle, double maxMillisecondsBetweenLines, int maxCharacters, bool onlyContinuousLines)
        {
            string language = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
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

            if (!lastMerged && subtitle.Paragraphs.Count > 0)
            {
                mergedSubtitle.Paragraphs.Add(new Paragraph(subtitle.GetParagraphOrDefault(subtitle.Paragraphs.Count - 1)));
            }

            return mergedSubtitle;
        }

        public static string GetEndTag(string text)
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
            int start = text.LastIndexOf("</", StringComparison.Ordinal);
            if (start > 0 && start >= text.Length - 8)
            {
                endTag = text.Substring(start);
            }
            return endTag;
        }

        public static string GetStartTag(string text)
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
            int end = text.IndexOf('>');
            if (end > 0 && end < 25)
            {
                startTag = text.Substring(0, end + 1);
            }
            return startTag;
        }
    }
}
