using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// Runs Subtitle Edit's full FixCommonErrors rule suite against a Subtitle. Each of the
/// 39 rules in <see cref="Nikse.SubtitleEdit.Core.Forms.FixCommonErrors"/> is invoked
/// once with an <see cref="EmptyFixCallback"/> (no UI reporting). Results mutate the
/// passed <paramref name="subtitle"/> in place.
/// </summary>
internal static class FixCommonErrorsRunner
{
    public static void RunAll(Subtitle subtitle)
    {
        if (subtitle == null || subtitle.Paragraphs.Count == 0)
        {
            return;
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle) ?? "en";
        var callbacks = new EmptyFixCallback
        {
            Language = language,
        };

        foreach (var rule in BuildRules())
        {
            try
            {
                rule.Fix(subtitle, callbacks);
            }
            catch
            {
                // A rogue rule shouldn't kill the conversion. Skip and continue.
            }
        }
    }

    private static IEnumerable<IFixCommonError> BuildRules()
    {
        yield return new AddMissingQuotes();
        yield return new Fix3PlusLines();
        yield return new FixAloneLowercaseIToUppercaseI();
        yield return new FixCommas();
        yield return new FixContinuationStyle
        {
            FixAction = "Fix continuation style",
        };
        yield return new FixDanishLetterI();
        yield return new FixDialogsOnOneLine();
        yield return new FixDoubleApostrophes();
        yield return new FixDoubleDash();
        yield return new FixDoubleGreaterThan();
        yield return new FixEllipsesStart();
        yield return new FixEmptyLines();
        yield return new FixHyphensInDialog();
        yield return new FixHyphensRemoveDashSingleLine();
        yield return new FixInvalidItalicTags();
        yield return new FixLongDisplayTimes();
        yield return new FixLongLines();
        yield return new FixMissingOpenBracket();
        yield return new FixMissingPeriodsAtEndOfLine();
        yield return new FixMissingSpaces();
        yield return new FixMusicNotation();
        yield return new FixOverlappingDisplayTimes();
        yield return new FixShortDisplayTimes();
        yield return new FixShortGaps();
        yield return new FixShortLines();
        yield return new FixShortLinesAll();
        yield return new FixShortLinesPixelWidth(MeasurePixelWidth);
        yield return new FixSpanishInvertedQuestionAndExclamationMarks();
        yield return new FixStartWithUppercaseLetterAfterColon();
        yield return new FixStartWithUppercaseLetterAfterParagraph();
        yield return new FixStartWithUppercaseLetterAfterPeriodInsideParagraph();
        yield return new FixTurkishAnsiToUnicode();
        yield return new FixUnnecessaryLeadingDots();
        yield return new FixUnneededPeriods();
        yield return new FixUnneededSpaces();
        yield return new FixUppercaseIInsideWords();
        yield return new NormalizeStrings();
        yield return new RemoveDialogFirstLineInNonDialogs();
        yield return new RemoveSpaceBetweenNumbers();
        // FixCommonOcrErrors is intentionally omitted — it requires an UI-side IOcrFixEngine
        // and SpellCheck setup that seconv doesn't carry. The other 38 rules cover most cleanup.
    }

    /// <summary>
    /// Pixel-width measurer for FixShortLinesPixelWidth. Mirrors UI's implementation:
    /// 14pt of the default Skia typeface. Headless contexts get the same numbers.
    /// </summary>
    private static int MeasurePixelWidth(string text)
    {
        using var typeface = SKTypeface.Default;
        using var font = new SKFont(typeface, 14);
        var width = font.MeasureText(text);
        return (int)Math.Round(width, MidpointRounding.AwayFromZero);
    }
}
