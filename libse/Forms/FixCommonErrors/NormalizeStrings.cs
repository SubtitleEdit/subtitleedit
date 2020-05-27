using Nikse.SubtitleEdit.Core.Interfaces;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class NormalizeStrings : IFixCommonError
    {
        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var language = Configuration.Settings.Language.FixCommonErrors;
            string fixAction = language.NormalizeStrings;
            int noOfFixes = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var oldText = p.Text;
                var text = p.Text
                        .Normalize()

                        .Replace('\u00a0', ' ') // replace non-break-space (160 decimal) ascii char with normal space
                        .Replace("\u200B", string.Empty) // Zero Width Space
                        .Replace("\uFEFF", string.Empty) // Zero Width No-Break Space

                        .Replace('\u02F8', ':') // ˸ Modifier Letter Raised Colon (\u02F8)
                        .Replace('\uFF1A', ':') // ： Fullwidth Colon (\uFF1A)
                        .Replace('\uFE13', ':') // ︓ Presentation Form for Vertical Colon (\uFE13)

                        .Replace('\u2043', '-') // ⁃ Hyphen bullet (\u2043)
                        .Replace('\u2010', '-') // ‐ Hyphen (\u2010)
                        .Replace('\u2012', '-') // ‒ Figure dash (\u2012)
                        .Replace('\u2013', '-') // – En dash (\u2013)
                        .Replace('\u2014', '-') // — Em dash (\u2014)
                        .Replace('\u2015', '-') // ― Horizontal bar (\u2015)
                    ;

                if (oldText != text && callbacks.AllowFix(p, fixAction))
                {
                    p.Text = text;
                    noOfFixes++;
                    callbacks.AddFixToListView(p, fixAction, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, language.FixCommonOcrErrors, language.FixDialogsOneLineExample);
        }
    }
}
