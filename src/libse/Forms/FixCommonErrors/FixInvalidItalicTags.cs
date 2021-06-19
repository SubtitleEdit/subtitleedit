using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixInvalidItalicTags : IFixCommonError
    {
        public static class Language
        {
            public static string FixInvalidItalicTag { get; set; } = "Fix invalid italic tag";
            public static string FixInvalidItalicTags { get; set; } = "Fix invalid italic tags";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.FixInvalidItalicTag;
            int noOfInvalidHtmlTags = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                if (callbacks.AllowFix(subtitle.Paragraphs[i], fixAction))
                {
                    var text = subtitle.Paragraphs[i].Text;
                    if (text.Contains('<'))
                    {
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

            callbacks.UpdateFixStatus(noOfInvalidHtmlTags, Language.FixInvalidItalicTags);
        }

    }
}
