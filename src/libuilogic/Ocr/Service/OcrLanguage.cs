using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.UiLogic.Ocr.Service
{
    public class OcrLanguage
    {
        public string Code { get; set; } = string.Empty;

        public override string ToString()
        {
            var displayName = Code;

            if (Code.Length == 2)
            {
                var isoName = Iso639Dash2LanguageCode.List.FirstOrDefault(p => p.TwoLetterCode == Code);
                if (isoName != null)
                {
                    displayName = isoName.EnglishName;
                }
            }

            return displayName;
        }
    }
}