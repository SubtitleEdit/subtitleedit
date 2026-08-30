using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Main.AssistedSplit;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features;

/// <summary>
/// Guard tests for the 2026-08-30 bug hunt: an SE 4 import that silently replaced one binding
/// with another, and a split suggester whose decimal-number guard also swallowed real sentence
/// ends.
/// </summary>
public class BugHunt21Tests
{
    [Fact]
    public void Se4Import_KeepsTheFirstBindingWhenTwoActionsShareOneCommand()
    {
        // "MainFileSave" and "MainFileSaveAll" both map onto CommandFileSaveCommand. Both used to
        // be handed to the caller, which removes any existing entry for a command before adding -
        // so the second silently replaced the first while still counting as imported.
        const string xml = "<Settings><Shortcuts>" +
                           "<MainFileSave>Control+S</MainFileSave>" +
                           "<MainFileSaveAll>Control+Shift+S</MainFileSaveAll>" +
                           "</Shortcuts></Settings>";

        var result = Se4ShortcutsImporter.ImportFromXml(xml);

        var save = result.Shortcuts.Where(s => s.ActionName == nameof(MainViewModel.CommandFileSaveCommand)).ToList();
        Assert.Single(save);
        Assert.Equal(new[] { "Control", "S" }, save[0].Keys);
        Assert.Equal(1, result.SkippedDuplicate);
    }

    [Fact]
    public void Se4Import_DistinctCommandsAreAllKept()
    {
        const string xml = "<Settings><Shortcuts>" +
                           "<MainFileSave>Control+S</MainFileSave>" +
                           "<MainFileSaveAs>Control+Shift+S</MainFileSaveAs>" +
                           "</Shortcuts></Settings>";

        var result = Se4ShortcutsImporter.ImportFromXml(xml);

        Assert.Equal(2, result.Shortcuts.Count);
        Assert.Equal(0, result.SkippedDuplicate);
    }

    private static List<AssistedSplitCandidate> Split(string text)
    {
        var line = new SubtitleLineViewModel { Text = text, StartTime = System.TimeSpan.Zero, EndTime = System.TimeSpan.FromSeconds(4) };
        return AssistedSplitCandidateGenerator.Generate(line, "en", new SplitManager());
    }

    [Fact]
    public void AssistedSplit_OffersASentenceEndFollowedByAClosingQuote()
    {
        // The decimal-number guard skipped every '.' that was not followed by whitespace, which
        // also skipped a '.' followed by the closing quote - and that made the closing-quote run
        // below it unreachable, so this line got no sentence-end suggestion at all.
        var candidates = Split("She said \"Stop.\" He left the room without another word.");

        Assert.Contains(candidates, c => c.FirstText.TrimEnd().EndsWith("\"", System.StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("The price was 1.5 million and nobody blinked at it")]
    [InlineData("Go to nikse.dk for the latest build of the program")]
    public void AssistedSplit_StillDoesNotSplitInsideNumbersOrAddresses(string text)
    {
        var candidates = Split(text);

        Assert.DoesNotContain(candidates, c => c.Title == Se.Language.General.SplitAtSentenceEnd);
    }
}
