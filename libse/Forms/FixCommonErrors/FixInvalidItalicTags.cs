using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixInvalidItalicTags : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            const string beginTagUpper = "<I>";
            const string endTagUpper = "</I>";
            const string beginTag = "<i>";
            const string endTag = "</i>";
            string fixAction = language.FixInvalidItalicTag;
            int noOfInvalidHtmlTags = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                if (callbacks.AllowFix(subtitle.Paragraphs[i], fixAction))
                {
                    var text = subtitle.Paragraphs[i].Text;
                    if (text.Contains('<'))
                    {
                        if (!text.Contains("i>") && !text.Contains("<i"))
                        {
                            text = text.Replace(beginTagUpper, beginTag).Replace(endTagUpper, endTag);
                        }

                        string oldText = text;

                        text = HtmlUtil.FixInvalidItalicTags(text);
                        if (text != oldText)
                        {
                            subtitle.Paragraphs[i].Text = text;
                            noOfInvalidHtmlTags++;
                            callbacks.AddFixToListView(subtitle.Paragraphs[i], fixAction, oldText, text);
                        }
                    }
                }
            }

            callbacks.UpdateFixStatus(noOfInvalidHtmlTags, language.FixInvalidItalicTags, string.Format(language.XInvalidHtmlTagsFixed, noOfInvalidHtmlTags));
        }

    }
}
