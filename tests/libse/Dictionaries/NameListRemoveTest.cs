using Nikse.SubtitleEdit.Core.Dictionaries;
using System.Xml.Linq;

namespace LibSETests.Dictionaries;

// #16472: removing a name always added it to the user file's blacklist, and Add refused any
// blacklisted name - so a name the user added, removed (spell-check Undo) and added again was
// never saved. Only shipped names need a blacklist entry to stay removed.
public class NameListRemoveTest : IDisposable
{
    private readonly string _folder;

    public NameListRemoveTest()
    {
        _folder = Path.Combine(Path.GetTempPath(), "SeNameListRemoveTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        File.WriteAllText(
            Path.Combine(_folder, "en_names.xml"),
            "<names><name>Shipped</name><blacklist></blacklist></names>");
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch
        {
            // Best-effort cleanup.
        }
    }

    private NameList Load() => new NameList(_folder, "en_US", false, string.Empty);

    private string[] UserFileBlacklist() => XDocument.Load(Path.Combine(_folder, "en_names_user.xml"))
        .Root!.Elements("blacklist").Elements("name").Select(e => e.Value).ToArray();

    private string[] UserFileNames() => XDocument.Load(Path.Combine(_folder, "en_names_user.xml"))
        .Root!.Elements("name").Select(e => e.Value).ToArray();

    [Fact]
    public void Remove_UserAddedName_IsNotBlacklisted()
    {
        Assert.True(Load().Add("Zorbulon"));

        Assert.True(Load().Remove("Zorbulon"));

        Assert.DoesNotContain("Zorbulon", UserFileBlacklist());
        Assert.DoesNotContain("Zorbulon", UserFileNames());
        Assert.DoesNotContain("Zorbulon", Load().GetNames());
    }

    [Fact]
    public void Add_AfterRemove_IsSaved()
    {
        Load().Add("Zorbulon");
        Load().Remove("Zorbulon");

        Assert.True(Load().Add("Zorbulon"));

        Assert.Contains("Zorbulon", Load().GetNames());
    }

    [Fact]
    public void Remove_ShippedName_IsBlacklisted()
    {
        Assert.True(Load().Remove("Shipped"));

        Assert.Equal(new[] { "Shipped" }, UserFileBlacklist());
        Assert.DoesNotContain("Shipped", Load().GetNames());
    }

    [Fact]
    public void Add_BlacklistedShippedName_ClearsBlacklistEntry()
    {
        Load().Remove("Shipped");

        Assert.True(Load().Add("Shipped"));

        Assert.Empty(UserFileBlacklist());
        Assert.Contains("Shipped", Load().GetNames());
    }
}
