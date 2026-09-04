using Nikse.SubtitleEdit.Logic.Ocr.GoogleLens;

namespace UITests.Logic;

/// <summary>
/// The Lens request's "vpw"/"vph" values came from calling ToString() on the int[] "viewport"
/// config entry, which sent "System.Int32[]" for both.
/// </summary>
public class GoogleLensViewportTests
{
    [Fact]
    public void ViewportDimensions_AreRealNumbers()
    {
        Assert.True(HeaderData.ViewportWidth > 0);
        Assert.True(HeaderData.ViewportHeight > 0);
        Assert.Equal("1920", HeaderData.ViewportWidth.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal("1080", HeaderData.ViewportHeight.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ViewportConfigEntry_MatchesTheConstants()
    {
        HeaderData.PopulateConfig();

        var viewport = Assert.IsType<int[]>(HeaderData.Config["viewport"]);
        Assert.Equal(new[] { HeaderData.ViewportWidth, HeaderData.ViewportHeight }, viewport);
    }
}
