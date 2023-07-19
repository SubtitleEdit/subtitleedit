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
            var fixAction = Language.UnneededSpace;
            var doubleSpaces = 0;
            foreach (var p in subtitle.Paragraphs)
            {
                if (!callbacks.AllowFix(p, fixAction))
                    continue;

                var oldText = p.Text;
                var text = Utilities.RemoveUnneededSpaces(p.Text, callbacks.Language);

                if (text.Length == oldText.Length)
                    continue;

                var newTextCount = CalculateCharacterCounts(text);
                var oldTextCount = CalculateCharacterCounts(oldText);

                if (newTextCount < oldTextCount)
                {
                    doubleSpaces++;
                    p.Text = text;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }

            callbacks.UpdateFixStatus(doubleSpaces, Language.RemoveUnneededSpaces);
        }

        private int CalculateCharacterCounts(string text)
        {
            return Utilities.CountTagInText(text, ' ')
                   + Utilities.CountTagInText(text, '\u00A0')
                   + Utilities.CountTagInText(text, '\t')
                   + Utilities.CountTagInText(text, Environment.NewLine);
        }
    }
}