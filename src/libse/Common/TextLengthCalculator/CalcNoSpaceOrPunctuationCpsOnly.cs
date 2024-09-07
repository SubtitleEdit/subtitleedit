namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcNoSpaceOrPunctuationCpsOnly : ICalcLength
    {
        /// <summary>
        /// Calculate all text except punctuation or space (tags are not counted) for cps only.
        /// Line length calc all characters.
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            if (forCps)
            {
                return new CalcNoSpaceOrPunctuation().CountCharacters(text, false);
            }

            return new CalcAll().CountCharacters(text, false);
        }
    }
}
