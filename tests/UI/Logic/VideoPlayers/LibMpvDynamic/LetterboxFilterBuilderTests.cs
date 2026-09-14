using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace UITests.Logic.VideoPlayers.LibMpvDynamic;

public class LetterboxFilterBuilderTests
{
    [Fact]
    public void Disabled_ProducesEmptyFilter()
    {
        var settings = new SeVideoLetterbox { Enabled = false, TopHeightPercent = 15, BottomHeightPercent = 15 };

        Assert.Equal(string.Empty, LetterboxFilterBuilder.Build(settings));
    }

    [Fact]
    public void EnabledButBothHeightsZero_ProducesEmptyFilter()
    {
        var settings = new SeVideoLetterbox { Enabled = true, TopHeightPercent = 0, BottomHeightPercent = 0 };

        Assert.Equal(string.Empty, LetterboxFilterBuilder.Build(settings));
    }

    [Fact]
    public void TopOnly_ProducesOneDrawboxAtTheTopEdge()
    {
        var settings = new SeVideoLetterbox { Enabled = true, TopHeightPercent = 15, BottomHeightPercent = 0, Color = "#000000" };

        var filter = LetterboxFilterBuilder.Build(settings);

        Assert.Equal("drawbox=x=0:y=0:w=iw:h=ih*0.15:color=#000000:t=fill", filter);
    }

    [Fact]
    public void BottomOnly_ProducesOneDrawboxAnchoredToTheBottomEdge()
    {
        var settings = new SeVideoLetterbox { Enabled = true, TopHeightPercent = 0, BottomHeightPercent = 20, Color = "#000000" };

        var filter = LetterboxFilterBuilder.Build(settings);

        Assert.Equal("drawbox=x=0:y=ih-ih*0.2:w=iw:h=ih*0.2:color=#000000:t=fill", filter);
    }

    [Fact]
    public void TopAndBottom_ProducesTwoCommaJoinedDrawboxes()
    {
        var settings = new SeVideoLetterbox { Enabled = true, TopHeightPercent = 10, BottomHeightPercent = 12, Color = "#112233" };

        var filter = LetterboxFilterBuilder.Build(settings);

        Assert.Equal(
            "drawbox=x=0:y=0:w=iw:h=ih*0.1:color=#112233:t=fill," +
            "drawbox=x=0:y=ih-ih*0.12:w=iw:h=ih*0.12:color=#112233:t=fill",
            filter);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(double.NaN)]
    public void NonPositiveOrNaNHeight_IsTreatedAsZero(double topHeightPercent)
    {
        var settings = new SeVideoLetterbox { Enabled = true, TopHeightPercent = topHeightPercent, BottomHeightPercent = 0 };

        Assert.Equal(string.Empty, LetterboxFilterBuilder.Build(settings));
    }

    [Fact]
    public void HeightAbove50Percent_IsClampedTo50()
    {
        // A stale settings file (or a future version allowing a wider range) must not be able to
        // collapse or invert the picture by asking for more than half the frame per bar.
        var settings = new SeVideoLetterbox { Enabled = true, TopHeightPercent = 90, BottomHeightPercent = 0, Color = "#000000" };

        var filter = LetterboxFilterBuilder.Build(settings);

        Assert.Equal("drawbox=x=0:y=0:w=iw:h=ih*0.5:color=#000000:t=fill", filter);
    }

    [Fact]
    public void MissingColor_FallsBackToBlack()
    {
        var settings = new SeVideoLetterbox { Enabled = true, TopHeightPercent = 10, BottomHeightPercent = 0, Color = null! };

        var filter = LetterboxFilterBuilder.Build(settings);

        Assert.Equal("drawbox=x=0:y=0:w=iw:h=ih*0.1:color=#000000:t=fill", filter);
    }
}
