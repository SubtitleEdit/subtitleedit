using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

/// <summary>
/// Minimize, Zoom and Close window in the macOS menu bar. AppKit gives an app no default
/// Window menu and no default ⌘M / ⌘W: a key equivalent works only where a menu item
/// carries it, so before these items existed both shortcuts were simply dead in SE, with
/// nothing in a build or a run to show it. The items carry no view-model command either -
/// they act on the window directly - so the source-level check in
/// <see cref="MacNativeMenuParityTests"/> cannot see them, and this test pins them instead.
///
/// The builder itself is platform-independent (only its caller in Program.cs is macOS-only),
/// so the structure is asserted on any OS - which matters, because CI runs tests on Linux.
/// </summary>
public class MacWindowMenuItemsTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        // InitNativeMacMenu keeps one MenuState per window and drops it on Closed; leaving a
        // window open would leak that state into the next test's menu bar.
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    [AvaloniaFact]
    public void WindowMenu_HasMinimizeAndZoom_WithCommandM()
    {
        var l = Se.Language.Main.Menu;
        var windowMenu = BuildMenu().First(i => i.Header == Strip(l.WindowTitle));

        var minimize = windowMenu.Menu!.Items.OfType<NativeMenuItem>()
            .First(i => i.Header == Strip(l.WindowMinimize));
        Assert.Equal(new KeyGesture(Key.M, KeyModifiers.Meta), minimize.Gesture);

        var zoom = windowMenu.Menu!.Items.OfType<NativeMenuItem>()
            .FirstOrDefault(i => i.Header == Strip(l.WindowZoom));
        Assert.NotNull(zoom);

        // Both sit above the window list: AppKit appends its window entries below whatever the
        // menu already holds, which is the standard macOS Window-menu layout.
        Assert.Equal(0, windowMenu.Menu!.Items.IndexOf(minimize));
        Assert.Equal(1, windowMenu.Menu!.Items.IndexOf(zoom!));
    }

    [AvaloniaFact]
    public void FileMenu_HasCloseWindow_WithCommandW()
    {
        var l = Se.Language.Main.Menu;
        var fileMenu = BuildMenu().First(i => i.Header == Strip(l.File));

        var close = fileMenu.Menu!.Items.OfType<NativeMenuItem>()
            .First(i => i.Header == Strip(l.CloseWindow));

        // ⌘W closes a window on macOS and lives in File, not in the Window menu.
        Assert.Equal(new KeyGesture(Key.W, KeyModifiers.Meta), close.Gesture);
    }

    private List<NativeMenuItem> BuildMenu()
    {
        var window = new Window();
        _windows.Add(window);

        var root = new NativeMenu();
        InitNativeMacMenu.MakeStructure(root, window);

        return root.Items.OfType<NativeMenuItem>().ToList();
    }

    // The menu builder strips the access-key underscores the language strings carry for the
    // Windows/Linux menu, so headers have to be compared the same way.
    private static string Strip(string header)
    {
        return header.Replace("_", string.Empty);
    }
}
