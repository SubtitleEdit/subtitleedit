using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.SubtitleLoading;
using System.Text;

namespace UITests.Logic;

/// <summary>
/// File > Open never tried the load-only text formats (SubtitleFormat.GetTextOtherFormats), so a
/// Captionate, WSB, FTE, ... file that SE 4 opened was reported as an unknown format in SE 5.
/// </summary>
public class LoadOnlyTextFormatLoaderTests
{
    public LoadOnlyTextFormatLoaderTests()
    {
        // FTE decodes with code page 1252, as it does in the app, where Program registers the provider.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static string WriteTempFile(string content, string extension)
    {
        var fileName = Path.Combine(Path.GetTempPath(), "se-load-only-" + Guid.NewGuid() + extension);
        File.WriteAllText(fileName, content, new UTF8Encoding(false));
        return fileName;
    }

    [Fact]
    public void TryLoad_JsonTypeOnlyLoad1_LoadsLinesThatSubtitleParseMisses()
    {
        var fileName = WriteTempFile(
            "{\"words\":[{\"time\":1.0,\"duration\":0.5,\"name\":\"Hello\"},{\"time\":1.5,\"duration\":0.2,\"name\":\".\"}," +
            "{\"time\":3.0,\"duration\":0.6,\"name\":\"World\"}]}",
            ".json");
        try
        {
            Assert.Null(Subtitle.Parse(fileName));

            var subtitle = LoadOnlyTextFormatLoader.TryLoad(fileName, Encoding.UTF8);

            Assert.NotNull(subtitle);
            Assert.IsType<JsonTypeOnlyLoad1>(subtitle.OriginalFormat);
            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal("Hello.", subtitle.Paragraphs[0].Text);
            Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.Equal(1700, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 3);
            Assert.Equal("World", subtitle.Paragraphs[1].Text);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void TryLoad_ImageListFormat_IsSkipped()
    {
        // SE's own "HTML index" image export: loaded as text the grid would show png file names.
        var fileName = WriteTempFile(
            "<html><body>" + Environment.NewLine +
            "#1:00:00:01,000-->00:00:03,000<div style='text-align:center'><img src='0001.png' /></div><br /><hr />" + Environment.NewLine +
            "#2:00:00:04,000-->00:00:06,000<div style='text-align:center'><img src='0002.png' /></div><br /><hr />" + Environment.NewLine +
            "</body></html>",
            ".html");
        try
        {
            Assert.True(new SeImageHtmlIndex().IsMine(FileUtil.ReadAllLinesShared(fileName, Encoding.UTF8), fileName));
            Assert.Null(LoadOnlyTextFormatLoader.TryLoad(fileName, Encoding.UTF8));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void TryLoad_UnrecognizedText_ReturnsNull()
    {
        var fileName = WriteTempFile("Just some prose." + Environment.NewLine + "Nothing timed here.", ".txt");
        try
        {
            Assert.Null(LoadOnlyTextFormatLoader.TryLoad(fileName, Encoding.UTF8));
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
