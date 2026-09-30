using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.UiLogic.SubtitleLoading;
using System.Text;

namespace UITests.Logic;

/// <summary>
/// SE 4 opened image-list subtitle files (each cue names an image next to the file) for OCR.
/// SE 5 either showed the image file names as text (SubRip, SON parsed as Scenarist) or did
/// not open them at all (DOST, SpuImage, ...).
/// </summary>
public class ImageListSubtitleLoaderTests : IDisposable
{
    private readonly string _tempDirectory;

    public ImageListSubtitleLoaderTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _tempDirectory = Path.Combine(Path.GetTempPath(), "SubtitleEdit.UITests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private string WriteFile(string name, string content)
    {
        var fileName = Path.Combine(_tempDirectory, name);
        File.WriteAllText(fileName, content, new UTF8Encoding(false));
        return fileName;
    }

    private static Subtitle? Load(string fileName)
    {
        return ImageListSubtitleLoader.TryLoad(fileName, Encoding.UTF8, Subtitle.Parse(fileName));
    }

    private static string MakeSrt(params string[] texts)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < texts.Length; i++)
        {
            sb.AppendLine((i + 1).ToString());
            sb.AppendLine($"00:00:0{i * 2 + 1},000 --> 00:00:0{i * 2 + 2},000");
            sb.AppendLine(texts[i]);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    [Fact]
    public void SubRipWithImageFileNames_IsAnImageList()
    {
        var fileName = WriteFile("images.srt", MakeSrt("0001.png", "0002.png", "0003.PNG", "0004.bmp"));

        var subtitle = Load(fileName);

        Assert.NotNull(subtitle);
        Assert.IsType<SubRip>(subtitle.OriginalFormat);
        Assert.Equal(4, subtitle.Paragraphs.Count);
        Assert.Equal("0001.png", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void SubRipWithText_IsNotAnImageList()
    {
        // A line or two that merely ends in ".png" is text, not an image list.
        var fileName = WriteFile("text.srt", MakeSrt("Hello.", "Save it as logo.png", "Goodbye.", "Bye."));

        Assert.Null(Load(fileName));
    }

    [Fact]
    public void Son_ParsedAsScenarist_IsAnImageList()
    {
        var fileName = WriteFile("images.son",
            "St_Title\tTest" + Environment.NewLine +
            "0001\t00:00:01:00\t00:00:03:00\ta_0001.tif" + Environment.NewLine +
            "0002\t00:00:04:00\t00:00:06:00\ta_0002.tif" + Environment.NewLine);

        var subtitle = Load(fileName);

        Assert.NotNull(subtitle);
        Assert.IsType<Son>(subtitle.OriginalFormat);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("a_0001.tif", subtitle.Paragraphs[0].Text);
    }

    private const string DostContent =
        "$FORMAT=480" + "\r\n" +
        "$VERTICAL=0" + "\r\n" +
        "NO\tINTIME\tOUTTIME\tXOFFSET\tYOFFSET\tFILENAME\tFADEIN\tFADEOUT" + "\r\n" +
        "0001\t00:00:01:00\t00:00:03:00\t0\t0\tclip_0001.png\t0\t0" + "\r\n" +
        "0002\t00:00:04:00\t00:00:06:00\t0\t0\tclip_0002.png\t0\t0" + "\r\n";

    [Fact]
    public void Dost_IsAnImageList()
    {
        var fileName = WriteFile("images.dost", DostContent.Replace("\r\n", Environment.NewLine));

        // Subtitle.Parse takes it for Adobe Encore (line/tabs) text, image file names and all.
        Assert.NotNull(Subtitle.Parse(fileName));
        var subtitle = Load(fileName);

        Assert.NotNull(subtitle);
        Assert.IsType<Dost>(subtitle.OriginalFormat);
        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.Equal("clip_0002.png", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void SeImageHtmlIndex_IsAnImageList()
    {
        var fileName = WriteFile("index.html",
            "<html><body>" + Environment.NewLine +
            "#1:00:00:01,000-->00:00:03,000<div style='text-align:center'><img src='0001.png' /></div><br /><hr />" + Environment.NewLine +
            "#2:00:00:04,000-->00:00:06,000<div style='text-align:center'><img src='0002.png' /></div><br /><hr />" + Environment.NewLine +
            "</body></html>");

        var subtitle = Load(fileName);

        Assert.NotNull(subtitle);
        Assert.IsType<SeImageHtmlIndex>(subtitle.OriginalFormat);
        Assert.Equal("0002.png", subtitle.Paragraphs[1].Text);
    }

    [Fact]
    public void OrdinaryScenaristText_IsNotAnImageList()
    {
        var fileName = WriteFile("text.txt",
            "0001\t00:00:01:00\t00:00:03:00\tHello there." + Environment.NewLine +
            "0002\t00:00:04:00\t00:00:06:00\tGeneral Kenobi!" + Environment.NewLine);

        Assert.Null(Load(fileName));
    }
}
