using Nikse.SubtitleEdit.Core.VobSub;
using System.Collections.Generic;

namespace LibSETests.VobSub;

public class IdxTest
{
    [Fact]
    public void SizeLineIsParsed()
    {
        var idx = new Idx(new List<string>
        {
            "# VobSub index file, v7 (do not modify this line!)",
            "size: 720x576",
            "palette: 000000, f0f0f0, cccccc, 999999, 3333fa, 1111bb, fa3333, bb1111, 33fa33, 11bb11, fafa33, bbbb11, fa33fa, bb11bb, 33fafa, 11bbbb",
            "id: en, index: 0",
        });

        Assert.Equal(720, idx.ScreenWidth);
        Assert.Equal(576, idx.ScreenHeight);
    }

    [Fact]
    public void MissingSizeLineLeavesSizeAtZero()
    {
        var idx = new Idx(new List<string> { "id: en, index: 0" });

        Assert.Equal(0, idx.ScreenWidth);
        Assert.Equal(0, idx.ScreenHeight);
    }
}
