using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic.Initializers;

namespace UITests.Logic;

/// <summary>
/// #16375: before the user overlay file, edits to a shipped *_NoBreakAfterList.xml were lost when
/// the dictionaries were unpacked on upgrade. The first upgrade with the fix keeps added items.
/// </summary>
public class DictionaryInitializerNoBreakAfterMigrationTests : IDisposable
{
    private readonly string _folder;

    public DictionaryInitializerNoBreakAfterMigrationTests()
    {
        _folder = Path.Combine(Path.GetTempPath(), "SeNoBreakMigration_" + Guid.NewGuid().ToString("N"));
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
            // temp cleanup only
        }
    }

    private string Shipped => Path.Combine(_folder, "en_NoBreakAfterList.xml");
    private string User => Path.Combine(_folder, "en_NoBreakAfterList_User.xml");

    [Fact]
    public void FirstUpgrade_KeepsItemsAddedToShippedFile_AndWritesMarker()
    {
        File.WriteAllText(Shipped, "<NoBreakAfterList><Item>Mr.</Item><Item>Prof.</Item></NoBreakAfterList>");

        var snapshot = DictionaryInitializer.SnapshotNoBreakAfterListsForMigration(_folder);
        File.WriteAllText(Shipped, "<NoBreakAfterList><Item>Mr.</Item><Item>Mrs.</Item></NoBreakAfterList>"); // the unpack
        DictionaryInitializer.MigrateNoBreakAfterListEdits(_folder, snapshot);

        Assert.Equal(new[] { "Prof." }, NoBreakAfterListFile.LoadItems(User).Select(p => p.Text));
        Assert.Equal(new[] { "Mr.", "Mrs.", "Prof." }, NoBreakAfterListFile.LoadMerged(Shipped).Select(p => p.Text));
        Assert.True(File.Exists(Path.Combine(_folder, DictionaryInitializer.NoBreakAfterMigrationMarkerFileName)));
    }

    [Fact]
    public void LaterUpgrade_DoesNotTurnUpstreamRemovalsIntoUserItems()
    {
        File.WriteAllText(Path.Combine(_folder, DictionaryInitializer.NoBreakAfterMigrationMarkerFileName), "5.5.0");
        File.WriteAllText(Shipped, "<NoBreakAfterList><Item>Mr.</Item><Item>Old.</Item></NoBreakAfterList>");

        var snapshot = DictionaryInitializer.SnapshotNoBreakAfterListsForMigration(_folder);
        File.WriteAllText(Shipped, "<NoBreakAfterList><Item>Mr.</Item></NoBreakAfterList>");
        DictionaryInitializer.MigrateNoBreakAfterListEdits(_folder, snapshot);

        Assert.Empty(snapshot);
        Assert.False(File.Exists(User));
    }

    [Fact]
    public void FreshInstall_NoFolder_SnapshotIsEmpty()
    {
        var snapshot = DictionaryInitializer.SnapshotNoBreakAfterListsForMigration(Path.Combine(_folder, "missing"));

        Assert.Empty(snapshot);
    }
}
