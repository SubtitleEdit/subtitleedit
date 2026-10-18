namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageExport
{
    public string ExportImagesProfiles { get; set; }
    public string LeftRightMargin { get;  set; }
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
    public string TitleExportDvbTeletext { get; set; }
    public string ExportDvbTeletextPageNumber { get; set; }
    public string ExportDvbTeletextSubtitleType { get; set; }
    public string ExportDvbTeletextSubtitleTypeNormal { get; set; }
    public string ExportDvbTeletextSubtitleTypeHearingImpaired { get; set; }
    public string ExportPacCodePage { get; set; }
    public string ExportPacSecondaryCodePage { get; set; }
    public string CustomTextFormatsDotDotDot { get; set; }
    public string PlainTextDotDotDot { get; set; }
    public string DialogueListDotDotDot { get; set; }
    public string TitleExportDialogueList { get; set; }
    public string DialogueListLineBreaksInCell { get; set; }
    public string DialogueListHeaderRow { get; set; }
    public string DialogueListFramesFormat { get; set; }
    public string DialogueListNoColumns { get; set; }
    public string DialogueListEnd { get; set; }
    public string CustomTextFormats { get; set; }
    public string TitleExportCustomFormat { get; set; }
    public string EditCustomFormat { get; set; }
    public string NewCustomFormat { get; set; }
    public string DeleteSelectedCustomTextFormatX { get; set; }
    public string NewLineFormat { get; set; }
    public string PleaseEnterNameForTheCustomFormat { get; set; }
    public string TitleExportPlainText { get; set; }
    public string LineNumbers { get; set; }
    public string ShowLineNumbers { get; set; }
    public string AddNewLineAfterLineNumber { get; set; }
    public string AddNewLineAfterTimeCode { get; set; }
    public string AddNewLineAfterText { get; set; }
    public string AddLineBetweenSubtitles { get; set; }
    public string MergeAllLines { get; set; }
    public string TitleExportDCinemaInteropPng { get; set; }
    public string TitleExportDCinemaSmpte2014Png { get; set; }
    public string TitleExportDCinemaSmpte2014Mxf { get; set; }
    public string TitleExportDCinemaSmpte2014PngMxf { get; set; }
    public string ImageBasedSubtitleSaved { get; set; }
    public string FullFrameImage { get; set; }
    public string FullFrameImageHint { get; set; }
    public string Stereo3D { get; set; }
    public string Stereo3DHint { get; set; }
    public string HalfSideBySide { get; set; }
    public string HalfTopBottom { get; set; }
    public string Depth3D { get; set; }
    public string Depth3DHint { get; set; }
    public string Plane3D { get; set; }
    public string Plane3DHint { get; set; }
    public string OpenPlane3DTitle { get; set; }
    public string UnableToLoadPlane3DX { get; set; }
    public string Plane3DXDepthYToZ { get; set; }
    public string Plane3DXNoDepth { get; set; }
    public string TextEffect { get; set; }
    public string TextEffectSettingsTitle { get; set; }
    public string TextEffectStrength { get; set; }
    public string TextEffectLetterSpacing { get; set; }
    public string TextEffectArcBend { get; set; }
    public string TextEffectWave { get; set; }
    public string TextEffectSoftShadow { get; set; }
    public string TextEffectGradientGold { get; set; }
    public string TextEffectDoubleOutline { get; set; }
    public string TextEffectNeonGlow { get; set; }
    public string TextEffectExtrude3D { get; set; }
    public string TextEffectChrome { get; set; }
    public string TextEffectFire { get; set; }
    public string TextEffectComic { get; set; }
    public string TextEffectRetro80s { get; set; }
    public string TextEffectAnaglyph3D { get; set; }
    public string TextEffectIce { get; set; }
    public string TextEffectEmboss { get; set; }
    public string TextEffectHollow { get; set; }
    public string TextEffectMarble { get; set; }
    public string TextEffectWood { get; set; }
    public string TextEffectLava { get; set; }
    public string TextEffectBrushedSteel { get; set; }
    public string TextEffectCandyCane { get; set; }
    public string TextEffectRainbow { get; set; }
    public string TextEffectPolkaDots { get; set; }

    public LanguageExport()
    {
        ExportImagesProfiles = "Export images profiles";
        LeftRightMargin = "Left/right margin";
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
        TitleExportDvbTeletext = "DVB teletext (Manzanita)";
        ExportDvbTeletextPageNumber = "Teletext page number";
        ExportDvbTeletextSubtitleType = "Subtitle type";
        ExportDvbTeletextSubtitleTypeNormal = "Normal";
        ExportDvbTeletextSubtitleTypeHearingImpaired = "Hearing impaired";
        ExportPacCodePage = "Choose PAC code page";
        ExportPacSecondaryCodePage = "Secondary code page";
        CustomTextFormatsDotDotDot = "_Custom text formats...";
        PlainTextDotDotDot = "_Plain text...";
        DialogueListDotDotDot = "Excel dialogue list (.xlsx)...";
        TitleExportDialogueList = "Export Excel dialogue list";
        DialogueListLineBreaksInCell = "Keep line breaks inside the cell (off: join lines with \" | \")";
        DialogueListHeaderRow = "Header row";
        DialogueListFramesFormat = "hh:mm:ss:ff (frames)";
        DialogueListNoColumns = "Choose at least one column.";
        DialogueListEnd = "End";
        CustomTextFormats = "Custom text formats";
        TitleExportCustomFormat = "Export custom text format";
        EditCustomFormat = "Edit custom text format";
        NewCustomFormat = "New custom text format";
        DeleteSelectedCustomTextFormatX = "Delete selected custom text format \"{0}\"?";
        NewLineFormat = "New line format";
        PleaseEnterNameForTheCustomFormat = "Please enter name for the custom format";
        TitleExportPlainText = "Export plain text";
        LineNumbers = "Line numbers";   
        ShowLineNumbers = "Show line numbers";
        AddNewLineAfterLineNumber = "Add new line after line number";
        AddNewLineAfterTimeCode = "Add new line after time code";
        AddNewLineAfterText = "Add new line after text";
        AddLineBetweenSubtitles = "Add line between subtitles";
        MergeAllLines = "Merge all lines";
        TitleExportDCinemaInteropPng = "D-Cinema interop/png";
        TitleExportDCinemaSmpte2014Png = "D-Cinema SMPTE 2014/png";
        TitleExportDCinemaSmpte2014Mxf = "D-Cinema SMPTE 2014 (MXF)";
        TitleExportDCinemaSmpte2014PngMxf = "D-Cinema SMPTE 2014/png (MXF)";
        ImageBasedSubtitleSaved = "Image-based subtitle saved";
        FullFrameImage = "Full frame image";
        FullFrameImageHint = "Make each image the size of the video frame, with the subtitle in its place";
        Stereo3D = "3D";
        Stereo3DHint = "For 3D video: draw the subtitle once in each eye's half of the frame";
        HalfSideBySide = "Half side-by-side";
        HalfTopBottom = "Half top/bottom";
        Depth3D = "Depth";
        Depth3DHint = "How far the subtitle stands out of the screen - positive brings it closer, negative pushes it back";
        Plane3D = "3D-Plane";
        Plane3DHint = "Give each subtitle the depth a 3D Blu-ray stores for it, from a 3D-Plane (.ofs) file made by BD3D2MK3D or OFSExtractor. Depth is used where the 3D-Plane has none";
        OpenPlane3DTitle = "Open 3D-Plane file";
        UnableToLoadPlane3DX = "Unable to load 3D-Plane: {0}";
        Plane3DXDepthYToZ = "{0} (depth {1} to {2})";
        Plane3DXNoDepth = "{0} (no depth values)";
        TextEffect = "Text effect";
        TextEffectSettingsTitle = "Text effect settings";
        TextEffectStrength = "Strength";
        TextEffectLetterSpacing = "Letter spacing";
        TextEffectArcBend = "Curve";
        TextEffectWave = "Wave";
        TextEffectSoftShadow = "Soft shadow";
        TextEffectGradientGold = "Gradient (gold)";
        TextEffectDoubleOutline = "Double outline";
        TextEffectNeonGlow = "Neon glow";
        TextEffectExtrude3D = "3D extrude";
        TextEffectChrome = "Chrome";
        TextEffectFire = "Fire";
        TextEffectComic = "Comic book";
        TextEffectRetro80s = "Retro 80s";
        TextEffectAnaglyph3D = "3D glasses";
        TextEffectIce = "Ice";
        TextEffectEmboss = "Emboss";
        TextEffectHollow = "Hollow (outline only)";
        TextEffectMarble = "Marble";
        TextEffectWood = "Wood";
        TextEffectLava = "Lava";
        TextEffectBrushedSteel = "Brushed steel";
        TextEffectCandyCane = "Candy cane";
        TextEffectRainbow = "Rainbow";
        TextEffectPolkaDots = "Polka dots";
    }
}