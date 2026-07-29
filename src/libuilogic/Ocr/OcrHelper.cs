using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.UiLogic.Ocr
{
    public static class OcrHelper
    {
        public static string PostOcr(string input, string language)
        {
            return FixInvalidCarriageReturnLineFeedCharacters(input);
        }

        private static string FixInvalidCarriageReturnLineFeedCharacters(string input)
        {
            return string.Join(Environment.NewLine, input.SplitToLines()).Trim();
        }
    }
}
