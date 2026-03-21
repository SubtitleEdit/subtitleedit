using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public partial class WhisperJobItem : ObservableObject
{
    [ObservableProperty] private string _inputVideoFileName;
    [ObservableProperty] private string _inputVideoFileNameShort;
    [ObservableProperty] private long _size;
    [ObservableProperty] private string _sizeDisplay;
    [ObservableProperty] private string _status;

    public FfmpegMediaInfo MediaInfo { get; set; }

    public WhisperJobItem(string inputVideoFileName, string status, FfmpegMediaInfo mediaInfo)
    {
        InputVideoFileName = inputVideoFileName;
        if (inputVideoFileName.Length > 75)
        {
            InputVideoFileNameShort = Path.GetFileName(inputVideoFileName);
        }
        else
        {
            InputVideoFileNameShort = inputVideoFileName;
        }

        var fileInfo = new FileInfo(inputVideoFileName);
        Size = fileInfo.Length;

        SizeDisplay = Utilities.FormatBytesToDisplayFileSize(fileInfo.Length);
        Status = status;
        
        MediaInfo = mediaInfo;
    }
}