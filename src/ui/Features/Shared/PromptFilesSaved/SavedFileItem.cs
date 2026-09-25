using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Shared.PromptFilesSaved;

/// <summary>
/// One row in the "files generated" dialog: a produced file, or a job that produced nothing
/// (skipped/failed) - those keep their status text instead of the file chips.
/// </summary>
public partial class SavedFileItem : ObservableObject
{
    public string FileName { get; }
    public string FileNameDisplay { get; }
    public string FolderDisplay { get; }
    public bool IsSuccess { get; }
    public bool IsFailed => !IsSuccess;
    public string StatusText { get; }
    public string ExtensionChip { get; }
    public bool HasExtensionChip => IsSuccess && !string.IsNullOrEmpty(ExtensionChip);
    public long FileSize { get; set; }

    /// <summary>False when every file went to the one folder the dialog header already names.</summary>
    public bool ShowFolder { get; set; } = true;

    [ObservableProperty] private string _fileSizeChip = string.Empty;
    [ObservableProperty] private bool _hasFileSizeChip;
    [ObservableProperty] private string _durationChip = string.Empty;
    [ObservableProperty] private bool _hasDurationChip;

    public SavedFileItem(string fileName, bool isSuccess, string statusText)
    {
        FileName = fileName;
        IsSuccess = isSuccess;
        StatusText = statusText;
        FileNameDisplay = Path.GetFileName(fileName);
        var folder = Path.GetDirectoryName(fileName);
        FolderDisplay = string.IsNullOrEmpty(folder)
            ? string.Empty
            : folder.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        ExtensionChip = Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant();
    }
}
