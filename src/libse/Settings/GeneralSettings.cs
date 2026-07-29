using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using System;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class GeneralSettings
    {

        public double DefaultFrameRate { get; set; }
        public double CurrentFrameRate { get; set; }
        public string DefaultSubtitleFormat { get; set; }
        public string FavoriteSubtitleFormats { get; set; }
        public string DefaultEncoding { get; set; }
        public bool AutoGuessAnsiEncoding { get; set; }

        public int SubtitleLineMaximumPixelWidth { get; set; }
        public int SubtitleLineMaximumLength { get; set; }
        public int MaxNumberOfLines { get; set; }
        public int MergeLinesShorterThan { get; set; }
        public int SubtitleMinimumDisplayMilliseconds { get; set; }
        public int SubtitleMaximumDisplayMilliseconds { get; set; }
        public int MinimumMillisecondsBetweenLines { get; set; }
        public double SubtitleMaximumCharactersPerSeconds { get; set; }
        public double SubtitleOptimalCharactersPerSeconds { get; set; }
        public string CpsLineLengthStrategy { get; set; }
        public double SubtitleMaximumWordsPerMinute { get; set; }
        public DialogType DialogStyle { get; set; }
        public ContinuationStyle ContinuationStyle { get; set; }
        public int ContinuationPause { get; set; }
        public string CustomContinuationStyleSuffix { get; set; }
        public bool CustomContinuationStyleSuffixApplyIfComma { get; set; }
        public bool CustomContinuationStyleSuffixAddSpace { get; set; }
        public bool CustomContinuationStyleSuffixReplaceComma { get; set; }
        public string CustomContinuationStylePrefix { get; set; }
        public bool CustomContinuationStylePrefixAddSpace { get; set; }
        public bool CustomContinuationStyleUseDifferentStyleGap { get; set; }
        public string CustomContinuationStyleGapSuffix { get; set; }
        public bool CustomContinuationStyleGapSuffixApplyIfComma { get; set; }
        public bool CustomContinuationStyleGapSuffixAddSpace { get; set; }
        public bool CustomContinuationStyleGapSuffixReplaceComma { get; set; }
        public string CustomContinuationStyleGapPrefix { get; set; }
        public bool CustomContinuationStyleGapPrefixAddSpace { get; set; }
        public bool FixContinuationStyleUncheckInsertsAllCaps { get; set; }
        public bool FixContinuationStyleUncheckInsertsItalic { get; set; }
        public bool FixContinuationStyleUncheckInsertsLowercase { get; set; }
        public bool FixContinuationStyleHideContinuationCandidatesWithoutName { get; set; }
        public bool FixContinuationStyleIgnoreLyrics { get; set; }
        public string UppercaseLetters { get; set; }

        public string FFmpegLocation { get; set; }
        public bool UseTimeFormatHHMMSSFF { get; set; }
        public bool SplitRemovesDashes { get; set; }
        public int NewEmptyDefaultMs { get; set; }
        public bool RightToLeftMode { get; set; }
        public bool CurrentVideoIsSmpte { get; set; }
        public bool UseLegacyDownloader { get; set; }
        public string DefaultLanguages { get; set; }

        public GeneralSettings()
        {
            DefaultFrameRate = 23.976;
            CurrentFrameRate = DefaultFrameRate;
            SubtitleLineMaximumPixelWidth = 576;
            DefaultSubtitleFormat = "SubRip";
            DefaultEncoding = TextEncoding.Utf8WithBom;
            AutoGuessAnsiEncoding = true;
            SubtitleLineMaximumLength = 43;
            MaxNumberOfLines = 2;
            MergeLinesShorterThan = 33;
            SubtitleMinimumDisplayMilliseconds = 1000;
            SubtitleMaximumDisplayMilliseconds = 8 * 1000;
            MinimumMillisecondsBetweenLines = 24;
            SubtitleMaximumCharactersPerSeconds = 25.0;
            SubtitleOptimalCharactersPerSeconds = 15.0;
            SubtitleMaximumWordsPerMinute = 400;
            DialogStyle = DialogType.DashBothLinesWithSpace;
            ContinuationStyle = ContinuationStyle.None;
            ContinuationPause = 300;
            CustomContinuationStyleSuffix = "";
            CustomContinuationStyleSuffixApplyIfComma = false;
            CustomContinuationStyleSuffixAddSpace = false;
            CustomContinuationStyleSuffixReplaceComma = false;
            CustomContinuationStylePrefix = "";
            CustomContinuationStylePrefixAddSpace = false;
            CustomContinuationStyleUseDifferentStyleGap = true;
            CustomContinuationStyleGapSuffix = "...";
            CustomContinuationStyleGapSuffixApplyIfComma = true;
            CustomContinuationStyleGapSuffixAddSpace = false;
            CustomContinuationStyleGapSuffixReplaceComma = true;
            CustomContinuationStyleGapPrefix = "...";
            CustomContinuationStyleGapPrefixAddSpace = false;
            FixContinuationStyleUncheckInsertsAllCaps = true;
            FixContinuationStyleUncheckInsertsItalic = true;
            FixContinuationStyleUncheckInsertsLowercase = true;
            FixContinuationStyleHideContinuationCandidatesWithoutName = true;
            FixContinuationStyleIgnoreLyrics = true;
            UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ";
            UseTimeFormatHHMMSSFF = false;
            SplitRemovesDashes = true;
            RightToLeftMode = false;
            NewEmptyDefaultMs = 2000;
            DialogStyle = DialogType.DashBothLinesWithSpace;
            ContinuationStyle = ContinuationStyle.None;
        }
    }
}