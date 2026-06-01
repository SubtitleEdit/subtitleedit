using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;

namespace SeConv.Core;

/// <summary>
/// Runs Subtitle Edit's FixCommonErrors rule suite against a Subtitle. Each rule from
/// <see cref="Nikse.SubtitleEdit.Core.Forms.FixCommonErrors"/> is invoked once with an
/// <see cref="EmptyFixCallback"/> (no UI reporting). Results mutate the passed subtitle
/// in place.
///
/// Rule IDs are stable string keys (matching the rule class name) so users can pass
/// them via <c>--FixCommonErrorsRules</c>. Matching is case-insensitive.
/// <c>FixCommonOcrErrors</c> is intentionally excluded — it requires UI-side
/// spell-check / OCR engine setup that seconv lacks.
/// </summary>
internal static class FixCommonErrorsRunner
{
    private static readonly IReadOnlyList<(string Id, Func<IFixCommonError> Factory)> Rules = BuildRules();

    public static IReadOnlyList<string> AvailableRuleIds { get; } =
        Rules.Select(r => r.Id).ToArray();

    /// <summary>
    /// Runs every available rule against the subtitle. Equivalent to
    /// <c>Run(subtitle, null)</c>.
    /// </summary>
    public static void RunAll(Subtitle subtitle) => Run(subtitle, null);

    /// <summary>
    /// Runs the specified rules. Pass <c>null</c> or an empty collection to run all rules.
    /// Rules execute in canonical order, not caller order, to keep behaviour stable
    /// across invocations.
    /// </summary>
    public static void Run(Subtitle subtitle, IReadOnlyCollection<string>? ruleIds)
    {
        if (subtitle == null || subtitle.Paragraphs.Count == 0)
        {
            return;
        }

        HashSet<string>? wanted = null;
        if (ruleIds != null && ruleIds.Count > 0)
        {
            wanted = new HashSet<string>(ruleIds, StringComparer.OrdinalIgnoreCase);
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle) ?? "en";
        var callbacks = new EmptyFixCallback
        {
            Language = language,
        };

        foreach (var (id, factory) in Rules)
        {
            if (wanted != null && !wanted.Contains(id))
            {
                continue;
            }

            // Language-conditional rules mirror what the GUI's Fix Common Errors
            // window does: it only surfaces FixSpanishInvertedQuestionAndExclamationMarks
            // when the detected language is "es" (FixCommonErrorsViewModel.cs:377).
            // Running it on non-Spanish content would insert ¿ / ¡ on questions and
            // exclamations in any language, which surprises users (issue #11037 item 3).
            // The user can still opt in explicitly via --FixCommonErrorsRules.
            if (wanted == null && IsLanguageOnlyRule(id, language))
            {
                continue;
            }

            try
            {
                factory().Fix(subtitle, callbacks);
            }
            catch
            {
                // A rogue rule shouldn't kill the conversion. Skip and continue.
            }
        }
    }

    /// <summary>
    /// Returns true when <paramref name="ruleId"/> is a language-conditional rule
    /// that should not run in the default ("all rules") pass because the detected
    /// language doesn't match. Skipped here, included by name in an explicit
    /// <c>--FixCommonErrorsRules</c> list.
    /// </summary>
    private static bool IsLanguageOnlyRule(string ruleId, string language)
    {
        return ruleId switch
        {
            "FixSpanishInvertedQuestionAndExclamationMarks"
                => !"es".Equals(language, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    /// <summary>
    /// Resolves a comma-separated rule spec into a concrete set of rule IDs. Supports:
    /// <list type="bullet">
    ///   <item><c>all</c> — every rule (also the default when spec is null/empty/whitespace).</item>
    ///   <item><c>FixCommas,FixEllipsesStart</c> — explicit allow-list.</item>
    ///   <item><c>all,-FixDanishLetterI</c> — start from all, then subtract.</item>
    ///   <item><c>-FixCommas</c> (negations only) — implied <c>all</c>, then subtract.</item>
    /// </list>
    /// Matching is case-insensitive. Throws <see cref="ArgumentException"/> for unknown IDs.
    /// Returned IDs are in canonical order.
    /// </summary>
    public static IReadOnlyList<string> ResolveRuleIds(string? spec)
    {
        if (string.IsNullOrWhiteSpace(spec))
        {
            return AvailableRuleIds;
        }

        var available = new HashSet<string>(AvailableRuleIds, StringComparer.OrdinalIgnoreCase);
        var tokens = spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var hasPositive = tokens.Any(t => !t.StartsWith('-'));
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Negation-only specs ("-FixCommas,-FixDanishLetterI") imply a leading "all".
        if (!hasPositive)
        {
            foreach (var a in AvailableRuleIds)
            {
                selected.Add(a);
            }
        }

        foreach (var raw in tokens)
        {
            var negate = raw.StartsWith('-');
            var id = negate ? raw[1..].Trim() : raw;

            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            if (id.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                if (negate)
                {
                    selected.Clear();
                }
                else
                {
                    foreach (var a in AvailableRuleIds)
                    {
                        selected.Add(a);
                    }
                }
                continue;
            }

            if (!available.Contains(id))
            {
                throw new ArgumentException(
                    $"Unknown FixCommonErrors rule '{id}'. Run 'seconv list-fce-rules' to see available IDs.");
            }

            if (negate)
            {
                selected.Remove(id);
            }
            else
            {
                selected.Add(id);
            }
        }

        return AvailableRuleIds.Where(selected.Contains).ToArray();
    }

    /// <summary>
    /// Canonical rule list. Order here defines execution order. <c>FixCommonOcrErrors</c>
    /// is intentionally omitted — it requires an UI-side IOcrFixEngine and SpellCheck
    /// setup that seconv doesn't carry. The other 38 rules cover most cleanup.
    /// </summary>
    private static IReadOnlyList<(string Id, Func<IFixCommonError> Factory)> BuildRules() =>
    [
        ("AddMissingQuotes", () => new AddMissingQuotes()),
        ("Fix3PlusLines", () => new Fix3PlusLines()),
        ("FixAloneLowercaseIToUppercaseI", () => new FixAloneLowercaseIToUppercaseI()),
        ("FixCommas", () => new FixCommas()),
        ("FixContinuationStyle", () => new FixContinuationStyle { FixAction = "Fix continuation style" }),
        ("FixDanishLetterI", () => new FixDanishLetterI()),
        ("FixDialogsOnOneLine", () => new FixDialogsOnOneLine()),
        ("FixDoubleApostrophes", () => new FixDoubleApostrophes()),
        ("FixDoubleDash", () => new FixDoubleDash()),
        ("FixDoubleGreaterThan", () => new FixDoubleGreaterThan()),
        ("FixEllipsesStart", () => new FixEllipsesStart()),
        ("FixEmptyLines", () => new FixEmptyLines()),
        ("FixHyphensInDialog", () => new FixHyphensInDialog()),
        ("FixHyphensRemoveDashSingleLine", () => new FixHyphensRemoveDashSingleLine()),
        ("FixInvalidItalicTags", () => new FixInvalidItalicTags()),
        ("FixLongDisplayTimes", () => new FixLongDisplayTimes()),
        ("FixLongLines", () => new FixLongLines()),
        ("FixMissingOpenBracket", () => new FixMissingOpenBracket()),
        ("FixMissingPeriodsAtEndOfLine", () => new FixMissingPeriodsAtEndOfLine()),
        ("FixMissingSpaces", () => new FixMissingSpaces()),
        ("FixMusicNotation", () => new FixMusicNotation()),
        ("FixOverlappingDisplayTimes", () => new FixOverlappingDisplayTimes()),
        ("FixShortDisplayTimes", () => new FixShortDisplayTimes()),
        ("FixShortGaps", () => new FixShortGaps()),
        ("FixShortLines", () => new FixShortLines()),
        ("FixShortLinesAll", () => new FixShortLinesAll()),
        ("FixShortLinesPixelWidth", () => new FixShortLinesPixelWidth(MeasurePixelWidth)),
        ("FixSpanishInvertedQuestionAndExclamationMarks", () => new FixSpanishInvertedQuestionAndExclamationMarks()),
        ("FixStartWithUppercaseLetterAfterColon", () => new FixStartWithUppercaseLetterAfterColon()),
        ("FixStartWithUppercaseLetterAfterParagraph", () => new FixStartWithUppercaseLetterAfterParagraph()),
        ("FixStartWithUppercaseLetterAfterPeriodInsideParagraph", () => new FixStartWithUppercaseLetterAfterPeriodInsideParagraph()),
        ("FixTurkishAnsiToUnicode", () => new FixTurkishAnsiToUnicode()),
        ("FixUnnecessaryLeadingDots", () => new FixUnnecessaryLeadingDots()),
        ("FixUnneededPeriods", () => new FixUnneededPeriods()),
        ("FixUnneededSpaces", () => new FixUnneededSpaces()),
        ("FixUppercaseIInsideWords", () => new FixUppercaseIInsideWords()),
        ("NormalizeStrings", () => new NormalizeStrings()),
        ("RemoveDialogFirstLineInNonDialogs", () => new RemoveDialogFirstLineInNonDialogs()),
        ("RemoveSpaceBetweenNumbers", () => new RemoveSpaceBetweenNumbers()),
    ];

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
