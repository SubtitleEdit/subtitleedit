using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr.Service
{
    public class OcrLanguage
    {
        public string Code { get; set; }

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