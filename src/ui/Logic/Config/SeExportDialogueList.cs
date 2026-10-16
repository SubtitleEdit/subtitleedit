using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// Last used File > Export > Excel dialogue list options. Column names are
/// <see cref="Nikse.SubtitleEdit.Features.Files.ExportDialogueList.DialogueListColumn"/> names.
/// </summary>
public class SeExportDialogueList
{
    /// <summary>All columns in the chosen order, ticked or not; empty = default order.</summary>
    public List<string> ColumnOrder { get; set; } = new();

    /// <summary>The ticked columns; null until the first export, which picks sensible defaults.</summary>
    public List<string>? SelectedColumns { get; set; }

    public string TimeCodeFormat { get; set; } = "Frames";
    public bool LineBreaksInCell { get; set; } = true;
    public bool IncludeHeader { get; set; } = true;
    public bool RemoveFormatting { get; set; } = true;
}
