using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// Issue #16535: since the suggestion always carried the extension, the macOS save panel showed
/// "movie.srt.srt" on a Mac where no app has registered ".srt".
/// </summary>
public class MacSuggestedFileNameTests
{
    [Theory]
    [InlineData("movie", ".srt", "movie")]
    [InlineData("movie.srt", ".srt", "movie")]
    [InlineData("movie.SRT", ".srt", "movie")]
    [InlineData("movie.da", ".srt", "movie.da")]
    [InlineData("/videos/movie", ".ass", "movie")]
    [InlineData(".srt", ".srt", ".srt")]
    public void UnregisteredOrMatchingType_SuggestsNoExtension(string suggested, string extension, string expected)
    {
        Assert.Equal(expected, FileHelper.MakeMacSuggestedFileName(suggested, extension, typePrefersOtherExtension: false));
    }

    [Theory]
    [InlineData("movie", ".ssa", "movie.ssa")]
    [InlineData("movie.ssa", ".ssa", "movie.ssa")]
    [InlineData("movie.da", ".ssa", "movie.da.ssa")]
    public void TypePrefersOtherExtension_SuggestsTheFormatsExtension(string suggested, string extension, string expected)
    {
        Assert.Equal(expected, FileHelper.MakeMacSuggestedFileName(suggested, extension, typePrefersOtherExtension: true));
    }

    [Fact]
    public void PrefersOtherExtension_IsFalseForUnregisteredAndMatchingExtensions()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        Assert.False(MacFileTypes.PrefersOtherExtension(".nosuchsubtitleextension"));
        Assert.False(MacFileTypes.PrefersOtherExtension(".txt"));
    }

    [Fact]
    public void PrefersOtherExtension_IsTrueForASecondaryExtensionOfARegisteredType()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        // public.jpeg is built into macOS and prefers "jpeg".
        Assert.True(MacFileTypes.PrefersOtherExtension(".jpg"));
        Assert.False(MacFileTypes.PrefersOtherExtension(".jpeg"));
    }
}
