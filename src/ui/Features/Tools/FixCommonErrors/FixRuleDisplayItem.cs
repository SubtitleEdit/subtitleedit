using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public partial class FixRuleDisplayItem : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private string _example;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private int _sortOrder;

    public string FixCommonErrorFunctionName { get; set; }

    /// <summary>
    /// The kind of fix the rule performs, taken from the fix class itself so the category
    /// has one source of truth. Null when <see cref="FixCommonErrorFunctionName"/> does not
    /// name a known fix - such a rule is only listed under "All" in the type filter.
    /// </summary>
    public FixType? FixType { get; private set; }

    // Built once: GetFixCommonErrorItems() news up every fix class, and the copy-ctor runs
    // per rule per profile.
    private static readonly Lazy<Dictionary<string, FixType>> FixTypesByFunctionName = new(() =>
        GetFixCommonErrorItems().ToDictionary(p => p.GetType().Name, p => p.FixType, StringComparer.Ordinal));

    public static bool TryResolveFixType(string fixCommonErrorFunctionName, out FixType fixType)
    {
        return FixTypesByFunctionName.Value.TryGetValue(fixCommonErrorFunctionName, out fixType);
    }

    public FixRuleDisplayItem()
    {
        Name = string.Empty;
        Example = string.Empty;
        FixCommonErrorFunctionName = string.Empty;
    }

    public FixRuleDisplayItem(FixRuleDisplayItem item)
    {
        Name = item.Name;
        Example = item.Example;
        IsSelected = item.IsSelected;
        SortOrder = item.SortOrder;
        FixCommonErrorFunctionName = item.FixCommonErrorFunctionName;
        FixType = item.FixType;
    }

    public FixRuleDisplayItem(string name, string example, int sortOrder, bool isSelected, string fixCommonErrorFunctionName)
    {
        Name = name;
        Example = example;
        SortOrder = sortOrder;
        IsSelected = isSelected;
        FixCommonErrorFunctionName = fixCommonErrorFunctionName;
        FixType = TryResolveFixType(fixCommonErrorFunctionName, out var fixType) ? fixType : null;
    }

    public IFixCommonError GetFixCommonErrorFunction()
    {
        var function = GetFixCommonErrorItems().First(p => p.GetType().Name == FixCommonErrorFunctionName);
        return function;
    }

    public static List<IFixCommonError> GetFixCommonErrorItems()
    {
        var list = new List<IFixCommonError>
        {
            new AddMissingQuotes(),
            new Fix3PlusLines(),
            new FixAloneLowercaseIToUppercaseI(),
            new FixCommas(),
            new FixContinuationStyle
            {
                FixAction = string.Format(Se.Language.Tools.FixCommonErrors.FixContinuationStyleX, Se.Language.Options.Settings.GetContinuationStyleName(Configuration.Settings.General.ContinuationStyle))
            },
            new FixDanishLetterI(),
            new FixDialogsOnOneLine(),
            new FixDoubleApostrophes(),
            new FixDoubleDash(),
            new FixDoubleGreaterThan(),
            new FixEllipsesStart(),
            new FixEmptyLines(),
            new FixHyphensInDialog(),
            new FixHyphensRemoveDashSingleLine(),
            new FixInvalidItalicTags(),
            new FixLongDisplayTimes(),
            new FixLongLines(),
            new FixMissingOpenBracket(),
            new FixMissingPeriodsAtEndOfLine(),
            new FixMissingSpaces(),
            new FixMusicNotation(),
            new FixOverlappingDisplayTimes(),
            new FixShortDisplayTimes(),
            new FixShortGaps(),
            new FixShortLines(),
            new FixShortLinesAll(),
            new FixShortLinesPixelWidth(CalcPixelWidth),
            new FixSpanishInvertedQuestionAndExclamationMarks(),
            new FixStartWithUppercaseLetterAfterColon(),
            new FixStartWithUppercaseLetterAfterParagraph(),
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph(),
            new FixTurkishAnsiToUnicode(),
            new FixUnnecessaryLeadingDots(),
            new FixUnneededPeriods(),
            new FixUnneededSpaces(),
            new FixUppercaseIInsideWords(),
            new NormalizeStrings(),
            new RemoveDialogFirstLineInNonDialogs(),
            new RemoveSpaceBetweenNumbers(),
            new FixCommonOcrErrors(),
        };

        return list;
    }

    private static int CalcPixelWidth(string arg)
    {
        using var typeface = SKTypeface.Default;
        using var font = new SKFont(typeface, 14);
        var width = font.MeasureText(arg);
        return (int)Math.Round(width, MidpointRounding.AwayFromZero);
    }
}
