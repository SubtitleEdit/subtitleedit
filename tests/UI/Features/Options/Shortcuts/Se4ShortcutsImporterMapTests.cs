using System.Collections.Generic;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Xunit;

namespace UITests.Features.Options.Shortcuts;

/// <summary>
/// Guards that every SE 4 shortcut the importer maps lands on a command that is actually
/// registered in the SE 5 shortcut system. An entry that only exists in the importer map is
/// worse than a skipped one: the import reports it as carried over, but the stored action name
/// never binds to a command, so the shortcut is silently dead (#12088 — the SE 4 "Column"
/// list-view shortcuts were mapped but unregistered).
/// </summary>
public class Se4ShortcutsImporterMapTests
{
    [Fact]
    public void EveryMappedSe4ShortcutTargetsARegisteredCommand()
    {
        var missing = new List<string>();
        foreach (var (se4Name, se5CommandName) in Se4ShortcutsImporter.Se4ToSe5CommandMap)
        {
            if (!ShortcutsMain.CommandTranslationLookup.ContainsKey(se5CommandName))
            {
                missing.Add($"{se4Name} -> {se5CommandName}");
            }
        }

        Assert.True(missing.Count == 0,
            "Se4ShortcutsImporter maps these SE 4 shortcuts to commands that are not registered " +
            "in ShortcutsMain (imported shortcuts would be stored but never fire):\n" +
            string.Join("\n", missing));
    }
}
