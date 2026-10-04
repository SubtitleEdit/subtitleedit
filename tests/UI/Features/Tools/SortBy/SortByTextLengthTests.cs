using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Tools.SortBy;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Tools.SortBy;

/// <summary>
/// "Text - total length" / "Text - single line max length" sorts counted ASSA override blocks as
/// text, so a typesetting sign with one visible character sorted above real dialogue (#15669).
/// The sort keys must count like the "Total chars" / "Single line length" labels.
/// </summary>
public class SortByTextLengthTests
{
    private const string TaggedSign =
        "{\\c&H000000&\\fay-0.06\\fnManjiro'sHw21\\blur0.9\\fs1.001\\fscx3300.82\\fscy3300.82\\b0\\pos(933.439,792.84)\\frz353.6}A";

    [AvaloniaFact]
    public void TotalLength_IgnoresAssaOverrideBlock()
    {
        Assert.Equal(1, SortByViewModel.GetTextTotalLength(TaggedSign));
        Assert.True(SortByViewModel.GetTextTotalLength(TaggedSign) < SortByViewModel.GetTextTotalLength("Hello there"));
    }

    [AvaloniaFact]
    public void SingleLineMaxLength_IgnoresTags()
    {
        Assert.Equal(1, SortByViewModel.GetTextSingleLineMaxLength(TaggedSign));
        Assert.Equal(5, SortByViewModel.GetTextSingleLineMaxLength("<i>Hi</i>\n{\\an8}Hello"));
    }

    [AvaloniaFact]
    public void TotalLength_MatchesTotalCharsLabel()
    {
        const string text = "{\\an8}<i>Hello</i>\nthere";
        var expected = (int)SubtitleTextInfoHelper.GetTotalLength(SubtitleTextInfoHelper.StripHtml(text));
        Assert.Equal(expected, SortByViewModel.GetTextTotalLength(text));
    }

    [AvaloniaFact]
    public void EmptyText_IsZero()
    {
        Assert.Equal(0, SortByViewModel.GetTextTotalLength(null));
        Assert.Equal(0, SortByViewModel.GetTextSingleLineMaxLength(string.Empty));
    }
}
