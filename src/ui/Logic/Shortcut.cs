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
        var sb = new System.Text.StringBuilder();
        foreach (var key in keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase))
        {
            sb.Append(key.ToLowerInvariant()).Append('+');
        }

        sb.Append(control?.ToLowerInvariant() ?? string.Empty);
        return sb.ToString();
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