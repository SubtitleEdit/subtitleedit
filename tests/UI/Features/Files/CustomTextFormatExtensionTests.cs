using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Files;

/// <summary>
/// Custom text formats store their extension without the dot ("srt", "txt"). The save picker
/// got the filter "*srt", which the macOS save panel can't map to a file type, so the export
/// was saved with no extension at all (#15699); batch convert appended it as "moviesrt".
/// </summary>
public class CustomTextFormatExtensionTests
{
    [Theory]
    [InlineData("srt", ".srt")]
    [InlineData(".srt", ".srt")]
    [InlineData(" txt ", ".txt")]
    [InlineData("", "")]
    [InlineData(".", "")]
    public void GetDottedExtension_AddsSingleLeadingDot(string extension, string expected)
    {
        var item = new CustomFormatItem { Extension = extension };

        Assert.Equal(expected, item.GetDottedExtension());
    }

    [Theory]
    [InlineData("srt", "*.srt")]
    [InlineData(".srt", "*.srt")]
    [InlineData("", "*")]
    public void MakeExtensionPattern_AlwaysHasDot(string extension, string expected)
    {
        Assert.Equal(expected, FileHelper.MakeExtensionPattern(extension));
    }
}
