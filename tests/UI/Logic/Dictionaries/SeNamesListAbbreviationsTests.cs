using Nikse.SubtitleEdit.Logic.Dictionaries;

namespace UITests.Logic.Dictionaries;

public class SeNamesListAbbreviationsTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "se-nameslist-" + Guid.NewGuid().ToString("N"));

    public SeNamesListAbbreviationsTests()
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

    private void WriteNames(string fileName, params string[] names)
    {
        var xml = "<names>" + string.Concat(names.Select(n => "<name>" + n + "</name>")) + "</names>";
        File.WriteAllText(Path.Combine(_folder, fileName), xml);
    }

    private void WriteAbbreviations(string fileName, params string[] items)
    {
        var xml = "<Abbreviations>" + string.Concat(items.Select(i => "<Item>" + i + "</Item>")) + "</Abbreviations>";
        File.WriteAllText(Path.Combine(_folder, fileName), xml);
    }

    // Names ending with a period stay a source of abbreviations, but "Dr." must also match "dr.".
    [Fact]
    public void NameAbbreviationsMatchCaseInsensitively()
    {
        WriteNames("names.xml", "Dr.", "Mr.", "Robert");

        var namesList = new SeNamesList();
        namesList.Load(_folder, "nl");

        var abbreviations = namesList.GetAbbreviations();
        Assert.Contains("dr.", abbreviations);
        Assert.Contains("Dr.", abbreviations);
        Assert.DoesNotContain("Robert", abbreviations);
    }

    // Abbreviations that are not names come from <lang>_abbreviations.xml (#13082).
    [Fact]
    public void LoadsLanguageAbbreviationFile()
    {
        WriteNames("names.xml", "Dr.");
        WriteAbbreviations("nl_abbreviations.xml", "dhr.", "enz.");

        var namesList = new SeNamesList();
        namesList.Load(_folder, "nl");

        var abbreviations = namesList.GetAbbreviations();
        Assert.Contains("dhr.", abbreviations);
        Assert.Contains("Enz.", abbreviations);
        Assert.Contains("Dr.", abbreviations);
    }

    // A second Load must not keep the previous language's abbreviations.
    [Fact]
    public void ReloadReplacesPreviousLanguageAbbreviations()
    {
        WriteAbbreviations("nl_abbreviations.xml", "dhr.");
        WriteAbbreviations("es_abbreviations.xml", "sr.");

        var namesList = new SeNamesList();
        namesList.Load(_folder, "nl");
        namesList.Load(_folder, "es");

        var abbreviations = namesList.GetAbbreviations();
        Assert.Contains("sr.", abbreviations);
        Assert.DoesNotContain("dhr.", abbreviations);
    }
}
