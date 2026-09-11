using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

public partial class MergeShortLinesItem : ObservableObject
{
    /// <summary>
    /// Whether the line identified by <see cref="SourceLineId"/> may be merged into the line
    /// before it. Rows that do not represent such a merge (the first line of a highlighted
    /// group) have <see cref="CanToggle"/> false and no checkbox.
    /// </summary>
    [ObservableProperty] private bool _apply = true;

    public string Name { get; set; }
    public int Number { get; set; }
    public string Fix { get; set; }
    public SubtitleLineViewModel SubtitleLine { get; set; }

    /// <summary>
    /// Id of the subtitle line this row would merge into the preceding line. Indices shift
    /// when the settings regroup lines and when a refused merge turns a line into a new group
    /// head, so the line's Guid is the identity the untick has to follow.
    /// </summary>
    public Guid SourceLineId { get; }

    public bool CanToggle { get; }

    public MergeShortLinesItem(string name, int number, string fix, SubtitleLineViewModel subtitleLine, Guid sourceLineId, bool canToggle = true)
    {
        Name = name;
        Number = number;
        Fix = fix;
        SubtitleLine = subtitleLine;
        SourceLineId = sourceLineId;
        CanToggle = canToggle;
    }
}
