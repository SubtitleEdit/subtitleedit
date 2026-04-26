using Nikse.SubtitleEdit.Features.Files.Compare;
using System.Reflection;

namespace UITests.Features.Files.Compare;

public class CompareViewModelTests
{
    [Fact]
    public void GetHtmlText_PreservesLineBreaks()
    {
        var method = typeof(CompareViewModel).GetMethod("GetHtmlText", BindingFlags.NonPublic | BindingFlags.Static);
        var item = new CompareItem { Text = "not default" };

        Assert.NotNull(method);
        var html = (string)method!.Invoke(null, [item, "First\r\nSecond\nThird\rFourth <tag>"])!;

        Assert.Equal("First<br />Second<br />Third<br />Fourth&nbsp;&lt;tag&gt;", html);
    }
}