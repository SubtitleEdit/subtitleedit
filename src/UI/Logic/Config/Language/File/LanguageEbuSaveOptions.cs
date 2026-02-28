namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageEbuSaveOptions
{
    public string Title { get; set; }
    public string GeneralSubtitleInformation { get; set; }
    public string CodePageNumber { get; set; }
    public string DiskFormatCode { get; set; }
    public string DisplayStandardCode { get; set; }
    public string ColorRequiresTeletext { get; set; }
    public string AlignmentRequiresTeletext { get; set; }
    public string TeletextCharsShouldBe38 { get; set; }
    public string CharacterCodeTable { get; set; }
    public string LanguageCode { get; set; }
    public string OriginalProgramTitle { get; set; }
    public string OriginalEpisodeTitle { get; set; }
    public string TranslatedProgramTitle { get; set; }
    public string TranslatedEpisodeTitle { get; set; }
    public string TranslatorsName { get; set; }
    public string SubtitleListReferenceCode { get; set; }
    public string CountryOfOrigin { get; set; }
    public string TimeCodeStatus { get; set; }
    public string TimeCodeStartOfProgramme { get; set; }
    public string RevisionNumber { get; set; }
    public string MaxNoOfDisplayableChars { get; set; }
    public string MaxNumberOfDisplayableRows { get; set; }
    public string DiskSequenceNumber { get; set; }
    public string TotalNumberOfDisks { get; set; }
    public string Import { get; set; }
    public string TextAndTimingInformation { get; set; }
    public string JustificationCode { get; set; }
    public string VerticalPosition { get; set; }
    public string MarginTop { get; set; }
    public string MarginBottom { get; set; }
    public string NewLineRows { get; set; }
    public string Teletext { get; set; }
    public string UseBox { get; set; }
    public string DoubleHeight { get; set; }
    public string Errors { get; set; }
    public string ErrorsX { get; set; }
    public string MaxLengthError { get; set; }
    public string TextUnchangedPresentation { get; set; }
    public string TextLeftJustifiedText { get; set; }
    public string TextCenteredText { get; set; }
    public string TextRightJustifiedText { get; set; }
    public string UseBoxForOneNewLine { get; set; }
    public string DiscSequenceNumber { get; set; }

    public LanguageEbuSaveOptions()
    {
        Title = "EBU save options";
        GeneralSubtitleInformation = "General subtitle information";
        CodePageNumber = "Code page number";
        DiskFormatCode = "Disk format code";
        DisplayStandardCode = "Display standard code";
        ColorRequiresTeletext = "Colors require teletext!";
        AlignmentRequiresTeletext = "Alignment requires teletext!";
        TeletextCharsShouldBe38 = "'Max# of chars per row' for teletext should be 38!";
        CharacterCodeTable = "Character table";
        LanguageCode = "Language code";
        OriginalProgramTitle = "Original program title";
        OriginalEpisodeTitle = "Original episode title";
        TranslatedProgramTitle = "Translated program title";
        TranslatedEpisodeTitle = "Translated episode title";
        TranslatorsName = "Translator's name";
        SubtitleListReferenceCode = "Subtitle list reference code";
        CountryOfOrigin = "Country of origin";
        TimeCodeStatus = "Time code status";
        TimeCodeStartOfProgramme = "Time code: Start of programme";
        RevisionNumber = "Revision number";
        MaxNoOfDisplayableChars = "Max# of chars per row";
        MaxNumberOfDisplayableRows = "Max# of rows";
        DiskSequenceNumber = "Disk sequence number";
        TotalNumberOfDisks = "Total number of disks";
        Import = "Import...";
        TextAndTimingInformation = "Text and timing information";
        JustificationCode = "Justification code";
        VerticalPosition = "Vertical position";
        MarginTop = "Margin top (for top aligned subtitles)";
        MarginBottom = "Margin bottom (for bottom aligned subtitles)";
        NewLineRows = "Number of rows added by a new line";
        Teletext = "Teletext";
        UseBox = "Use box around text";
        DoubleHeight = "Use double height for text";
        Errors = "Errors";
        ErrorsX = "Errors: {0}";
        MaxLengthError = "Line {0} exceeds max length ({1}) by {2}: {3}";
        TextUnchangedPresentation = "Unchanged presentation";
        TextLeftJustifiedText = "Left justified text";
        TextCenteredText = "Centered text";
        TextRightJustifiedText = "Right justified text";
        UseBoxForOneNewLine = "Check 'Use box around text' for only one new-line";
        DiscSequenceNumber = "Disc sequence number";
    }
}