using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixShortLinesAll : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.MergeShortLineAll;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                string text = p.Text;
                if (callbacks.AllowFix(p, fixAction) && text.Contains(Environment.NewLine))
                {
                    text = HtmlUtil.RemoveHtmlTags(text, true);
                    text = text.Replace(Environment.NewLine, " ");
                    text = text.FixExtraSpaces();
                    if (text.Length < Configuration.Settings.Tools.MergeLinesShorterThan)
                    {
                        text = Utilities.AutoBreakLine(p.Text, callbacks.Language);
                        if (text != p.Text)
                        {
                            fixCount++;
                            callbacks.AddFixToListView(p, fixAction, p.Text, text);
                            p.Text = text;
                        }
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, language.RemoveLineBreaks, string.Format(language.XLinesUnbreaked, fixCount));
        }
    }
}
