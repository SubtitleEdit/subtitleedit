using Nikse.SubtitleEdit.Core.Common;

namespace SeConv.Core;

/// <summary>
/// Applies the <c>--remove-formatting</c> pass. A null rule list (bare
/// <c>--remove-formatting</c>) strips every tag wholesale via
/// <see cref="RemoveFormattingType.All"/> - same as the GUI batch convert's "Remove all
/// formatting" checkbox, and same as seconv behaved before rule selection existed. A rule
/// list from <c>--remove-formatting-rules</c> maps each ID onto its
/// <see cref="RemoveFormattingType"/> flag instead. Note that <c>all</c> in a rule spec
/// means "all named rules", which is narrower than the wholesale pass: tags no rule covers
/// (e.g. <c>{\pos(..)}</c>) survive it (#13518).
/// </summary>
internal static class RemoveFormattingRunner
{
    // Rule IDs are stable string keys users pass via --remove-formatting-rules; matching is
    // case-insensitive. Order matches the GUI batch convert "Remove formatting" checkboxes
    // and defines the canonical order ResolveRuleIds returns.
    private static readonly IReadOnlyList<(string Id, RemoveFormattingType Type)> Rules =
    [
        ("RemoveItalic", RemoveFormattingType.Italic),
        ("RemoveBold", RemoveFormattingType.Bold),
        ("RemoveUnderline", RemoveFormattingType.Underline),
        ("RemoveFontName", RemoveFormattingType.FontName),
        ("RemoveAlignment", RemoveFormattingType.Alignment),
        ("RemoveColor", RemoveFormattingType.Color),
    ];

    public static IReadOnlyList<string> AvailableRuleIds { get; } = Rules.Select(r => r.Id).ToArray();

    /// <summary>
    /// Maps each CLI rule ID to the checkbox label the GUI's batch convert "Remove formatting"
    /// function shows for the same option, so users who prototyped in the desktop app can find
    /// the matching <c>--remove-formatting-rules</c> ID. Single source of truth for the
    /// <c>list-rf-rules</c> "GUI equivalent" column. Kept in sync with the GUI strings in
    /// <c>LanguageGeneral</c> / <c>ViewRemoveFormatting</c>; a test asserts every
    /// <see cref="AvailableRuleIds"/> entry has a label here.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GuiLabels { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["RemoveItalic"] = "Remove italic",
            ["RemoveBold"] = "Remove bold",
            ["RemoveUnderline"] = "Remove underline",
            ["RemoveFontName"] = "Remove font name",
            ["RemoveAlignment"] = "Remove alignment",
            ["RemoveColor"] = "Remove color",
        };

    /// <summary>
    /// Resolves a comma-separated rule spec (e.g. <c>"all,-RemoveItalic"</c>) into concrete
    /// rule IDs in canonical order. Throws <see cref="ArgumentException"/> for unknown IDs.
    /// </summary>
    public static IReadOnlyList<string> ResolveRuleIds(string? spec) =>
        RuleIdSpec.Resolve(spec, AvailableRuleIds, "remove-formatting", "list-rf-rules");

    public static void Run(Subtitle subtitle, IReadOnlyCollection<string>? ruleIds = null)
    {
        var types = ToTypes(ruleIds);
        if (types == RemoveFormattingType.None)
        {
            return;
        }

        foreach (var p in subtitle.Paragraphs)
        {
            p.Text = RemoveFormattingUtil.Remove(p.Text, types);
        }
    }

    /// <summary>
    /// Null = bare <c>--remove-formatting</c> = wholesale <see cref="RemoveFormattingType.All"/>.
    /// An empty list (the user subtracted every rule, e.g. <c>all,-…</c> naming all of them)
    /// selects nothing and yields <see cref="RemoveFormattingType.None"/> rather than silently
    /// falling back to the wholesale pass.
    /// </summary>
    internal static RemoveFormattingType ToTypes(IReadOnlyCollection<string>? ruleIds)
    {
        if (ruleIds == null)
        {
            return RemoveFormattingType.All;
        }

        var wanted = new HashSet<string>(ruleIds, StringComparer.OrdinalIgnoreCase);
        var types = RemoveFormattingType.None;
        foreach (var (id, type) in Rules)
        {
            if (wanted.Contains(id))
            {
                types |= type;
            }
        }

        return types;
    }
}
