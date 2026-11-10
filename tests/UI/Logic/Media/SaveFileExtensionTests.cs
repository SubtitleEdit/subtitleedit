using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Logic.Media;

/// <summary>
/// Issue #16740: the save pickers returned the dialog's name verbatim, so a Blu-ray sup
/// export typed as "test.hdr" (or suggested as "Movie.en" from "Movie.en.srt") was written
/// without ".sup". All save pickers now go through FileHelper.EnsureExtension.
/// </summary>
public class SaveFileExtensionTests
{
    [Theory]
    [InlineData("test.hdr", ".sup", "test.hdr.sup")]
    [InlineData("Movie.en", ".sup", "Movie.en.sup")]
    [InlineData("test", ".sup", "test.sup")]
    [InlineData("test.sup", ".sup", "test.sup")]
    [InlineData("test.SUP", ".sup", "test.SUP")]
    [InlineData("test.", ".sup", "test.sup")]
    [InlineData(@"C:\subs\Movie.en", ".sup", @"C:\subs\Movie.en.sup")]
    [InlineData("/home/me/Movie.en", "sup", "/home/me/Movie.en.sup")]
    [InlineData("se", "shortcuts", "se.shortcuts")]
    [InlineData("profile.DCinema-interop-profile", ".DCinema-interop-profile", "profile.DCinema-interop-profile")]
    public void SingleExtension(string fileName, string extension, string expected)
    {
        Assert.Equal(expected, FileHelper.EnsureExtension(fileName, new[] { extension }));
    }

    [Theory]
    [InlineData("rules.csv", "rules.csv")]
    [InlineData("rules.template", "rules.template")]
    [InlineData("rules.txt", "rules.txt.template")]
    [InlineData("rules", "rules.template")]
    public void SeveralExtensions_AnyIsKept_OtherwiseFirstIsAppended(string fileName, string expected)
    {
        Assert.Equal(expected, FileHelper.EnsureExtension(fileName, new[] { ".template", ".csv" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("*.*")]
    public void NoUsableExtension_KeepsName(string extension)
    {
        Assert.Equal("attachment.bin", FileHelper.EnsureExtension("attachment.bin", new[] { extension }));
        Assert.Equal("attachment", FileHelper.EnsureExtension("attachment", new[] { extension }));
    }
}
