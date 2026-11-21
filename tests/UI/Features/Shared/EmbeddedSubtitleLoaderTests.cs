using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;

namespace UITests.Features.Shared;

/// <summary>
/// Transport stream tracks for import time codes / point sync via other (#17089) - the time
/// codes of a DVB track come without OCR.
/// </summary>
public class EmbeddedSubtitleLoaderTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static string FilePath(string name) => Path.Combine(AppContext.BaseDirectory, "Files", name);

    private static EmbeddedSubtitleLoader MakeLoader() => new(new WindowService(new NullServiceProvider()), null);

    [AvaloniaFact]
    public async Task TryLoad_TransportStreamDvb_ReturnsImageTrackTimes()
    {
        var fileName = FilePath("sample_TS_with_graphics.ts");
        Assert.True(EmbeddedSubtitleLoader.IsTransportStreamFile(fileName));

        var result = await MakeLoader().TryLoad(fileName);

        Assert.NotNull(result);
        Assert.Null(result.Error);
        Assert.NotNull(result.ImageSubtitle);
        var subtitle = result.ToTimeCodeSubtitle();
        Assert.Equal(10, subtitle.Paragraphs.Count);
        Assert.All(subtitle.Paragraphs, p => Assert.True(p.DurationTotalMilliseconds > 0));
    }

    [AvaloniaFact]
    public async Task TryLoad_TransportStreamClosedCaptions_ReturnsTextTrack()
    {
        var result = await MakeLoader().TryLoad(FilePath("sample_TS_cea608_scte20.ts"));

        Assert.NotNull(result);
        Assert.Null(result.Error);
        Assert.NotNull(result.Paragraphs);
        Assert.Equal(4, result.ToTimeCodeSubtitle().Paragraphs.Count);
    }

    [Fact]
    public void IsTransportStreamFile_SubRip_IsFalse()
    {
        var fileName = Path.Combine(Path.GetTempPath(), "se-embedded-" + Guid.NewGuid() + ".ts");
        try
        {
            File.WriteAllText(fileName, "1\r\n00:00:01,000 --> 00:00:02,000\r\nHi\r\n" + new string('x', 20000));
            Assert.False(EmbeddedSubtitleLoader.IsTransportStreamFile(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
