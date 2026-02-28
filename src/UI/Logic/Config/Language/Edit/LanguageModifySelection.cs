namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageModifySelection
{

    public string Title { get; set; }


    public string Contains { get; set; }
    public string StartsWith { get; set; }
    public string EndsWith { get; set; }
    public string NotContains { get; set; }
    public string AllUppercase { get; set; }
    public string Odd { get; set; }
    public string Even { get; set; }
    public string DurationLessThan { get; set; }
    public string DurationGreaterThan { get; set; }
    public string CpsLessThan { get; set; }
    public string CpsGreaterThan { get; set; }
    public string LengthLessThan { get; set; }
    public string LengthGreaterThan { get; set; }
    public string ExactlyOneLine { get; set; }
    public string ExactlyTwoLines { get; set; }
    public string MoreThanTwoLines { get; set; }
    public string Bookmarked { get; set; }
    public string BookmarkContains { get; set; }
    public string BlankLines { get; set; }
    public string SelectionNew { get; set; }
    public string SelectionAdd { get; set; }
    public string SelectionSubtract { get; set; }
    public string SelectionIntersect { get; set; }

    public LanguageModifySelection()
    {
        Title = "Modify selection";
        Contains = "Contains";
        StartsWith = "Starts with";
        EndsWith = "Ends with";
        NotContains = "Not contains";
        AllUppercase = "All uppercase";
        Odd = "Odd number";
        Even = "Even number";
        DurationLessThan = "Duration in ms <";
        DurationGreaterThan = "Duration in ms >";
        CpsLessThan = "CPS <";
        CpsGreaterThan = "CPS >";
        LengthLessThan = "Length <";
        LengthGreaterThan = "Length >";
        ExactlyOneLine = "Exactly one line";
        ExactlyTwoLines = "Exactly two lines";
        MoreThanTwoLines = "More than two lines";
        Bookmarked = "Bookmarked";
        BookmarkContains = "Bookmark contains";
        BlankLines = "Blank lines";
        SelectionNew = "New selection";
        SelectionAdd = "Add to selection";
        SelectionSubtract = "Subtract from selection";
        SelectionIntersect = "Intersect with selection";
    }
}