using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageVideo
{
    public LanguageBurnIn BurnIn { get; set; } = new();
    public LanguageTransparentVideo VideoTransparent { get; set; } = new();
    public LanguageAudioToText AudioToText { get; set; } = new();
    public LanguageTextToSpeech TextToSpeech { get; set; } = new();
    public LanguageShotChanges ShotChanges { get; set; } = new();
    public LanguageVideoOcr VideoOcr { get; set; } = new();
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
    public string AddRemoveEmbeddedSubtitlesMp4Title { get; set; }
    public string AddCurrentSubtitle { get; set; }
    public string EmbeddedTrackColumnNew { get; set; }
    public string EmbeddedTrackNoSubtitlesFoundTitle { get; set; }
    public string EmbeddedTrackNoSubtitlesFoundMessage { get; set; }
    public string EmbeddedTrackNoTracksTitle { get; set; }
    public string EmbeddedTrackNoTracksMessage { get; set; }
    public string EmbeddedTrackUnableToGenerateTitle { get; set; }
    public string EmbeddedTrackUnableToGenerateMessage { get; set; }
    public string EmbeddedTrackPreviewUnavailableTitle { get; set; }
    public string EmbeddedTrackPreviewUnavailableMessage { get; set; }
    public string EmbeddedTrackGeneratingVideoXY { get; set; }
    public string EmbeddedTrackGeneratingVideo { get; set; }
    public string Mp4FilesFilter { get; set; }
    public string TitleOrLanguage { get; set; }
    public string ViewMatroskaTrackX { get; set; }
    public string ResolutionSeparator { get; set; }
    public string OpenFromUrlTitle { get; set; }
    public string OpenFromUrlOpenOnline { get; set; }
    public string OpenFromUrlOpenOnlineDescription { get; set; }
    public string OpenFromUrlOpenOnlineNote { get; set; }
    public string OpenFromUrlDownloadAndOpen { get; set; }
    public string OpenFromUrlDownloadAndOpenDescription { get; set; }
    public string OpenFromUrlDownloadAndOpenNote { get; set; }
    public string OpenFromUrlMissing { get; set; }
    public string OpenFromUrlDownloadingTitle { get; set; }
    public string OpenFromUrlSaveAs { get; set; }
    public string OpenFromUrlDownloadSubtitles { get; set; }
    public string PickOnlineSubtitleTitle { get; set; }
    public string PickOnlineSubtitleFetching { get; set; }
    public string PickOnlineSubtitleNoneFound { get; set; }
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
        AddRemoveEmbeddedSubtitlesMp4Title = "Add/remove embedded subtitles (MP4)";
        AddCurrentSubtitle = "Add current subtitle";
        EmbeddedTrackColumnNew = "New";
        EmbeddedTrackNoSubtitlesFoundTitle = "No subtitles found";
        EmbeddedTrackNoSubtitlesFoundMessage = "The selected subtitle file does not contain any subtitles.";
        EmbeddedTrackNoTracksTitle = "No tracks added";
        EmbeddedTrackNoTracksMessage = "Add one or more subtitle tracks, or load a video that already has embedded subtitles.";
        EmbeddedTrackUnableToGenerateTitle = "Unable to generate video";
        EmbeddedTrackUnableToGenerateMessage = "Output video file not generated: {0}{1}Parameters: {2}";
        EmbeddedTrackPreviewUnavailableTitle = "Preview not available";
        EmbeddedTrackPreviewUnavailableMessage = "Could not extract the selected subtitle stream. The codec may not be a text format that ffmpeg can convert to SRT (e.g. bitmap-based subtitles).";
        EmbeddedTrackGeneratingVideoXY = "Generating video... {0}%     {1}";
        EmbeddedTrackGeneratingVideo = "Generating video...";
        Mp4FilesFilter = "MP4 files";
        TitleOrLanguage = "Title/language";
        ViewMatroskaTrackX = "View Matroska track - {0}";
        ResolutionSeparator = "x";
        OpenFromUrlTitle = "Open video file from URL";
        OpenFromUrlOpenOnline = "Open online";
        OpenFromUrlOpenOnlineDescription = "Stream the video directly. Fastest start.";
        OpenFromUrlOpenOnlineNote = "Speech-to-text and waveform spikes are not available in this mode.";
        OpenFromUrlDownloadAndOpen = "Download and open";
        OpenFromUrlDownloadAndOpenDescription = "Save the video locally, then open it.";
        OpenFromUrlDownloadAndOpenNote = "Required for speech-to-text and waveform spikes.";
        OpenFromUrlMissing = "Please enter a video URL first.";
        OpenFromUrlDownloadingTitle = "Downloading video";
        OpenFromUrlSaveAs = "Save video as";
        OpenFromUrlDownloadSubtitles = "Also download subtitles";
        PickOnlineSubtitleTitle = "Pick subtitle to download";
        PickOnlineSubtitleFetching = "Downloading subtitles...";
        PickOnlineSubtitleNoneFound = "No subtitles found for this URL.";
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