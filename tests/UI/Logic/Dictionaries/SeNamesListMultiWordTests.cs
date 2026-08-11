using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UITests.Logic.Dictionaries;

// SeNamesList.IsInNamesMultiWordList answers from a reverse index (name part -> the multi-word
// names holding it) instead of scanning every multi-word name for every word - the same change as
// in libse's NameList. Pin it against the scan it replaces.
public class SeNamesListMultiWordTests : IDisposable
{
    private static readonly string[] MultiWordNames =
    {
        "Jean Luc",
        "Mary Ann",
        "Mary Ann Smith",
        "van der Berg",
        "Los Angeles",
        "Jan  Hansen", // double space
    };

    private readonly string _folder;

    public SeNamesListMultiWordTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "SeNamesListMultiWordTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);

        var names = string.Join(Environment.NewLine, MultiWordNames.Concat(new[] { "Ben", "Hansen" }).Select(n => "  <name>" + n + "</name>"));
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

    private SeNamesList MakeNamesList()
    {
        var list = new SeNamesList();
        list.Load(_folder + Path.DirectorySeparatorChar, "en_US");
        return list;
    }

    [Fact]
    public void MatchesTheFullScanForEveryPartOfEveryMultiWordName()
    {
        var namesList = MakeNamesList();
        var multiNames = namesList.GetMultiNames();

        foreach (var multiName in MultiWordNames)
        {
            foreach (var part in multiName.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var withName = "So then " + multiName + " walked in.";
                var withoutName = "So then " + part + " walked in.";

                Assert.Equal(Reference(multiNames, withName, part), namesList.IsInNamesMultiWordList(withName, part));
                Assert.Equal(Reference(multiNames, withoutName, part), namesList.IsInNamesMultiWordList(withoutName, part));
            }
        }
    }

    [Theory]
    [InlineData("We should probably head back before it gets dark.", "should")]
    [InlineData("", "word")]
    [InlineData("Some line", "")]
    [InlineData("I saw Mary Ann\r\nSmith yesterday.", "Smith")]
    [InlineData("I saw Mary   Ann there.", "Ann")]
    [InlineData("MARY ANN was here.", "MARY ANN")]
    [InlineData("Los Angeles is far.", "Angeles")]
    [InlineData("Angeles is far.", "Angeles")]
    public void MatchesTheFullScanForWord(string text, string word)
    {
        var namesList = MakeNamesList();
        Assert.Equal(Reference(namesList.GetMultiNames(), text, word), namesList.IsInNamesMultiWordList(text, word));
    }

    [Fact]
    public void AddedMultiWordNameIsSeenImmediately()
    {
        var namesList = MakeNamesList();
        const string word = "Zzyzx";
        const string line = "I met Zzyzx Quorbal today.";

        Assert.False(namesList.IsInNamesMultiWordList(line, word));

        namesList.Add("Zzyzx Quorbal");
        Assert.True(namesList.IsInNamesMultiWordList(line, word));
        Assert.False(namesList.IsInNamesMultiWordList("I met Zzyzx today.", word));

        namesList.Remove("Zzyzx Quorbal");
        Assert.False(namesList.IsInNamesMultiWordList(line, word));
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
