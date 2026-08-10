namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public class CalcNoSpaceOrPunctuationCpsOnly : ICalcLength
    {
        // Both are stateless; CalcFactory memoizes the strategy lookup, so allocating a fresh
        // one per call - on every grid repaint and keystroke - threw that away.
        private static readonly CalcNoSpaceOrPunctuation NoSpaceOrPunctuation = new CalcNoSpaceOrPunctuation();
        private static readonly CalcAll All = new CalcAll();

        /// <summary>
        /// Calculate all text except punctuation or space (tags are not counted) for cps only.
        /// Line length calc all characters.
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            if (forCps)
            {
                return NoSpaceOrPunctuation.CountCharacters(text, false);
            }

            return All.CountCharacters(text, false);
        }
    }
}
