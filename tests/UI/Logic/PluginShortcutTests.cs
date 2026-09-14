using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Plugins;

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

    [Fact]
    public void PluginsWhoseNamesCollideAfterSanitizingBothKeepAnEntry()
    {
        // "Foo Bar" and "Foo-Bar" both sanitize to Plugin_Foo_Bar; the menus are built from
        // these entries, so dropping the second one made that plugin vanish from the menu.
        var plugins = new[] { MakePlugin("Foo Bar"), MakePlugin("Foo-Bar"), MakePlugin("Foo_Bar") };

        var entries = MainViewModel.BuildPluginShortcutEntries(plugins, _ => new RelayCommand(() => { }));

        Assert.Equal(3, entries.Count);
        Assert.Equal(new[] { "Plugin_Foo_Bar", "Plugin_Foo_Bar_2", "Plugin_Foo_Bar_3" }, entries.Select(e => e.ActionName));
        Assert.Equal(new[] { "Foo Bar", "Foo-Bar", "Foo_Bar" }, entries.Select(e => e.Plugin.Manifest.Name));
        Assert.All(entries, e => Assert.DoesNotContain(' ', e.ActionName));
    }

    private static InstalledPlugin MakePlugin(string name) => new()
    {
        Manifest = new PluginManifest { Name = name },
        FolderPath = Path.Combine(Path.GetTempPath(), name),
        ManifestPath = Path.Combine(Path.GetTempPath(), name, "plugin.json"),
        LaunchPath = Path.Combine(Path.GetTempPath(), name, "plugin"),
    };
}
