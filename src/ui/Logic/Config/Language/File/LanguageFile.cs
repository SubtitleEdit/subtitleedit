using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.File;

public class LanguageFile
{
    public LanguageEbuSaveOptions EbuSaveOptions { get; set; } = new();
    public LanguageImport Import { get; set; } = new();
    public LanguageExport Export { get; set; } = new();
    public LanguageStatistics Statistics { get; set; } = new();
    public LanguageManualChosenEncoding ManualChosenEncoding { get; set; } = new();
    public LanguageRestoreAutoBackup RestoreAutoBackup { get; set; } = new();
    public LanguageFilePropertiesDCinema PropertiesDCinema { get; set; } = new();
    public LanguageWebVtt WebVtt { get; set; } = new();
    public string Compare { get; set; }
    public string PreviousDifference { get; set; }
    public string NextDifference { get; set; }
    public string SubtitlesNotAlike { get; set; }
    public string XNumberOfDifference { get; set; }
    public string XNumberOfDifferenceAndPercentChanged { get; set; }
    public string XNumberOfDifferenceAndPercentLettersChanged { get; set; }
    public string ShowOnlyDifferences { get; set; }
    public string CompareOnlyInOneFile { get; set; }
    public string CompareTextOrTimeDifference { get; set; }
    public string CompareNumberDifference { get; set; }
    public string IgnoreWhitespace { get; set; }
    public string IgnoreWhitespaceHint { get; set; }
    public string IgnoreFormatting { get; set; }
    public string IgnoreFormattingHint { get; set; }
    public string ShowOnlyDifferencesInText { get; set; }
    public string LoadXFromFile { get; set; }
    public string SaveCompareHtmlTitle { get; set; }
    public string PickMatroskaTrackX { get; set; }
    public string PickTransportStreamTrackX { get; set; }
    public string PickMp4TrackX { get; set; }
    public string RosettaProperties { get; set; }
    public string RosettaFontSize { get; set; }
    public string PropertyTimeBase { get; set; }
    public string PropertyFrameRateMultiplier { get; set; }
    public string PropertyDropMode { get; set; }
    public string PropertyDefaultStyle { get; set; }
    public string PropertyDefaultRegion { get; set; }
    public string PropertyStyleAttributeName { get; set; }
    public string PropertyTimeCodeFormat { get; set; }
    public string PropertyFileExtension { get; set; }
    public string PropertyTopOrigin { get; set; }
    public string PropertyTopExtent { get; set; }
    public string PropertyBottomOrigin { get; set; }
    public string PropertyBottomExtent { get; set; }
    public string XProperties { get; set; }

    public LanguageFile()
    {
        Compare = "Compare";
        PreviousDifference = "Previous difference";
        NextDifference = "Next difference";
        SubtitlesNotAlike = "Subtitles have no similarities";
        XNumberOfDifference = "Number of differences: {0}";
        XNumberOfDifferenceAndPercentChanged = "Number of differences: {0} ({1:0.##}% of words changed)";
        XNumberOfDifferenceAndPercentLettersChanged = "Number of differences: {0} ({1:0.##}% of letters changed)";
        ShowOnlyDifferences = "Only differences";
        CompareOnlyInOneFile = "Only in one file";
        CompareTextOrTimeDifference = "Text/time difference";
        CompareNumberDifference = "Number difference";
        ShowOnlyDifferencesInText = "Only differences in text";
        IgnoreWhitespace = "Ignore whitespace";
        IgnoreWhitespaceHint = "Lines that differ only in spaces, tabs or line breaks do not count as different";
        IgnoreFormatting = "Ignore formatting";
        IgnoreFormattingHint = "Lines that differ only in formatting tags, like <i> or {\\an8}, do not count as different";
        LoadXFromFile = "Load \"{0}\" from file";
        SaveCompareHtmlTitle = "Save compare HTML file";
        PickMatroskaTrackX = "Pick Matroska track - {0}";
        PickTransportStreamTrackX = "Pick transport stream track - {0}";
        PickMp4TrackX = "Pick MP4 track - {0}";
        RosettaProperties = "Timed Text Rosetta IMSC properties";
        RosettaFontSize = "Font size (row height)";
        PropertyTimeBase = "Time base";
        PropertyFrameRateMultiplier = "Frame rate multiplier";
        PropertyDropMode = "Drop mode";
        PropertyDefaultStyle = "Default style";
        PropertyDefaultRegion = "Default region";
        PropertyStyleAttributeName = "Style attribute name";
        PropertyTimeCodeFormat = "Time code format";
        PropertyFileExtension = "File extension";
        PropertyTopOrigin = "Top origin";
        PropertyTopExtent = "Top extent";
        PropertyBottomOrigin = "Bottom origin";
        PropertyBottomExtent = "Bottom extent";
        XProperties = "{0} properties";
    }
}