namespace SeConv.Core;

/// <summary>
/// Parses the comma-separated rule-selection spec shared by <c>--fix-common-errors-rules</c>
/// and <c>--remove-formatting-rules</c>. Supports:
/// <list type="bullet">
///   <item><c>all</c> — every rule (also the default when spec is null/empty/whitespace).</item>
///   <item><c>RuleA,RuleB</c> — explicit allow-list.</item>
///   <item><c>all,-RuleA</c> — start from all, then subtract.</item>
///   <item><c>-RuleA</c> (negations only) — implied <c>all</c>, then subtract.</item>
/// </list>
/// Matching is case-insensitive. Throws <see cref="ArgumentException"/> for unknown IDs.
/// Returned IDs are in the canonical order of <paramref name="availableIds"/>.
/// </summary>
internal static class RuleIdSpec
{
    /// <param name="spec">The raw option value, e.g. <c>"all,-RemoveItalic"</c>.</param>
    /// <param name="availableIds">Canonical rule IDs; defines the result order.</param>
    /// <param name="ruleKind">Rule family name for error messages, e.g. <c>"FixCommonErrors"</c>.</param>
    /// <param name="listCommand">Subcommand that lists the IDs, e.g. <c>"list-fce-rules"</c>.</param>
    public static IReadOnlyList<string> Resolve(
        string? spec,
        IReadOnlyList<string> availableIds,
        string ruleKind,
        string listCommand)
    {
        if (string.IsNullOrWhiteSpace(spec))
        {
            return availableIds;
        }

        var available = new HashSet<string>(availableIds, StringComparer.OrdinalIgnoreCase);
        var tokens = spec.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var hasPositive = tokens.Any(t => !t.StartsWith('-'));
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Negation-only specs ("-FixCommas,-FixDanishLetterI") imply a leading "all".
        if (!hasPositive)
        {
            foreach (var a in availableIds)
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
                    foreach (var a in availableIds)
                    {
                        selected.Add(a);
                    }
                }
                continue;
            }

            if (!available.Contains(id))
            {
                throw new ArgumentException(
                    $"Unknown {ruleKind} rule '{id}'. Run 'seconv {listCommand}' to see available IDs.");
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

        return availableIds.Where(selected.Contains).ToArray();
    }
}
