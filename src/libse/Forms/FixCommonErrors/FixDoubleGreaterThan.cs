using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixDoubleGreaterThan : IFixCommonError
    {
        public static class Language
        {
            public static string FixDoubleGreaterThan { get; set; } = "Remove '>>'";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, Language.FixDoubleGreaterThan))
                {
                    if (!p.Text.Contains(">>", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var text = p.Text;
                    var oldText = text;
                    if (!text.Contains(Environment.NewLine))
                    {
                        text = Helper.FixDoubleGreaterThanHelper(text);
                    }
                    else
                    {
                        var lines = text.SplitToLines();
                        for (int k = 0; k < lines.Count; k++)
                        {
                            lines[k] = Helper.FixDoubleGreaterThanHelper(lines[k]);
                        }
                        text = string.Join(Environment.NewLine, lines);
                    }
                    if (oldText != text)
                    {
                        fixCount++;
                        p.Text = text;
                        callbacks.AddFixToListView(p, Language.FixDoubleGreaterThan, oldText, text);
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, Language.FixDoubleGreaterThan);
        }

    }
}
