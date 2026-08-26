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

    /// <summary>
    /// The map is keyed by the exact element names SE 4 writes to Settings.xml, and a wrong
    /// key fails silently as "skipped" (the keep-gap frame moves were mapped under a
    /// "MainAdjust" prefix SE 4 never uses, #13818). Guards the names most easily gotten
    /// wrong by importing them from an SE 4-shaped document.
    /// </summary>
    [Theory]
    [InlineData("MoveStartOneFrameBackKeepGapPrev", "MoveStartOneFrameBackKeepGapPrevCommand")]
    [InlineData("MoveStartOneFrameForwardKeepGapPrev", "MoveStartOneFrameForwardKeepGapPrevCommand")]
    [InlineData("MoveEndOneFrameBackKeepGapNext", "MoveEndOneFrameBackKeepGapNextCommand")]
    [InlineData("MoveEndOneFrameForwardKeepGapNext", "MoveEndOneFrameForwardKeepGapNextCommand")]
    [InlineData("MainVideo3000MsLeft", "VideoMoveCustom3BackCommand")]
    [InlineData("MainVideo3000MsRight", "VideoMoveCustom3ForwardCommand")]
    [InlineData("MainVideo5000MsLeft", "VideoMoveCustom4BackCommand")]
    [InlineData("MainVideo5000MsRight", "VideoMoveCustom4ForwardCommand")]
    [InlineData("MainVideoGoToPrevChapter", "GoToPreviousChapterCommand")]
    [InlineData("MainVideoGoToNextChapter", "GoToNextChapterCommand")]
    [InlineData("MainAdjustSetStartAndOffsetTheWholeSubtitle", "WaveformSetStartAndKeepDurationCommand")]
    [InlineData("GeneralGoToNextSubtitleAndPlay", "PlayNextCommand")]
    [InlineData("GeneralGoToPrevSubtitleAndPlay", "PlayPreviousCommand")]
    [InlineData("GeneralGoToStartOfCurrentSubtitle", "VideoSetPositionCurrentSubtitleStartCommand")]
    [InlineData("GeneralGoToEndOfCurrentSubtitle", "VideoSetPositionCurrentSubtitleEndCommand")]
    [InlineData("GeneralPlayFirstSelected", "PlaySelectedLinesWithoutLoopCommand")]
    [InlineData("GeneralTogglePreviewOnVideo", "ToggleSubtitlesOnVideoPlayerCommand")]
    [InlineData("GeneralSwitchOriginalAndTranslation", "SwitchOriginalAndTranslationTextSelectedLinesCommand")]
    [InlineData("GeneralAutoCalcCurrentDuration", "RecalculateDurationSelectedLinesCommand")]
    [InlineData("MainListViewToggleCustomTags", "SurroundWith1Command")]
    [InlineData("GeneralAutoCalcCurrentDurationByOptimalReadingSpeed", "RecalculateDurationSelectedLinesCommand")]
    [InlineData("GeneralAutoCalcCurrentDurationByMinReadingSpeed", "SetDurationMaxCpsSelectedLinesCommand")]
    [InlineData("MainVideo1FrameLeftWithPlay", "VideoOneFrameBackWithPlayCommand")]
    [InlineData("MainVideo1FrameRightWithPlay", "VideoOneFrameForwardWithPlayCommand")]
    [InlineData("MainVideoToggleContrast", "VideoToggleContrastCommand")]
    [InlineData("GeneralGoToBookmark", "ListBookmarksCommand")]
    [InlineData("GeneralClearBookmarks", "ClearBookmarksCommand")]
    [InlineData("MainListViewUnderline", "ToggleLinesUnderlineOrSelectedTextCommand")]
    [InlineData("WaveformGuessStart", "WaveformGuessStartCommand")]
    public void ImportsSe4ShortcutBySerializedName(string se4Name, string expectedSe5Command)
    {
        var xml = $"<Shortcuts><{se4Name}>Control+Shift+F12</{se4Name}></Shortcuts>";

        var result = Se4ShortcutsImporter.ImportFromXml(xml);

        Assert.Equal(0, result.SkippedNoMapping);
        var shortcut = Assert.Single(result.Shortcuts);
        Assert.Equal(expectedSe5Command, shortcut.ActionName);
    }
}
