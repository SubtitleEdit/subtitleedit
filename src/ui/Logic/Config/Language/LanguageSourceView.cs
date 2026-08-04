namespace Nikse.SubtitleEdit.Logic.Config.Language;

public class LanguageSourceView
{
    public string DiscardChangesTitle { get; set; }
    public string DiscardChanges { get; set; }
    public string XLinesYSubtitles { get; set; }
    public string XLinesYSubtitlesZErrors { get; set; }
    public string CouldNotParseAsX { get; set; }
    public string SelectedXCharactersYLines { get; set; }
    public string ReplacedXOccurrences { get; set; }
    public string InvalidRegularExpression { get; set; }
    public string MoveLineUp { get; set; }
    public string MoveLineDown { get; set; }
    public string DuplicateLine { get; set; }
    public string DeleteLine { get; set; }
    public string CloseSearch { get; set; }

    public LanguageSourceView()
    {
        DiscardChangesTitle = "Discard changes?";
        DiscardChanges = "The source text has been changed." + System.Environment.NewLine + "Discard the changes?";
        XLinesYSubtitles = "{0} lines, {1} subtitles";
        XLinesYSubtitlesZErrors = "{0} lines, {1} subtitles, {2} error(s)";
        CouldNotParseAsX = "Could not parse as {0}";
        SelectedXCharactersYLines = "Selected: {0} characters in {1} line(s)";
        ReplacedXOccurrences = "Replaced {0} occurrence(s)";
        InvalidRegularExpression = "Invalid regular expression";
        MoveLineUp = "Move line up";
        MoveLineDown = "Move line down";
        DuplicateLine = "Duplicate line";
        DeleteLine = "Delete line";
        CloseSearch = "Close search";
    }
}
