using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public partial class DisplayFile : ObservableObject
{
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private long _size;

    public string FullPath { get; set; }

    private static readonly Regex TimeCodeFormat1 = new Regex(@"^\d+_\d+_\d+_\d+__\d+_\d+_\d+_\d+_\d+$", RegexOptions.Compiled);
    private static readonly Regex TimeCodeFormat1WithExtension = new Regex(@"^\d+_\d+_\d+_\d+__\d+_\d+_\d+_\d+_\d+\..+$", RegexOptions.Compiled);
    private static readonly Regex TimeCodeFormat2 = new Regex(@"^\d+_\d+_\d+_\d+__\d+_\d+_\d+_\d+$", RegexOptions.Compiled);

    public DisplayFile(string fileName, long size)
    {
        FullPath = fileName;

        var displayName = Path.GetFileNameWithoutExtension(fileName);
        if (displayName.Length > 20)
        {
            displayName = displayName.Remove(0, 20);
        }

        FileName = displayName;
        Size = size;
    }

    public (TimeSpan, TimeSpan) GetTimeCodes()
    {
        var name = Path.GetFileNameWithoutExtension(FullPath);
        if (name.Contains("-to-"))
        {
            var arr = name.Replace("-to-", "_").Split('_');
            if (arr.Length == 3 && int.TryParse(arr[1], out var startTime) && int.TryParse(arr[2], out var endTime))
            {
                return (TimeSpan.FromMilliseconds(startTime), TimeSpan.FromMilliseconds(endTime));
            }
        }
        else if (TimeCodeFormat1.IsMatch(name) || TimeCodeFormat1WithExtension.IsMatch(name) || TimeCodeFormat2.IsMatch(name))
        {
            var arr = name.Replace("__", "_").Split('_');
            if (arr.Length >= 8)
            {
                try
                {
                    var start = new TimeSpan(0, int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]), int.Parse(arr[3]));
                    var end = new TimeSpan(0, int.Parse(arr[4]), int.Parse(arr[5]), int.Parse(arr[6]), int.Parse(arr[7]));
                    return (start, end);
                }
                catch
                {
                    // Ignore parsing errors and return default time codes
                }
            }
        }

        return (TimeSpan.Zero, TimeSpan.Zero);
    }   
}
