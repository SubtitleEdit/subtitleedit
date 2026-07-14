using Nikse.SubtitleEdit.Logic;

namespace UITests.Logic;

// "Check for updates" must never offer an older build as an update - an rc1 install
// was being told that beta15 was a new version available.
public class SemanticVersionTests
{
    [Theory]
    [InlineData("v5.1.0-beta15", "v5.1.0-rc1")] // beta < rc, even with a higher number
    [InlineData("v5.1.0-alpha2", "v5.1.0-beta1")]
    [InlineData("v5.1.0-beta2", "v5.1.0-beta10")] // numeric, not alphabetic, ordering
    [InlineData("v5.1.0-rc1", "v5.1.0")] // any pre-release < final
    [InlineData("v5.0.9", "v5.1.0-beta1")]
    [InlineData("v4.0.16", "v5.0.0")]
    public void IsLessThan(string lower, string higher)
    {
        Assert.True(new SemanticVersion(lower).IsLessThan(new SemanticVersion(higher)));
        Assert.True(new SemanticVersion(higher).IsGreaterThan(new SemanticVersion(lower)));
    }

    [Theory]
    [InlineData("v5.1.0-rc1", "5.1.0-rc1")] // the "v" prefix is optional
    [InlineData("v5.1.0", "v5.1.0")]
    public void IsEqualTo(string a, string b)
    {
        Assert.True(new SemanticVersion(a).IsEqualTo(new SemanticVersion(b)));
        Assert.False(new SemanticVersion(a).IsGreaterThan(new SemanticVersion(b)));
    }
}
