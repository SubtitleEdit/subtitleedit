using Nikse.SubtitleEdit.Core.Dictionaries;
using Entry = Nikse.SubtitleEdit.Core.Dictionaries.NoBreakAfterListFile.Entry;

namespace LibSETests.Dictionaries;

// #16375: the do-not-break-after editor wrote into the shipped <lang>_NoBreakAfterList.xml, which
// every upgrade overwrites. User edits now live in <lang>_NoBreakAfterList_User.xml.
public class NoBreakAfterListFileTest : IDisposable
{
    private readonly string _folder;
    private readonly string _shippedFileName;
    private readonly string _userFileName;

    public NoBreakAfterListFileTest()
    {
        _folder = Path.Combine(Path.GetTempPath(), "SeNoBreakAfterListFileTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
        _shippedFileName = Path.Combine(_folder, "en_NoBreakAfterList.xml");
        _userFileName = Path.Combine(_folder, "en_NoBreakAfterList_User.xml");
        File.WriteAllText(_shippedFileName,
            "<NoBreakAfterList><Item>Mr.</Item><Item>Dr.</Item><Item RegEx=\"True\">^\\d+$</Item></NoBreakAfterList>");
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

    [Fact]
    public void GetUserFileName_AppendsUserSuffix()
    {
        Assert.Equal(_userFileName, NoBreakAfterListFile.GetUserFileName(_shippedFileName));
    }

    [Fact]
    public void LoadMerged_NoUserFile_IsShippedList()
    {
        var merged = NoBreakAfterListFile.LoadMerged(_shippedFileName);

        Assert.Equal(new[] { "Mr.", "Dr.", "^\\d+$" }, merged.Select(p => p.Text));
        Assert.True(merged[2].IsRegex);
    }

    [Fact]
    public void SaveUserChanges_WritesOnlyDifferenceAndLeavesShippedFile()
    {
        var shippedBefore = File.ReadAllText(_shippedFileName);
        var wanted = new[] { new Entry("Mr.", false), new Entry("^\\d+$", true), new Entry("Prof.", false) };

        NoBreakAfterListFile.SaveUserChanges(_shippedFileName, wanted);

        Assert.Equal(shippedBefore, File.ReadAllText(_shippedFileName));
        Assert.Equal(new[] { "Prof." }, NoBreakAfterListFile.LoadItems(_userFileName).Select(p => p.Text));
        Assert.Equal(new[] { "Mr.", "^\\d+$", "Prof." }, NoBreakAfterListFile.LoadMerged(_shippedFileName).Select(p => p.Text));
    }

    [Fact]
    public void Upgrade_ReplacingShippedFile_KeepsUserEdits()
    {
        NoBreakAfterListFile.SaveUserChanges(_shippedFileName,
            new[] { new Entry("Mr.", false), new Entry("^\\d+$", true), new Entry("Prof.", false) }); // removed "Dr."

        // A new version ships an extra item; the user's addition and removal both survive.
        File.WriteAllText(_shippedFileName,
            "<NoBreakAfterList><Item>Mr.</Item><Item>Dr.</Item><Item>Mrs.</Item><Item RegEx=\"True\">^\\d+$</Item></NoBreakAfterList>");

        var merged = NoBreakAfterListFile.LoadMerged(_shippedFileName).Select(p => p.Text).ToList();
        Assert.Equal(new[] { "Mr.", "Mrs.", "^\\d+$", "Prof." }, merged);
    }

    [Fact]
    public void SaveUserChanges_BackToShipped_DeletesUserFile()
    {
        NoBreakAfterListFile.SaveUserChanges(_shippedFileName, new[] { new Entry("Mr.", false) });
        Assert.True(File.Exists(_userFileName));

        NoBreakAfterListFile.SaveUserChanges(_shippedFileName, NoBreakAfterListFile.LoadItems(_shippedFileName));

        Assert.False(File.Exists(_userFileName));
    }

    [Fact]
    public void RegexAndTextWithSameValue_AreDifferentEntries()
    {
        NoBreakAfterListFile.SaveUserChanges(_shippedFileName,
            NoBreakAfterListFile.LoadItems(_shippedFileName).Append(new Entry("^\\d+$", false)));

        var merged = NoBreakAfterListFile.LoadMerged(_shippedFileName);
        Assert.Equal(2, merged.Count(p => p.Text == "^\\d+$"));
    }

    [Fact]
    public void PreserveAddedItems_KeepsItemsMissingFromNewShippedFile()
    {
        var previous = new List<Entry> { new("Mr.", false), new("Dr.", false), new("^\\d+$", true), new("Prof.", false), new("St\\.", true) };

        NoBreakAfterListFile.PreserveAddedItems(_shippedFileName, previous);

        var userItems = NoBreakAfterListFile.LoadItems(_userFileName);
        Assert.Equal(new[] { "Prof.", "St\\." }, userItems.Select(p => p.Text));
        Assert.True(userItems[1].IsRegex);
    }

    [Fact]
    public void PreserveAddedItems_NothingAdded_WritesNoUserFile()
    {
        NoBreakAfterListFile.PreserveAddedItems(_shippedFileName, NoBreakAfterListFile.LoadItems(_shippedFileName));

        Assert.False(File.Exists(_userFileName));
    }

    [Fact]
    public void AutoBreak_UsesUserAddedItem()
    {
        var oldDataDirectory = Nikse.SubtitleEdit.Core.Common.Configuration.DataDirectory;
        var oldUseNoLineBreakAfter = Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools.UseNoLineBreakAfter;
        var dataFolder = Path.Combine(_folder, "data");
        var dictionaries = Path.Combine(dataFolder, "Dictionaries");
        Directory.CreateDirectory(dictionaries);
        try
        {
            var shipped = Path.Combine(dictionaries, "zy_NoBreakAfterList.xml");
            File.WriteAllText(shipped, "<NoBreakAfterList><Item>AT LEAST</Item></NoBreakAfterList>");
            NoBreakAfterListFile.SaveUserChanges(shipped, new[] { new Entry("AT LEAST", false), new Entry("SORT OF", false) });
            Nikse.SubtitleEdit.Core.Common.Configuration.DataDirectory = dataFolder;
            Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools.UseNoLineBreakAfter = true;
            Nikse.SubtitleEdit.Core.Common.Utilities.ResetNoBreakAfterList();

            var result = Nikse.SubtitleEdit.Core.Common.Utilities.AutoBreakLinePrivate("HE WAS SORT OF WEIRD", 10, 0, "zy", false);

            Assert.DoesNotContain("SORT" + Environment.NewLine + "OF", result);
            Assert.Contains("SORT OF", result);
        }
        finally
        {
            Nikse.SubtitleEdit.Core.Common.Configuration.DataDirectory = oldDataDirectory;
            Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools.UseNoLineBreakAfter = oldUseNoLineBreakAfter;
            Nikse.SubtitleEdit.Core.Common.Utilities.ResetNoBreakAfterList();
        }
    }
}
