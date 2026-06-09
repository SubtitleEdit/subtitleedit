using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

namespace UITests.Features.Tools.FixCommonErrors;

public class FixDisplayItemTests
{
    [Fact]
    public void ContinuationVariants_KeepDistinctActionKeys_ButShareVisibleActionLabel()
    {
        const string action = "Fix continuation style: Custom";
        var suffixItem = new FixDisplayItem(new Paragraph(), 1, FixActionKey.Create(action, "suffix"), "before", "after", true);
        var prefixItem = new FixDisplayItem(new Paragraph(), 2, FixActionKey.Create(action, "prefix"), "before", "after", true);

        Assert.NotEqual(suffixItem.Action, prefixItem.Action);
        Assert.Equal(action, suffixItem.ActionDisplay);
        Assert.Equal(action, prefixItem.ActionDisplay);
    }

    [Fact]
    public void RegularActions_UseSameTextForActionKeyAndDisplay()
    {
        const string action = "Fix commas";
        var item = new FixDisplayItem(new Paragraph(), 1, action, "before", "after", true);

        Assert.Equal(action, item.Action);
        Assert.Equal(action, item.ActionDisplay);
    }
}
