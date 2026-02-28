using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageMainMenu
{
    public string File { get; set; }
    public string New { get; set; }
    public string NewKeepVideo { get; set; }
    public string Open { get; set; }
    public string OpenKeepVideo { get; set; }
    public string OpenOriginal { get; set; }
    public string CloseOriginal { get; set; }
    public string Reopen { get; set; }
    public string ClearRecentFiles { get; set; }
    public string RestoreAutoBackup { get; set; }
    public string Save { get; set; }
    public string SaveAs { get; set; }
    public string OpenContainingFolder { get; set; }
    public string Compare { get; set; }
    public string Statistics { get; set; }
    public string Import { get; set; }
    public string Export { get; set; }
    public string Exit { get; set; }

    public string Edit { get; set; }
    public string Undo { get; set; }
    public string Redo { get; set; }
    public string ShowHistory { get; set; }
    public string Find { get; set; }
    public string FindNext { get; set; }
    public string Replace { get; set; }
    public string MultipleReplace { get; set; }
    public string GoToLineNumber { get; set; }
    public string RightToLeftMode { get; set; }
    public string ModifySelectionDotDotDot { get; set; }


    public string Tools { get; set; }
    public string ToolsSelectedLines { get; set; }
    public string AdjustDurations { get; set; }
    public string ApplyDurationLimits { get; set; }
    public string BatchConvert { get; set; }
    public string BridgeGaps { get; set; }
    public string ApplyMinGap { get; set; }
    public string ChangeCasing { get; set; }
    public string ChangeFormatting { get; set; }
    public string FixCommonErrors { get; set; }
    public string CheckAndFixNetflixErrors { get; set; }
    public string MakeEmptyTranslationFromCurrentSubtitle { get; set; }
    public string MergeLinesWithSameText { get; set; }
    public string MergeLinesWithSameTimeCodes { get; set; }
    public string SplitBreakLongLines { get; set; }
    public string MergeShortLines { get; set; }
    public string RemoveTextForHearingImpaired { get; set; }
    public string JoinSubtitles { get; set; }
    public string SplitSubtitle { get; set; }

    public string AssaTools { get; set; }
    public string AssaProgressBar { get; set; }
    public string AssaChangeResolution { get; set; }
    public string AssaGenerateBackground { get; set; }
    public string AssaApplyCustomOverrideTags { get; set; }
    public string AssaSetPosition { get; set; }
    public string AssaImageColorPicker { get; set; }
    public string AssaDraw { get; set; }
    public string AssaStyles { get; set; }
    public string AssaProperties { get; set; }
    public string AssaAttachments { get; set; }


    public string SpellCheckTitle { get; set; }
    public string SpellCheck { get; set; }
    public string FindDoubleWords { get; set; }
    public string AddNameToNamesList { get; set; }
    public string GetDictionaries { get; set; }

    public string Video { get; set; }
    public string OpenVideo { get; set; }
    public string OpenVideoFromUrl { get; set; }
    public string CloseVideoFile { get; set; }
    public string AudioTracks { get; set; }
    public string SpeechToText { get; set; }
    public string TextToSpeech { get; set; }
    public string SetVideoOffset { get; set; }
    public string UpdateVideoOffsetX { get; set; }
    public string SmpteTiming { get; set; }
    public string GenerateBurnIn { get; set; }
    public string GenerateTransparent { get; set; }
    public string GenerateImportShotChanges { get; set; }
    public string ListShotChanges { get; set; }
    public string UndockVideoControls { get; set; }
    public string DockVideoControls { get; set; }

    public string Synchronization { get; set; }
    public string AdjustAllTimes { get; set; }
    public string ChangeFrameRate { get; set; }
    public string ChangeSpeed { get; set; }
    public string VisualSync { get; set; }


    public string Options { get; set; }
    public string Settings { get; set; }
    public string Shortcuts { get; set; }
    public string WordLists { get; set; }
    public string ChooseLanguage { get; set; }

    public string Translate { get; set; }
    public string AutoTranslate { get; set; }
    public string TranslateViaCopyPaste { get; set; }

    public string HelpTitle { get; set; }
    public string Help { get; set; }
    public string About { get; set; }
    public string FixRightToLeftViaUnicodeControlCharacters { get; set; }
    public string RemoveUnicodeControlCharacters { get; set; }
    public string ReverseRightToLeftStartEnd { get; set; }
    public string PointSync { get; set; }
    public string PointSyncViaOther { get; set; }
    public string SortSubtitles { get; set; }
    public string SetLayer { get; set; }
    public string FilterLayersForDisplayDotDotDot { get; set; }

    public LanguageMainMenu()
    {
        File = "_File";
        New = "_New";
        NewKeepVideo = "New (keep _video)";
        Open = "_Open...";
        OpenKeepVideo = "Open (_keep video)...";
        OpenOriginal = "Open ori_ginal...";
        CloseOriginal = "_Close original";
        Reopen = "_Reopen...";
        ClearRecentFiles = "_Clear recent files";
        RestoreAutoBackup = "Restore auto-_backup...";
        Save = "_Save";
        SaveAs = "Save _as...";
        Compare = "Com_pare...";
        OpenContainingFolder = "Open containing _folder";
        Statistics = "Stat_istics...";
        Import = "_Import";
        Export = "_Export";
        Exit = "E_xit";

        Edit = "_Edit";
        Undo = "_Undo";
        Redo = "Re_do";
        ShowHistory = "_Show history for undo...";
        Find = "_Find...";
        FindNext = "Find _next";
        Replace = "_Replace...";
        MultipleReplace = "_Multiple replace...";
        GoToLineNumber = "_Go to line number...";
        RightToLeftMode = "R_ight-to-left mode";
        ModifySelectionDotDotDot = "Modify _selection...";

        Tools = "_Tools";
        ToolsSelectedLines = "_Tools (selected lines)";
        AdjustDurations = "_Adjust durations...";
        ApplyDurationLimits = "Apply duration _limits...";
        FixCommonErrors = "_Fix common errors...";
        CheckAndFixNetflixErrors = "Check and fix Netfli_x errors...";
        MakeEmptyTranslationFromCurrentSubtitle = "Make new _empty translation from current subtitle";
        MergeLinesWithSameText = "_Merge lines with same text...";
        MergeLinesWithSameTimeCodes = "Merge lines with same time codes...";
        SplitBreakLongLines = "Split/rebalance long lines...";
        MergeShortLines = "Merge short lines...";
        RemoveTextForHearingImpaired = "_Remove text for hearing impaired...";
        ChangeCasing = "_Change casing...";
        ChangeFormatting = "Change formatting...";
        BridgeGaps = "Bridge _gaps...";
        ApplyMinGap = "Apply min. gap between subtitles...";
        BatchConvert = "_Batch convert...";
        JoinSubtitles = "_Join subtitles...";
        SplitSubtitle = "_Split subtitle...";

        AssaTools = "_ASSA tools";
        AssaChangeResolution = "Change _resolution...";
        AssaGenerateBackground = "Generate background _boxes...";
        AssaProgressBar = "Generate _progress bar...";
        AssaApplyCustomOverrideTags = "Apply _override tags...";
        AssaSetPosition = "_Set position...";
        AssaImageColorPicker = "_Image color picker...";
        AssaDraw = "_Draw...";
        AssaStyles = "S_tyles...";
        AssaProperties = "P_roperties...";
        AssaAttachments = "_Attachments...";

        SpellCheckTitle = "_Spell check";
        FindDoubleWords = "_Find double words...";
        AddNameToNamesList = "_Add name to names list...";
        SpellCheck = "_Spell check...";
        GetDictionaries = "_Get dictionaries...";

        Video = "_Video";
        OpenVideo = "_Open video...";
        OpenVideoFromUrl = "Open video from _URL...";
        CloseVideoFile = "_Close video file";
        AudioTracks = "_Audio tracks";
        SpeechToText = "_Speech to text...";
        TextToSpeech = "_Text to speech...";
        UndockVideoControls = "_Undock video controls";
        ListShotChanges = "List s_hot changes...";
        GenerateImportShotChanges = "Generate/import s_hot changes...";
        DockVideoControls = "_Dock video controls";
        SetVideoOffset = "Set video offset...";
        UpdateVideoOffsetX = "Update video offset from {0}...";
        SmpteTiming = "SMPTE timing (non-integer frame rate)";
        GenerateBurnIn = "Generate video with _burned-in subtitles...";
        GenerateTransparent = "_Generate transparent video with subtitles...";

        Synchronization = "S_ynchronization";
        AdjustAllTimes = "_Adjust all times...";
        VisualSync = "_Visual sync...";
        PointSync = "_Point sync...";
        PointSyncViaOther = "Point sync via _other subtitle...";
        ChangeFrameRate = "Change _frame rate...";
        ChangeSpeed = "Change _speed...";

        Options = "_Options";
        Settings = "_Settings...";
        Shortcuts = "S_hortcuts...";
        WordLists = "_Word lists...";
        ChooseLanguage = "_Choose UI language...";

        Translate = "Tr_anslate";
        AutoTranslate = "_Auto-translate...";
        TranslateViaCopyPaste = "Auto-translate via _copy-paste...";

        HelpTitle = "_Help";
        Help = "_Help...";
        About = "_About...";

        FixRightToLeftViaUnicodeControlCharacters = "Fix RTL via Unicode control chars (selected lines)";
        RemoveUnicodeControlCharacters = "Remove Unicode control chars (selected lines)";
        ReverseRightToLeftStartEnd = "Reverse RTL start/end (selected lines)";
        SortSubtitles = "_Sort subtitles...";
        SetLayer = "Set layer...";
        FilterLayersForDisplayDotDotDot = "Filter layers for display...";
    }
}