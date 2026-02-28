using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Assa;
using System;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeVideo
{
    public SeVideoBurnIn BurnIn { get; set; }
    public SeVideoTransparent Transparent { get; set; }
    public SeVideoTextToSpeech TextToSpeech { get; set; }
    public string VideoPlayer { get; set; }
    public double Volume { get; set; }
    public bool ShowStopButton { get; set; }
    public bool ShowFullscreenButton { get; set; }
    public bool AutoOpen { get; set; }
    public bool OpenSearchParentFolder { get; set; }
    public string CutType { get; set; }
    public string ShowChangesFFmpegArguments { get; set; }
    public bool VideoPlayerDisplayTimeLeft { get; set; }
    public string CutDefaultVideoExtension { get; set; }
    public int MoveVideoPositionCustom1Back { get; set; }
    public int MoveVideoPositionCustom1Forward { get; set; }
    public int MoveVideoPositionCustom2Back { get; set; }
    public int MoveVideoPositionCustom2Forward { get; set; }

    public string MpvPreviewFontName { get; set; }
    public int MpvPreviewFontSize { get; set; }
    public bool MpvPreviewFontBold { get; set; }
    public string MpvPreviewColorPrimary { get; set; }
    public string MpvPreviewColorOutline { get; set; }
    public string MpvPreviewColorShadow { get; set; }
    public decimal MpvPreviewOutlineWidth { get; set; }
    public decimal MpvPreviewShadowWidth { get; set; }
    public int MpvPreviewBorderType { get; set; }

    public SeVideo()
    {
        BurnIn = new();
        Transparent = new();
        TextToSpeech = new();
        VideoPlayer = "mpv-opengl";
        Volume = 60;
        ShowStopButton = true;
        ShowFullscreenButton = true;
        AutoOpen = true;
        OpenSearchParentFolder = true;
        CutType = Features.Video.CutVideo.CutType.MergeSegments.ToString();
        CutDefaultVideoExtension = ".mkv";
        ShowChangesFFmpegArguments = "-i \"{0}\" -vf \"select=gt(scene\\,{1}),showinfo\" -threads 0 -vsync vfr -f null -";
        MoveVideoPositionCustom1Back = 2000; // 2 seconds
        MoveVideoPositionCustom1Forward = 2000; // 2 seconds
        MoveVideoPositionCustom2Back = 5000; // 5 seconds
        MoveVideoPositionCustom2Forward = 5000; // 5 seconds

        MpvPreviewFontName = "Arial";
        MpvPreviewFontSize = 20;
        MpvPreviewFontBold = true;
        MpvPreviewOutlineWidth = 2;
        MpvPreviewShadowWidth = 1;
        MpvPreviewColorPrimary = Color.FromRgb(255, 255, 255).FromColorToHex();
        MpvPreviewColorOutline = Color.FromRgb(0, 0, 0).FromColorToHex();
        MpvPreviewColorShadow = Color.FromRgb(0, 0, 0).FromColorToHex();
        MpvPreviewBorderType = (int)BorderStyleType.Outline;
    }
}