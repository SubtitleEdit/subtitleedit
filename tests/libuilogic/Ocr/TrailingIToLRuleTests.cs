using System.Xml;
using Nikse.SubtitleEdit.UiLogic.Ocr.FixEngine;

namespace LibUiLogicTests.Ocr;

// Issue #14824: the italic i->l rule (#13660) rewrote the French "toi" in a Dutch subtitle to the
// Dutch word "tol". Any three-letter foreign word ending in i whose l-variant is a native word
// ("moi"/"mol", "roi"/"rol", "loi"/"lol") was hit the same way. The shipped rule now needs a stem
// of at least three letters, matching the 5-letter floor of OcrFixEngine.TryFixLMisreadAsI.
public class TrailingIToLRuleTests
{
    private static readonly HashSet<string> DutchWords = new(StringComparer.Ordinal)
    {
        "Mieux", "tol", "mol", "rol", "lol", "Monsieur", "zal", "wel", "spel", "spiegel", "Spiegel", "hoi",
    };

    private static List<SpellCheckRegex> LoadShippedDutchRules()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        var doc = new XmlDocument();
        doc.Load(Path.Combine(dir!.FullName, "Dictionaries", "nld_OCRFixReplaceList.xml"));
        var rules = SpellCheckRegex.LoadRegExList(doc, "RegularExpressionsIfSpelledCorrectly");
        Assert.NotEmpty(rules);
        return rules;
    }

    private static string Apply(string input)
    {
        var text = input;
        foreach (var rule in LoadShippedDutchRules())
        {
            text = rule.Apply(text, new List<string>(), w => DutchWords.Contains(w));
        }

        return text;
    }

    [Theory]
    [InlineData("Mieux que toi, Monsieur.")]
    [InlineData("C'est moi, le roi.")]
    [InlineData("La loi est la loi.")]
    public void ShortForeignWordEndingInI_IsLeftAlone(string line)
    {
        Assert.Equal(line, Apply(line));
    }

    [Theory]
    [InlineData("spiegei", "spiegel")]
    [InlineData("Spiegei", "Spiegel")]
    public void LongerMisreadWord_IsStillFixed(string input, string expected)
    {
        Assert.Equal(expected, Apply(input));
    }

    [Fact]
    public void ShippedDutchTrailingIRule_RequiresThreeLetterStem()
    {
        var rule = LoadShippedDutchRules().Single(r => r.SpellCheckWord == "$1l" && r.Find.EndsWith("i\\b", StringComparison.Ordinal) && !r.Find.Contains("ii", StringComparison.Ordinal));
        Assert.Contains("{3,})i\\b", rule.Find);
    }
}
