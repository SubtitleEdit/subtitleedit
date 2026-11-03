namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageRemoveBurnedIn
{
    public string Title { get; set; }
    public string RemoveBurnedInDotDotDot { get; set; }
    public string Info { get; set; }
    public string TextMask { get; set; }
    public string Method { get; set; }
    public string MethodRemove { get; set; }
    public string MethodBlackBox { get; set; }
    public string MethodBlur { get; set; }
    public string MethodDelogo { get; set; }
    public string MethodHint { get; set; }
    public string BlurStrength { get; set; }
    public string BlurStrengthHint { get; set; }
    public string EncodingXYZ { get; set; }
    public string RequireOutline { get; set; }
    public string RequireOutlineHint { get; set; }
    public string GrowMask { get; set; }
    public string GrowMaskHint { get; set; }
    public string ExtraWidth { get; set; }
    public string ExtraWidthHint { get; set; }
    public string TemporalConsistency { get; set; }
    public string TemporalConsistencyHint { get; set; }
    public string OutputFile { get; set; }
    public string Quality { get; set; }
    public string QualityHint { get; set; }
    public string Preview { get; set; }
    public string Start { get; set; }
    public string BuildingMasksX { get; set; }
    public string RemovingXYZ { get; set; }
    public string Failed { get; set; }
    public string EngineTooOld { get; set; }
    public string NoTextFound { get; set; }
    public string AbortTitle { get; set; }
    public string AbortMessage { get; set; }

    public string Review { get; set; }
    public string Timeline { get; set; }
    public string TimelineHint { get; set; }
    public string Lines { get; set; }
    public string LinesIncludedXY { get; set; }
    public string ShowResult { get; set; }
    public string ShowResultHint { get; set; }
    public string PreviousLine { get; set; }
    public string NextLine { get; set; }
    public string PreviousFrame { get; set; }
    public string NextFrame { get; set; }
    public string GeneratingWaveform { get; set; }
    public string FindingText { get; set; }
    public string Inpainting { get; set; }
    public string ResultInXMs { get; set; }
    public string NoSubtitleHere { get; set; }
    public string MaskedXPixels { get; set; }
    public string DoneInXY { get; set; }
    public string OpenVideo { get; set; }
    public string ShowInFolder { get; set; }

    public LanguageRemoveBurnedIn()
    {
        Title = "Remove burned-in subtitles";
        RemoveBurnedInDotDotDot = "Remove burned-in subtitles...";
        Info = "Removes the burned-in subtitle text found by OCR with an AI inpainting model (CrispEmbed), black boxes, a blur or ffmpeg's delogo. Only the area around each line is changed, and only while it is shown. Black boxes are loaded into the main window as an ASSA subtitle; the other methods re-encode the video and copy the audio.";
        TextMask = "Text mask";
        Method = "Method";
        MethodRemove = "Remove - AI inpainting";
        MethodBlackBox = "Cover with black boxes (ASSA subtitle)";
        MethodBlur = "Blur (ffmpeg)";
        MethodDelogo = "Delogo - fill from the surrounding pixels (ffmpeg)";
        MethodHint = "AI inpainting gives the cleanest result but is slow. Black boxes are loaded into the main window as an ASSA subtitle. Blur and delogo are fast ffmpeg filters over each line's box";
        BlurStrength = "Blur strength";
        BlurStrengthHint = "Box blur radius in pixels";
        EncodingXYZ = "Encoding... frame {0:#,##0} of {1:#,##0} ({2:0.0} fps)";
        RequireOutline = "Text has a dark outline";
        RequireOutlineHint = "Only remove bright pixels next to a dark outline - keeps bright picture details out of the mask";
        GrowMask = "Grow mask (pixels)";
        GrowMaskHint = "Extra pixels around the text, to cover its outline and soft edges";
        ExtraWidth = "Extra width (pixels)";
        ExtraWidthHint = "Widen the mask this much on the left and right of each line - for outlines or shadows at the first and last letter";
        TemporalConsistency = "Temporal consistency (less flicker)";
        TemporalConsistencyHint = "Reuse the real background from nearby frames and the previous fill while the picture is still";
        OutputFile = "Output file";
        Quality = "Quality (CRF)";
        QualityHint = "x264 constant rate factor: lower is better quality and bigger files (18 is visually lossless)";
        Preview = "Preview";
        Start = "Remove subtitles";
        BuildingMasksX = "Finding subtitle text... {0}";
        RemovingXYZ = "Removing subtitles... frame {0:#,##0} of {1:#,##0} ({2:0.0} fps)";
        Failed = "Removing burned-in subtitles failed.";
        EngineTooOld = "The installed CrispEmbed engine cannot inpaint yet. Download the engine again to update it.";
        NoTextFound = "No subtitle text was found in the scan area while the OCR lines are shown. Try a lower text brightness minimum, or turn off \"Text has a dark outline\".";
        AbortTitle = "Abort?";
        Review = "Review";
        Timeline = "Timeline";
        TimelineHint = "Click to move - drag a box or its edges to fix a line's timing";
        Lines = "Lines";
        LinesIncludedXY = "{0} of {1} included";
        ShowResult = "Show result";
        ShowResultHint = "Show the result instead of the mask (R)";
        PreviousLine = "Previous line";
        NextLine = "Next line";
        PreviousFrame = "Previous frame";
        NextFrame = "Next frame";
        GeneratingWaveform = "Generating waveform...";
        FindingText = "Finding subtitle text...";
        Inpainting = "Inpainting...";
        ResultInXMs = "Result ({0:#,##0} ms)";
        NoSubtitleHere = "No subtitle here";
        MaskedXPixels = "Mask: {0:#,##0} pixels";
        DoneInXY = "Done: {0} ({1})";
        OpenVideo = "Open video";
        ShowInFolder = "Show in folder";
        AbortMessage = "Stop removing burned-in subtitles?";
    }
}
