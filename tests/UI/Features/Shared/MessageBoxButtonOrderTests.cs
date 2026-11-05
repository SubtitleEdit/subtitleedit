using Nikse.SubtitleEdit.Features.Shared;

namespace UITests.Features.Shared;

/// <summary>
/// <see cref="MessageBox"/> lays its buttons out like the other dialogs' button bars: dismissing
/// buttons (Cancel, No) on the left, the positive answer (Yes/OK) on the far right. The default
/// (primary, focused) button stays what it was when it was the leftmost one.
/// </summary>
public class MessageBoxButtonOrderTests
{
    [Theory]
    [InlineData(MessageBoxButtons.OK, "OK*")]
    [InlineData(MessageBoxButtons.OKCancel, "Cancel OK*")]
    [InlineData(MessageBoxButtons.YesNo, "No Yes*")]
    [InlineData(MessageBoxButtons.YesNoCancel, "Cancel No Yes*")]
    [InlineData(MessageBoxButtons.Cancel, "Cancel*")]
    public void StandardButtons_DismissLeft_PositiveRightAndDefault(MessageBoxButtons buttons, string expected)
    {
        Assert.Equal(expected, Describe(MessageBox.GetButtonLayout(buttons)));
    }

    [Fact]
    public void CustomChoicesWithCancel_KeepTheirOrder_FirstIsDefault()
    {
        var layout = MessageBox.GetButtonLayout(MessageBoxButtons.Cancel, "CUDA", "Vulkan", "CPU");
        Assert.Equal("Cancel Custom1* Custom2 Custom3", Describe(layout));
    }

    [Fact]
    public void CustomChoiceWithYesNo_SitsBetweenNoAndYes_AndIsDefault()
    {
        var layout = MessageBox.GetButtonLayout(MessageBoxButtons.YesNo, "Yes, never ask again");
        Assert.Equal("No Custom1* Yes", Describe(layout));
    }

    [Fact]
    public void CustomChoiceWithOk_IsDefault()
    {
        var layout = MessageBox.GetButtonLayout(MessageBoxButtons.OK, "Import plain text");
        Assert.Equal("Custom1* OK", Describe(layout));
    }

    private static string Describe(IReadOnlyList<(MessageBoxResult Result, bool IsDefault)> layout) =>
        string.Join(" ", layout.Select(b => b.Result + (b.IsDefault ? "*" : string.Empty)));
}
