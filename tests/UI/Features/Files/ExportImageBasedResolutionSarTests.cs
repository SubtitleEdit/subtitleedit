using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Files;

/// <summary>
/// The SD presets carry their pixel aspect (doom9 p=2052176), and a saved profile picks the
/// preset by size and pixel aspect - PAL 16:9, PAL 4:3 and square-pixel PAL are all 720x576.
/// </summary>
public class ExportImageBasedResolutionSarTests
{
    [Theory]
    [InlineData(720, 576, ExportPixelAspect.Pal16x9, "PAL 16:9 (anamorphic)")]
    [InlineData(720, 576, ExportPixelAspect.Pal4x3, "PAL 4:3")]
    [InlineData(720, 480, ExportPixelAspect.Ntsc16x9, "NTSC 16:9 (anamorphic)")]
    [InlineData(720, 480, ExportPixelAspect.Ntsc4x3, "NTSC 4:3")]
    [InlineData(720, 576, "", "SD PAL (square pixels)")]
    [InlineData(720, 480, null, "SD NTSC (square pixels)")]
    public void Matches_PicksPresetBySizeAndPixelAspect(int width, int height, string? pixelAspect, string expectedName)
    {
        var matches = ResolutionItem.GetResolutions().Skip(1).Where(r => r.Matches(width, height, pixelAspect)).ToList();

        Assert.Single(matches);
        Assert.Equal(expectedName, matches[0].Name);
    }

    [Fact]
    public void NoSdPresetIsLabelledThreeByTwo()
    {
        Assert.DoesNotContain(ResolutionItem.GetResolutions().Skip(1), r => r.Name.Contains("3:2"));
    }
}
