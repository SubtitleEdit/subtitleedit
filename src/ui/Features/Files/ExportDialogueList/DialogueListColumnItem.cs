using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Files.ExportDialogueList;

public partial class DialogueListColumnItem : ObservableObject
{
    public DialogueListColumn Column { get; }
    public string Name { get; }

    /// <summary>False for Original text when no original is loaded - nothing to export.</summary>
    public bool IsAvailable { get; }

    /// <summary>Whether the file has data for the column (an empty Actor column is not ticked by default).</summary>
    public bool HasData { get; }

    [ObservableProperty] private bool _isChecked;

    public DialogueListColumnItem(DialogueListColumn column, string name, bool isAvailable, bool hasData, bool isChecked)
    {
        Column = column;
        Name = name;
        IsAvailable = isAvailable;
        HasData = hasData;
        _isChecked = isChecked && isAvailable;
    }
}
