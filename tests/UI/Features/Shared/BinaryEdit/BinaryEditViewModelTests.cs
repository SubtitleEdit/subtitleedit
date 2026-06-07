using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using System.Collections.Generic;

namespace UITests.Features.Shared.BinaryEdit;

public class BinaryEditViewModelTests
{
    [Fact]
    public void FindActiveSubtitleIndex_IncludesCueStartAndExcludesCueEnd()
    {
        var subtitles = new List<BinarySubtitleItem>
        {
            new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3)),
            new(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5)),
        };

        Assert.Equal(0, BinaryEditViewModel.FindActiveSubtitleIndex(subtitles, TimeSpan.FromSeconds(1)));
        Assert.Equal(1, BinaryEditViewModel.FindActiveSubtitleIndex(subtitles, TimeSpan.FromSeconds(3)));
        Assert.Equal(-1, BinaryEditViewModel.FindActiveSubtitleIndex(subtitles, TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void FindActiveSubtitleIndex_ReturnsMinusOneInsideGap()
    {
        var subtitles = new List<BinarySubtitleItem>
        {
            new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)),
            new(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(5)),
        };

        var result = BinaryEditViewModel.FindActiveSubtitleIndex(subtitles, TimeSpan.FromSeconds(3));

        Assert.Equal(-1, result);
    }
}
