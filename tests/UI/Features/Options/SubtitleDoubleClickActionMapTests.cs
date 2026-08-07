using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config;
using Xunit;

namespace UITests.Features.Options;

/// <summary>
/// Guards the subtitle-grid double-click action list in Options. The combo box shows translated
/// texts and the setting stores enum names, so an action can only be picked when it has a text in
/// the action-to-text map. A value missing from the map is invisible in Options and any stored
/// setting for it silently falls back to "go to subtitle and pause" - which is how the SE 4
/// actions went missing in SE 5 (#13324).
/// </summary>
public class SubtitleDoubleClickActionMapTests
{
    private static readonly Dictionary<SubtitleDoubleClickActionType, Func<string>> ExpectedTexts = new()
    {
        [SubtitleDoubleClickActionType.None] = () => Se.Language.General.None,
        [SubtitleDoubleClickActionType.GoToSubtitleAndPause] = () => Se.Language.Options.Settings.GridGoToSubtitleAndPause,
        [SubtitleDoubleClickActionType.GoToSubtitleAndPlay] = () => Se.Language.Options.Settings.GridGoToSubtitleAndPlay,
        [SubtitleDoubleClickActionType.GoToSubtitleOnly] = () => Se.Language.Options.Settings.GridGoToSubtitleAndSetVideoPosition,
        [SubtitleDoubleClickActionType.GoToSubtitleAndPauseAndFocusTextBox] = () => Se.Language.Options.Settings.GridGoToSubtitleAndPauseAndFocusTextBox,
        [SubtitleDoubleClickActionType.GoToSubtitleAndPlayAndFocusTextBox] = () => Se.Language.Options.Settings.GridGoToSubtitleAndPlayAndFocusTextBox,
        [SubtitleDoubleClickActionType.GoToSubtitleAndPlayCurrentAndPause] = () => Se.Language.Options.Settings.SubtitleListActionVideoGoToPositionAndPlayCurrentAndPause,
        [SubtitleDoubleClickActionType.GoToSubtitleMinus1SecAndPause] = () => Se.Language.Options.Settings.SubtitleListActionVideoGoToPositionMinus1SecAndPause,
        [SubtitleDoubleClickActionType.GoToSubtitleMinusHalfSecAndPause] = () => Se.Language.Options.Settings.SubtitleListActionVideoGoToPositionMinusHalfSecAndPause,
        [SubtitleDoubleClickActionType.GoToSubtitleMinus1SecAndPlay] = () => Se.Language.Options.Settings.SubtitleListActionVideoGoToPositionMinus1SecAndPlay,
    };

    [Fact]
    public void EveryActionHasAnExpectedText()
    {
        var missing = Enum.GetValues<SubtitleDoubleClickActionType>()
            .Where(a => !ExpectedTexts.ContainsKey(a))
            .ToList();

        Assert.True(missing.Count == 0,
            "These double-click actions have no entry in this test - add them here, to the map in " +
            "SettingsViewModel and to the Options combo box list: " + string.Join(", ", missing));
    }

    [Theory]
    [MemberData(nameof(AllActions))]
    public void ActionTextMapsBackToTheAction(SubtitleDoubleClickActionType action)
    {
        var text = ExpectedTexts[action]();

        Assert.False(string.IsNullOrEmpty(text), $"{action} has no text in the English language file");
        Assert.Equal(action.ToString(), SettingsViewModel.MapToSelectedSubtitleDoubleClickAction(text));
    }

    public static TheoryData<SubtitleDoubleClickActionType> AllActions()
    {
        var data = new TheoryData<SubtitleDoubleClickActionType>();
        foreach (var action in Enum.GetValues<SubtitleDoubleClickActionType>())
        {
            data.Add(action);
        }

        return data;
    }
}
