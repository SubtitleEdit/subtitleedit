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
    private static readonly string[] ToggleOperations =
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
    public static List<string> BuildOperations(IReadOnlyList<string> rawArgs, bool fceRequested)
    {
        var operations = new List<string>();
        if (rawArgs == null)
        {
            return operations;
        }

        var sawBareFce = false;
        int? rulesPosition = null;

        foreach (var arg in rawArgs)
        {
            if (string.IsNullOrEmpty(arg) || (arg[0] != '-' && arg[0] != '/'))
            {
                continue; // positional (e.g. a file pattern) or a value, not a flag
            }

            var key = Normalize(arg);

            // Fix Common Errors rules option configures the FCE pass(es) globally; it does
            // not itself add a pass, but it does imply one when no bare flag is present.
            if (key == "fixcommonerrorsrules")
            {
                rulesPosition ??= operations.Count;
                continue;
            }

            if (ByNormalizedToken.TryGetValue(key, out var operation))
            {
                if (operation == "FixCommonErrors")
                {
                    sawBareFce = true;
                }

                operations.Add(operation);
            }
        }

        // Back-compat: "--fix-common-errors-rules:..." on its own still runs one FCE pass.
        if (fceRequested && !sawBareFce)
        {
            operations.Insert(rulesPosition ?? operations.Count, "FixCommonErrors");
        }

        return operations;
    }
}
