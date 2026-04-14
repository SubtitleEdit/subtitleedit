using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class VideoPlayerItem : ObservableObject
{
    [ObservableProperty] private string _code;
    [ObservableProperty] private string _name;

    public VideoPlayerItem()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public override string ToString()
    {
        return Name;
    }

    public static List<VideoPlayerItem> ListVideoPlayerItem()
    {
        var result = new List<VideoPlayerItem>();
        result.Add(new VideoPlayerItem { Name = Se.Language.Options.Settings.MpvOpenGl, Code = VideoPlayerName.MpvOpenGl });

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            result.Add(new VideoPlayerItem { Name = Se.Language.Options.Settings.MpvWidRendering, Code = VideoPlayerName.MpvWid });
        }

        result.Add(new VideoPlayerItem { Name = Se.Language.Options.Settings.MpvSoftwareRendering, Code = VideoPlayerName.MpvSw });

        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            result.Add(new VideoPlayerItem { Name = Se.Language.Options.Settings.VlcWidRendering, Code = VideoPlayerName.Vlc });
        }
        
        if (OperatingSystem.IsMacOS())
        {
         //   result.Add(new VideoPlayerItem { Name = "libmpv - Metal", Code = VideoPlayerName.MpvMetal });
        }

        return result;
    }
}