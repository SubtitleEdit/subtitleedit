using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Core.Common.TextLengthCalculator
{
    public static class CalcFactory
    {
        public static List<ICalcLength> Calculators = new List<ICalcLength>
        {
            new CalcAll(),
            new CalcNoSpaceCpsOnly(),
            new CalcNoSpace(),
            new CalcCjk(),
            new CalcCjkNoSpace(),
            new CalcIncludeCompositionCharacters(),
            new CalcIncludeCompositionCharactersNotSpace(),
            new CalcNoSpaceOrPunctuation(),
            new CalcNoSpaceOrPunctuationCpsOnly(),
            new CalcIgnoreArabicDiacritics(),
            new CalcIgnoreArabicDiacriticsNoSpace(),
        };

        public static ICalcLength MakeCalculator(string strategy)
        {
            // Called per line on grid repaints (characters-per-second) - avoid the
            // LINQ closure and the fallback allocation; the list entries are shared anyway.
            var calculators = Calculators;
            for (var i = 0; i < calculators.Count; i++)
            {
                if (calculators[i].GetType().Name == strategy)
                {
                    return calculators[i];
                }
            }

            return calculators[0]; // CalcAll
        }
    }
}
