using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class NormalizeStrings : IFixCommonError
    {
        public static class Language
        {
            public static string NormalizeStrings { get; set; } = "Normalize strings";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            var twoLetterLanguageCode = callbacks.Language;
            var noOfFixes = 0;
            foreach (var p in subtitle.Paragraphs)
            {
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
                        .Replace('\u2011', '-') // ‑ Non-breaking hyphen (\u2011)
                        .Replace('\u2012', '-') // ‒ Figure dash (\u2012)
                        .Replace('\u2013', '-') // – En dash (\u2013)
                                                //.Replace('\u2014', '-') // — Em dash (\u2014) - we keep em dash
                        .Replace('\u2015', '\u2014') // ― Horizontal bar (\u2015)

                        // Machine translation hands back typographic punctuation the source never
                        // had - DeepL alone answers plain ASCII input with curly quotes, en dashes
                        // and a one-character ellipsis (issue #14092).
                        .Replace("\u2026", "...") // … Horizontal ellipsis (\u2026)
                        .Replace('\u2032', '\'') // ′ Prime (\u2032)
                        .Replace('\u2033', '"') // ″ Double prime (\u2033)
                    ;

                // Quotation marks are the language-specific part of this: „…“ and ‚…‘ are how German
                // and several of its neighbours quote, so replacing them there would take the
                // language's own punctuation away rather than normalize anything.
                var lowQuoteLanguages = new[] { "de", "cs", "sk", "pl", "sl", "hr", "sr", "hu", "lt", "et", "ro", "is", "bg", "ka", "sq" };
                if (!lowQuoteLanguages.Contains(twoLetterLanguageCode))
                {
                    text = text
                            .Replace('\u2018', '\'') // ‘ Left single quotation mark (\u2018)
                            .Replace('\u2019', '\'') // ’ Right single quotation mark (\u2019) - the typographic apostrophe
                            .Replace('\u201A', '\'') // ‚ Single low-9 quotation mark (\u201A)
                            .Replace('\u201B', '\'') // ‛ Single high-reversed-9 quotation mark (\u201B)
                            .Replace('\u201C', '"') // “ Left double quotation mark (\u201C)
                            .Replace('\u201D', '"') // ” Right double quotation mark (\u201D)
                            .Replace('\u201E', '"') // „ Double low-9 quotation mark (\u201E)
                            .Replace('\u201F', '"') // ‟ Double high-reversed-9 quotation mark (\u201F)
                        ;
                }

                // Replacing Cyrillic "е" with Latin "e" is an OCR heuristic for Latin-script
                // languages. Only Russian was exempt, so Bulgarian, Ukrainian, Serbian, Macedonian
                // and the rest had a Latin "e" spliced into Cyrillic words - breaking spell check,
                // search and sorting.
                var cyrillicLanguages = new[] { "ru", "uk", "bg", "sr", "mk", "be", "kk", "ky", "mn", "tg" };
                if (!cyrillicLanguages.Contains(twoLetterLanguageCode))
                {
                    text = text
                            .Replace('\u0435', 'e') // Cyrillic Small Letter Ie: "е"
                        ;
                }

                if (twoLetterLanguageCode != "el")
                {
                    text = text
                            .Replace("\u03a4", "T") // Greek Capital Letter Tau
                            .Replace("\u03a5", "Y") // Greek Capital Letter Upsilon
                            .Replace("\u03b3", "Y") // Greek Small Letter Gamma
                            .Replace("\u03a7", "X") // Greek Capital Letter Chi
                            .Replace("\u03ba", "k") // Greek Small Letter Kappa
                            .Replace("\u03bd", "v") // Greek Small Letter Nu
                            .Replace("\u03c1", "p") // Greek Small Letter Rho
                            .Replace("\u03c5", "u") // Greek Small Letter Upsilon
                            .Replace("\u039c", "M") // Greek Capital Letter Mu
                            .Replace("\u039a", "K") // Greek Capital Letter Kappa
                            .Replace("\u039d", "N") // Greek Capital Letter Nu
                            .Replace("\u039f", "O") // Greek Capital Letter Omicron
                            .Replace("\u03a1", "P") // Greek Capital Letter Rho
                            .Replace("\u0395", "E") // Greek Capital Letter Epsilon
                            .Replace("\u0396", "Z") // Greek Capital Letter Zeta
                            .Replace("\u0397", "H") // Greek Capital Letter Eta
                            .Replace("\u0384", "'") // Greek Tonos
                            .Replace("\u0392", "B") // Greek Capital Letter Beta
                            .Replace("\u0391", "A") // Greek Capital Letter Alpha
                            .Replace("\u03bf", "o") // Greek small letter Omicron
                        ;
                }

                if (oldText != text && callbacks.AllowFix(p, Language.NormalizeStrings))
                {
                    p.Text = text;
                    noOfFixes++;
                    callbacks.AddFixToListView(p, Language.NormalizeStrings, oldText, p.Text);
                }
            }
            callbacks.UpdateFixStatus(noOfFixes, Language.NormalizeStrings);
        }
    }
}
