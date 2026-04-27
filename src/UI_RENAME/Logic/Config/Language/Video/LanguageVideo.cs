using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageVideo
{
    public LanguageBurnIn BurnIn { get; set; } = new();
    public LanguageTransparentVideo VideoTransparent { get; set; } = new();
    public LanguageAudioToText AudioToText { get; set; } = new();
    public LanguageTextToSpeech TextToSpeech { get; set; } = new();
    public LanguageShotChanges ShotChanges { get; set; } = new();
    public string GoToVideoPosition { get; set; }
    public string GenerateBlankVideoDotDotDot { get; set; }
    public string GenerateBlankVideoTitle { get; set; }
    public string ReEncodeVideoForBetterSubtitlingTitle { get; set; }
    public string ReEncodeVideoForBetterSubtitlingDotDotDot { get; set; }
    public string OpenSecondarySubtitleOnVideoPlayerDotDotDot { get; set; }
    public string OpenSecondarySubtitleOnVideoPlayer { get; set; }
    public string RemoveSecondarySubtitleOnVideoPlayer { get; set; }
    public string CutVideoTitle { get; set; }
    public string CutVideoDotDotDot { get; set; }
    public string EmbedSubtitlesDotDotDot { get; set; }
    public string GenerateTimeCodes { get; set; }
    public string CheckeredImage { get; set; }
    public string SaveVideoAsTitle { get; set; }
    public string PromptForFfmpegParamsAndGenerate { get; set; }
    public string CutVideoCutSegments { get; set; }
    public string CutVideoMergeSegments { get; set; }
    public string CutVideoSplitSegments { get; set; }
    public string CutVideoType { get; set; }
    public string MpvRenderAuto { get; set; }
    public string MpvRenderNative { get; set; }
    public string MpvRenderOpenGl { get; set; }
    public string MpvRenderSoftware { get; set; }
    public string ImportCurrentSubtitle { get; set; }
    public string AddRemoveEmbeddedSubtitlesTitle { get; set; }
    public string AddCurrentSubtitle { get; set; }
    public string TitleOrLanguage { get; set; }
    public string ViewMatroskaTrackX { get; set; }
    public string ResolutionSeparator { get; set; }
    public string OpenFromUrlTitle { get; set; }
    public string ToggleCurrentSubtitleWhilePlaying { get; set; }
    public string OnlyMkvCanSupportEmbeddedSubtitleEditing { get; set; }
    public string ReEncodeInfo { get; set; }
    public string ReEncodeGeneratingVideoX { get; set; }
    public string ReEncodeGeneratingVideoXofY { get; set; }
    public string ReEncodeUnableToGenerateVideo { get; set; }
    public string ReEncodeOutputVideoFileNotGenerated { get; set; }
    public string ReEncodeGeneratingDone { get; set; }
    public string ReEncodeGeneratedFilesX { get; set; }
    public string ReEncodeFfmpegParameters { get; set; }

    public LanguageVideo()
    {

        GoToVideoPosition = "Go to video position";
        GenerateBlankVideoTitle = "Generate blank video";
        GenerateBlankVideoDotDotDot = "Generate blank video...";
        ReEncodeVideoForBetterSubtitlingTitle = "Re-encode video for better subtitling";
        ReEncodeVideoForBetterSubtitlingDotDotDot = "Re-encode video for better subtitling...";
        OpenSecondarySubtitleOnVideoPlayer = "Secondary subtitle (on video player)";
        OpenSecondarySubtitleOnVideoPlayerDotDotDot = "Secondary subtitle on video player, open...";
        RemoveSecondarySubtitleOnVideoPlayer = "Secondary subtitle on video player, remove";
        CutVideoTitle = "Cut video";
        CutVideoDotDotDot = "Cut video...";
        EmbedSubtitlesDotDotDot = "Add/remove embedded subtitles...";
        GenerateTimeCodes = "Generate time codes";
        CheckeredImage = "Checkered image";
        SaveVideoAsTitle = "Save video as";
        PromptForFfmpegParamsAndGenerate = "Prompt for ffmpeg parameters and generate";
        CutVideoCutSegments = "Cut segments";
        CutVideoMergeSegments = "Merge segments";
        CutVideoSplitSegments = "Save segments individually";
        CutVideoType = "Cut type";
        MpvRenderAuto = "Auto";
        MpvRenderNative = "Native";
        MpvRenderOpenGl = "OpenGL";
        MpvRenderSoftware = "Software (slow)";
        ImportCurrentSubtitle = "Import current subtitle";
        AddRemoveEmbeddedSubtitlesTitle = "Add/remove embedded subtitles";
        AddCurrentSubtitle = "Add current subtitle";
        TitleOrLanguage = "Title/language";
        ViewMatroskaTrackX = "View Matroska track - {0}";
        ResolutionSeparator = "x";
        OpenFromUrlTitle = "Open video file from URL";
        ToggleCurrentSubtitleWhilePlaying = "Toggle current subtitle while playing";
        OnlyMkvCanSupportEmbeddedSubtitleEditing = "Only Matroska (.mkv, .webm) files are supported for editing embedded subtitles.";
        ReEncodeInfo = "Re-encoding can make subtitling smoother:" + Environment.NewLine +
                       "• Smaller resolution (high resolutions make subtitling slow)" + Environment.NewLine +
                       "• Re-encode the video to H.264 + yuv420p makes it more compatible" + Environment.NewLine +
                       "• Optimized for fast seeking";
        ReEncodeGeneratingVideoX = "Generating video... {0}%     {1}";
        ReEncodeGeneratingVideoXofY = "Generating video {0}/{1}... {2}%     {3}";
        ReEncodeUnableToGenerateVideo = "Unable to generate video";
        ReEncodeOutputVideoFileNotGenerated = "Output video file not generated: {0}" + Environment.NewLine + "Parameters: {1}";
        ReEncodeGeneratingDone = "Generating done";
        ReEncodeGeneratedFilesX = "Generated files ({0}):";
        ReEncodeFfmpegParameters = "ffmpeg parameters";
    }
}