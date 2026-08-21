using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;

namespace LibSETests.Dictionaries;

// IsInNamesMultiWordList answers from a reverse index (name part -> the multi-word names holding
// it) instead of scanning every multi-word name for every word. Pin it against the scan it
// replaces.
public class NameListMultiWordTest : IDisposable
{
    private static readonly string[] MultiWordNames =
    {
        "Jean Luc",
        "Mary Ann",
        "Mary Ann Smith",
        "van der Berg",
        "Los Angeles",
        "New York City",
        "Jan  Hansen", // double space
        "Ann Mary",
    };

    private static readonly string[] SingleNames = { "Ben", "Soo", "Hansen", "York" };

    private readonly string _folder;

    public NameListMultiWordTest()
    {
        _folder = Path.Combine(Path.GetTempPath(), "SeNameListMultiWordTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);

        var names = string.Join(Environment.NewLine, MultiWordNames.Concat(SingleNames).Select(n => "  <name>" + n + "</name>"));
        File.WriteAllText(
            Path.Combine(_folder, "names.xml"),
            "<names>" + Environment.NewLine + names + Environment.NewLine + "  <blacklist></blacklist>" + Environment.NewLine + "</names>");
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // ignored - temp folder
        }
    }

    private NameList MakeNameList() => new NameList(_folder + Path.DirectorySeparatorChar, "en_US", false, string.Empty);

    [Fact]
    public void MatchesTheFullScanForEveryPartOfEveryMultiWordName()
    {
        var nameList = MakeNameList();
        var multiNames = nameList.GetMultiNames();
        Assert.NotEmpty(multiNames);

        foreach (var multiName in MultiWordNames)
        {
            foreach (var part in multiName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                // With the combined name in the line, and without it - the second case is the one
                // the index must still answer "no" to.
                var withName = "So then " + multiName + " walked in.";
                var withoutName = "So then " + part + " walked in.";

                Assert.Equal(Reference(multiNames, withName, part), nameList.IsInNamesMultiWordList(withName, part));
                Assert.Equal(Reference(multiNames, withoutName, part), nameList.IsInNamesMultiWordList(withoutName, part));
            }
        }
    }

    [Theory]
    [InlineData("We should probably head back before it gets dark.", "should")]
    [InlineData("We should probably head back before it gets dark.", "dark")]
    [InlineData("", "word")]
    [InlineData("Some line", "")]
    [InlineData("I saw Mary Ann\r\nSmith yesterday.", "Smith")]
    [InlineData("I saw Mary   Ann there.", "Ann")]
    [InlineData("MARY ANN was here.", "MARY ANN")]
    [InlineData("Mary Ann was here.", "Mary Ann")]
    [InlineData("Jan  Hansen was here.", "Hansen")]
    [InlineData("Los Angeles is far.", "Angeles")]
    [InlineData("Angeles is far.", "Angeles")]
    public void MatchesTheFullScanForWord(string text, string word)
    {
        var nameList = MakeNameList();
        Assert.Equal(Reference(nameList.GetMultiNames(), text, word), nameList.IsInNamesMultiWordList(text, word));
    }

    [Fact]
    public void AddedMultiWordNameIsSeenImmediately()
    {
        var nameList = MakeNameList();
        const string word = "Zzyzx";
        const string line = "I met Zzyzx Quorbal today.";

        // Warm the index first, so the add has something to invalidate.
        Assert.False(nameList.IsInNamesMultiWordList(line, word));

        nameList.Add("Zzyzx Quorbal");
        Assert.True(nameList.IsInNamesMultiWordList(line, word));
        Assert.False(nameList.IsInNamesMultiWordList("I met Zzyzx today.", word));

        nameList.Remove("Zzyzx Quorbal");
        Assert.False(nameList.IsInNamesMultiWordList(line, word));
    }

    /// <summary>The scan the index replaces.</summary>
    private static bool Reference(HashSet<string> multiNames, string input, string word)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(word))
        {
            return false;
        }

        var text = input.Replace(Environment.NewLine, " ").FixExtraSpaces();
        if (multiNames.Contains(word))
        {
            return true;
        }

        foreach (var multiWordName in multiNames)
        {
            if (multiWordName.ToUpperInvariant() == word)
            {
                return true;
            }
        }

        foreach (var multiWordName in multiNames)
        {
            if (text.FastIndexOf(multiWordName) < 0)
            {
                continue;
            }

            if (multiWordName.StartsWith(word + " ", StringComparison.Ordinal) ||
                multiWordName.EndsWith(" " + word, StringComparison.Ordinal) ||
                multiWordName.Contains(" " + word + " "))
            {
                return true;
            }
        }

        return false;
    }
}
