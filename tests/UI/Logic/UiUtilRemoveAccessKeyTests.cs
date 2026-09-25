using Nikse.SubtitleEdit.Logic;
using Xunit;

namespace UITests.Logic;

/// <summary>
/// Batch tools show the "Done" button caption as a row status - with its access-key marker the
/// rows read "_Done".
/// </summary>
public class UiUtilRemoveAccessKeyTests
{
    [Theory]
    [InlineData("_Done", "Done")]
    [InlineData("C_ancel", "Cancel")]
    [InlineData("Done", "Done")]
    [InlineData("", "")]
    public void RemovesTheAccessKeyMarker(string text, string expected)
    {
        Assert.Equal(expected, UiUtil.RemoveAccessKey(text));
    }
}
