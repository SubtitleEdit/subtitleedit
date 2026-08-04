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
    private const KeyModifiers ControlAltModifiers = KeyModifiers.Control | KeyModifiers.Alt;
    private readonly HashSet<Key> _activeKeys = [];
    // Parallel to _activeKeys but uses the physical-key-aware string token (see
    // GetShortcutKeyName). Numpad keys collapse to NumLock-on names in Avalonia's
    // Key enum, so HashSet<Key> can't tell main-Delete from numpad-Delete (NumLock
    // off) from numpad-Decimal (NumLock on). This set is what we hash for lookup
    // so users can bind each of those independently.
    private readonly HashSet<string> _activeKeyNames = [];
    private readonly List<ShortCut> _shortcuts = [];
    private FrozenDictionary<string, ShortCut>? _lookupTable;
    private bool _isDirty = true;
    private bool _isControlPressed = false;
    private bool _isShiftPressed = false;
    private bool _isWindowsAltGrPressed = false;

    public static string GetKeyDisplayName(string key)
    {
        bool isMac = OperatingSystem.IsMacOS();
        var shortcuts = Se.Language.Options.Shortcuts;

        switch (key)
        {
            case "Ctrl":
            case "Control":
                return isMac ? shortcuts.ControlMac : shortcuts.Control;
            case "Alt":
                return isMac ? shortcuts.AltMac : shortcuts.Alt;
            case "Shift":
                return isMac ? shortcuts.ShiftMac : shortcuts.Shift;
            case "Win":
            case "Cmd":
                return isMac ? shortcuts.WinMac : shortcuts.Win;
        }

        // Render "NumPadDelete" / "NumPad0" as "Numpad Delete" / "Numpad 0" so
        // the lists and shortcut hints read naturally. Storage stays in the
        // PascalCase form for matching.
        if (key.Length > "NumPad".Length && key.StartsWith("NumPad", StringComparison.Ordinal))
        {
            return "Numpad " + key.Substring("NumPad".Length);
        }

        return key;
    }

    /// <summary>
    /// Orders shortcut key tokens canonically — Control, Alt, Shift, Win, then regular
    /// keys — so a shortcut always reads the same way regardless of stored order.
    /// </summary>
    public static List<string> OrderKeys(IEnumerable<string> keys)
    {
        return keys
            .OrderBy(GetCanonicalKeyRank)
            .ThenBy(NormalizeKeyToken, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int GetCanonicalKeyRank(string key)
    {
        return NormalizeKeyToken(key) switch
        {
            "Control" => 0,
            "Alt" => 1,
            "Shift" => 2,
            "Win" => 3,
            _ => 4,
        };
    }

    public static Key GetShortcutKey(KeyEventArgs e)
    {
        return GetShortcutKey(e.Key, e.PhysicalKey);
    }

    // Legacy accessor: collapses any numpad physical key to its NumLock-on Key
    // value. Kept for callers that still operate on the Avalonia Key enum (e.g.
    // single-key allowance checks). New code should prefer GetShortcutKeyName so
    // numpad keys can be distinguished across NumLock states and from their
    // main-keyboard counterparts.
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

    // Returns a stable token for the pressed key that distinguishes numpad keys
    // from their main-keyboard counterparts and from each other across NumLock
    // states. Numpad physical keys get the "NumPad" prefix unless the Avalonia
    // Key name already carries it (NumPad0..NumPad9). For non-numpad physical
    // keys this is just Key.ToString(), preserving every existing binding string.
    public static string GetShortcutKeyName(KeyEventArgs e)
    {
        return GetShortcutKeyName(e.Key, e.PhysicalKey);
    }

    public static string GetShortcutKeyName(Key key, PhysicalKey physicalKey)
    {
        if (IsNumPadPhysicalKey(physicalKey))
        {
            // The four arithmetic operators (+ - * /) are unaffected by NumLock and have no
            // main-keyboard Key equivalent, so their plain Key name ("Add"/"Subtract"/
            // "Multiply"/"Divide") is already unambiguous. Prefixing them ("NumPadAdd") only
            // breaks matching against the Avalonia Key names used by the default shortcuts
            // (e.g. Shift+Add waveform zoom), the shortcut-picker dropdown, and SE 4 import.
            // Everything else on the numpad (digits, Decimal, Enter) DOES need the prefix to
            // stay distinct across NumLock states and from the main Enter key.
            if (physicalKey is PhysicalKey.NumPadAdd or PhysicalKey.NumPadSubtract or
                PhysicalKey.NumPadMultiply or PhysicalKey.NumPadDivide)
            {
                return key.ToString();
            }

            var keyName = key.ToString();
            return keyName.StartsWith("NumPad", StringComparison.Ordinal)
                ? keyName
                : "NumPad" + keyName;
        }

        // Avalonia's Key enum derives Oem* values from the character produced
        // against a US-keyboard table, so Shift+. on a Swedish layout reports
        // as OemSemicolon (because ':' lives on the semicolon key in US) and
        // collides with Shift+,. The non-ASCII '<' next to Z is mis-reported
        // as OemComma for the same reason. Fall back to the layout-independent
        // PhysicalKey so each physical key gets a unique, stable token.
        var plainKeyName = key.ToString();
        if (plainKeyName.StartsWith("Oem", StringComparison.Ordinal))
        {
            return physicalKey.ToString();
        }

        return plainKeyName;
    }

    // Maps tokens that were stored before the PhysicalKey fix onto the modern
    // PhysicalKey names. Existing user shortcut files have entries like
    // "OemPeriod" / "Oem1"; without this migration they would silently stop
    // matching after the upgrade.
    private static readonly Dictionary<string, string> LegacyOemKeyMap = new(StringComparer.Ordinal)
    {
        ["OemPeriod"] = "Period",
        ["OemComma"] = "Comma",
        ["OemSemicolon"] = "Semicolon",
        ["Oem1"] = "Semicolon",
        ["OemQuestion"] = "Slash",
        ["Oem2"] = "Slash",
        ["OemTilde"] = "Backquote",
        ["Oem3"] = "Backquote",
        ["OemOpenBrackets"] = "BracketLeft",
        ["Oem4"] = "BracketLeft",
        ["OemPipe"] = "Backslash",
        ["Oem5"] = "Backslash",
        ["OemCloseBrackets"] = "BracketRight",
        ["Oem6"] = "BracketRight",
        ["OemQuotes"] = "Quote",
        ["Oem7"] = "Quote",
        ["Oem8"] = "IntlBackslash",
        ["OemMinus"] = "Minus",
        ["OemPlus"] = "Equal",
        ["OemBackslash"] = "IntlBackslash",
        ["Oem102"] = "IntlBackslash",

        // Numpad arithmetic operators were briefly emitted with a "NumPad" prefix
        // (RC builds), which didn't match the bare Avalonia Key names used elsewhere.
        // Migrate any such stored tokens back onto the bare names now produced again.
        ["NumPadAdd"] = "Add",
        ["NumPadSubtract"] = "Subtract",
        ["NumPadMultiply"] = "Multiply",
        ["NumPadDivide"] = "Divide",
    };

    public static void MigrateLegacyOemKeys(List<string> keys)
    {
        for (var i = 0; i < keys.Count; i++)
        {
            if (LegacyOemKeyMap.TryGetValue(keys[i], out var modern))
            {
                keys[i] = modern;
            }
            else if (keys[i].Length == 1 && keys[i][0] >= '0' && keys[i][0] <= '9')
            {
                // A bare top-row digit ("4") never matches at runtime: Avalonia's
                // Key.ToString() reports it as "D4". Normalize so old/imported
                // bindings line up with the dispatched key name.
                keys[i] = "D" + keys[i];
            }
        }
    }

    private static bool IsNumPadPhysicalKey(PhysicalKey physicalKey)
    {
        return physicalKey is
            PhysicalKey.NumPad0 or PhysicalKey.NumPad1 or PhysicalKey.NumPad2 or
            PhysicalKey.NumPad3 or PhysicalKey.NumPad4 or PhysicalKey.NumPad5 or
            PhysicalKey.NumPad6 or PhysicalKey.NumPad7 or PhysicalKey.NumPad8 or
            PhysicalKey.NumPad9 or PhysicalKey.NumPadAdd or PhysicalKey.NumPadDecimal or
            PhysicalKey.NumPadDivide or PhysicalKey.NumPadMultiply or
            PhysicalKey.NumPadSubtract or PhysicalKey.NumPadEnter;
    }

    public void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        // When IME is processing input, clear all active keys to prevent stale keys
        // from corrupting shortcut hash lookups (e.g., after using Chinese input methods)
        if (e.Key is Key.ImeProcessed or Key.ImeConvert or Key.ImeNonConvert or
            Key.ImeAccept or Key.ImeModeChange or Key.DeadCharProcessed or Key.None)
        {
            _activeKeys.Clear();
            _activeKeyNames.Clear();
            return;
        }

        var key = GetShortcutKey(e);
        var isRightAlt = key == Key.RightAlt || e.PhysicalKey == PhysicalKey.AltRight;
        var isControlAltPressed = (e.KeyModifiers & ControlAltModifiers) == ControlAltModifiers;
        if (!isControlAltPressed)
        {
            _isWindowsAltGrPressed = false;
        }

        // Track physical right Alt independently of the logical layout mapping.
        if (OperatingSystem.IsWindows() &&
            isRightAlt &&
            isControlAltPressed)
        {
            _isWindowsAltGrPressed = true;
        }

        // Skip standalone modifier-like keys: Ctrl/Shift/Alt/Win redundantly
        // duplicate KeyEventArgs.KeyModifiers, and NumLock is a state toggle —
        // we never want it captured as a shortcut chord on its own.
        if (key is not (Key.LeftCtrl or Key.RightCtrl or
            Key.LeftShift or Key.RightShift or
            Key.LeftAlt or Key.RightAlt or
            Key.LWin or Key.RWin or Key.NumLock))
        {
            _activeKeys.Add(key);
            _activeKeyNames.Add(GetShortcutKeyName(e));
        }

        _isControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
    }

    public void OnKeyReleased(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.RightAlt ||
            e.PhysicalKey == PhysicalKey.AltRight ||
            (e.KeyModifiers & ControlAltModifiers) != ControlAltModifiers)
        {
            _isWindowsAltGrPressed = false;
        }

        if (e.Key is Key.ImeProcessed or Key.ImeConvert or Key.ImeNonConvert or
            Key.ImeAccept or Key.ImeModeChange or Key.DeadCharProcessed or Key.None)
        {
            _activeKeys.Clear();
            _activeKeyNames.Clear();
        }
        else
        {
            _activeKeys.Remove(GetShortcutKey(e));
            _activeKeyNames.Remove(GetShortcutKeyName(e));
        }

        _isControlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        _isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
    }

    public void ClearKeys()
    {
        _activeKeys.Clear();
        _activeKeyNames.Clear();
        _isControlPressed = false;
        _isShiftPressed = false;
        _isWindowsAltGrPressed = false;
    }

    public void RegisterShortcut(ShortCut shortcut)
    {
        _shortcuts.Add(shortcut);
        _isDirty = true;
    }

    /// <summary>
    /// True when the user has assigned <paramref name="keyName"/> (alone, no modifiers) to any
    /// action. Built-in key handling (e.g. F10 activating the menu bar) checks this so a
    /// user-configured shortcut wins over the default behavior (#12504).
    /// </summary>
    public bool HasSingleKeyShortcut(string keyName)
    {
        foreach (var shortcut in _shortcuts)
        {
            if (shortcut.Keys.Count == 1 && shortcut.Keys[0].Equals(keyName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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
        // Windows reports AltGr as a synthetic LeftCtrl followed by RightAlt.
        // Modifier transitions must not complete a shortcut using a still-held typing key.
        if (keyEventArgs.Key is (Key.LeftCtrl or Key.RightCtrl or
            Key.LeftShift or Key.RightShift or
            Key.LeftAlt or Key.RightAlt or
            Key.LWin or Key.RWin or Key.NumLock) &&
            _activeKeyNames.Count > 0)
        {
            return null;
        }

        // Prefer the character produced by AltGr over an equivalent Ctrl+Alt shortcut.
        if (_isWindowsAltGrPressed &&
            (keyEventArgs.KeyModifiers & ControlAltModifiers) == ControlAltModifiers)
        {
            return null;
        }

        if (_isDirty || _lookupTable is null)
        {
            RebuildLookupTable();
        }

        // Build the current state key list with initial capacity. Read from the
        // physical-key-aware name set so numpad keys hash distinctly from their
        // main-keyboard counterparts.
        var currentInputKeys = new List<string>(_activeKeyNames.Count + 2);

        foreach (var keyName in _activeKeyNames)
        {
            currentInputKeys.Add(keyName);
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

        // 2. Check normalized hash with activeControl. In practice no token normalizes on a
        // real keypress (_activeKeyNames excludes the modifier keys themselves and the
        // modifiers appended above are already canonical), so only build the second list and
        // hash when normalization actually changes something. NormalizeKeyToken returns the
        // input instance for unmatched tokens, so a reference check suffices.
        List<string>? normalizedKeys = null;
        for (var keyIndex = 0; keyIndex < currentInputKeys.Count; keyIndex++)
        {
            var normalized = NormalizeKeyToken(currentInputKeys[keyIndex]);
            if (!ReferenceEquals(normalized, currentInputKeys[keyIndex]))
            {
                normalizedKeys ??= new List<string>(currentInputKeys);
                normalizedKeys[keyIndex] = normalized;
            }
        }

        if (normalizedKeys != null)
        {
            var normalizedHash = ShortCut.CalculateHash(normalizedKeys, activeControl);
            if (normalizedHash != inputHash && _lookupTable!.TryGetValue(normalizedHash, out shortcut))
            {
                return shortcut.Action;
            }
        }

        // 3. Numpad navigation fallback: with NumLock off the numpad emits Home/End/
        // PageUp/... with a "NumPad" prefix so they can be bound independently (#10934).
        // When no numpad-specific binding matched above, retry with the main-keyboard
        // token so a plain "Ctrl+Home" binding fires from the numpad Home key too (#13194).
        List<string>? collapsedKeys = null;
        var lookupKeys = normalizedKeys ?? currentInputKeys;
        for (var keyIndex = 0; keyIndex < lookupKeys.Count; keyIndex++)
        {
            var collapsed = CollapseNumPadNavigationToken(lookupKeys[keyIndex]);
            if (!ReferenceEquals(collapsed, lookupKeys[keyIndex]))
            {
                collapsedKeys ??= new List<string>(lookupKeys);
                collapsedKeys[keyIndex] = collapsed;
            }
        }

        if (collapsedKeys != null)
        {
            var collapsedHash = ShortCut.CalculateHash(collapsedKeys, activeControl);
            if (collapsedHash != inputHash && _lookupTable!.TryGetValue(collapsedHash, out shortcut))
            {
                return shortcut.Action;
            }
        }

        return null;
    }

    /// <summary>
    /// Maps modifier-key aliases ("Ctrl"/"LeftCtrl"/"Control", ...) onto one canonical
    /// token, matching how runtime lookup unifies them. Used by duplicate detection so
    /// e.g. "Control+S" (SE 4 import) and "Ctrl+S" (UI checkboxes) count as the same.
    /// </summary>
    public static string NormalizeKeyToken(string key)
    {
        return key switch
        {
            "LeftCtrl" or "RightCtrl" or "Ctrl" => "Control",
            "LeftShift" or "RightShift" => "Shift",
            "LeftAlt" or "RightAlt" => "Alt",
            "LWin" or "RWin" => "Win",
            _ => key
        };
    }

    /// <summary>
    /// Maps the NumLock-off numpad navigation tokens ("NumPadHome", ...) onto their
    /// main-keyboard counterparts. Used ONLY as the last lookup stage in
    /// <see cref="CheckShortcuts"/> - never in binding hashes - so a numpad-specific
    /// binding keeps its distinct identity and always wins (#10934), while a plain
    /// "Ctrl+Home" binding also fires from the numpad Home key (#13194). Both alias
    /// spellings of the page keys are listed because Key.ToString() picks an
    /// unspecified alias for enum members sharing a value (PageUp/Prior, PageDown/Next).
    /// </summary>
    public static string CollapseNumPadNavigationToken(string key)
    {
        return key switch
        {
            "NumPadHome" => "Home",
            "NumPadEnd" => "End",
            "NumPadPageUp" => "PageUp",
            "NumPadPrior" => "Prior",
            "NumPadPageDown" => "PageDown",
            "NumPadNext" => "Next",
            "NumPadUp" => "Up",
            "NumPadDown" => "Down",
            "NumPadLeft" => "Left",
            "NumPadRight" => "Right",
            "NumPadInsert" => "Insert",
            "NumPadDelete" => "Delete",
            "NumPadClear" => "Clear",
            _ => key
        };
    }

    public static string CalculateNormalizedHash(List<string> inputKeys, string? control)
    {
        var keys = new List<string>(inputKeys.Count);
        foreach (var key in inputKeys)
        {
            keys.Add(NormalizeKeyToken(key));
        }

        return ShortCut.CalculateHash(keys, control);
    }

    public HashSet<Key> GetActiveKeys() => [.. _activeKeys];

    /// <summary>
    /// Allocation-free alternatives to <see cref="GetActiveKeys"/> for the per-keystroke
    /// probes in the window key handler, which only need the count or the single active key.
    /// </summary>
    public int ActiveKeyCount => _activeKeys.Count;

    public bool TryGetSingleActiveKey(out Key key)
    {
        if (_activeKeys.Count == 1)
        {
            foreach (var k in _activeKeys)
            {
                key = k;
                return true;
            }
        }

        key = default;
        return false;
    }

    public bool IsControlPressed() => _isControlPressed;
    public bool IsShiftPressed() => _isShiftPressed;
}
