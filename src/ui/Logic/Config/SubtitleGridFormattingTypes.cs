namespace Nikse.SubtitleEdit.Logic.Config;

public enum SubtitleGridFormattingTypes
{ 
    NoFormatting = 0,
    ShowFormatting = 1,
    ShowTags = 2,
    HideTags = 3,

    /// <summary>
    /// Like <see cref="ShowFormatting"/>, but tags that have no visible effect in the grid
    /// (position, alignment, animation, borders...) are kept as text instead of hidden.
    /// </summary>
    ShowFormattingKeepTags = 4,
}
