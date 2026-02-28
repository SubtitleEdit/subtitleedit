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

    public LanguageVideo()
    {

        GoToVideoPosition = "Go to video position";
        GenerateBlankVideoTitle = "Generate blank video";
        GenerateBlankVideoDotDotDot = "Generate blank video...";
        ReEncodeVideoForBetterSubtitlingTitle = "Re-encode video for better subtitling";
        ReEncodeVideoForBetterSubtitlingDotDotDot = "Re-encode video for better subtitling...";
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
    }
}