using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

public class FindServiceTests
{
    [Fact]
    public void RegexMultilineEndAnchorMatchesCrLfLineEndings()
    {
        var text = "First-\r\nSecond-";
        var service = new FindService();
        service.Initialize([text], 0, false, FindService.FindMode.RegularExpression);

        Assert.Equal(2, service.Count(@"(?m)-$"));

        var matches = service.FindAll(@"(?m)-$");
        Assert.Equal(2, matches.Count);
        Assert.Equal(5, matches[0].TextIndex);
        Assert.Equal(14, matches[1].TextIndex);

        Assert.Equal(0, service.FindNext(@"(?m)-$", [text], 0, 6));
        Assert.Equal(14, service.CurrentTextIndex);
    }
}