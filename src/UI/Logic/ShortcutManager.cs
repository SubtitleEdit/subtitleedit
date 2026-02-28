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

    public void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        // Avoid adding modifier keys to the active keys set to prevent redundancy 
        // with KeyEventArgs.KeyModifiers
        if (e.Key is not (Key.LeftCtrl or Key.RightCtrl or
            Key.LeftShift or Key.RightShift or
            Key.LeftAlt or Key.RightAlt or
            Key.LWin or Key.RWin))
        {
            _activeKeys.Add(e.Key);
        }

        _isControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
    }

    public void OnKeyReleased(object? sender, KeyEventArgs e)
    {
        _activeKeys.Remove(e.Key);

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