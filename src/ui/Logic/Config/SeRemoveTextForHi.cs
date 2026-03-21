using System.Collections.Generic;

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
    public bool IsOnlySingleLine { get; set; }

    public bool IsRemoveTextBeforeColonOn { get; set; }
    public bool IsRemoveTextBeforeColonUppercaseOn { get; set; }
    public bool IsRemoveTextBeforeColonSeparateLineOn { get; set; }

    public bool IsRemoveTextUppercaseLineOn { get; set; }

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

        Interjections =
        [
            new InterjectionLanguage
            {
                LanguageCode = "en",
                Interjections =
                [
                    "Ugh",
                    "Oh",
                    "Ah",
                    "Whoa",
                    "Gee",
                    "Ouch",
                    "Ow",
                    "Hmm",
                    "Uh",
                    "Er",
                    "Uh-huh"
                ],
                SkipStartList = [],
            },
        ];
    }
}