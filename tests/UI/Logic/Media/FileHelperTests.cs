using Nikse.SubtitleEdit.Logic.Media;
using System.Reflection;

namespace UITests.Logic.Media;

public class FileHelperTests
{
    [Theory]
    // Bare filename -> append chosen extension.
    [InlineData("subtitle", ".srt", "subtitle.srt")]
    // Filename already ends with the chosen extension -> unchanged.
    [InlineData("subtitle.srt", ".srt", "subtitle.srt")]
    [InlineData("subtitle.SRT", ".srt", "subtitle.SRT")] // case-insensitive
    // Filename ends with a DIFFERENT known subtitle extension -> preserve user intent.
    [InlineData("subtitle.ass", ".srt", "subtitle.ass")]
    // Filename ends with a language tag that LOOKS like an extension (issue #10349).
    // Path.HasExtension returned true for these and the format extension was dropped.
    [InlineData("Half Man S01E01.sv", ".srt", "Half Man S01E01.sv.srt")]
    [InlineData("Half Man S01E01.en", ".srt", "Half Man S01E01.en.srt")]
    [InlineData("Over Atlanten S10E05.sv", ".srt", "Over Atlanten S10E05.sv.srt")]
    [InlineData("something.en", ".vtt", "something.en.vtt")]
    // Extension passed without leading dot - still works.
    [InlineData("subtitle", "srt", "subtitle.srt")]
    public void AddMissingExtension_AppendsOnlyWhenNeeded(string fileName, string extension, string expected)
    {
        var method = typeof(FileHelper).GetMethod("AddMissingExtension", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        Assert.Equal(expected, method!.Invoke(null, [fileName, extension]));
    }

    [Fact]
    public void AddMissingExtension_EmptyFileName_ReturnsEmpty()
    {
        var method = typeof(FileHelper).GetMethod("AddMissingExtension", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);
        Assert.Equal("", method!.Invoke(null, ["", ".srt"]));
    }
}
