using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeRemoveTextForHi
{
    /// <summary>
    /// The user's changes to the built-in interjections of one language. Only the differences
    /// are stored, so improvements to the built-in lists still reach users who edited theirs.
    /// </summary>
    public class InterjectionLanguage
    {
        public string? LanguageCode { get; set; }
        public List<string> Added { get; set; } = new();
        public List<string> Removed { get; set; } = new();
        public List<string> SkipStartAdded { get; set; } = new();
        public List<string> SkipStartRemoved { get; set; } = new();

        // Full lists written by earlier versions - converted to Added/Removed on load.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Interjections { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? SkipStartList { get; set; }

        internal bool IsEmpty => Added.Count == 0 && Removed.Count == 0 &&
                                 SkipStartAdded.Count == 0 && SkipStartRemoved.Count == 0;
    }

    public class InterjectionList
    {
        public List<string> Interjections { get; set; } = new();
        public List<string> SkipStartList { get; set; } = new();
    }

    public bool IsRemoveBracketsOn { get; set; }
    public bool IsRemoveCurlyBracketsOn { get; set; }
    public bool IsRemoveParenthesesOn { get; set; }
    public bool IsRemoveCustomOn { get; set; }
    public string CustomStart { get; set; }
    public string CustomEnd { get; set; }
    public bool IsOnlySeparateLine { get; set; }

    public bool IsRemoveTextBeforeColonOn { get; set; }
    public bool IsRemoveTextBeforeColonUppercaseOn { get; set; }
    public bool IsRemoveTextBeforeColonSeparateLineOn { get; set; }

    public bool IsRemoveTextUppercaseLineOn { get; set; }

    // Comma-separated all-uppercase words kept by "remove if all uppercase" (issue #11563).
    public string UppercaseWhitelist { get; set; }

    public bool IsRemoveTextContainsOn { get; set; }
    public string TextContains { get; set; }

    public bool IsRemoveOnlyMusicSymbolsOn { get; set; }

    public bool IsRemoveInterjectionsOn { get; set; }
    public bool IsInterjectionsSeparateLineOn { get; set; }

    // User changes only - use GetInterjections for the list to apply.
    public List<InterjectionLanguage> Interjections { get; set; }

    public SeRemoveTextForHi()
    {
        IsRemoveBracketsOn = true;
        IsRemoveCurlyBracketsOn = true;
        IsRemoveParenthesesOn = true;
        IsRemoveTextBeforeColonOn = true;

        CustomStart = "?";
        CustomEnd = "?";
        TextContains = string.Empty;
        UppercaseWhitelist = "YES, NO, WHY, HI, OK, TV";

        Interjections = [];
    }

    /// <summary>
    /// The built-in list with the user's changes applied, or null when there is neither a
    /// built-in list nor user changes for the language.
    /// </summary>
    public InterjectionList? GetInterjections(string? languageCode)
    {
        var code = languageCode ?? string.Empty;
        DefaultInterjections.TryGetValue(code, out var defaults);
        var user = Interjections?.FirstOrDefault(p => p.LanguageCode == code);
        if (defaults == null && user == null)
        {
            return null;
        }

        return new InterjectionList
        {
            Interjections = Apply(defaults?.Interjections, user?.Added, user?.Removed),
            SkipStartList = Apply(defaults?.SkipStartList, user?.SkipStartAdded, user?.SkipStartRemoved),
        };
    }

    /// <summary>
    /// Stores the edited lists for a language as differences from the built-in list.
    /// </summary>
    public void SetInterjections(string languageCode, IEnumerable<string> interjections, IEnumerable<string> skipStartList)
    {
        DefaultInterjections.TryGetValue(languageCode, out var defaults);
        var words = Distinct(interjections);
        var skip = Distinct(skipStartList);
        var defaultWords = defaults?.Interjections ?? [];
        var defaultSkip = defaults?.SkipStartList ?? [];

        Interjections ??= [];
        Interjections.RemoveAll(p => p.LanguageCode == languageCode);
        var language = new InterjectionLanguage
        {
            LanguageCode = languageCode,
            Added = words.Except(defaultWords, StringComparer.Ordinal).ToList(),
            Removed = defaultWords.Except(words, StringComparer.Ordinal).ToList(),
            SkipStartAdded = skip.Except(defaultSkip, StringComparer.Ordinal).ToList(),
            SkipStartRemoved = defaultSkip.Except(skip, StringComparer.Ordinal).ToList(),
        };

        if (!language.IsEmpty)
        {
            Interjections.Add(language);
        }
    }

    /// <summary>
    /// Converts full lists saved by earlier versions to differences from the built-in lists.
    /// Only English had a built-in list then, so a word counts as removed only when it was in
    /// that old English list and is missing from the saved one - words the user never saw are
    /// not treated as removed.
    /// </summary>
    public void MigrateFullInterjectionLists()
    {
        Interjections ??= [];
        foreach (var language in Interjections.ToList())
        {
            if (language.Interjections == null && language.SkipStartList == null)
            {
                continue;
            }

            var code = language.LanguageCode ?? string.Empty;
            DefaultInterjections.TryGetValue(code, out var defaults);
            var words = Distinct(language.Interjections ?? []);
            var skip = Distinct(language.SkipStartList ?? []);
            var previousDefaults = code == "en" ? PreviousEnglishDefaults : [];

            language.Added = language.Added.Union(words.Except(defaults?.Interjections ?? [], StringComparer.Ordinal), StringComparer.Ordinal).ToList();
            language.Removed = language.Removed.Union(previousDefaults
                .Where(p => !words.Contains(p, StringComparer.Ordinal) && (defaults?.Interjections.Contains(p, StringComparer.Ordinal) ?? false)), StringComparer.Ordinal).ToList();
            language.SkipStartAdded = language.SkipStartAdded.Union(skip.Except(defaults?.SkipStartList ?? [], StringComparer.Ordinal), StringComparer.Ordinal).ToList();
            language.Interjections = null;
            language.SkipStartList = null;

            if (language.IsEmpty)
            {
                Interjections.Remove(language);
            }
        }
    }

    private static List<string> Apply(List<string>? defaults, List<string>? added, List<string>? removed)
    {
        var result = (defaults ?? []).Where(p => removed == null || !removed.Contains(p, StringComparer.Ordinal)).ToList();
        foreach (var w in added ?? [])
        {
            if (!result.Contains(w, StringComparer.Ordinal))
            {
                result.Add(w);
            }
        }

        result.Sort(StringComparer.OrdinalIgnoreCase);
        return result;
    }

    private static List<string> Distinct(IEnumerable<string> items)
    {
        return items
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    // The English list built in before the per-language lists were added.
    private static readonly List<string> PreviousEnglishDefaults =
        ["Ugh", "Oh", "Ah", "Whoa", "Gee", "Ouch", "Ow", "Hmm", "Uh", "Er", "Uh-huh"];

    // Seeded from the SE4 Dictionaries/<lang>_interjections_se.xml files.
    public static readonly IReadOnlyDictionary<string, InterjectionList> DefaultInterjections =
        new Dictionary<string, InterjectionList>
        {
            ["da"] = new InterjectionList
            {
                Interjections =
                [
                    "Æh", "Æhh", "Æhhh", "Ah", "Ahem", "Ahh", "Ahhh", "Ahhhh", "Eh", "Ehh", "Ehhh", "Erm",
                    "Gah", "Hm", "Hmm", "Hmmm", "Huh", "Mm", "Mmm", "Mmmm", "Oh", "Øh", "Ohh", "Øhh", "Ohhh",
                    "Øhhh", "Ow", "Oww", "Owww", "Ugh", "Ughh", "Uh", "Uhh", "Uhhh", "Whew",
                ],
            },
            ["de"] = new InterjectionList
            {
                Interjections =
                [
                    "Ach", "Ah", "Ahh", "Ahhh", "Aha", "Äh", "Ähh", "Ähhh", "Ähm", "Ähmm", "Au", "Aua",
                    "Autsch", "Bäh", "Boah", "Hä", "Hm", "Hmm", "Hmmm", "Hoppla", "Huch", "Hui", "Igitt",
                    "Mhm", "Mmh", "Nanu", "Oh", "Ohh", "Ohhh", "Oha", "Oje", "Pff", "Pfft", "Puh", "Tja",
                    "Uff", "Ups", "Wow",
                ],
            },
            ["en"] = new InterjectionList
            {
                Interjections =
                [
                    "Ah", "Ahem", "Ahh", "Ahhh", "Ahhhh", "Eh", "Ehh", "Ehhh", "Er", "Erm", "Gah", "Gee",
                    "Hm", "Hmm", "Hmmm", "Huh", "Mm", "Mmm", "Mmmm", "Oh", "Ohh", "Ohhh", "Ouch", "Ow", "Oww",
                    "Owww", "Phew", "Ugh", "Ughh", "Uh", "Uh-huh", "Uhh", "Uhhh", "Whew", "Whoa",
                ],
            },
            ["es"] = new InterjectionList
            {
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Ahhhh", "Ajá", "Ay", "Bah", "Bam", "Buah", "Buh", "Caray", "Chist",
                    "Ea", "Eah", "Eh", "Ehh", "Ehhh", "Epa", "Ey", "Fiu", "Guao", "Guau", "Guay", "Hm", "Hmm",
                    "Hmmm", "Ja", "Mm", "Mmm", "Mmmm", "Oh", "Ohh", "Ohhh", "Pah", "Páh", "Puaj", "Puf",
                    "Pum", "Shh", "Shhh", "Tch", "Tchk", "Uf", "Ufa", "Uff", "Ugh", "Ughh", "Uh", "Uhh",
                    "Uhhh", "Ujum", "Um", "Ups", "Uy", "Zas", "Zás", "Zaz",
                ],
            },
            ["et"] = new InterjectionList
            {
                Interjections =
                [
                    "Dr", "Dr.", "Hr", "Hr.", "Pr,", "Pr.", "dr,", "dr.", "hr,", "hr.", "pr", "pr.",
                ],
            },
            ["fr"] = new InterjectionList
            {
                Interjections =
                [
                    "Ah", "Chut", "Eh", "Euh", "Han-han", "Ho", "Hum-hum", "Hum", "Hé", "Oh-oh", "Oh", "Ouf",
                    "Ouh", "Ouille", "Oups", "Whoa", "Wouah", "Wow",
                ],
                SkipStartList =
                [
                    "Ah bon",
                ],
            },
            ["it"] = new InterjectionList
            {
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Ahi", "Ahia", "Bah", "Beh", "Boh", "Eh", "Ehh", "Ehi", "Ehm",
                    "Ehmm", "Mah", "Mm", "Mmm", "Mmmm", "Oh", "Ohh", "Ohhh", "Ohi", "Ops", "Puah", "Puh",
                    "Toh", "Uff", "Uffa", "Uh", "Uhh", "Uhm", "Uhmm", "Wow",
                ],
            },
            ["nb"] = new InterjectionList
            {
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Au", "Aua", "Eh", "Ehh", "Fy", "Hm", "Hmm", "Hmmm", "Hoppsann",
                    "Huff", "Huh", "Hæ", "Mm", "Mmm", "Oh", "Ohh", "Ohhh", "Oho", "Oi", "Oisann", "Pff",
                    "Puh", "Uff", "Uffda", "Uh", "Wow", "Æsj", "Øh", "Øhh",
                ],
            },
            ["nl"] = new InterjectionList
            {
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Aha", "Au", "Auw", "Bah", "Eh", "Ehh", "Ehm", "Ehmm", "Hé", "Hm",
                    "Hmm", "Hmmm", "Hoera", "Hoppa", "Huh", "Jakkes", "Mm", "Mmm", "Oei", "Oeps", "Oh", "Ohh",
                    "Ohhh", "Oho", "Pff", "Pfft", "Poeh", "Sst", "Tja", "Uh", "Uhh", "Uhm", "Uhmm", "Wauw",
                    "Wow",
                ],
            },
            ["pl"] = new InterjectionList
            {
                Interjections =
                [
                    "Ach", "Adieu", "Aha", "Aj", "Aloha", "Apage", "Bach", "Bam", "Banzai", "Basta", "Be",
                    "Bęc", "Biada", "Blech", "Brr", "Brzęk", "Brzdęk", "Buch", "Bzz", "Cap", "Ciach", "Chlup",
                    "Cholera", "Choroba", "Ech", "Ej", "Ejże", "Fe", "Frr", "Fu", "Fuj", "Ha", "halo", "hau",
                    "Hej", "Ho", "Hola", "Hop", "Huzia", "Hyc", "Jazda", "Jejku", "Juhu", "Kap", "Klo",
                    "Kukuryku", "Kurczę", "Kurde", "Ło", "Mee", "Miau", "Mniam", "Nie", "Och", "Oho", "Oj",
                    "Ojej", "O rety", "O żesz", "Prast", "Precz", "Prr", "Psiajucha", "Psiakrew", "Pst",
                    "Sio", "Skrzyp", "Smyk", "Szur", "Tak", "Trzask", "Uff", "Uwaga", "Wara", "Wio", "Won",
                ],
            },
            ["sv"] = new InterjectionList
            {
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Aha", "Aj", "Aja", "Attans", "Eh", "Ehh", "Fy", "Hm", "Hmm", "Hmmm",
                    "Hoppsan", "Huh", "Mm", "Mmm", "Oh", "Ohh", "Ohhh", "Oho", "Oj", "Ojdå", "Pff", "Puh",
                    "Uh", "Usch", "Wow", "Äsch", "Öh", "Öhh",
                ],
            },
        };
}
