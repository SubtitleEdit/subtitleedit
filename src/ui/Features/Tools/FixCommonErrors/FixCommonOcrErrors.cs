using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.SpellCheck;
using Nikse.SubtitleEdit.Logic.Config;
using System.Linq;

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
            var spellChecker = spellCheckers.FirstOrDefault(x => x.GetThreeLetterCode() == threeLetterCode);
            if (spellChecker == null)
            {
                return;
            }

            OcrFixEngine.Initialize(subtitle, threeLetterCode, spellChecker);

            var fixAction = Language.FixText;
            var fixCount = 0;
            for (var i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var result = OcrFixEngine.FixOcrErrors(i, p.Text, true);
                var text = result.GetText();
                if (text != p.Text)
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