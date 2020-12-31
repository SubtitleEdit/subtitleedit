using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixUnneededSpaces : IFixCommonError
    {
        public static class Language
        {
            public static string UnneededSpace { get; set; } = "Unneeded space";
            public static string RemoveUnneededSpaces { get; set; } = "Remove unneeded spaces";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            string fixAction = Language.UnneededSpace;
            int doubleSpaces = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                if (callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;
                    var text = Utilities.RemoveUnneededSpaces(p.Text, callbacks.Language);
                    if (text.Length != oldText.Length && Utilities.CountTagInText(text, ' ') + Utilities.CountTagInText(text, '\t') + Utilities.CountTagInText(text, Environment.NewLine) < (Utilities.CountTagInText(oldText, ' ') + Utilities.CountTagInText(oldText, '\u00A0') + Utilities.CountTagInText(oldText, '\t') + Utilities.CountTagInText(oldText, Environment.NewLine)))
                    {
                        doubleSpaces++;
                        p.Text = text;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(doubleSpaces, Language.RemoveUnneededSpaces);
        }
    }
}
