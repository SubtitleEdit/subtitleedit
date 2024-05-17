using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic
{
    public class CpsLineLength
    {
        public string Code { get; set; }
        public override string ToString()
        {
            switch (Code)
            {
                case nameof(CalcNoSpaceCpsOnly):
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcNoSpaceCpsOnly;
                case nameof(CalcNoSpace):
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcNoSpace;
                case nameof(CalcCjk):
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcCjk;
                case nameof(CalcCjkNoSpace):
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcCjkNoSpace;
                case nameof(CalcIncludeCompositionCharacters):
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcIncludeCompositionCharacters;
                case nameof(CalcIncludeCompositionCharactersNotSpace):
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcIncludeCompositionCharactersNotSpace;
                case nameof(CalcNoSpaceOrPunctuation):
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcNoSpaceOrPunctuation;
                case nameof(CalcNoSpaceOrPunctuationCpsOnly):
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcNoSpaceOrPunctuationCpsOnly;
                default:
                    return LanguageSettings.Current.Settings.CpsLineLengthStyleCalcAll;
            }
        }

        public static List<CpsLineLength> List()
        {
            return CalcFactory.Calculators.Select(p => new CpsLineLength { Code = p.GetType().Name }).ToList();
        }
    }
}
