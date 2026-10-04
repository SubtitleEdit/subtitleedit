using Avalonia.Headless.XUnit;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace UITests.Features.Options.Shortcuts;

/// <summary>
/// Windows reports AltGr as Ctrl+Alt, so "Ctrl+AltGr+K" can never fire: the AltGr checkbox is
/// exclusive with Ctrl and Alt. Where the AltGr checkbox is hidden (non-Windows), editing a
/// shortcut drops a stored AltGr token (#15618).
/// </summary>
public class ShortcutsAltGrCheckboxTests
{
    private static (ShortcutsViewModel Vm, ShortCut ShortCut) Make(bool altGrSupported, List<string> keys)
    {
        var shortCut = new ShortCut("Test", keys, ShortcutCategory.General, new RelayCommand(() => { }));
        var vm = new ShortcutsViewModel(null!, null!)
        {
            IsAltGrSupported = altGrSupported,
        };
        vm.SelectedNode = new ShortcutTreeNode("General", "Test", string.Empty, shortCut);
        vm.SelectedShortcut = "K";
        return (vm, shortCut);
    }

    [AvaloniaFact]
    public void TickingAltGrUnticksCtrlAndAlt()
    {
        var (vm, shortCut) = Make(true, []);
        vm.CtrlIsSelected = true;
        vm.AltIsSelected = true;

        vm.AltGrIsSelected = true;

        Assert.False(vm.CtrlIsSelected);
        Assert.False(vm.AltIsSelected);
        Assert.Equal([ShortcutManager.AltGrToken, "K"], shortCut.Keys);
    }

    [AvaloniaFact]
    public void TickingCtrlOrAltUnticksAltGr()
    {
        var (vm, shortCut) = Make(true, []);
        vm.AltGrIsSelected = true;
        vm.ShiftIsSelected = true;

        vm.CtrlIsSelected = true;

        Assert.False(vm.AltGrIsSelected);
        Assert.Equal(["Ctrl", "Shift", "K"], shortCut.Keys);

        vm.AltGrIsSelected = true;
        vm.AltIsSelected = true;

        Assert.False(vm.AltGrIsSelected);
        Assert.Equal(["Alt", "Shift", "K"], shortCut.Keys);
    }

    [AvaloniaFact]
    public void HiddenAltGrIsDroppedOnEdit()
    {
        var (vm, shortCut) = Make(false, [ShortcutManager.AltGrToken, "K"]);
        vm.AltGrIsSelected = true; // as loaded from a Windows settings file

        vm.ShiftIsSelected = true;

        Assert.Equal(["Shift", "K"], shortCut.Keys);
    }
}
