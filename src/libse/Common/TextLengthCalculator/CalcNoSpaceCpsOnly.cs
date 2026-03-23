namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public  class CalcNoSpaceCpsOnly : ICalcLength
    {
        /// <summary>
        /// Calculate all text excluding space (tags are not counted).
        /// </summary>
        public decimal CountCharacters(string text, bool forCps)
        {
            if (forCps)
            {
                return new CalcNoSpace().CountCharacters(text, true);
            }

            return new CalcAll().CountCharacters(text, false);
        }
    }
}
