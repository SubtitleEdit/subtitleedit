using Nikse.SubtitleEdit.Features.WebVtt;
using System.Collections.Generic;

namespace UITests.Features.WebVtt;

public class WebVttClassRenamerTests
{
    [Theory]
    [InlineData("<c.yellow>Hi</c>", "<c.gold>Hi</c>")]
    [InlineData("<c.big.yellow>Hi</c>", "<c.big.gold>Hi</c>")]
    [InlineData("<v.yellow Bob>Hi</v>", "<v.gold Bob>Hi</v>")]
    [InlineData("<c.yellowish>Hi</c>", "<c.yellowish>Hi</c>")]
    [InlineData("yellow <c>Hi</c>", "yellow <c>Hi</c>")]
    [InlineData("<v Bob.yellow>Hi</v>", "<v Bob.yellow>Hi</v>")]
    public void RenameClasses_RewritesOnlyClassList(string input, string expected)
    {
        var renames = new Dictionary<string, string> { ["yellow"] = "gold" };

        Assert.Equal(expected, WebVttClassRenamer.RenameClasses(input, renames));
    }

    [Fact]
    public void RenameClasses_SwapsInOnePass()
    {
        var renames = new Dictionary<string, string> { ["a"] = "b", ["b"] = "a" };

        Assert.Equal("<c.b>x</c> <c.a>y</c>", WebVttClassRenamer.RenameClasses("<c.a>x</c> <c.b>y</c>", renames));
    }
}
