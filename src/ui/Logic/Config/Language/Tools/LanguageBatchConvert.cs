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
    public string KeepSourceEmbeddedFonts { get; set; }
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
    public string AssaChangeStylePropertiesTitle { get; set; }
    public string AssaChangeStylePropertiesInfo { get; set; }
    public string AssaChangeStylePropertiesSetSpacing { get; set; }
    public string AssaChangeStylePropertiesSetAlignment { get; set; }
    public string ConvertColorsToDialogTitle { get; set; }
    public string ConvertColorsToDialogRemoveColorTags { get; set; }
    public string ConvertColorsToDialogAddNewLines { get; set; }
    public string ConvertColorsToDialogReBreakLines { get; set; }
    public string SnapTimeCodesToFramesInfo { get; set; }
    public string AddFolderDotDotDot { get; set; }
    public string AddFolderRecursiveDotDotDot { get; set; }
    public string SelectFolderToConvert { get; set; }
    public string IncludeSubfolders { get; set; }
    public string VideoFilesWhenAddingFolder { get; set; }
    public string VideoFilesAsk { get; set; }
    public string VideoFilesInclude { get; set; }
    public string VideoFilesSkip { get; set; }
    public string FolderContainsXVideoFiles { get; set; }
    public string AddVideoFiles { get; set; }
    public string SkipVideoFiles { get; set; }
    public string DoNotAskAgainVideoFiles { get; set; }
    public string SettingsSectionOutput { get; set; }
    public string SettingsSectionAddingFiles { get; set; }
    public string SettingsSectionImageBasedSubtitles { get; set; }
    public string SettingsSectionOther { get; set; }
    public string KeepSourceFileTimestamp { get; set; }
    public string PreventSleepWhileConverting { get; set; }
    public string ScanningFolderX { get; set; }
    public string TransportStreamSettings { get; set; }
    public string TransportStreamSettingsDotDotDot { get; set; }
    public string TransportStreamSettingsInfo { get; set; }
    public string TransportStreamOverrideXPosition { get; set; }
    public string TransportStreamOverrideYPosition { get; set; }
    public string TransportStreamOverrideVideoSize { get; set; }
    public string TransportStreamBottomMargin { get; set; }
    public string TransportStreamFileNameEnding { get; set; }
    public string TransportStreamFileNameEndingInfo { get; set; }
    public string TransportStreamOnlyTeletext { get; set; }
    public string TransportStreamGetSizeFromVideo { get; set; }
    public string TwoLetterLanguageCodeUppercase { get; set; }
    public string ThreeLetterLanguageCodeUppercase { get; set; }
    public string FormatContainsDotDotDot { get; set; }
    public string ExtensionIsDotDotDot { get; set; }
    public string StatusIsError { get; set; }
    public string ForcedTracksOnly { get; set; }
    public string XConvertedYFailedInZ { get; set; }
    public string XNotProcessed { get; set; }
    public string RemoveUnicodeControlCharactersTitle { get; set; }
    public string RemoveUnicodeControlCharactersInfo { get; set; }
    public string Preset { get; set; }
    public string SavePresetDotDotDot { get; set; }
    public string DeletePreset { get; set; }
    public string PresetName { get; set; }
    public string DeletePresetX { get; set; }

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
        UseSourceStylesIfPossible = "Use source styles if possible (the header below is then only used for non-ASSA input)";
        KeepSourceEmbeddedFonts = "Keep source embedded fonts (footer)";
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
        AssaChangeStylePropertiesTitle = "Change style properties";
        AssaChangeStylePropertiesInfo = "Changes the chosen fields in every style in the file, leaving the styles themselves alone.";
        AssaChangeStylePropertiesSetSpacing = "Set spacing to";
        AssaChangeStylePropertiesSetAlignment = "Set alignment to";
        AssaSource = "ASSA source";
        ConvertColorsToDialogTitle = "Convert colors to dialog";
        ConvertColorsToDialogRemoveColorTags = "Remove color tags";
        ConvertColorsToDialogAddNewLines = "Add new lines";
        ConvertColorsToDialogReBreakLines = "Re-break lines";
        SnapTimeCodesToFramesInfo = "Rounds every start and end time to the nearest frame. The frame rate is read from a video file with the same name as the subtitle file, unless a fixed frame rate is chosen.";
        AddFolderDotDotDot = "Add folder...";
        AddFolderRecursiveDotDotDot = "Add folder recursive...";
        SelectFolderToConvert = "Select folder with files to convert";
        IncludeSubfolders = "Include subfolders when adding a folder";
        VideoFilesWhenAddingFolder = "Video files (mkv, mp4, ts...) when adding a folder";
        VideoFilesAsk = "Ask";
        VideoFilesInclude = "Add their subtitle tracks";
        VideoFilesSkip = "Skip";
        FolderContainsXVideoFiles = "The folder contains {0} video file(s) (mkv, mp4, ts...).\n\nAdd their embedded subtitle tracks too, or only the subtitle files?";
        AddVideoFiles = "Add video files";
        SkipVideoFiles = "Subtitle files only";
        DoNotAskAgainVideoFiles = "Do not ask again (change in Batch convert settings)";
        SettingsSectionOutput = "Output";
        SettingsSectionAddingFiles = "Adding files";
        SettingsSectionImageBasedSubtitles = "Image-based subtitles (OCR)";
        SettingsSectionOther = "Other";
        KeepSourceFileTimestamp = "Keep source file date/time on output files";
        PreventSleepWhileConverting = "Prevent computer from sleeping while converting";
        ScanningFolderX = "Scanning {0}...";
        TransportStreamSettings = "Transport Stream settings";
        TransportStreamSettingsDotDotDot = "Transport Stream settings...";
        TransportStreamSettingsInfo = "Applies to subtitle tracks extracted from Transport Stream files (.ts/.m2ts). Position and video size only affect DVB image tracks exported to an image based format.";
        TransportStreamOverrideXPosition = "Override original X position";
        TransportStreamOverrideYPosition = "Override original Y position";
        TransportStreamOverrideVideoSize = "Override original video size";
        TransportStreamBottomMargin = "Bottom margin";
        TransportStreamFileNameEnding = "File name ending";
        TransportStreamFileNameEndingInfo = "Added before the extension for each extracted track. Leave empty to use the regular language post fix.";
        TransportStreamOnlyTeletext = "Only teletext";
        TransportStreamGetSizeFromVideo = "Get size from video...";
        TwoLetterLanguageCodeUppercase = "Two-letter language code (uppercase)";
        ThreeLetterLanguageCodeUppercase = "Three-letter language code (uppercase)";
        FormatContainsDotDotDot = "Format contains...";
        ExtensionIsDotDotDot = "Extension is...";
        StatusIsError = "Status is error";
        ForcedTracksOnly = "Forced tracks only";
        XConvertedYFailedInZ = "{0:#,###,##0} converted, {1:#,###,##0} failed in {2}";
        XNotProcessed = "{0:#,###,##0} not processed";
        RemoveUnicodeControlCharactersTitle = "Remove Unicode control characters";
        RemoveUnicodeControlCharactersInfo = "Removes the invisible Unicode direction control characters (LRM, RLM, LRE, RLE, PDF, LRO and RLO) from the text, and replaces no-break spaces with normal spaces.";
        Preset = "Preset";
        SavePresetDotDotDot = "Save preset...";
        DeletePreset = "Delete preset";
        PresetName = "Preset name";
        DeletePresetX = "Delete preset \"{0}\"?";
    }
}