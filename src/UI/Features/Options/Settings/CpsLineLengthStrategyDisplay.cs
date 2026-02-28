using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class CpsLineLengthStrategyDisplay
{
    public string Name { get; set; }
    public string Code { get; set; }

    public CpsLineLengthStrategyDisplay()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public CpsLineLengthStrategyDisplay(CpsLineLengthStrategyDisplay other)
    {
        Name = other.Name;
        Code = other.Code;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<CpsLineLengthStrategyDisplay> List()
    {
        return
        [
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcAll,
                Code = nameof(CalcAll)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcCjk,
                Code = nameof(CalcCjk)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcCjkNoSpace,
                Code = nameof(CalcCjkNoSpace)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcIgnoreArabicDiacritics,
                Code = nameof(CalcIgnoreArabicDiacritics)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcIgnoreArabicDiacriticsNoSpace,
                Code = nameof(CalcIgnoreArabicDiacriticsNoSpace)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcIncludeCompositionCharacters,
                Code = nameof(CalcIncludeCompositionCharacters)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcIncludeCompositionCharactersNotSpace,
                Code = nameof(CalcIncludeCompositionCharactersNotSpace)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcNoSpace,
                Code = nameof(CalcNoSpace)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcNoSpaceCpsOnly,
                Code = nameof(CalcNoSpaceCpsOnly)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcNoSpaceOrPunctuation,
                Code = nameof(CalcNoSpaceOrPunctuation)
            },
            new()
            {
                Name = Se.Language.Options.Settings.CpsLineLengthStyleCalcNoSpaceOrPunctuationCpsOnly,
                Code = nameof(CalcNoSpaceOrPunctuationCpsOnly)
            },
        ];
    }
}

