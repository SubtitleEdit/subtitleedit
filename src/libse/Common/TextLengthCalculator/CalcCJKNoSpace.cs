namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcCjkNoSpace : ICalcLength
    {
        /// <summary>
        /// Calculate all text including space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var s = HtmlUtil.RemoveHtmlTags(text, true);
            return CalcCjk.Count(s, skipSpace: true, includeJapaneseFullWidth: false);
        }
    }
}
