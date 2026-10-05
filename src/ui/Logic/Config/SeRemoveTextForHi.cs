using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeRemoveTextForHi
{
    public class InterjectionLanguage
    {
        public string? LanguageCode { get; set; }
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

        Interjections = CreateDefaultInterjections();
    }

    /// <summary>
    /// Adds the built-in list for every language the settings file has no entry for yet, so a
    /// settings file saved when only English had a default still picks up the other languages.
    /// Existing entries are left alone - they hold the user's edits.
    /// </summary>
    public void AddMissingDefaultInterjections()
    {
        Interjections ??= [];
        foreach (var language in CreateDefaultInterjections())
        {
            if (!Interjections.Any(p => p.LanguageCode == language.LanguageCode))
            {
                Interjections.Add(language);
            }
        }
    }

    // Seeded from the SE4 Dictionaries/<lang>_interjections_se.xml files.
    public static List<InterjectionLanguage> CreateDefaultInterjections()
    {
        return
        [
            new InterjectionLanguage
            {
                LanguageCode = "da",
                Interjections =
                [
                    "Æh", "Æhh", "Æhhh", "Ah", "Ahem", "Ahh", "Ahhh", "Ahhhh", "Eh", "Ehh", "Ehhh", "Erm",
                    "Gah", "Hm", "Hmm", "Hmmm", "Huh", "Mm", "Mmm", "Mmmm", "Oh", "Øh", "Ohh", "Øhh", "Ohhh",
                    "Øhhh", "Ow", "Oww", "Owww", "Ugh", "Ughh", "Uh", "Uhh", "Uhhh", "Whew",
                ],
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "de",
                Interjections =
                [
                    "Ach", "Ah", "Ahh", "Ahhh", "Aha", "Äh", "Ähh", "Ähhh", "Ähm", "Ähmm", "Au", "Aua",
                    "Autsch", "Bäh", "Boah", "Hä", "Hm", "Hmm", "Hmmm", "Hoppla", "Huch", "Hui", "Igitt",
                    "Mhm", "Mmh", "Nanu", "Oh", "Ohh", "Ohhh", "Oha", "Oje", "Pff", "Pfft", "Puh", "Tja",
                    "Uff", "Ups", "Wow",
                ],
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "en",
                Interjections =
                [
                    "Ah", "Ahem", "Ahh", "Ahhh", "Ahhhh", "Eh", "Ehh", "Ehhh", "Er", "Erm", "Gah", "Gee",
                    "Hm", "Hmm", "Hmmm", "Huh", "Mm", "Mmm", "Mmmm", "Oh", "Ohh", "Ohhh", "Ouch", "Ow", "Oww",
                    "Owww", "Phew", "Ugh", "Ughh", "Uh", "Uh-huh", "Uhh", "Uhhh", "Whew", "Whoa",
                ],
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "es",
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Ahhhh", "Ajá", "Ay", "Bah", "Bam", "Buah", "Buh", "Caray", "Chist",
                    "Ea", "Eah", "Eh", "Ehh", "Ehhh", "Epa", "Ey", "Fiu", "Guao", "Guau", "Guay", "Hm", "Hmm",
                    "Hmmm", "Ja", "Mm", "Mmm", "Mmmm", "Oh", "Ohh", "Ohhh", "Pah", "Páh", "Puaj", "Puf",
                    "Pum", "Shh", "Shhh", "Tch", "Tchk", "Uf", "Ufa", "Uff", "Ugh", "Ughh", "Uh", "Uhh",
                    "Uhhh", "Ujum", "Um", "Ups", "Uy", "Zas", "Zás", "Zaz",
                ],
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "et",
                Interjections =
                [
                    "Dr", "Dr.", "Hr", "Hr.", "Pr,", "Pr.", "dr,", "dr.", "hr,", "hr.", "pr", "pr.",
                ],
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "fr",
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
            new InterjectionLanguage
            {
                LanguageCode = "it",
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Ahi", "Ahia", "Bah", "Beh", "Boh", "Eh", "Ehh", "Ehi", "Ehm",
                    "Ehmm", "Mah", "Mm", "Mmm", "Mmmm", "Oh", "Ohh", "Ohhh", "Ohi", "Ops", "Puah", "Puh",
                    "Toh", "Uff", "Uffa", "Uh", "Uhh", "Uhm", "Uhmm", "Wow",
                ],
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "nb",
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Au", "Aua", "Eh", "Ehh", "Fy", "Hm", "Hmm", "Hmmm", "Hoppsann",
                    "Huff", "Huh", "Hæ", "Mm", "Mmm", "Oh", "Ohh", "Ohhh", "Oho", "Oi", "Oisann", "Pff",
                    "Puh", "Uff", "Uffda", "Uh", "Wow", "Æsj", "Øh", "Øhh",
                ],
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "nl",
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Aha", "Au", "Auw", "Bah", "Eh", "Ehh", "Ehm", "Ehmm", "Hé", "Hm",
                    "Hmm", "Hmmm", "Hoera", "Hoppa", "Huh", "Jakkes", "Mm", "Mmm", "Oei", "Oeps", "Oh", "Ohh",
                    "Ohhh", "Oho", "Pff", "Pfft", "Poeh", "Sst", "Tja", "Uh", "Uhh", "Uhm", "Uhmm", "Wauw",
                    "Wow",
                ],
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "pl",
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
                SkipStartList = [],
            },
            new InterjectionLanguage
            {
                LanguageCode = "sv",
                Interjections =
                [
                    "Ah", "Ahh", "Ahhh", "Aha", "Aj", "Aja", "Attans", "Eh", "Ehh", "Fy", "Hm", "Hmm", "Hmmm",
                    "Hoppsan", "Huh", "Mm", "Mmm", "Oh", "Ohh", "Ohhh", "Oho", "Oj", "Ojdå", "Pff", "Puh",
                    "Uh", "Usch", "Wow", "Äsch", "Öh", "Öhh",
                ],
                SkipStartList = [],
            },
        ];
    }
}