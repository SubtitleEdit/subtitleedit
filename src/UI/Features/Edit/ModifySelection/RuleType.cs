namespace Nikse.SubtitleEdit.Features.Edit.ModifySelection;

public enum RuleType
{
    Contains,
    StartsWith,
    EndsWith,
    NotContains,
    AllUppercase,
    RegEx,
    Odd,
    Even,
    DurationLessThan,
    DurationGreaterThan,
    CpsLessThan,
    CpsGreaterThan,
    LengthLessThan,
    LengthGreaterThan,
    PixelWidthLengthGreaterThan,
    ExactlyOneLine,
    ExactlyTwoLines,
    MoreThanTwoLines,
    Bookmarked,
    BookmarkContains,
    BlankLines,
    Style,
    Actor
}
