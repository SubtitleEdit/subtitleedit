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
    // Upper bound on convergence passes (SE4 used a fixed 3); we loop until stable but
    // never beyond this, to guard against a rule pair that oscillates forever.
    private const int MaxPasses = 10;

    private static readonly IReadOnlyList<(string Id, Func<IFixCommonError> Factory)> Rules = BuildRules();

    public static IReadOnlyList<string> AvailableRuleIds { get; } =
        Rules.Select(r => r.Id).ToArray();

    /// <summary>
    /// Runs every available rule against the subtitle. Equivalent to
    /// <c>Run(subtitle, null)</c>.
    /// </summary>
    public static void RunAll(Subtitle subtitle) => Run(subtitle, null);

    /// <summary>
    /// Back-compat overload. Pass <c>null</c> or an empty collection to run all rules
    /// (gates language-conditional rules to their language). When a non-empty list is
    /// passed, every entry is treated as both <em>wanted</em> and <em>explicitly named</em> —
    /// language gates are bypassed for those rules. Rules execute in canonical order,
    /// not caller order, to keep behaviour stable across invocations.
    /// </summary>
    public static void Run(Subtitle subtitle, IReadOnlyCollection<string>? ruleIds)
        => Run(subtitle, ruleIds, explicitlyNamedRules: ruleIds);

    /// <summary>
    /// Runs the specified rules.
    /// <para><paramref name="ruleIds"/> selects which rules execute (<c>null</c>/empty = all).</para>
    /// <para><paramref name="explicitlyNamedRules"/> lists rules the user named by hand —
    /// these bypass language gating. <c>null</c> means "no signal, treat <paramref name="ruleIds"/>
    /// as explicit" (back-compat); an empty collection means "no rule was explicitly named"
    /// (the CLI's implicit <c>--FixCommonErrors</c> path), which keeps language gates active.</para>
    /// The split matters because the CLI pre-resolves a bare <c>--FixCommonErrors</c> to
    /// the full rule list, so a <c>wanted == null</c> check alone would never fire for the
    /// default path — see <see cref="ParseExplicitlyNamedRules"/>.
    /// </summary>
    public static void Run(
        Subtitle subtitle,
        IReadOnlyCollection<string>? ruleIds,
        IReadOnlyCollection<string>? explicitlyNamedRules)
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

        // null = treat wanted as explicit (back-compat path used by direct callers / tests).
        // empty = "user named nothing" — keeps language gates fully active.
        // non-empty = exact set of rules the user typed by hand.
        HashSet<string>? explicitlyNamed;
        if (explicitlyNamedRules == null)
        {
            explicitlyNamed = wanted;
        }
        else if (explicitlyNamedRules.Count == 0)
        {
            explicitlyNamed = null;
        }
        else
        {
            explicitlyNamed = new HashSet<string>(explicitlyNamedRules, StringComparer.OrdinalIgnoreCase);
        }

        var language = LanguageAutoDetect.AutoDetectGoogleLanguageOrNull(subtitle) ?? "en";
        var callbacks = new EmptyFixCallback
        {
            Language = language,
        };

        // Fix Common Errors is not idempotent in a single pass: one rule can create a
        // condition another rule fixes on the next pass. SE4's batch converter ran the
        // whole suite three times per /FixCommonErrors (issue #11873). Run to convergence
        // here - repeat until a pass changes nothing, capped to avoid pathological loops.
        var previousSnapshot = Snapshot(subtitle);
        for (var pass = 0; pass < MaxPasses; pass++)
        {
            RunSinglePass(subtitle, wanted, explicitlyNamed, language, callbacks);

            var snapshot = Snapshot(subtitle);
            if (snapshot == previousSnapshot)
            {
                break;
            }

            previousSnapshot = snapshot;
        }
    }

    private static void RunSinglePass(
        Subtitle subtitle,
        HashSet<string>? wanted,
        HashSet<string>? explicitlyNamed,
        string language,
        EmptyFixCallback callbacks)
    {
        foreach (var (id, factory) in Rules)
        {
            if (wanted != null && !wanted.Contains(id))
            {
                continue;
            }

            // Language-conditional rules mirror the GUI's Fix Common Errors window
            // (FixCommonErrorsViewModel.cs:359-377), which only surfaces these rules
            // when the detected language matches. Running e.g. the Spanish inverted-mark
            // fix on French content would insert ¿ / ¡ on every question — issue #11037.
            // The user can still opt in explicitly by naming the rule in --FixCommonErrorsRules.
            if (IsLanguageOnlyRule(id, language)
                && (explicitlyNamed == null || !explicitlyNamed.Contains(id)))
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

    // A signature of the subtitle's timing + text, used to detect when a Fix Common
    // Errors pass has stopped changing anything (convergence).
    private static string Snapshot(Subtitle subtitle)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var p in subtitle.Paragraphs)
        {
            sb.Append(p.StartTime.TotalMilliseconds).Append('|')
              .Append(p.EndTime.TotalMilliseconds).Append('|')
              .Append(p.Text).Append('\n');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Extracts the rule IDs a user named by hand in a <c>--FixCommonErrorsRules</c> spec.
    /// Returns an empty list for null / empty / whitespace / <c>all</c> specs (= no rule
    /// was explicitly named). Negative tokens (<c>-FixCommas</c>) and the literal <c>all</c>
    /// are excluded — only positive, named rules count as "explicit". Unknown rule names
    /// are kept as-is here; <see cref="ResolveRuleIds"/> is responsible for validation.
    /// </summary>
    public static IReadOnlyList<string> ParseExplicitlyNamedRules(string? spec)
    {
        if (string.IsNullOrWhiteSpace(spec))
        {
            return [];
        }

        return spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => !t.StartsWith('-') && !"all".Equals(t, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    /// <summary>
    /// Returns true when <paramref name="ruleId"/> is a language-conditional rule
    /// that should not run because the detected language doesn't match. Mirrors
    /// the GUI's per-language rule additions in <c>FixCommonErrorsViewModel</c>.
    /// Bypassable via explicit <c>--FixCommonErrorsRules</c>.
    /// </summary>
    private static bool IsLanguageOnlyRule(string ruleId, string language)
    {
        return ruleId switch
        {
            "FixAloneLowercaseIToUppercaseI"
                => !"en".Equals(language, StringComparison.OrdinalIgnoreCase),
            "FixDanishLetterI"
                => !"da".Equals(language, StringComparison.OrdinalIgnoreCase),
            "FixSpanishInvertedQuestionAndExclamationMarks"
                => !"es".Equals(language, StringComparison.OrdinalIgnoreCase),
            "FixTurkishAnsiToUnicode"
                => !"tr".Equals(language, StringComparison.OrdinalIgnoreCase),
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
