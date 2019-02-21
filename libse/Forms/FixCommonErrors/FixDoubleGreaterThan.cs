using System;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixDoubleGreaterThan : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixDoubleGreaterThan;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
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
                        callbacks.AddFixToListView(p, fixAction, oldText, text);
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, language.FixDoubleGreaterThan, language.XFixDoubleGreaterThan);
        }

    }
}
