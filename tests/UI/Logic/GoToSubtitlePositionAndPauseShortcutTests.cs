using System;
using System.Linq;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// SE 4's "Go to sub position and pause" jumps the video to the selected line's start and stops
/// there. SE 5 had the jump on its own but nothing that also paused, so the picture ran on past
/// the cue (#13938). The action is only usable once it is registered in the shortcut system - an
/// unregistered command is stored but never fires (#12088).
/// </summary>
public class GoToSubtitlePositionAndPauseShortcutTests
{
    [Fact]
    public void TheActionIsRegisteredAndNamed()
    {
        var name = nameof(MainViewModel.VideoGoToSubtitlePositionAndPauseCommand);

        Assert.True(ShortcutsMain.CommandTranslationLookup.ContainsKey(name),
            $"{name} is not registered in ShortcutsMain, so a shortcut bound to it would never fire.");
        Assert.False(string.IsNullOrWhiteSpace(ShortcutsMain.CommandTranslationLookup[name]));
    }

    /// <summary>It is a distinct action from the plain jump, which must keep working as before.</summary>
    [Fact]
    public void ThePlainJumpIsStillItsOwnAction()
    {
        var pausing = nameof(MainViewModel.VideoGoToSubtitlePositionAndPauseCommand);
        var plain = nameof(MainViewModel.VideoSetPositionCurrentSubtitleStartCommand);

        Assert.True(ShortcutsMain.CommandTranslationLookup.ContainsKey(plain));
        Assert.NotEqual(ShortcutsMain.CommandTranslationLookup[plain], ShortcutsMain.CommandTranslationLookup[pausing]);
    }
}
