using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Settings
{
    public class FixCommonErrorsSettings
    {
        public string StartPosition { get; set; }
        public string StartSize { get; set; }
        public bool EmptyLinesTicked { get; set; }
        public bool OverlappingDisplayTimeTicked { get; set; }
        public bool TooShortDisplayTimeTicked { get; set; }
        public bool TooLongDisplayTimeTicked { get; set; }
        public bool TooShortGapTicked { get; set; }
        public bool InvalidItalicTagsTicked { get; set; }
        public bool BreakLongLinesTicked { get; set; }
        public bool MergeShortLinesTicked { get; set; }
        public bool MergeShortLinesAllTicked { get; set; }
        public bool MergeShortLinesPixelWidthTicked { get; set; }
        public bool UnneededSpacesTicked { get; set; }
        public bool UnneededPeriodsTicked { get; set; }
        public bool FixCommasTicked { get; set; }
        public bool MissingSpacesTicked { get; set; }
        public bool AddMissingQuotesTicked { get; set; }
        public bool Fix3PlusLinesTicked { get; set; }
        public bool FixHyphensTicked { get; set; }
        public bool FixHyphensRemoveSingleLineTicked { get; set; }
        public bool UppercaseIInsideLowercaseWordTicked { get; set; }
        public bool DoubleApostropheToQuoteTicked { get; set; }
        public bool AddPeriodAfterParagraphTicked { get; set; }
        public bool StartWithUppercaseLetterAfterParagraphTicked { get; set; }
        public bool StartWithUppercaseLetterAfterPeriodInsideParagraphTicked { get; set; }
        public bool StartWithUppercaseLetterAfterColonTicked { get; set; }
        public bool AloneLowercaseIToUppercaseIEnglishTicked { get; set; }
        public bool FixOcrErrorsViaReplaceListTicked { get; set; }
        public bool RemoveSpaceBetweenNumberTicked { get; set; }
        public bool FixDialogsOnOneLineTicked { get; set; }
        public bool RemoveDialogFirstLineInNonDialogs { get; set; }
        public bool TurkishAnsiTicked { get; set; }
        public bool DanishLetterITicked { get; set; }
        public bool SpanishInvertedQuestionAndExclamationMarksTicked { get; set; }
        public bool FixDoubleDashTicked { get; set; }
        public bool FixDoubleGreaterThanTicked { get; set; }
        public bool FixEllipsesStartTicked { get; set; }
        public bool FixMissingOpenBracketTicked { get; set; }
        public bool FixMusicNotationTicked { get; set; }
        public bool FixContinuationStyleTicked { get; set; }
        public bool FixUnnecessaryLeadingDotsTicked { get; set; }
        public bool NormalizeStringsTicked { get; set; }
        public string DefaultFixes { get; set; }


        public FixCommonErrorsSettings()
        {
            SetDefaultFixes();
        }

        public void SaveUserDefaultFixes()
        {
            var sb = new StringBuilder();

            if (EmptyLinesTicked)
            {
                sb.Append(nameof(EmptyLinesTicked) + ";");
            }

            if (OverlappingDisplayTimeTicked)
            {
                sb.Append(nameof(OverlappingDisplayTimeTicked) + ";");
            }

            if (TooShortDisplayTimeTicked)
            {
                sb.Append(nameof(TooShortDisplayTimeTicked) + ";");
            }

            if (TooLongDisplayTimeTicked)
            {
                sb.Append(nameof(TooLongDisplayTimeTicked) + ";");
            }

            if (TooShortGapTicked)
            {
                sb.Append(nameof(TooShortGapTicked) + ";");
            }

            if (InvalidItalicTagsTicked)
            {
                sb.Append(nameof(InvalidItalicTagsTicked) + ";");
            }

            if (BreakLongLinesTicked)
            {
                sb.Append(nameof(BreakLongLinesTicked) + ";");
            }

            if (MergeShortLinesTicked)
            {
                sb.Append(nameof(MergeShortLinesTicked) + ";");
            }

            if (MergeShortLinesAllTicked)
            {
                sb.Append(nameof(MergeShortLinesAllTicked) + ";");
            }

            if (MergeShortLinesPixelWidthTicked)
            {
                sb.Append(nameof(MergeShortLinesPixelWidthTicked) + ";");
            }

            if (UnneededSpacesTicked)
            {
                sb.Append(nameof(UnneededSpacesTicked) + ";");
            }

            if (UnneededPeriodsTicked)
            {
                sb.Append(nameof(UnneededPeriodsTicked) + ";");
            }

            if (FixCommasTicked)
            {
                sb.Append(nameof(FixCommasTicked) + ";");
            }

            if (MissingSpacesTicked)
            {
                sb.Append(nameof(MissingSpacesTicked) + ";");
            }

            if (AddMissingQuotesTicked)
            {
                sb.Append(nameof(AddMissingQuotesTicked) + ";");
            }

            if (Fix3PlusLinesTicked)
            {
                sb.Append(nameof(Fix3PlusLinesTicked) + ";");
            }

            if (FixHyphensTicked)
            {
                sb.Append(nameof(FixHyphensTicked) + ";");
            }

            if (FixHyphensRemoveSingleLineTicked)
            {
                sb.Append(nameof(FixHyphensRemoveSingleLineTicked) + ";");
            }

            if (UppercaseIInsideLowercaseWordTicked)
            {
                sb.Append(nameof(UppercaseIInsideLowercaseWordTicked) + ";");
            }

            if (DoubleApostropheToQuoteTicked)
            {
                sb.Append(nameof(DoubleApostropheToQuoteTicked) + ";");
            }

            if (AddPeriodAfterParagraphTicked)
            {
                sb.Append(nameof(AddPeriodAfterParagraphTicked) + ";");
            }

            if (StartWithUppercaseLetterAfterParagraphTicked)
            {
                sb.Append(nameof(StartWithUppercaseLetterAfterParagraphTicked) + ";");
            }

            if (StartWithUppercaseLetterAfterPeriodInsideParagraphTicked)
            {
                sb.Append(nameof(StartWithUppercaseLetterAfterPeriodInsideParagraphTicked) + ";");
            }

            if (StartWithUppercaseLetterAfterColonTicked)
            {
                sb.Append(nameof(StartWithUppercaseLetterAfterColonTicked) + ";");
            }

            if (AloneLowercaseIToUppercaseIEnglishTicked)
            {
                sb.Append(nameof(AloneLowercaseIToUppercaseIEnglishTicked) + ";");
            }

            if (FixOcrErrorsViaReplaceListTicked)
            {
                sb.Append(nameof(FixOcrErrorsViaReplaceListTicked) + ";");
            }

            if (RemoveSpaceBetweenNumberTicked)
            {
                sb.Append(nameof(RemoveSpaceBetweenNumberTicked) + ";");
            }

            if (FixDialogsOnOneLineTicked)
            {
                sb.Append(nameof(FixDialogsOnOneLineTicked) + ";");
            }

            if (RemoveDialogFirstLineInNonDialogs)
            {
                sb.Append(nameof(RemoveDialogFirstLineInNonDialogs) + ";");
            }

            if (TurkishAnsiTicked)
            {
                sb.Append(nameof(TurkishAnsiTicked) + ";");
            }

            if (DanishLetterITicked)
            {
                sb.Append(nameof(DanishLetterITicked) + ";");
            }

            if (SpanishInvertedQuestionAndExclamationMarksTicked)
            {
                sb.Append(nameof(SpanishInvertedQuestionAndExclamationMarksTicked) + ";");
            }

            if (FixDoubleDashTicked)
            {
                sb.Append(nameof(FixDoubleDashTicked) + ";");
            }

            if (FixEllipsesStartTicked)
            {
                sb.Append(nameof(FixEllipsesStartTicked) + ";");
            }

            if (FixMissingOpenBracketTicked)
            {
                sb.Append(nameof(FixMissingOpenBracketTicked) + ";");
            }

            if (FixMusicNotationTicked)
            {
                sb.Append(nameof(FixMusicNotationTicked) + ";");
            }

            if (FixContinuationStyleTicked)
            {
                sb.Append(nameof(FixContinuationStyleTicked) + ";");
            }

            if (FixUnnecessaryLeadingDotsTicked)
            {
                sb.Append(nameof(FixUnnecessaryLeadingDotsTicked) + ";");
            }

            if (NormalizeStringsTicked)
            {
                sb.Append(nameof(NormalizeStringsTicked) + ";");
            }

            DefaultFixes = sb.ToString().TrimEnd(';');
        }

        public void LoadUserDefaultFixes(string fixes)
        {
            var hashSet = fixes.Split(';').ToHashSet();
            EmptyLinesTicked = hashSet.Contains(nameof(EmptyLinesTicked));
            OverlappingDisplayTimeTicked = hashSet.Contains(nameof(OverlappingDisplayTimeTicked));
            TooShortDisplayTimeTicked = hashSet.Contains(nameof(TooShortDisplayTimeTicked));
            TooLongDisplayTimeTicked = hashSet.Contains(nameof(TooLongDisplayTimeTicked));
            TooShortGapTicked = hashSet.Contains(nameof(TooShortGapTicked));
            InvalidItalicTagsTicked = hashSet.Contains(nameof(InvalidItalicTagsTicked));
            BreakLongLinesTicked = hashSet.Contains(nameof(BreakLongLinesTicked));
            MergeShortLinesTicked = hashSet.Contains(nameof(MergeShortLinesTicked));
            MergeShortLinesAllTicked = hashSet.Contains(nameof(MergeShortLinesAllTicked));
            MergeShortLinesPixelWidthTicked = hashSet.Contains(nameof(MergeShortLinesPixelWidthTicked));
            UnneededSpacesTicked = hashSet.Contains(nameof(UnneededSpacesTicked));
            UnneededPeriodsTicked = hashSet.Contains(nameof(UnneededPeriodsTicked));
            FixCommasTicked = hashSet.Contains(nameof(FixCommasTicked));
            MissingSpacesTicked = hashSet.Contains(nameof(MissingSpacesTicked));
            AddMissingQuotesTicked = hashSet.Contains(nameof(AddMissingQuotesTicked));
            Fix3PlusLinesTicked = hashSet.Contains(nameof(Fix3PlusLinesTicked));
            FixHyphensTicked = hashSet.Contains(nameof(FixHyphensTicked));
            FixHyphensRemoveSingleLineTicked = hashSet.Contains(nameof(FixHyphensRemoveSingleLineTicked));
            UppercaseIInsideLowercaseWordTicked = hashSet.Contains(nameof(UppercaseIInsideLowercaseWordTicked));
            DoubleApostropheToQuoteTicked = hashSet.Contains(nameof(DoubleApostropheToQuoteTicked));
            AddPeriodAfterParagraphTicked = hashSet.Contains(nameof(AddPeriodAfterParagraphTicked));
            StartWithUppercaseLetterAfterParagraphTicked = hashSet.Contains(nameof(StartWithUppercaseLetterAfterParagraphTicked));
            StartWithUppercaseLetterAfterPeriodInsideParagraphTicked = hashSet.Contains(nameof(StartWithUppercaseLetterAfterPeriodInsideParagraphTicked));
            StartWithUppercaseLetterAfterColonTicked = hashSet.Contains(nameof(StartWithUppercaseLetterAfterColonTicked));
            AloneLowercaseIToUppercaseIEnglishTicked = hashSet.Contains(nameof(AloneLowercaseIToUppercaseIEnglishTicked));
            FixOcrErrorsViaReplaceListTicked = hashSet.Contains(nameof(FixOcrErrorsViaReplaceListTicked));
            RemoveSpaceBetweenNumberTicked = hashSet.Contains(nameof(RemoveSpaceBetweenNumberTicked));
            FixDialogsOnOneLineTicked = hashSet.Contains(nameof(FixDialogsOnOneLineTicked));
            RemoveDialogFirstLineInNonDialogs = hashSet.Contains(nameof(RemoveDialogFirstLineInNonDialogs));
            TurkishAnsiTicked = hashSet.Contains(nameof(TurkishAnsiTicked));
            DanishLetterITicked = hashSet.Contains(nameof(DanishLetterITicked));
            SpanishInvertedQuestionAndExclamationMarksTicked = hashSet.Contains(nameof(SpanishInvertedQuestionAndExclamationMarksTicked));
            FixDoubleDashTicked = hashSet.Contains(nameof(FixDoubleDashTicked));
            FixEllipsesStartTicked = hashSet.Contains(nameof(FixEllipsesStartTicked));
            FixMissingOpenBracketTicked = hashSet.Contains(nameof(FixMissingOpenBracketTicked));
            FixMusicNotationTicked = hashSet.Contains(nameof(FixMusicNotationTicked));
            FixContinuationStyleTicked = hashSet.Contains(nameof(FixContinuationStyleTicked));
            FixUnnecessaryLeadingDotsTicked = hashSet.Contains(nameof(FixUnnecessaryLeadingDotsTicked));
            NormalizeStringsTicked = hashSet.Contains(nameof(NormalizeStringsTicked));
        }

        public void SetDefaultFixes()
        {
            LoadUserDefaultFixes(string.Empty);
            EmptyLinesTicked = true;
            OverlappingDisplayTimeTicked = true;
            TooShortDisplayTimeTicked = true;
            TooLongDisplayTimeTicked = true;
            TooShortGapTicked = false;
            InvalidItalicTagsTicked = true;
            BreakLongLinesTicked = true;
            MergeShortLinesTicked = true;
            MergeShortLinesPixelWidthTicked = false;
            UnneededPeriodsTicked = true;
            FixCommasTicked = true;
            UnneededSpacesTicked = true;
            MissingSpacesTicked = true;
            UppercaseIInsideLowercaseWordTicked = true;
            DoubleApostropheToQuoteTicked = true;
            AddPeriodAfterParagraphTicked = false;
            StartWithUppercaseLetterAfterParagraphTicked = true;
            StartWithUppercaseLetterAfterPeriodInsideParagraphTicked = false;
            StartWithUppercaseLetterAfterColonTicked = false;
            AloneLowercaseIToUppercaseIEnglishTicked = false;
            TurkishAnsiTicked = false;
            DanishLetterITicked = false;
            FixDoubleDashTicked = true;
            FixDoubleGreaterThanTicked = true;
            FixEllipsesStartTicked = true;
            FixMissingOpenBracketTicked = true;
            FixMusicNotationTicked = true;
            FixContinuationStyleTicked = false;
            FixUnnecessaryLeadingDotsTicked = true;
            NormalizeStringsTicked = false;
            SaveUserDefaultFixes();
        }
    }
}