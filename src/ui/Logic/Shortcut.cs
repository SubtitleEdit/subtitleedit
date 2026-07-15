using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Logic;

public class ShortCut
{
    public List<string> Keys { get; set; }
    public ShortcutCategory Category { get; set; }
    public ShortcutGroup Group { get; set; }
    public string? Control { get; set; }
    public string Name { get; set; }
    public IRelayCommand Action { get; set; }
    public string HashCode { get; set; }
    public string NormalizedHashCode { get; set; }

    public ShortCut(string name, List<string> keys, ShortcutCategory category, IRelayCommand action)
    {
        ShortcutManager.MigrateLegacyOemKeys(keys);
        Name = name;
        Keys = keys;
        Category = category;
        Group = ShortcutGroupUi.FromCategory(category);
        Control = category.ToString();
        Action = action;
        HashCode = CalculateHash(keys, Control);
        NormalizedHashCode = ShortcutManager.CalculateNormalizedHash(keys, Control);
    }

    public static string CalculateHash(List<string> keys, string? control)
    {
        // Runs twice per key event (direct + normalized lookup) and twice per shortcut
        // when the lookup table is rebuilt, so avoid LINQ ordering and the per-key
        // lowercase string allocations.
        var count = keys.Count;

        // Stable insertion sort of a small index span - same order as
        // OrderBy(k => k, StringComparer.OrdinalIgnoreCase).
        Span<int> order = count <= 8 ? stackalloc int[8] : new int[count];
        for (var i = 0; i < count; i++)
        {
            order[i] = i;
        }

        for (var i = 1; i < count; i++)
        {
            var current = order[i];
            var j = i - 1;
            while (j >= 0 && string.Compare(keys[order[j]], keys[current], StringComparison.OrdinalIgnoreCase) > 0)
            {
                order[j + 1] = order[j];
                j--;
            }

            order[j + 1] = current;
        }

        var totalLength = count + (control?.Length ?? 0);
        for (var i = 0; i < count; i++)
        {
            totalLength += keys[i].Length;
        }

        // Lowercase char-by-char into one buffer; a single string allocation for the result.
        Span<char> buffer = totalLength <= 256 ? stackalloc char[256] : new char[totalLength];
        var pos = 0;
        for (var i = 0; i < count; i++)
        {
            var key = keys[order[i]];
            for (var k = 0; k < key.Length; k++)
            {
                buffer[pos++] = char.ToLowerInvariant(key[k]);
            }

            buffer[pos++] = '+';
        }

        if (control != null)
        {
            for (var k = 0; k < control.Length; k++)
            {
                buffer[pos++] = char.ToLowerInvariant(control[k]);
            }
        }

        return new string(buffer[..pos]);
    }

    public ShortCut(ShortcutsMain.AvailableShortcut shortcut, SeShortCut keys)
    {
        // Mutates the underlying SeShortCut.Keys list so the migrated names
        // also get persisted on the next save — drops legacy "Oem*" tokens.
        ShortcutManager.MigrateLegacyOemKeys(keys.Keys);
        Keys = keys.Keys;
        Action = shortcut.RelayCommand;
        Category = shortcut.Category;
        Group = shortcut.Group;
        Name = shortcut.Name;
        Control = shortcut.Category.ToString();
        HashCode = CalculateHash(Keys, Control);
        NormalizedHashCode = ShortcutManager.CalculateNormalizedHash(Keys, Control);
    }

    public ShortCut(ShortcutsMain.AvailableShortcut shortcut)
    {
        Keys = new List<string>();
        Action = shortcut.RelayCommand;
        Category = shortcut.Category;
        Group = shortcut.Group;
        Name = shortcut.Name;
        Control = string.Empty;
        HashCode = CalculateHash(Keys, Control);
        NormalizedHashCode = ShortcutManager.CalculateNormalizedHash(Keys, Control);
    }
}