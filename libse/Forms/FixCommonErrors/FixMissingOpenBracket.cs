using System;
using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixMissingOpenBracket : IFixCommonError
    {
        private static string Fix(string text, string openB)
        {
            string pre = string.Empty;
            string closeB = openB == "(" ? ")" : "]";

            if (text.Contains(" " + closeB))
            {
                openB = openB + " ";
            }

            do
            {
                if (text.Length > 1 && text.StartsWith('-'))
                {
                    pre += "- ";
                    if (text[1] == ' ')
                    {
                        text = text.Substring(2);
                    }
                    else
                    {
                        text = text.Substring(1);
                    }
                }
                if (text.Length > 3 && text.StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    pre += "<i>";
                    if (text[3] == ' ')
                    {
                        text = text.Substring(4);
                    }
                    else
                    {
                        text = text.Substring(3);
                    }
                }
                if (text.Length > 1 && (text[0] == ' ' || text[0] == '.'))
                {
                    pre += text[0] == '.' ? '.' : ' ';
                    text = text.Substring(1);
                    while (text.Length > 0 && text[0] == '.')
                    {
                        pre += ".";
                        text = text.Substring(1);
                    }
                    text = text.TrimStart(' ');
                }
            } while (text.StartsWith("<i>", StringComparison.Ordinal) || text.StartsWith('-'));

            text = pre + openB + text;
            return text;
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.FixMissingOpenBracket;
            int fixCount = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];

                if (callbacks.AllowFix(p, fixAction))
                {
                    var hit = false;
                    string oldText = p.Text;
                    var openIdx = p.Text.IndexOf('(');
                    var closeIdx = p.Text.IndexOf(')');
                    if (closeIdx >= 0 && (closeIdx < openIdx || openIdx < 0))
                    {
                        p.Text = Fix(p.Text, "(");
                        hit = true;
                    }

                    openIdx = p.Text.IndexOf('[');
                    closeIdx = p.Text.IndexOf(']');
                    if (closeIdx >= 0 && (closeIdx < openIdx || openIdx < 0))
                    {
                        p.Text = Fix(p.Text, "[");
                        hit = true;
                    }

                    if (hit)
                    {
                        fixCount++;
                        callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                    }
                }
            }
            callbacks.UpdateFixStatus(fixCount, language.FixMissingOpenBracket, language.XFixMissingOpenBracket);
        }

    }
}
