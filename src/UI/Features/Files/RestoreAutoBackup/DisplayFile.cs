using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Files.RestoreAutoBackup;

public partial class DisplayFile : ObservableObject
{
    [ObservableProperty] private string _dateAndTime;
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _extension;
    [ObservableProperty] private string _size;

    public string FullPath { get; set; }

    public DisplayFile(string fileName, string dateAndTime, string size)
    {
        FullPath = fileName;

        var displayName = Path.GetFileNameWithoutExtension(fileName);
        if (displayName.Length > 20)
        {
            displayName = displayName.Remove(0, 20);
        }

        DateAndTime = dateAndTime;
        FileName = displayName;
        Extension = Path.GetExtension(fileName);
        Size = size;
    }
}
