using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

public class ContinuationSettingsSyncTests
{
    [Fact]
    public void ApplyGeneralSettingsToLibSe_CopiesContinuationRuntimeFields()
    {
        var source = new SeGeneral
        {
            SubtitleLineMaximumLength = 51,
            SubtitleMaximumCharactersPerSeconds = 19.5,
            SubtitleOptimalCharactersPerSeconds = 14.5,
            SubtitleMaximumWordsPerMinute = 333.5,
            MaxNumberOfLines = 3,
            UnbreakLinesShorterThan = 17,
            DialogStyle = DialogType.DashSecondLineWithSpace.ToString(),
            ContinuationStyle = ContinuationStyle.Custom.ToString(),
            CpsLineLengthStrategy = "CalcNoSpace",
            ContinuationPause = 456,
            CustomContinuationStyleSuffix = "!!",
            CustomContinuationStyleSuffixApplyIfComma = true,
            CustomContinuationStyleSuffixAddSpace = true,
            CustomContinuationStyleSuffixReplaceComma = true,
            CustomContinuationStylePrefix = "--",
            CustomContinuationStylePrefixAddSpace = true,
            CustomContinuationStyleUseDifferentStyleGap = true,
            CustomContinuationStyleGapSuffix = "??",
            CustomContinuationStyleGapSuffixApplyIfComma = true,
            CustomContinuationStyleGapSuffixAddSpace = true,
            CustomContinuationStyleGapSuffixReplaceComma = true,
            CustomContinuationStyleGapPrefix = "__",
            CustomContinuationStyleGapPrefixAddSpace = true,
            FixContinuationStyleUncheckInsertsAllCaps = false,
            FixContinuationStyleUncheckInsertsItalic = false,
            FixContinuationStyleUncheckInsertsLowercase = false,
            FixContinuationStyleHideContinuationCandidatesWithoutName = false,
            FixContinuationStyleIgnoreLyrics = false,
        };
        source.MinimumBetweenLines.Milliseconds = 123;

        var target = new GeneralSettings();

        MainViewModel.ApplyGeneralSettingsToLibSe(source, target);

        Assert.Equal(source.SubtitleLineMaximumLength, target.SubtitleLineMaximumLength);
        Assert.Equal(source.SubtitleMaximumCharactersPerSeconds, target.SubtitleMaximumCharactersPerSeconds);
        Assert.Equal(source.SubtitleOptimalCharactersPerSeconds, target.SubtitleOptimalCharactersPerSeconds);
        Assert.Equal(source.SubtitleMaximumWordsPerMinute, target.SubtitleMaximumWordsPerMinute);
        Assert.Equal(source.MinimumBetweenLines.GetMilliseconds(), target.MinimumMillisecondsBetweenLines);
        Assert.Equal(source.MaxNumberOfLines, target.MaxNumberOfLines);
        Assert.Equal(source.UnbreakLinesShorterThan, target.MergeLinesShorterThan);
        Assert.Equal(DialogType.DashSecondLineWithSpace, target.DialogStyle);
        Assert.Equal(ContinuationStyle.Custom, target.ContinuationStyle);
        Assert.Equal(source.CpsLineLengthStrategy, target.CpsLineLengthStrategy);
        Assert.Equal(source.ContinuationPause, target.ContinuationPause);
        Assert.Equal(source.CustomContinuationStyleSuffix, target.CustomContinuationStyleSuffix);
        Assert.Equal(source.CustomContinuationStyleSuffixApplyIfComma, target.CustomContinuationStyleSuffixApplyIfComma);
        Assert.Equal(source.CustomContinuationStyleSuffixAddSpace, target.CustomContinuationStyleSuffixAddSpace);
        Assert.Equal(source.CustomContinuationStyleSuffixReplaceComma, target.CustomContinuationStyleSuffixReplaceComma);
        Assert.Equal(source.CustomContinuationStylePrefix, target.CustomContinuationStylePrefix);
        Assert.Equal(source.CustomContinuationStylePrefixAddSpace, target.CustomContinuationStylePrefixAddSpace);
        Assert.Equal(source.CustomContinuationStyleUseDifferentStyleGap, target.CustomContinuationStyleUseDifferentStyleGap);
        Assert.Equal(source.CustomContinuationStyleGapSuffix, target.CustomContinuationStyleGapSuffix);
        Assert.Equal(source.CustomContinuationStyleGapSuffixApplyIfComma, target.CustomContinuationStyleGapSuffixApplyIfComma);
        Assert.Equal(source.CustomContinuationStyleGapSuffixAddSpace, target.CustomContinuationStyleGapSuffixAddSpace);
        Assert.Equal(source.CustomContinuationStyleGapSuffixReplaceComma, target.CustomContinuationStyleGapSuffixReplaceComma);
        Assert.Equal(source.CustomContinuationStyleGapPrefix, target.CustomContinuationStyleGapPrefix);
        Assert.Equal(source.CustomContinuationStyleGapPrefixAddSpace, target.CustomContinuationStyleGapPrefixAddSpace);
        Assert.Equal(source.FixContinuationStyleUncheckInsertsAllCaps, target.FixContinuationStyleUncheckInsertsAllCaps);
        Assert.Equal(source.FixContinuationStyleUncheckInsertsItalic, target.FixContinuationStyleUncheckInsertsItalic);
        Assert.Equal(source.FixContinuationStyleUncheckInsertsLowercase, target.FixContinuationStyleUncheckInsertsLowercase);
        Assert.Equal(source.FixContinuationStyleHideContinuationCandidatesWithoutName, target.FixContinuationStyleHideContinuationCandidatesWithoutName);
        Assert.Equal(source.FixContinuationStyleIgnoreLyrics, target.FixContinuationStyleIgnoreLyrics);
    }

    [Fact]
    public void ApplyGeneralSettingsToLibSe_UpdatesContinuationUtilitiesConsumers()
    {
        var source = new SeGeneral
        {
            ContinuationStyle = ContinuationStyle.Custom.ToString(),
            ContinuationPause = 345,
            CustomContinuationStyleSuffix = "!!",
            CustomContinuationStyleSuffixApplyIfComma = true,
            CustomContinuationStyleSuffixAddSpace = true,
            CustomContinuationStyleSuffixReplaceComma = false,
            CustomContinuationStylePrefix = "--",
            CustomContinuationStylePrefixAddSpace = true,
            CustomContinuationStyleUseDifferentStyleGap = true,
            CustomContinuationStyleGapSuffix = "??",
            CustomContinuationStyleGapSuffixApplyIfComma = false,
            CustomContinuationStyleGapSuffixAddSpace = true,
            CustomContinuationStyleGapSuffixReplaceComma = true,
            CustomContinuationStyleGapPrefix = "__",
            CustomContinuationStyleGapPrefixAddSpace = false,
            FixContinuationStyleIgnoreLyrics = false,
        };
        source.MinimumBetweenLines.Milliseconds = 120;

        var settings = Configuration.Settings.General;
        var savedContinuationStyle = settings.ContinuationStyle;
        var savedContinuationPause = settings.ContinuationPause;
        var savedMinimumMillisecondsBetweenLines = settings.MinimumMillisecondsBetweenLines;
        var savedCustomContinuationStyleSuffix = settings.CustomContinuationStyleSuffix;
        var savedCustomContinuationStyleSuffixApplyIfComma = settings.CustomContinuationStyleSuffixApplyIfComma;
        var savedCustomContinuationStyleSuffixAddSpace = settings.CustomContinuationStyleSuffixAddSpace;
        var savedCustomContinuationStyleSuffixReplaceComma = settings.CustomContinuationStyleSuffixReplaceComma;
        var savedCustomContinuationStylePrefix = settings.CustomContinuationStylePrefix;
        var savedCustomContinuationStylePrefixAddSpace = settings.CustomContinuationStylePrefixAddSpace;
        var savedCustomContinuationStyleUseDifferentStyleGap = settings.CustomContinuationStyleUseDifferentStyleGap;
        var savedCustomContinuationStyleGapSuffix = settings.CustomContinuationStyleGapSuffix;
        var savedCustomContinuationStyleGapSuffixApplyIfComma = settings.CustomContinuationStyleGapSuffixApplyIfComma;
        var savedCustomContinuationStyleGapSuffixAddSpace = settings.CustomContinuationStyleGapSuffixAddSpace;
        var savedCustomContinuationStyleGapSuffixReplaceComma = settings.CustomContinuationStyleGapSuffixReplaceComma;
        var savedCustomContinuationStyleGapPrefix = settings.CustomContinuationStyleGapPrefix;
        var savedCustomContinuationStyleGapPrefixAddSpace = settings.CustomContinuationStyleGapPrefixAddSpace;
        var savedFixContinuationStyleUncheckInsertsAllCaps = settings.FixContinuationStyleUncheckInsertsAllCaps;
        var savedFixContinuationStyleUncheckInsertsItalic = settings.FixContinuationStyleUncheckInsertsItalic;
        var savedFixContinuationStyleUncheckInsertsLowercase = settings.FixContinuationStyleUncheckInsertsLowercase;
        var savedFixContinuationStyleHideContinuationCandidatesWithoutName = settings.FixContinuationStyleHideContinuationCandidatesWithoutName;
        var savedFixContinuationStyleIgnoreLyrics = settings.FixContinuationStyleIgnoreLyrics;

        try
        {
            MainViewModel.ApplyGeneralSettingsToLibSe(source, settings);

            var profile = ContinuationUtilities.GetContinuationProfile(ContinuationStyle.Custom);

            Assert.Equal(source.CustomContinuationStyleSuffix, profile.Suffix);
            Assert.Equal(source.CustomContinuationStyleSuffixApplyIfComma, profile.SuffixApplyIfComma);
            Assert.Equal(source.CustomContinuationStyleSuffixAddSpace, profile.SuffixAddSpace);
            Assert.Equal(source.CustomContinuationStyleSuffixReplaceComma, profile.SuffixReplaceComma);
            Assert.Equal(source.CustomContinuationStylePrefix, profile.Prefix);
            Assert.Equal(source.CustomContinuationStylePrefixAddSpace, profile.PrefixAddSpace);
            Assert.Equal(source.CustomContinuationStyleUseDifferentStyleGap, profile.UseDifferentStyleGap);
            Assert.Equal(source.CustomContinuationStyleGapSuffix, profile.GapSuffix);
            Assert.Equal(source.CustomContinuationStyleGapSuffixApplyIfComma, profile.GapSuffixApplyIfComma);
            Assert.Equal(source.CustomContinuationStyleGapSuffixAddSpace, profile.GapSuffixAddSpace);
            Assert.Equal(source.CustomContinuationStyleGapSuffixReplaceComma, profile.GapSuffixReplaceComma);
            Assert.Equal(source.CustomContinuationStyleGapPrefix, profile.GapPrefix);
            Assert.Equal(source.CustomContinuationStyleGapPrefixAddSpace, profile.GapPrefixAddSpace);
            Assert.Equal(source.ContinuationPause, ContinuationUtilities.GetMinimumGapMs());
        }
        finally
        {
            settings.ContinuationStyle = savedContinuationStyle;
            settings.ContinuationPause = savedContinuationPause;
            settings.MinimumMillisecondsBetweenLines = savedMinimumMillisecondsBetweenLines;
            settings.CustomContinuationStyleSuffix = savedCustomContinuationStyleSuffix;
            settings.CustomContinuationStyleSuffixApplyIfComma = savedCustomContinuationStyleSuffixApplyIfComma;
            settings.CustomContinuationStyleSuffixAddSpace = savedCustomContinuationStyleSuffixAddSpace;
            settings.CustomContinuationStyleSuffixReplaceComma = savedCustomContinuationStyleSuffixReplaceComma;
            settings.CustomContinuationStylePrefix = savedCustomContinuationStylePrefix;
            settings.CustomContinuationStylePrefixAddSpace = savedCustomContinuationStylePrefixAddSpace;
            settings.CustomContinuationStyleUseDifferentStyleGap = savedCustomContinuationStyleUseDifferentStyleGap;
            settings.CustomContinuationStyleGapSuffix = savedCustomContinuationStyleGapSuffix;
            settings.CustomContinuationStyleGapSuffixApplyIfComma = savedCustomContinuationStyleGapSuffixApplyIfComma;
            settings.CustomContinuationStyleGapSuffixAddSpace = savedCustomContinuationStyleGapSuffixAddSpace;
            settings.CustomContinuationStyleGapSuffixReplaceComma = savedCustomContinuationStyleGapSuffixReplaceComma;
            settings.CustomContinuationStyleGapPrefix = savedCustomContinuationStyleGapPrefix;
            settings.CustomContinuationStyleGapPrefixAddSpace = savedCustomContinuationStyleGapPrefixAddSpace;
            settings.FixContinuationStyleUncheckInsertsAllCaps = savedFixContinuationStyleUncheckInsertsAllCaps;
            settings.FixContinuationStyleUncheckInsertsItalic = savedFixContinuationStyleUncheckInsertsItalic;
            settings.FixContinuationStyleUncheckInsertsLowercase = savedFixContinuationStyleUncheckInsertsLowercase;
            settings.FixContinuationStyleHideContinuationCandidatesWithoutName = savedFixContinuationStyleHideContinuationCandidatesWithoutName;
            settings.FixContinuationStyleIgnoreLyrics = savedFixContinuationStyleIgnoreLyrics;
        }
    }
}
