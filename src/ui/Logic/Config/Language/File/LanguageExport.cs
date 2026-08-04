namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageExport
{
    public string ExportImagesProfiles { get; set; }
    public string LeftRightMargin { get;  set; }
    public string TopBottomMargin { get; set; }
    public string LineSpacingPercent { get;  set; }
    public string PaddingLeftRight { get; set; }
    public string PaddingTopBottom { get; set; }
    public string PreviewTitle { get; set; }
    public string TitleExportDvdSup { get; set; }
    public string TitleExportVobSub { get; set; }
    public string TitleExportImscImage { get; set; }
    public string TitleExportDostPng { get; set; }
    public string TitleExportFcpImage { get; set; }
    public string TitleExportWebVttThumbnails { get; set; }
    public string TitleExportCavena890 { get; set; }
    public string ExportCavenaTranslatedTitle { get; set; }
    public string ExportCavenaOriginalTitle { get; set; }
    public string ExportCavenaTranslator { get; set; }
    public string ExportCavenaComment { get; set; }
    public string ExportCavenaStartOfProgramme { get; set; }
    public string CustomTextFormatsDotDotDot { get; set; }
    public string PlainTextDotDotDot { get; set; }
    public string CustomTextFormats { get; set; }
    public string TitleExportCustomFormat { get; set; }
    public string EditCustomFormat { get; set; }
    public string NewCustomFormat { get; set; }
    public string DeleteSelectedCustomTextFormatX { get; set; }
    public string TimeCodeFormat { get; set; }
    public string NewLineFormat { get; set; }
    public string PleaseEnterNameForTheCustomFormat { get; set; }
    public string TitleExportPlainText { get; set; }
    public string LineNumbers { get; set; }
    public string ShowLineNumbers { get; set; }
    public string AddNewLineAfterLineNumber { get; set; }
    public string AddNewLineAfterTimeCode { get; set; }
    public string AddNewLineAfterText { get; set; }
    public string AddLineBetweenSubtitles { get; set; }
    public string TitleExportDCinemaInteropPng { get; set; }
    public string TitleExportDCinemaSmpte2014Png { get; set; }
    public string ImageBasedSubtitleSaved { get; set; }

    public LanguageExport()
    {
        ExportImagesProfiles = "Export images profiles";
        LeftRightMargin = "Left/right margin";
        TopBottomMargin = "Top/bottom margin";
        LineSpacingPercent = "Line spacing %";
        PaddingLeftRight = "Padding left/right";
        PaddingTopBottom = "Padding top/bottom";
        PreviewTitle = "Preview - current size: {0}x{1}, target size: {2}x{3}, zoom: {4}%";
        TitleExportDvdSup = "DVD sup (MuxMan/Scenarist)";
        TitleExportVobSub = "VobSub (sub/idx)";
        TitleExportImscImage = "IMSC 1.1 image profile";
        TitleExportDostPng = "DOST/png";
        TitleExportFcpImage = "Final Cut Pro + image";
        TitleExportWebVttThumbnails = "WebVTT png";
        TitleExportCavena890 = "Export Cavena 890";
        ExportCavenaTranslatedTitle = "Translated title";
        ExportCavenaOriginalTitle = "Original title";
        ExportCavenaTranslator = "Translator";
        ExportCavenaComment = "Comment";
        ExportCavenaStartOfProgramme = "Start of programme";
        CustomTextFormatsDotDotDot = "_Custom text formats...";
        PlainTextDotDotDot = "_Plain text...";
        CustomTextFormats = "Custom text formats";
        TitleExportCustomFormat = "Export custom text format";
        EditCustomFormat = "Edit custom text format";
        NewCustomFormat = "New custom text format";
        DeleteSelectedCustomTextFormatX = "Delete selected custom text format \"{0}\"?";
        TimeCodeFormat = "Time code format";
        NewLineFormat = "New line format";
        PleaseEnterNameForTheCustomFormat = "Please enter name for the custom format";
        TitleExportPlainText = "Export plain text";
        LineNumbers = "Line numbers";   
        ShowLineNumbers = "Show line numbers";
        AddNewLineAfterLineNumber = "Add new line after line number";
        AddNewLineAfterTimeCode = "Add new line after time code";
        AddNewLineAfterText = "Add new line after text";
        AddLineBetweenSubtitles = "Add line between subtitles";
        TitleExportDCinemaInteropPng = "D-Cinema interop/png";
        TitleExportDCinemaSmpte2014Png = "D-Cinema SMPTE 2014/png";
        ImageBasedSubtitleSaved = "Image-based subtitle saved";
    }
}