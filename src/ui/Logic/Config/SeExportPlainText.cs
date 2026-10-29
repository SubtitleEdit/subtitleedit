using Nikse.SubtitleEdit.UiLogic.Export;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeExportPlainText
{
    public string TextProcessing { get; set; }
    public bool TextRemoveStyling { get; set; }
    public bool ShowLineNumbers { get; set; }
    public bool AddNewLineAfterLineNumber { get; set; }
    public bool ShowTimeCodes { get; set; }
    public bool AddNewLineAfterTimeCode { get; set; }
    public string TimeCodeFormat { get; set; }
    public string TimeCodeSeparator { get; set; }
    public bool AddLineAfterText { get; set; }
    public bool AddLineBetweenSubtitles { get; set; }

    public SeExportPlainText()
    {
        TextProcessing = "None";
        TextRemoveStyling = true;
        ShowLineNumbers = false;
        AddNewLineAfterLineNumber = true;
        ShowTimeCodes = false;
        AddNewLineAfterTimeCode = true;
        TimeCodeFormat = "hh:mm:ss,zzz";
        TimeCodeSeparator = " --> ";
        AddLineAfterText = true;
        AddLineBetweenSubtitles = true;
    }
    public PlainTextOptions ToOptions()
    {
        return new PlainTextOptions
        {
            TextProcessing = TextProcessing == "Merge" ? PlainTextProcessing.Merge :
                             TextProcessing == "Unbreak" ? PlainTextProcessing.Unbreak : PlainTextProcessing.None,
            RemoveStyling = TextRemoveStyling,
            ShowLineNumbers = ShowLineNumbers,
            AddNewLineAfterLineNumber = AddNewLineAfterLineNumber,
            ShowTimeCodes = ShowTimeCodes,
            AddNewLineAfterTimeCode = AddNewLineAfterTimeCode,

            // The dialog stores its "Seconds" entry as the translated display text.
            TimeCodeFormat = TimeCodeFormat == Se.Language.General.Seconds ? PlainTextFormatter.TimeCodeFormatSeconds : TimeCodeFormat,
            TimeCodeSeparator = TimeCodeSeparator,
            AddLineAfterText = AddLineAfterText,
            AddLineBetweenSubtitles = AddLineBetweenSubtitles,
        };
    }
}
