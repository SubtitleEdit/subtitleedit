using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

/// <summary>
/// Plugins get one shortcut entry each (Options > Shortcuts > Plugins, like SE 4). The
/// manifest's optional "shortcut" is parsed into the stored key-token list; the action name
/// must survive the space filter in ShortcutsMain.GetUsedShortcuts.
/// </summary>
public class PluginShortcutTests
{
    [Theory]
    [InlineData("Control+Shift+U", new[] { "Control", "Shift", "U" })]
    [InlineData("ctrl+alt+F5", new[] { "Control", "Alt", "F5" })]
    [InlineData(" Shift + p ", new[] { "Shift", "P" })]
    [InlineData("Cmd+K", new[] { "Win", "K" })]
    [InlineData("F9", new[] { "F9" })]
    public void ParsesManifestShortcut(string input, string[] expected)
    {
        Assert.Equal(expected, ShortcutsMain.ParseManifestShortcut(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Control+Shift")]
    [InlineData("Control+A+B")]
    [InlineData("Control+NotAKey")]
    [InlineData("Control+None")]
    public void RejectsInvalidManifestShortcut(string? input)
    {
        Assert.Null(ShortcutsMain.ParseManifestShortcut(input));
    }

    [Fact]
    public void ActionNameHasNoSpaces()
    {
        var name = MainViewModel.GetPluginShortcutActionName("Uppercase Selected Lines (v2)");
        Assert.Equal("Plugin_Uppercase_Selected_Lines__v2_", name);
        Assert.DoesNotContain(' ', name);
    }
}
