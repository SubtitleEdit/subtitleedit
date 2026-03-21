using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBatchConvert
{
    public string Title { get; set; }
    public string OneActionsSelected { get; set; }
    public string XActionsSelected { get; set; }
    public string OutputFolderSource { get; set; }
    public string OutputFolderX { get; set; }
    public string EncodingXOverwriteY { get; set; }
    public string TargetFormatSettings { get; set; }
    public string FileNameContainsDotDotDot { get; set; }
    public string TrackLanguageContainsDotDotDot { get; set; }
    public string BatchConvertSettings { get; set; }
    public string AddFormatting { get; set; }
    public string AddItalic { get; set; }
    public string AddBold { get; set; }
    public string AddUnderline { get; set; }
    public string AddAlignment { get; set; }
    public string AddColor { get; set; }
    public string DeleteLinesWithSpecificActorsOrStyles { get; set; }
    public string UseSourceStylesIfPossible { get; set; }
    public string EditStyles { get; set; }
    public string EditProperties { get; set; }
    public string EditAttachments { get; set; }
    public string ErrorsExportedX { get; set; }

    public LanguageBatchConvert()
    {
        Title = "Batch convert";
        BatchConvertSettings = "Batch convert settings";
        OneActionsSelected = "One action selected";
        XActionsSelected = "{0} actions selected";
        OutputFolderSource = " Output folder: Source folder";
        OutputFolderX = " Output folder: {0}";
        EncodingXOverwriteY = "Encoding: {0}, overwrite existing files: {1}";
        TargetFormatSettings = "Target format settings";
        FileNameContainsDotDotDot = "File name contains...";
        TrackLanguageContainsDotDotDot = "Track language contains...";
        AddFormatting = "Add formatting";
        AddItalic = "Add italic";
        AddBold = "Add bold";
        AddUnderline = "Add underline";
        AddAlignment = "Add alignment";
        AddColor = "Add color";
        DeleteLinesWithSpecificActorsOrStyles = "Delete lines with actors or styles (separate multiple by comma)";
        UseSourceStylesIfPossible = "Use source styles if possible";
        EditStyles = "Edit styles";
        EditProperties = "Edit properties";
        EditAttachments = "Edit attachments";
        ErrorsExportedX = "Errors exported: {0}";
    }
}