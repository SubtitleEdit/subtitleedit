using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace Nikse.SubtitleEdit.Core.Forms.FixCommonErrors
{
    public class FixCommonOcrErrors : IFixCommonError
    {
        public static IOcrFixEngine? OcrFixEngine { get; internal set; }

        public static class Language
        {
            public static string FixText { get; set; } = "Fix common OCR errors";
        }

        public void Fix(Subtitle subtitle, IFixCallbacks callbacks)
        {
            if (OcrFixEngine == null)
            {
                return;
            }

            var language = callbacks.Language;
            var threeLetterCode = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(language);
            if (string.IsNullOrEmpty(threeLetterCode))
            {
                return;
            }

            var spellCheckManager = new SpellCheckManager();
            var spellCheckers = spellCheckManager.GetDictionaryLanguages(Se.DictionariesFolder);

            // Fall back to an empty dictionary display when no Hunspell dictionary is installed for the
            // language. The OCR replace list (e.g. spa_OCRFixReplaceList.xml) is applied regardless of
            // spell-checking; without a dictionary the spell-check-driven "guess" path is simply a no-op.
            // Requiring a matching .dic here made the feature silently do nothing on a fresh config, since
            // only en_US.dic ships with SE - a regression from SE 4 (issue #12126).
            var spellChecker = spellCheckers.FirstOrDefault(x => x.GetThreeLetterCode() == threeLetterCode)
                               ?? new SpellCheckDictionaryDisplay();

            OcrFixEngine.Initialize(subtitle, threeLetterCode, spellChecker);

            var fixAction = Language.FixText;
            var fixCount = 0;
            // Unknown-word guessing (which splits words it reads as two words run together) is this
            // tool's own setting and is off by default - on a normal subtitle it breaks far more words
            // than it fixes (#12441). It used to follow the OCR window's setting, which defaults on.
            var doTryToGuessUnknownWords = Se.Settings.Tools.FixCommonErrors.TryToGuessUnknownWords;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var result = OcrFixEngine.FixOcrErrors(i, p.Text, doTryToGuessUnknownWords);
                var text = result.GetText();

                // AllowFix is what honors the fix's check box in the apply pass (it is always true while
                // previewing). Without it, every OCR fix was written even when the user unticked it (#12441).
                if (text != p.Text && callbacks.AllowFix(p, fixAction))
                {
                    var oldText = p.Text;
                    fixCount++;
                    callbacks.AddFixToListView(p, fixAction, oldText, text);
                    p.Text = text;
                }
            }

            callbacks.UpdateFixStatus(fixCount, fixAction);
        }
    }
}