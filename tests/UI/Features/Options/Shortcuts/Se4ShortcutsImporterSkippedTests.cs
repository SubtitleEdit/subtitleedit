using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using System.Linq;
using Xunit;

namespace UITests.Features.Options.Shortcuts;

/// <summary>
/// The SE 4 import reported only how many shortcuts it had skipped ("16 skipped - no matching SE 5
/// action"), which left the user no way to find out which ones (issue #13818). The result now names
/// them, with the keys they were bound to.
/// </summary>
public class Se4ShortcutsImporterSkippedTests
{
    private const string Xml = """
        <Settings>
          <Shortcuts>
            <MainFileNew>Control+N</MainFileNew>
            <MainAdjustSetStartAndOffsetTheRest>Control+Space</MainAdjustSetStartAndOffsetTheRest>
            <MainVideoFullscreen>Alt+Enter</MainVideoFullscreen>
            <MainNeverHeardOfThis></MainNeverHeardOfThis>
          </Shortcuts>
        </Settings>
        """;

    [Fact]
    public void SkippedActions_AreNamedWithTheirKeys()
    {
        var result = Se4ShortcutsImporter.ImportFromXml(Xml);

        Assert.Equal(result.SkippedNoMapping, result.SkippedNoMappingActions.Count);
        Assert.All(result.SkippedNoMappingActions, s => Assert.False(string.IsNullOrWhiteSpace(s.Keys)));
    }

    [Fact]
    public void SkippedAction_ReadsAsAPhraseWithoutTheMainPrefix()
    {
        var skipped = new Se4ShortcutsImporter.SkippedShortcut("MainAdjustSetStartAndOffsetTheRest", "Control+Space");

        Assert.Equal("Adjust set start and offset the rest", skipped.DisplayName);
        Assert.Equal("Adjust set start and offset the rest [Control+Space]", skipped.ToString());
    }

    [Fact]
    public void MappedShortcuts_AreNotReportedAsSkipped()
    {
        var result = Se4ShortcutsImporter.ImportFromXml(Xml);

        Assert.NotEmpty(result.Shortcuts);
        Assert.DoesNotContain(result.SkippedNoMappingActions, s => s.Se4Name == "MainFileNew");
    }

    // An action with no keys assigned in SE 4 is nothing the user lost - it counts as empty, and
    // listing it as "skipped" would bury the ones that matter.
    [Fact]
    public void ActionWithoutKeys_IsNotListed()
    {
        var result = Se4ShortcutsImporter.ImportFromXml(Xml);

        Assert.DoesNotContain(result.SkippedNoMappingActions, s => s.Se4Name == "MainNeverHeardOfThis");
        Assert.True(result.SkippedEmpty >= 1);
    }
}
