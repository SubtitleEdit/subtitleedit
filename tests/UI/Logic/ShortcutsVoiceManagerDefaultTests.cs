using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Logic;

/// <summary>
/// v5.3.0 betas shipped Ctrl+Shift+V as the voice manager default, which "fill selected lines with
/// clipboard text" already had (#15326). The voice manager default is gone, and shortcut
/// migration v5 clears the stale persisted copy.
/// </summary>
public class ShortcutsVoiceManagerDefaultTests
{
    private const string VoiceManager = nameof(MainViewModel.ShowVideoVoiceManagerCommand);
    private const string FillWithClipboard = nameof(MainViewModel.FillSelectedLinesWithClipboardCommand);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void VoiceManagerHasNoDefaultAndFillKeepsCtrlShiftV(bool isMacOS)
    {
        // GetDefaultShortcuts only uses the vm parameter for nameof() - never dereferenced.
        var defaults = ShortcutsMain.GetDefaultShortcuts(null!, isMacOS);

        Assert.DoesNotContain(defaults, s => s.ActionName == VoiceManager);
        Assert.Equal(new[] { isMacOS ? "Win" : "Ctrl", "Shift", "V" }, defaults.Single(s => s.ActionName == FillWithClipboard).Keys);
    }

    [Theory]
    [InlineData(false, "Ctrl")]
    [InlineData(false, "Control")]
    [InlineData(true, "Win")]
    public void MigrationClearsStaleVoiceManagerDefault(bool isMacOS, string modifier)
    {
        var settings = new Se { ShortcutsMigrationVersion = 4 };
        settings.Shortcuts.Add(new SeShortCut(VoiceManager, [modifier, "Shift", "V"]));
        settings.Shortcuts.Add(new SeShortCut(FillWithClipboard, [modifier, "Shift", "V"]));

        settings.MigrateShortcuts(isMacOS);

        Assert.Empty(settings.Shortcuts.Single(s => s.ActionName == VoiceManager).Keys);
        Assert.Equal(new[] { modifier, "Shift", "V" }, settings.Shortcuts.Single(s => s.ActionName == FillWithClipboard).Keys);
        Assert.Equal(Se.CurrentShortcutsMigrationVersion, settings.ShortcutsMigrationVersion);
    }

    [Fact]
    public void MigrationKeepsCustomVoiceManagerBinding()
    {
        var settings = new Se { ShortcutsMigrationVersion = 4 };
        settings.Shortcuts.Add(new SeShortCut(VoiceManager, ["Control", "Alt", "Shift", "V"]));

        settings.MigrateShortcuts(isMacOS: false);

        Assert.Equal(new[] { "Control", "Alt", "Shift", "V" }, settings.Shortcuts.Single().Keys);
    }
}
