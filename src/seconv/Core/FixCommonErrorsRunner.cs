using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.Interfaces;
using Nikse.SubtitleEdit.Features.Ocr.FixEngine;
using Nikse.SubtitleEdit.Features.SpellCheck;
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
/// <c>FixCommonOcrErrors</c> runs as a final pass when a dictionary folder is configured
/// (<c>--dictionary-folder</c>), using the shared OCR-fix engine + Hunspell from libuilogic
/// (#11744).
/// </summary>
internal static class FixCommonErrorsRunner
{
    // Upper bound on convergence passes (SE4 used a fixed 3); we loop until stable but
    // never beyond this, to guard against a rule pair that oscillates forever.
    private const int MaxPasses = 10;

    // Pseudo-rule id for the OCR-fix pass (not an IFixCommonError; needs a dictionary + the engine).
    internal const string OcrFixRuleId = "FixCommonOcrErrors";

    private static readonly IReadOnlyList<(string Id, Func<IFixCommonError> Factory)> Rules = BuildRules();

    public static IReadOnlyList<string> AvailableRuleIds { get; } =
        Rules.Select(r => r.Id).Append(OcrFixRuleId).ToArray();

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

        // OCR-fix pass: deterministic rules above run first; this spell-check-driven pass runs last.
        // It needs a dictionary folder (--dictionary-folder) and the OCR-fix engine, so it is gated
        // separately from the IFixCommonError rule list. Runs as part of the full suite, or when the
        // user names it explicitly (#11744).
        if (wanted == null || wanted.Contains(OcrFixRuleId))
        {
            RunOcrFix(subtitle, language);
        }
    }

    /// <summary>
    /// Applies the OCR-fix engine (OCR replace lists + Hunspell spell-check guessing) to every line,
    /// the headless equivalent of the GUI's "Fix common OCR errors". No-op when no dictionary folder
    /// is configured or no Hunspell dictionary matches the detected language. (#11744)
    /// </summary>
    private static void RunOcrFix(Subtitle subtitle, string twoLetterLanguage)
    {
        var folder = SpellCheckConfig.DictionariesFolder();
        if (string.IsNullOrEmpty(folder) || !System.IO.Directory.Exists(folder))
        {
            return;
        }

        var threeLetter = Iso639Dash2LanguageCode.GetThreeLetterCodeFromTwoLetterCode(twoLetterLanguage);
        if (string.IsNullOrEmpty(threeLetter))
        {
            return;
        }

        var spellChecker = new SpellChecker();
        var dictionary = spellChecker.GetDictionaryLanguages(folder)
            .FirstOrDefault(d => d.GetThreeLetterCode() == threeLetter);
        if (dictionary == null)
        {
            return;
        }

        IOcrFixEngine engine = new OcrFixEngine(spellChecker);
        engine.Initialize(subtitle, threeLetter, dictionary);

        for (var i = 0; i < subtitle.Paragraphs.Count; i++)
        {
            var p = subtitle.Paragraphs[i];
            var fixedText = engine.FixOcrErrors(i, p.Text, true).GetText();
            if (fixedText != p.Text)
            {
                p.Text = fixedText;
            }
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
    // Errors pass has stopped changing anything (convergence). A 64-bit FNV-1a hash over
    // the same fields - only compared against the previous pass within this run, so it
    // avoids building (and discarding) a full-subtitle string on every pass.
    private static long Snapshot(Subtitle subtitle)
    {
        const long fnvPrime = 1099511628211L;
        var hash = unchecked((long)14695981039346656037UL); // FNV offset basis
        unchecked
        {
            foreach (var p in subtitle.Paragraphs)
            {
                hash = (hash ^ BitConverter.DoubleToInt64Bits(p.StartTime.TotalMilliseconds)) * fnvPrime;
                hash = (hash ^ BitConverter.DoubleToInt64Bits(p.EndTime.TotalMilliseconds)) * fnvPrime;

                var text = p.Text ?? string.Empty;
                foreach (var c in text)
                {
                    hash = (hash ^ c) * fnvPrime;
                }

                hash = (hash ^ '\n') * fnvPrime;
            }
        }

        return hash;
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
        (nameof(AddMissingQuotes), () => new AddMissingQuotes()),
        (nameof(Fix3PlusLines), () => new Fix3PlusLines()),
        (nameof(FixAloneLowercaseIToUppercaseI), () => new FixAloneLowercaseIToUppercaseI()),
        (nameof(FixCommas), () => new FixCommas()),
        (nameof(FixContinuationStyle), () => new FixContinuationStyle { FixAction = "Fix continuation style" }),
        (nameof(FixDanishLetterI), () => new FixDanishLetterI()),
        (nameof(FixDialogsOnOneLine), () => new FixDialogsOnOneLine()),
        (nameof(FixDoubleApostrophes), () => new FixDoubleApostrophes()),
        (nameof(FixDoubleDash), () => new FixDoubleDash()),
        (nameof(FixDoubleGreaterThan), () => new FixDoubleGreaterThan()),
        (nameof(FixEllipsesStart), () => new FixEllipsesStart()),
        (nameof(FixEmptyLines), () => new FixEmptyLines()),
        (nameof(FixHyphensInDialog), () => new FixHyphensInDialog()),
        (nameof(FixHyphensRemoveDashSingleLine), () => new FixHyphensRemoveDashSingleLine()),
        (nameof(FixInvalidItalicTags), () => new FixInvalidItalicTags()),
        (nameof(FixLongDisplayTimes), () => new FixLongDisplayTimes()),
        (nameof(FixLongLines), () => new FixLongLines()),
        (nameof(FixMissingOpenBracket), () => new FixMissingOpenBracket()),
        (nameof(FixMissingPeriodsAtEndOfLine), () => new FixMissingPeriodsAtEndOfLine()),
        (nameof(FixMissingSpaces), () => new FixMissingSpaces()),
        (nameof(FixMusicNotation), () => new FixMusicNotation()),
        (nameof(FixOverlappingDisplayTimes), () => new FixOverlappingDisplayTimes()),
        (nameof(FixShortDisplayTimes), () => new FixShortDisplayTimes()),
        (nameof(FixShortGaps), () => new FixShortGaps()),
        (nameof(FixShortLines), () => new FixShortLines()),
        (nameof(FixShortLinesAll), () => new FixShortLinesAll()),
        (nameof(FixShortLinesPixelWidth), () => new FixShortLinesPixelWidth(MeasurePixelWidth)),
        (nameof(FixSpanishInvertedQuestionAndExclamationMarks), () => new FixSpanishInvertedQuestionAndExclamationMarks()),
        (nameof(FixStartWithUppercaseLetterAfterColon), () => new FixStartWithUppercaseLetterAfterColon()),
        (nameof(FixStartWithUppercaseLetterAfterParagraph), () => new FixStartWithUppercaseLetterAfterParagraph()),
        (nameof(FixStartWithUppercaseLetterAfterPeriodInsideParagraph), () => new FixStartWithUppercaseLetterAfterPeriodInsideParagraph()),
        (nameof(FixTurkishAnsiToUnicode), () => new FixTurkishAnsiToUnicode()),
        (nameof(FixUnnecessaryLeadingDots), () => new FixUnnecessaryLeadingDots()),
        (nameof(FixUnneededPeriods), () => new FixUnneededPeriods()),
        (nameof(FixUnneededSpaces), () => new FixUnneededSpaces()),
        (nameof(FixUppercaseIInsideWords), () => new FixUppercaseIInsideWords()),
        (nameof(NormalizeStrings), () => new NormalizeStrings()),
        (nameof(RemoveDialogFirstLineInNonDialogs), () => new RemoveDialogFirstLineInNonDialogs()),
        (nameof(RemoveSpaceBetweenNumbers), () => new RemoveSpaceBetweenNumbers()),
    ];

    /// <summary>
    /// Pixel-width measurer for FixShortLinesPixelWidth. Mirrors UI's implementation:
    /// 14pt of the default Skia typeface. Headless contexts get the same numbers.
    /// </summary>
    // Cached per thread: MeasurePixelWidth runs once per subtitle line, so building a new
    // SKFont every call was wasteful - and the old code disposed SKTypeface.Default, which
    // is a shared instance that must not be disposed. [ThreadStatic] keeps it safe even if
    // conversions are ever run in parallel.
    [ThreadStatic] private static SKFont? _pixelWidthFont;

    private static int MeasurePixelWidth(string text)
    {
        _pixelWidthFont ??= new SKFont(SKTypeface.Default, 14);
        var width = _pixelWidthFont.MeasureText(text);
        return (int)Math.Round(width, MidpointRounding.AwayFromZero);
    }
}
