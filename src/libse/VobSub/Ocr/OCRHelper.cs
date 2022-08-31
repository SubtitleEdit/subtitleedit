using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Core.VobSub.Ocr
{
    public static class OCRHelper
    {
        public static string PostOCR(string input, string language)
        {
            var s = input;
            return FixInvalidCarriageReturnLineFeedCharacters(s);
        }

        private static string FixInvalidCarriageReturnLineFeedCharacters(string input)
        {
            // Fix new line chars
            return string.Join(Environment.NewLine, input.SplitToLines()).Trim();
        }
    }
}
