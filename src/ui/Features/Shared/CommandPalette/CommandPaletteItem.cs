using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.CommandPalette;

public class CommandPaletteItem
{
    public string ActionName { get; }
    public string DisplayName { get; }
    public string GroupName { get; }

    /// <summary>
    /// The keys of the shortcut, one per key cap. The shortcut text joins them with "+", and a plus
    /// key shows as a trailing "+".
    /// </summary>
    public IReadOnlyList<string> ShortcutKeys { get; }

    public IRelayCommand Command { get; }

    // Lower-cased once so filtering on every key press does not re-allocate.
    internal string SearchName { get; }
    internal string SearchText { get; }
    internal string Initials { get; }

    public CommandPaletteItem(string actionName, string displayName, string groupName, string shortcutText, IRelayCommand command)
    {
        ActionName = actionName;
        DisplayName = displayName;
        GroupName = groupName;
        var keys = shortcutText.Split('+', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (shortcutText.EndsWith('+'))
        {
            keys.Add("+");
        }

        ShortcutKeys = keys;
        Command = command;

        SearchName = displayName.ToLowerInvariant();
        SearchText = (displayName + " " + groupName + " " + shortcutText).ToLowerInvariant();
        Initials = MakeInitials(SearchName);
    }

    private static string MakeInitials(string text)
    {
        var chars = new System.Text.StringBuilder();
        var atWordStart = true;
        foreach (var ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                if (atWordStart)
                {
                    chars.Append(ch);
                }

                atWordStart = false;
            }
            else
            {
                atWordStart = true;
            }
        }

        return chars.ToString();
    }
}
