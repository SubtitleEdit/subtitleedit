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
            new CalcIgnoreArabicDiacriticsCpsOnly(),
            new CalcIgnoreArabicDiacriticsNoSpaceCpsOnly()
        };

        public static ICalcLength MakeCalculator(string strategy)
        {
            var c = Calculators.FirstOrDefault(calculator => calculator.GetType().Name == strategy);
            return c ?? new CalcAll();
        }
    }
}
