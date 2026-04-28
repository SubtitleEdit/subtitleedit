using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public class ShortcutManager : IShortcutManager
{
    private readonly HashSet<Key> _activeKeys = [];
    private readonly List<ShortCut> _shortcuts = [];
    private FrozenDictionary<string, ShortCut>? _lookupTable;
    private bool _isDirty = true;
    private bool _isControlPressed = false;
    private bool _isShiftPressed = false;

    public static string GetKeyDisplayName(string key)
    {
        bool isMac = OperatingSystem.IsMacOS();
        var shortcuts = Se.Language.Options.Shortcuts;

        return key switch
        {
            "Ctrl" or "Control" => isMac ? shortcuts.ControlMac : shortcuts.Control,
            "Alt" => isMac ? shortcuts.AltMac : shortcuts.Alt,
            "Shift" => isMac ? shortcuts.ShiftMac : shortcuts.Shift,
            "Win" or "Cmd" => isMac ? shortcuts.WinMac : shortcuts.Win,
            _ => key
        };
    }

    public static Key GetShortcutKey(KeyEventArgs e)
    {
        return GetShortcutKey(e.Key, e.PhysicalKey);
    }

    public static Key GetShortcutKey(Key key, PhysicalKey physicalKey)
    {
        return physicalKey switch
        {
            PhysicalKey.NumPad0 => Key.NumPad0,
            PhysicalKey.NumPad1 => Key.NumPad1,
            PhysicalKey.NumPad2 => Key.NumPad2,
            PhysicalKey.NumPad3 => Key.NumPad3,
            PhysicalKey.NumPad4 => Key.NumPad4,
            PhysicalKey.NumPad5 => Key.NumPad5,
            PhysicalKey.NumPad6 => Key.NumPad6,
            PhysicalKey.NumPad7 => Key.NumPad7,
            PhysicalKey.NumPad8 => Key.NumPad8,
            PhysicalKey.NumPad9 => Key.NumPad9,
            PhysicalKey.NumPadAdd => Key.Add,
            PhysicalKey.NumPadDecimal => Key.Decimal,
            PhysicalKey.NumPadDivide => Key.Divide,
            PhysicalKey.NumPadMultiply => Key.Multiply,
            PhysicalKey.NumPadSubtract => Key.Subtract,
            _ => key,
        };
    }

    public void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        // When IME is processing input, clear all active keys to prevent stale keys
        // from corrupting shortcut hash lookups (e.g., after using Chinese input methods)
        if (e.Key is Key.ImeProcessed or Key.ImeConvert or Key.ImeNonConvert or
            Key.ImeAccept or Key.ImeModeChange or Key.DeadCharProcessed or Key.None)
        {
            _activeKeys.Clear();
            return;
        }

        var key = GetShortcutKey(e);

        // Avoid adding modifier keys to the active keys set to prevent redundancy
        // with KeyEventArgs.KeyModifiers
        if (key is not (Key.LeftCtrl or Key.RightCtrl or
            Key.LeftShift or Key.RightShift or
            Key.LeftAlt or Key.RightAlt or
            Key.LWin or Key.RWin))
        {
            _activeKeys.Add(key);
        }

        _isControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
    }

    public void OnKeyReleased(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.ImeProcessed or Key.ImeConvert or Key.ImeNonConvert or
            Key.ImeAccept or Key.ImeModeChange or Key.DeadCharProcessed or Key.None)
        {
            _activeKeys.Clear();
        }
        else
        {
            _activeKeys.Remove(GetShortcutKey(e));
        }

        _isControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
    }

    public void ClearKeys()
    {
        _activeKeys.Clear();
        _isControlPressed = false;
        _isShiftPressed = false;
    }

    public void RegisterShortcut(ShortCut shortcut)
    {
        _shortcuts.Add(shortcut);
        _isDirty = true;
    }

    public void ClearShortcuts()
    {
        _shortcuts.Clear();
        _lookupTable = null;
        _isDirty = true;
    }

    private void RebuildLookupTable()
    {
        // Sort by key count descending so that specific shortcuts (Ctrl+Shift+S) 
        // take precedence over general ones (Ctrl+S) if they share hashes
        var sorted = _shortcuts.Where(s => s.Keys.Count > 0)
            .OrderByDescending(s => s.Keys.Count);

        var builder = new Dictionary<string, ShortCut>();
        foreach (var sc in sorted)
        {
            // Map the direct hash
            builder.TryAdd(sc.HashCode, sc);

            // Map the normalized hash (e.g., "LeftCtrl" -> "Control")
            if (!string.IsNullOrEmpty(sc.NormalizedHashCode))
            {
                builder.TryAdd(sc.NormalizedHashCode, sc);
            }
        }

        _lookupTable = builder.ToFrozenDictionary();
        _isDirty = false;
    }

    public IRelayCommand? CheckShortcuts(KeyEventArgs keyEventArgs, string activeControl)
    {
        if (_isDirty || _lookupTable is null)
        {
            RebuildLookupTable();
        }

        // Build the current state key list with initial capacity
        var currentInputKeys = new List<string>(_activeKeys.Count + 2);

        foreach (var key in _activeKeys)
        {
            currentInputKeys.Add(key.ToString());
        }

        // Add normalized modifiers based on the event state
        var modifiers = keyEventArgs.KeyModifiers;
        if ((modifiers & KeyModifiers.Control) != 0)
        {
            currentInputKeys.Add("Control");
        }

        if ((modifiers & KeyModifiers.Alt) != 0)
        {
            currentInputKeys.Add("Alt");
        }

        if ((modifiers & KeyModifiers.Shift) != 0)
        {
            currentInputKeys.Add("Shift");
        }

        if ((modifiers & KeyModifiers.Meta) != 0)
        {
            currentInputKeys.Add("Win");
        }

        // 1. Check primary hash with activeControl
        var inputHash = ShortCut.CalculateHash(currentInputKeys, activeControl);
        if (_lookupTable!.TryGetValue(inputHash, out var shortcut))
        {
            return shortcut.Action;
        }

        // 2. Check normalized hash with activeControl
        var normalizedHash = CalculateNormalizedHash(currentInputKeys, activeControl);
        if (normalizedHash != inputHash && _lookupTable!.TryGetValue(normalizedHash, out shortcut))
        {
            return shortcut.Action;
        }

        return null;
    }

    public static string CalculateNormalizedHash(List<string> inputKeys, string? control)
    {
        var keys = new List<string>(inputKeys.Count);
        foreach (var key in inputKeys)
        {
            keys.Add(key switch
            {
                "LeftCtrl" or "RightCtrl" or "Ctrl" => "Control",
                "LeftShift" or "RightShift" => "Shift",
                "LeftAlt" or "RightAlt" => "Alt",
                "LWin" or "RWin" => "Win",
                _ => key
            });
        }

        return ShortCut.CalculateHash(keys, control);
    }

    public HashSet<Key> GetActiveKeys() => [.. _activeKeys];

    public bool IsControlPressed() => _isControlPressed;
    public bool IsShiftPressed() => _isShiftPressed;
}