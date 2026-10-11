using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.CommandPalette;

public class CommandPaletteItem
{
    public string ActionName { get; }
    public string DisplayName { get; }
    public string GroupName { get; }
    public string ShortcutText { get; }
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
        ShortcutText = shortcutText;
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
