using System;
using System.Collections.Generic;

namespace SeConv.Core;

/// <summary>
/// Builds the ordered list of conversion operations from the raw command-line arguments,
/// preserving both the order the user typed them in and any repetitions. This mirrors
/// SE4's batch behaviour where, e.g., passing <c>--fix-common-errors</c> twice runs Fix
/// Common Errors twice (a second pass can fix issues the first pass introduced).
///
/// Spectre.Console.Cli binds repeated boolean flags down to a single value and discards
/// ordering, so the operation order/repetition has to be recovered from the raw args.
/// </summary>
internal static class OperationOrderParser
{
    // Canonical operation names (as understood by LibSEIntegration.ApplyOperations).
    // The CLI exposes each as a lowercase-hyphenated flag plus a PascalCase alias; both
    // normalize to the canonical name (lower-cased, dashes removed), so this list doubles
    // as the alias table.
    internal static readonly string[] ToggleOperations =
    {
        "ApplyDurationLimits",
        "BalanceLines",
        "BeautifyTimeCodes",
        "ConvertColorsToDialog",
        "FixCommonErrors",
        "FixRtlViaUnicodeChars",
        "MergeSameTexts",
        "MergeSameTimeCodes",
        "MergeShortLines",
        "RedoCasing",
        "RemoveFormatting",
        "RemoveLineBreaks",
        "RemoveTextForHI",
        "RemoveUnicodeControlChars",
        "ReverseRtlStartEnd",
        "SplitLongLines",
    };

    private static readonly char[] ValueSeparators = { ':', '=' };

    private static readonly Dictionary<string, string> ByNormalizedToken = BuildLookup();

    private static Dictionary<string, string> BuildLookup()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var op in ToggleOperations)
        {
            map[Normalize(op)] = op;
        }

        return map;
    }

    /// <summary>
    /// Normalizes an arg/flag token to a comparable key: drops the leading <c>--</c> / <c>-</c> / <c>/</c>,
    /// trims any <c>:value</c> or <c>=value</c> suffix, removes dashes, and lower-cases. So
    /// <c>--fix-common-errors</c>, <c>--FixCommonErrors</c> and <c>/FixCommonErrors:x</c> all map to
    /// <c>fixcommonerrors</c>.
    /// </summary>
    private static string Normalize(string token)
    {
        var t = token.TrimStart('/', '-');

        var cut = t.IndexOfAny(ValueSeparators);
        if (cut >= 0)
        {
            t = t.Substring(0, cut);
        }

        return t.Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// Returns the operations to apply, in the order they appear on the command line and
    /// repeated as many times as they appear.
    /// </summary>
    /// <param name="rawArgs">The original process arguments.</param>
    /// <param name="fceRequested">
    /// True when Fix Common Errors was requested (a bare flag and/or <c>--fix-common-errors-rules</c>).
    /// Guarantees at least one Fix Common Errors pass when only the rules option was supplied.
    /// </param>
    /// <param name="removeFormattingRequested">
    /// Same for Remove Formatting: true when requested via a bare flag and/or
    /// <c>--remove-formatting-rules</c>, guaranteeing at least one pass.
    /// </param>
    public static List<string> BuildOperations(
        IReadOnlyList<string> rawArgs,
        bool fceRequested,
        bool removeFormattingRequested = false)
    {
        var operations = new List<string>();
        if (rawArgs == null)
        {
            return operations;
        }

        var sawBareFce = false;
        var sawBareRemoveFormatting = false;
        int? fceRulesPosition = null;
        int? removeFormattingRulesPosition = null;
        int? fceRulesArgIndex = null;
        int? removeFormattingRulesArgIndex = null;

        for (var argIndex = 0; argIndex < rawArgs.Count; argIndex++)
        {
            var arg = rawArgs[argIndex];
            if (string.IsNullOrEmpty(arg) || (arg[0] != '-' && arg[0] != '/'))
            {
                continue; // positional (e.g. a file pattern) or a value, not a flag
            }

            var key = Normalize(arg);

            // A rules option configures its pass(es) globally; it does not itself add a
            // pass, but it does imply one when no bare flag is present.
            if (key == "fixcommonerrorsrules")
            {
                fceRulesPosition ??= operations.Count;
                fceRulesArgIndex ??= argIndex;
                continue;
            }

            if (key == "removeformattingrules")
            {
                removeFormattingRulesPosition ??= operations.Count;
                removeFormattingRulesArgIndex ??= argIndex;
                continue;
            }

            if (ByNormalizedToken.TryGetValue(key, out var operation))
            {
                if (operation == "FixCommonErrors")
                {
                    sawBareFce = true;
                }
                else if (operation == "RemoveFormatting")
                {
                    sawBareRemoveFormatting = true;
                }

                operations.Add(operation);
            }
        }

        // Back-compat: a rules option on its own still runs one pass, inserted where the
        // rules flag appeared. Insert higher positions first (ties broken by raw-arg order)
        // so an earlier insert cannot shift a later one.
        var implied = new List<(int Position, int ArgIndex, string Operation)>();
        if (fceRequested && !sawBareFce)
        {
            implied.Add((fceRulesPosition ?? operations.Count, fceRulesArgIndex ?? int.MaxValue, "FixCommonErrors"));
        }

        if (removeFormattingRequested && !sawBareRemoveFormatting)
        {
            implied.Add((removeFormattingRulesPosition ?? operations.Count, removeFormattingRulesArgIndex ?? int.MaxValue, "RemoveFormatting"));
        }

        foreach (var (position, _, operation) in implied
                     .OrderByDescending(i => i.Position)
                     .ThenByDescending(i => i.ArgIndex))
        {
            operations.Insert(position, operation);
        }

        return operations;
    }
}
