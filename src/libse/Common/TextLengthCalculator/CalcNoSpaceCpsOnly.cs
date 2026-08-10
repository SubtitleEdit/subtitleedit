namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public  class CalcNoSpaceCpsOnly : ICalcLength
    {
        // Both are stateless; CalcFactory memoizes the strategy lookup, so allocating a fresh
        // one per call - on every grid repaint and keystroke - threw that away.
        private static readonly CalcNoSpace NoSpace = new CalcNoSpace();
        private static readonly CalcAll All = new CalcAll();

        /// <summary>
        /// Calculate all text excluding space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            if (forCps)
            {
                return NoSpace.CountCharacters(text, true);
            }

            return All.CountCharacters(text, false);
        }
    }
}
