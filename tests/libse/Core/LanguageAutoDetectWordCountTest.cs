using System.Reflection;
using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Core;

/// <summary>
/// The word-list counter behind language auto-detection (WordListIndex) replaced one
/// \b(word|word|...)\b regex per list with a single tokenizing pass. These pin down the
/// regex semantics it has to reproduce, through the public GetLanguagesWithCount API.
/// </summary>
public class LanguageAutoDetectWordCountTest
{
    private static int Count(string languageCode, string text)
    {
        return LanguageAutoDetect.GetLanguagesWithCount(text).First(l => l.LanguageCode == languageCode).WordCount;
    }

    [Fact]
    public void CountsWholeWordsCaseInsensitively()
    {
        // "we", "are", "what", "your" are English entries; "wearing" and "yours" must not count.
        Assert.Equal(5, Count("en", "We ARE what we wearing? Your yours."));
    }

    [Fact]
    public void CountsEveryListInOnePass()
    {
        var text = "Jeg kan ikke finde på noget at lave i dag.\r\nWe are here because of your money.";
        Assert.Equal(5, Count("en", text));
        Assert.Equal(1, Count("da", text));
        Assert.Equal(1, Count("no", text));
        Assert.Equal(0, Count("th", text));
    }

    [Fact]
    public void PhraseEntryMatchesAcrossApostropheAndSpace()
    {
        // "You're" and "I'll" are English entries; "Das ist" and "Du bist" are German.
        Assert.Equal(2, Count("en", "You're sure? I'll go."));
        Assert.Equal(0, Count("en", "Youre sure? Ill go."));
        Assert.Equal(2, Count("de", "Das ist gut. Du bist müde."));
        // "bist" alone is also a German entry; neither phrase matches here.
        Assert.Equal(1, Count("de", "Das  ist gut. Du-bist müde."));
    }

    [Fact]
    public void AlternationEntriesExpand()
    {
        // Italian "quest[ao]", "tutt[io]"; Spanish "estoy?".
        Assert.Equal(4, Count("it", "questo questa tutti tutto queste"));
        Assert.Equal(2, Count("es", "esto estoy estoyo"));
    }

    [Fact]
    public void LinePatternCountsOncePerLine()
    {
        // Czech ".*[Řř].*" matched the whole line, so the Czech words "jsem", "ještě" and "ne"
        // on that line did not add; on a line without ř they count normally.
        Assert.Equal(1, Count("cs", "Řekl jsem, že ještě ne."));
        Assert.Equal(2, Count("cs", "Řekl jsem\r\nještě ne"));
        Assert.Equal(2, Count("cs", "Řekl jsem\r\nŘekl jsem"));
        Assert.Equal(3, Count("cs", "jsem, ne, nic."));
    }

    [Fact]
    public void PrefixEntryConsumesRestOfLine()
    {
        // Slovak "[Pp]red.*" ("som" is a Slovak entry).
        Assert.Equal(1, Count("sk", "predtým som"));
        Assert.Equal(2, Count("sk", "som predtým"));
        Assert.Equal(3, Count("sk", "som predtým\r\nsom"));
    }

    [Fact]
    public void WordBoundaryFollowsDotNetWordCharacters()
    {
        // Digits and underscore are word characters for \b, so they glue to the word.
        Assert.Equal(1, Count("en", "we1 _are your_ we"));
        // Combining marks (Mn) are word characters too: Arabic with tanwin is one word.
        Assert.Equal(2, Count("ar", "حسناً حسناً"));
        // Precomposed accented letters, any casing.
        Assert.Equal(3, Count("vi", "tôi Tôi TÔI"));
    }

    [Fact]
    public void GreekFinalSigmaFoldsToCapitalSigma()
    {
        // Known (intended) difference from the regex: an all-caps word ending in Σ now
        // matches its lowercase entry ending in final sigma ς.
        Assert.Equal(1, Count("el", "ξερεις"));
        Assert.Equal(1, Count("el", "ΞΕΡΕΙΣ"));
    }

    [Fact]
    public void EveryWordListIsRegistered()
    {
        var getCount = typeof(LanguageAutoDetect).GetMethod("GetCount", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(string), typeof(string[]) }, null);
        Assert.NotNull(getCount);
        var lists = typeof(LanguageAutoDetect)
            .GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string[]) && f.Name.StartsWith("AutoDetectWords", StringComparison.Ordinal))
            .ToList();
        Assert.True(lists.Count > 50);
        foreach (var field in lists)
        {
            var words = (string[])field.GetValue(null)!;
            // Throws if the list is not registered in the index or contains unsupported syntax.
            var count = (int)getCount!.Invoke(null, new object[] { string.Join(" ", words), words })!;
            Assert.True(count > 0, field.Name);
        }
    }
}
