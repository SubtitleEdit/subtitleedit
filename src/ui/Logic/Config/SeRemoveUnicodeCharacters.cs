using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeUnicodeReplaceListItem
{
    public string Character { get; set; } = string.Empty;
    public string ReplaceWith { get; set; } = string.Empty;
}

public class SeRemoveUnicodeCharacters
{
    /// <summary>
    /// Remembered "replace with" mappings for the remove/replace Unicode characters tool -
    /// characters found in a subtitle are prefilled from this list (an empty/missing entry
    /// means remove). Mirrors the SE 4 "Remove Unicode characters" plugin's replace list.
    /// </summary>
    public List<SeUnicodeReplaceListItem> ReplaceList { get; set; } = new()
    {
        new SeUnicodeReplaceListItem { Character = "♪", ReplaceWith = "#" },
        new SeUnicodeReplaceListItem { Character = "♫", ReplaceWith = "#" },
    };
}
