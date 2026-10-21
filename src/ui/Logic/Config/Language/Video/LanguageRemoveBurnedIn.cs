namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageRemoveBurnedIn
{
    public string Title { get; set; }
    public string RemoveBurnedInDotDotDot { get; set; }
    public string Info { get; set; }
    public string TextMask { get; set; }
    public string Model { get; set; }
    public string ModelLama { get; set; }
    public string ModelMiGan { get; set; }
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
    public string PreviewLine { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
    public string Start { get; set; }
    public string BuildingMasksX { get; set; }
    public string RemovingXYZ { get; set; }
    public string DoneX { get; set; }
    public string Failed { get; set; }
    public string EngineTooOld { get; set; }
    public string NoTextFound { get; set; }
    public string NoTextInPreviewFrame { get; set; }
    public string AbortTitle { get; set; }
    public string AbortMessage { get; set; }

    public LanguageRemoveBurnedIn()
    {
        Title = "Remove burned-in subtitles";
        RemoveBurnedInDotDotDot = "Remove burned-in subtitles...";
        Info = "Paints over the burned-in subtitle text found by OCR with the MI-GAN inpainting model (CrispEmbed). Only the scan area is changed, and only while an OCR line is shown. The video is re-encoded; the audio is copied.";
        TextMask = "Text mask";
        Model = "Model";
        ModelLama = "LaMa - best quality, slower";
        ModelMiGan = "MI-GAN - fast";
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
        PreviewLine = "Line";
        Before = "Before";
        After = "After";
        Start = "Remove subtitles";
        BuildingMasksX = "Finding subtitle text... {0}";
        RemovingXYZ = "Removing subtitles... frame {0:#,##0} of {1:#,##0} ({2:0.0} fps)";
        DoneX = "Burned-in subtitles removed:{0}{1}";
        Failed = "Removing burned-in subtitles failed.";
        EngineTooOld = "The installed CrispEmbed engine cannot inpaint yet. Download the engine again to update it.";
        NoTextFound = "No subtitle text was found in the scan area while the OCR lines are shown. Try a lower text brightness minimum, or turn off \"Text has a dark outline\".";
        NoTextInPreviewFrame = "No subtitle text was found in the scan area for this line.";
        AbortTitle = "Abort?";
        AbortMessage = "Stop removing burned-in subtitles?";
    }
}
