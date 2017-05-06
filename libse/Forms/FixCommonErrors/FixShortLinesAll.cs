using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixShortLinesAll : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.MergeShortLineAll;
            int noOfShortLines = 0;
            bool isCRLF = Environment.NewLine.Length == 2;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    string s = HtmlUtil.RemoveHtmlTags(p.Text, true);
                    int newLineCharCount = Utilities.CountTagInText(s, Environment.NewLine);
                    int len = s.Length;
                    if (isCRLF)
                    {
                        // keep one character to represent space
                        // e.g foobar\r\nfoobar => foobar foobar
                        len -= newLineCharCount;
                    }
                    if (newLineCharCount > 0 && len < Configuration.Settings.Tools.MergeLinesShorterThan)
                    {
                        s = Utilities.AutoBreakLine(p.Text, callbacks.Language);
                        if (s != p.Text)
                        {
                            string oldCurrent = p.Text;
                            p.Text = s;
                            noOfShortLines++;
                            callbacks.AddFixToListView(p, fixAction, oldCurrent, p.Text);
                        }
                    }
                }
            }
            callbacks.UpdateFixStatus(noOfShortLines, language.RemoveLineBreaks, string.Format(language.XLinesUnbreaked, noOfShortLines));
        }
    }
}
