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
    public string Compare { get; set; }
    public string PreviousDifference { get; set; }
    public string NextDifference { get; set; }
    public string SubtitlesNotAlike { get; set; }
    public string XNumberOfDifference { get; set; }
    public string XNumberOfDifferenceAndPercentChanged { get; set; }
    public string XNumberOfDifferenceAndPercentLettersChanged { get; set; }
    public string ShowOnlyDifferences { get; set; }
    public string IgnoreLineBreaks { get; set; }
    public string IgnoreWhitespace { get; set; }
    public string IgnoreFormatting { get; set; }
    public string OnlyLookForDifferencesInText { get; set; }
    public string CannotCompareWithImageBasedSubtitles { get; set; }
    public string StatisticsTitle { get; set; }
    public string ShowOnlyDifferencesInText { get; set; }
    public string LoadXFromFile { get; set; }
    public string SaveCompareHtmlTitle { get; set; }
    public string PickMatroskaTrackX { get; set; }
    public string RosettaProperties { get; set; }
    public string RosettaFontSize { get; set; }
    public string XProperties { get; set; }

    public LanguageFile()
    {
        StatisticsTitle = "Statistics";
        Compare = "Compare";
        PreviousDifference = "Previous difference";
        NextDifference = "Next difference";
        SubtitlesNotAlike = "Subtitles have no similarities";
        XNumberOfDifference = "Number of differences: {0}";
        XNumberOfDifferenceAndPercentChanged = "Number of differences: {0} ({1:0.##}% of words changed)";
        XNumberOfDifferenceAndPercentLettersChanged = "Number of differences: {0} ({1:0.##}% of letters changed)";
        ShowOnlyDifferences = "Only differences";
        ShowOnlyDifferencesInText = "Only differences in text";
        IgnoreLineBreaks = "Ignore line breaks";
        IgnoreWhitespace = "Ignore whitespace";
        IgnoreFormatting = "Ignore formatting";
        OnlyLookForDifferencesInText = "Only look for differences in text";
        CannotCompareWithImageBasedSubtitles = "Cannot compare with image-based subtitles";
        LoadXFromFile = "Load \"{0}\" from file";
        SaveCompareHtmlTitle = "Save compare HTML file";
        PickMatroskaTrackX = "Pick Matroska track - {0}";
        RosettaProperties = "Timed Text Rosetta IMSC properties";
        RosettaFontSize = "Font size (row height)";
        XProperties = "{0} properties";
    }
}