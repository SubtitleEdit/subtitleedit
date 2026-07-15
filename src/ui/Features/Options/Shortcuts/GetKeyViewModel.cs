using System;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class GetKeyViewModel : ObservableObject
{
    [ObservableProperty] private string _shortCutName;
    [ObservableProperty] private string _infoText;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string PressedKeyOnly { get; private set; }
    public string PressedKey { get; private set; }
    public bool IsControlPressed { get; private set; }
    public bool IsAltPressed { get; private set; }
    public bool IsShiftPressed { get; private set; }
    public bool IsWinPressed { get; private set; }

    private bool _isWinDown;

    public GetKeyViewModel()
    {
        ShortCutName = string.Empty;
        InfoText = Se.Language.Options.Shortcuts.PressAKey;
        PressedKey = string.Empty;
        PressedKeyOnly = string.Empty;
    }

    public void Initialize(string shortcutName)
    {
        ShortCutName = shortcutName;
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
            return;
        }

        if (e.Key is Key.LWin or Key.RWin)
        {
            e.Handled = true;
            _isWinDown = true;
            return;
        }

        if (e.Key is Key.LeftShift or Key.RightShift or
                     Key.LeftAlt or Key.RightAlt or
                     Key.LeftCtrl or Key.RightCtrl or
                     Key.NumLock)
        {
            // NumLock is a state toggle, not a chord key — swallow it instead of
            // capturing "NumLock" as the bound shortcut.
            e.Handled = true;
            return;
        }

        var infoText = string.Empty;

        var isMac = OperatingSystem.IsMacOS() ;
        var ctrlLabel = isMac ? Se.Language.Options.Shortcuts.ControlMac : Se.Language.Options.Shortcuts.Control;
        var altLabel = isMac ? Se.Language.Options.Shortcuts.AltMac : Se.Language.Options.Shortcuts.Alt;
        var shiftLabel = isMac ? Se.Language.Options.Shortcuts.ShiftMac : Se.Language.Options.Shortcuts.Shift;
        var winLabel = isMac ? Se.Language.Options.Shortcuts.WinMac : Se.Language.Options.Shortcuts.Win;

        // Use the physical-key-aware name so numpad-Delete (NumLock off) records
        // as "NumPadDelete" rather than the misleading "Decimal", and so it stays
        // bindable distinct from main Delete and from numpad-Decimal (NumLock on).
        var keyName = ShortcutManager.GetShortcutKeyName(e);

        PressedKeyOnly = keyName;
        IsControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        IsAltPressed = e.KeyModifiers.HasFlag(KeyModifiers.Alt);
        IsShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        IsWinPressed = _isWinDown;

        // Build the chord in canonical modifier order: Control, Alt, Shift, Win
        PressedKey = string.Empty;

        if (IsControlPressed)
        {
            PressedKey += "Ctrl+";
            infoText += ctrlLabel + " + ";
        }

        if (IsAltPressed)
        {
            PressedKey += "Alt+";
            infoText += altLabel + " + ";
        }

        if (IsShiftPressed)
        {
            PressedKey += "Shift+";
            infoText += shiftLabel + " + ";
        }

        if (IsWinPressed)
        {
            PressedKey += "Win+";
            infoText += winLabel + " + ";
        }

        PressedKey += keyName;
        infoText += keyName;

        InfoText = string.Format(Se.Language.Options.Shortcuts.PressedKeyX, infoText);
    }

    public void KeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.LWin or Key.RWin)
        {
            e.Handled = true;
            _isWinDown = false;
        }
    }
}