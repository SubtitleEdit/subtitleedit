using Nikse.SubtitleEdit.Features.Video.BurnIn;
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
    public string AssaSource { get; set; }
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
    public string SlowFontSizeChange { get; set; }
    public string IncreaseFontKerning { get; set; }
    public string ScrollUp { get; set; }
    public string ScrollDown { get; set; }
    public string RotateIn { get; set; }
    public string TiltBounce { get; set; }
    public string FontSizeBounceIn { get; set; }
    public string AssaChangeResolutionOnlyAppliesToAssa { get; set; }
    public string AssaChangeStyleTitle { get; set; }
    public string AssaEmbedFontsTitle { get; set; }
    public string AssaEmbedFontsInfo { get; set; }
    public string AdjustImageColorsTitle { get; set; }
    public string AdjustImageColorsInfo { get; set; }
    public string AssaChangeStyleFromStyle { get; set; }
    public string AssaChangeStyleToStyle { get; set; }
    public string AssaChangeStyleImportStyle { get; set; }
    public string AssaChangeStyleTrimUnusedStyles { get; set; }

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
        SlowFontSizeChange = "Slow font size change";
        IncreaseFontKerning = "Increase font kerning";
        ScrollUp = "Scroll up";
        ScrollDown = "Scroll down";
        RotateIn = "Rotate in";
        TiltBounce = "Tilt bounce";
        FontSizeBounceIn = "Font size bounce in";
        AssaChangeResolutionOnlyAppliesToAssa = "Only applies to Advanced Sub Station Alpha (ASSA) subtitles";
        AssaChangeStyleTitle = "Change style";
        AssaEmbedFontsTitle = "Embed fonts";
        AssaEmbedFontsInfo = "Embeds the font files the subtitle uses (styles and inline font tags) in the [Fonts] section of the output file. Fonts are searched in the Subtitle Edit fonts folder and the system font folders; fonts already embedded are kept. Only applies when the target format is Advanced Sub Station Alpha.";
        AdjustImageColorsTitle = "Adjust image brightness/alpha/color";
        AdjustImageColorsInfo = "Only applies when converting image-based subtitles to an image-based format (e.g. Blu-ray sup to Blu-ray sup).";
        AssaChangeStyleFromStyle = "Change style from";
        AssaChangeStyleToStyle = "to";
        AssaChangeStyleImportStyle = "Import style...";
        AssaChangeStyleTrimUnusedStyles = "Trim unused styles";
        AssaSource = "ASSA source";
    }
}