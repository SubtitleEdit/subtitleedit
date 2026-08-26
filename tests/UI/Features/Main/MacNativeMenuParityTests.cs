using System.Text.RegularExpressions;

namespace UITests.Features.Main;

/// <summary>
/// The macOS NSMenuBar in InitNativeMacMenu.cs is a hand-maintained mirror of the
/// Avalonia menu in InitMenu.cs - Avalonia's NativeMenu is a separate API, so there is
/// no shared builder the two could grow from. Every menu entry added to one file has to
/// be added to the other by hand, and when that is forgotten the item is simply missing
/// on macOS with nothing to notice it: the app still builds, still runs, and the gap is
/// only found by someone using that menu on a Mac.
///
/// This test is that missing signal. Both files are pure builders whose entries all name
/// a command on MainViewModel, so comparing the two command sets catches a one-sided add
/// at build time. It deliberately checks presence only, not placement: which submenu an
/// item lives in legitimately differs (macOS groups some entries differently, and the
/// native menus sort at build time), and pinning layout here would fight normal edits.
/// </summary>
public class MacNativeMenuParityTests
{
    // Matches the command references both builders use:
    //   InitMenu.cs           Command = vm.ShowFindCommand
    //   InitNativeMacMenu.cs  Item(..., v => v.ShowFindCommand)   /   state.Vm?.FilePropertiesShowCommand
    //                         GetVm()?.ShowAboutCommand           (the application-menu items)
    // A receiver is required so type names (IRelayCommand, ICommand) never match.
    private static readonly Regex CommandRegex =
        new(@"(?:(?<![A-Za-z0-9_])(?:vm|v|Vm)|GetVm\(\))\??\.([A-Za-z0-9_]+Command)\b", RegexOptions.Compiled);

    /// <summary>
    /// In InitMenu.cs but intentionally not in the macOS menu bar.
    /// </summary>
    private static readonly HashSet<string> AllowedMissingOnMac =
    [
        // macOS puts Quit in the application menu, where Avalonia appends it (along with
        // Hide / Hide Others / Show All) automatically. A second "Exit" in the File menu
        // is exactly what a Mac user does not expect.
        "CommandExitCommand",
    ];

    /// <summary>
    /// In InitNativeMacMenu.cs but intentionally not in InitMenu.cs.
    /// </summary>
    private static readonly HashSet<string> AllowedMacOnly =
    [
        // The audio-track submenu is rebuilt per track. NativeMenuItem has no ICommand
        // binding, so the macOS side routes every track through one parameterised command,
        // while InitMenu.cs builds bound MenuItems in MainViewModel instead.
        "PickAudioTrackCommand",
    ];

    [Fact]
    public void EveryMenuCommand_IsInBothMenuBuilders()
    {
        var crossPlatform = ExtractCommands("InitMenu.cs");
        var mac = ExtractCommands("InitNativeMacMenu.cs");

        Assert.NotEmpty(crossPlatform);
        Assert.NotEmpty(mac);

        var missingOnMac = crossPlatform.Except(mac).Except(AllowedMissingOnMac).Order().ToList();
        var macOnly = mac.Except(crossPlatform).Except(AllowedMacOnly).Order().ToList();

        Assert.True(
            missingOnMac.Count == 0 && macOnly.Count == 0,
            "The macOS menu bar and the Avalonia menu are out of sync." +
            Environment.NewLine +
            "Add the entry to the other builder, or - if the difference is deliberate - " +
            "list the command in AllowedMissingOnMac / AllowedMacOnly with the reason." +
            Environment.NewLine +
            $"In InitMenu.cs but missing from InitNativeMacMenu.cs: {Format(missingOnMac)}" +
            Environment.NewLine +
            $"In InitNativeMacMenu.cs but missing from InitMenu.cs: {Format(macOnly)}");
    }

    private static string Format(List<string> commands)
    {
        return commands.Count == 0 ? "(none)" : string.Join(", ", commands);
    }

    private static HashSet<string> ExtractCommands(string fileName)
    {
        var path = Path.Combine(FindRepoRoot(), "src", "ui", "Features", "Main", "Layout", fileName);
        Assert.True(File.Exists(path), $"Menu builder not found: {path}");

        return CommandRegex.Matches(File.ReadAllText(path))
            .Select(m => m.Groups[1].Value)
            .ToHashSet();
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src", "ui")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not find repo root");
    }
}
