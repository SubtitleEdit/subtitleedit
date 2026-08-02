using System.Xml.Linq;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;

namespace LibSETests.Dictionaries;

/// <summary>
/// Guards the shipped <c>Dictionaries/&lt;lang&gt;_abbreviations.xml</c> files: a bad entry is
/// silently ignored at runtime, so it would look like the abbreviation "just doesn't work".
/// </summary>
public class ShippedAbbreviationListsTests
{
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SubtitleEdit.sln")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        return dir!.FullName;
    }

    private static string DictionariesFolder() => Path.Combine(FindRepoRoot(), "Dictionaries");

    public static TheoryData<string> AbbreviationFiles()
    {
        var data = new TheoryData<string>();
        foreach (var file in Directory.GetFiles(DictionariesFolder(), "*_abbreviations.xml").OrderBy(f => f))
        {
            data.Add(Path.GetFileName(file));
        }

        return data;
    }

    [Fact]
    public void ThereAreAbbreviationListsToCheck()
    {
        Assert.NotEmpty(Directory.GetFiles(DictionariesFolder(), "*_abbreviations.xml"));
    }

    [Theory]
    [MemberData(nameof(AbbreviationFiles))]
    public void FileIsWellFormedAndEveryEntryIsUsable(string fileName)
    {
        var path = Path.Combine(DictionariesFolder(), fileName);
        var root = XDocument.Load(path).Root;

        Assert.NotNull(root);
        Assert.Equal("Abbreviations", root!.Name.LocalName);

        var items = root.Elements("Item").Select(e => e.Value).ToList();
        Assert.NotEmpty(items);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            Assert.Equal(item.Trim(), item);
            Assert.True(item.Length > 1, $"{fileName}: '{item}' is too short");
            Assert.EndsWith(".", item);
            Assert.DoesNotContain(' ', item);

            // An inner period is already matched by a general rule, so listing it is dead weight.
            Assert.DoesNotContain('.', item.Substring(0, item.Length - 1));

            Assert.True(seen.Add(item), $"{fileName}: '{item}' is listed twice");
        }

        // The language code in the file name is what the loader looks for.
        var languageName = fileName.Substring(0, fileName.IndexOf("_abbreviations.xml", StringComparison.Ordinal));
        var loaded = AbbreviationList.Load(DictionariesFolder(), languageName);
        Assert.Equal(items.Count, loaded.Count);

        // Every entry must be reachable by the lookup, which walks back over letters and inner
        // hyphens only - an entry with any other character would never match anything.
        var callbacks = new EmptyFixCallback { Abbreviations = loaded };
        foreach (var item in items)
        {
            var text = "Xx yy " + item;
            Assert.True(
                Helper.IsAbbreviation(text, text.Length - 1, callbacks),
                $"{fileName}: '{item}' can never be matched by the abbreviation lookup");
        }
    }
}
