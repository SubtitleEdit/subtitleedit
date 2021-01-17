using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixEllipsesStart : IFixCommonError
    {
        public static class Language
        {
            public static string FixEllipsesStart { get; set; } = "Remove leading '...'";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                var text = p.Text;
                if (text.Contains("..") && callbacks.AllowFix(p, Language.FixEllipsesStart))
                {
                    var oldText = text;
                    var lines = text.SplitToLines();
                    for (int k = 0; k < lines.Count; k++)
                    {
                        lines[k] = Helper.FixEllipsesStartHelper(lines[k]);
                    }
                    text = string.Join(Environment.NewLine, lines);
                    if (oldText.Length > text.Length)
                    {
                        p.Text = text;
                        fixCount++;
                        callbacks.AddFixToListView(p, Language.FixEllipsesStart, oldText, text);
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, Language.FixEllipsesStart);
        }
    }
}
