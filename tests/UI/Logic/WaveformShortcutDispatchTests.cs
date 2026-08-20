using System;
using System.Linq;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// The waveform set-start/set-end/insert actions are listed under Waveform in the shortcut
/// browser so they can be found where SE 4 puts them (#13921), but they must keep dispatching
/// from anywhere - that is what ShortcutCategory.General buys them, and the category, not the
/// group, is what decides which focused control a shortcut fires in. Narrowing them to
/// ShortcutCategory.Waveform would silently stop F11/F12 working outside the waveform.
/// </summary>
public class WaveformShortcutDispatchTests
{
    [Theory]
    [InlineData("WaveformSetStartCommand")]
    [InlineData("WaveformSetEndCommand")]
    public void WaveformSetStartEndStayGloballyDispatched(string commandName)
    {
        // GetDefaultShortcuts only uses the vm parameter for nameof() - never dereferenced.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!);

        var shortcut = defaults.FirstOrDefault(s => s.ActionName.Equals(commandName, StringComparison.Ordinal));

        Assert.NotNull(shortcut);
        // SeShortCut carries the category as ControlName - that is what the dispatcher matches on.
        Assert.Equal(ShortcutCategory.General.ToString(), shortcut.ControlName);
    }
}
