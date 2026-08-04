using Nikse.SubtitleEdit.Core.Dictionaries;

namespace LibSETests.Dictionaries;

public class AbbreviationListTest : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "se-abbr-" + Guid.NewGuid().ToString("N"));

    public AbbreviationListTest()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // temp folder cleanup is best effort
        }
    }

    private void WriteList(string fileName, params string[] items)
    {
        var xml = "<Abbreviations>" + string.Concat(items.Select(i => "<Item>" + i + "</Item>")) + "</Abbreviations>";
        File.WriteAllText(Path.Combine(_folder, fileName), xml);
    }

    [Fact]
    public void LoadsTwoLetterLanguageFile()
    {
        WriteList("nl_abbreviations.xml", "dhr.", "enz.");

        var list = AbbreviationList.Load(_folder, "nl");

        Assert.Equal(2, list.Count);
        Assert.Contains("dhr.", list);
        Assert.Contains("enz.", list);
    }

    [Fact]
    public void MatchesCaseInsensitively()
    {
        WriteList("nl_abbreviations.xml", "dhr.");

        var list = AbbreviationList.Load(_folder, "nl");

        Assert.Contains("Dhr.", list);
        Assert.Contains("DHR.", list);
    }

    // A region specific language name loads both the neutral and the region file, like the names list.
    [Fact]
    public void LoadsNeutralAndRegionFile()
    {
        WriteList("pt_abbreviations.xml", "sr.");
        WriteList("pt_BR_abbreviations.xml", "vosmecê.");

        var list = AbbreviationList.Load(_folder, "pt_BR");

        Assert.Contains("sr.", list);
        Assert.Contains("vosmecê.", list);
    }

    // Entries without a trailing period would never match and are dropped.
    [Fact]
    public void IgnoresEntriesWithoutTrailingPeriod()
    {
        WriteList("nl_abbreviations.xml", "dhr.", "dhr", ".", "");

        var list = AbbreviationList.Load(_folder, "nl");

        Assert.Equal(new[] { "dhr." }, list.ToArray());
    }

    [Theory]
    [InlineData("xx")] // no file for this language
    [InlineData("")]
    [InlineData(null)]
    public void ReturnsEmptySetWhenThereIsNoList(string? languageName)
    {
        Assert.Empty(AbbreviationList.Load(_folder, languageName!));
    }

    [Fact]
    public void ReturnsEmptySetForMissingFolder()
    {
        Assert.Empty(AbbreviationList.Load(Path.Combine(_folder, "does-not-exist"), "nl"));
    }

    // A broken file must not take down "Fix common errors".
    [Fact]
    public void ReturnsEmptySetForInvalidXml()
    {
        File.WriteAllText(Path.Combine(_folder, "nl_abbreviations.xml"), "<Abbreviations><Item>dhr.");

        Assert.Empty(AbbreviationList.Load(_folder, "nl"));
    }
}
