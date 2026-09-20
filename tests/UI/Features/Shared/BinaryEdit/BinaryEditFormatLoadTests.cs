using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Ocr.OcrSubtitle;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit;
using Nikse.SubtitleEdit.UiLogic.Export;
using SkiaSharp;

namespace UITests.Features.Shared.BinaryEdit;

/// <summary>
/// Round-trips for the image-based formats the binary edit window can now open:
/// each test exports with the matching export handler and reads the result back
/// through the same reader classes <c>LoadImageSubtitle</c> uses.
/// </summary>
public class BinaryEditFormatLoadTests
{
    private static SKBitmap MakeBitmap(int width, int height, SKColor color)
    {
        var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        using var paint = new SKPaint { Color = color };
        canvas.DrawRect(new SKRect(2, 2, width - 2, height - 2), paint);
        return bitmap;
    }

    private static void Export(IExportHandler handler, string fileOrFolderName)
    {
        // Mirrors BinaryEditViewModel.DoExport: the VobSub/DVD sup writers need the CLUT colors
        // and the D-Cinema SMPTE handler needs the frame rate.
        var imageParameter = new ImageParameter
        {
            ScreenWidth = 720,
            ScreenHeight = 576,
            FontColor = SKColors.White,
            OutlineColor = SKColors.Black,
            FramesPerSecond = 25.0,
        };

        handler.WriteHeader(fileOrFolderName, imageParameter);
        for (var i = 0; i < 2; i++)
        {
            using var bitmap = MakeBitmap(120, 40, SKColors.White);
            imageParameter.Bitmap = bitmap;
            imageParameter.Text = "Line " + (i + 1);
            imageParameter.StartTime = TimeSpan.FromSeconds(1 + i * 4);
            imageParameter.EndTime = TimeSpan.FromSeconds(3 + i * 4);
            imageParameter.Index = i;
            imageParameter.OverridePosition = new SKPointI(300, 500);

            handler.CreateParagraph(imageParameter);
            handler.WriteParagraph(imageParameter);
        }

        handler.WriteFooter();
    }

    [Fact]
    public void DvdSupExport_RoundTripsThroughSpDvdSupReader()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".sup");
        try
        {
            Export(new ExportHandlerDvdSup(), fileName);

            Assert.False(FileUtil.IsBluRaySup(fileName));
            Assert.True(FileUtil.IsSpDvdSup(fileName));

            var ocrSubtitle = new OcrSubtitleSpDvdSupImages(fileName);
            Assert.Equal(2, ocrSubtitle.Count);
            Assert.Equal(1000, ocrSubtitle.GetStartTime(0).TotalMilliseconds, 0);
            Assert.Equal(5000, ocrSubtitle.GetStartTime(1).TotalMilliseconds, 0);
            using var bitmap = ocrSubtitle.GetBitmap(0);
            Assert.True(bitmap.Width > 0 && bitmap.Height > 0);

            // The white source rectangle must survive the four color quantization as white on the
            // pattern index - with unset CLUT colors it used to come back as opaque black.
            var center = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);
            Assert.True(center.Alpha > 200, $"center pixel should be opaque, got {center}");
            Assert.True(center.Red > 200 && center.Green > 200 && center.Blue > 200,
                $"center pixel should be white, got {center}");
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void DCinemaSmpte2014Export_DeclaresTheEditRateItWasGiven()
    {
        var folderName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folderName);
        try
        {
            Export(new ExportHandlerDCinemaSmpte2014Png(), folderName);

            var xmlFileName = Path.Combine(folderName, "index.xml");
            Assert.True(File.Exists(xmlFileName));
            var xml = File.ReadAllText(xmlFileName);

            // 25 comes from the Export helper's FramesPerSecond - without it the handler fell
            // back to its 23.976 default and declared 24 regardless of the project frame rate.
            Assert.Contains("<EditRate>25 1</EditRate>", xml);
            Assert.Contains("<TimeCodeRate>25</TimeCodeRate>", xml);
        }
        finally
        {
            Directory.Delete(folderName, true);
        }
    }

    [Fact]
    public void ImscImageExport_RoundTripsThroughXmlImageLoader()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".ttml");
        try
        {
            Export(new ExportHandlerImscImage(), fileName);

            var ocrSubtitle = BinaryEditViewModel.LoadXmlImageSubtitle(fileName);

            Assert.NotNull(ocrSubtitle);
            Assert.IsType<OcrSubtitleIBinaryParagraph>(ocrSubtitle);
            Assert.Equal(2, ocrSubtitle.Count);
            Assert.Equal(1000, ocrSubtitle.GetStartTime(0).TotalMilliseconds, 0);
            Assert.Equal(7000, ocrSubtitle.GetEndTime(1).TotalMilliseconds, 0);
            using var bitmap = ocrSubtitle.GetBitmap(1);
            Assert.Equal(120, bitmap.Width);
            Assert.Equal(40, bitmap.Height);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void BdnXmlExport_RoundTripsThroughXmlImageLoader()
    {
        var folderName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(folderName);
        try
        {
            Export(new ExportHandlerBdnXml(), folderName);
            var xmlFileName = Path.Combine(folderName, "index.xml");
            Assert.True(File.Exists(xmlFileName));

            var ocrSubtitle = BinaryEditViewModel.LoadXmlImageSubtitle(xmlFileName);

            Assert.NotNull(ocrSubtitle);
            Assert.IsType<OcrSubtitleBdn>(ocrSubtitle);
            Assert.Equal(2, ocrSubtitle.Count);
            using var bitmap = ocrSubtitle.GetBitmap(0);
            Assert.Equal(120, bitmap.Width);
            Assert.Equal(40, bitmap.Height);
        }
        finally
        {
            Directory.Delete(folderName, true);
        }
    }

    [Fact]
    public void LoadXmlImageSubtitle_ReturnsNullForPlainTextSubtitleXml()
    {
        var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xml");
        try
        {
            File.WriteAllText(fileName,
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><tt xmlns=\"http://www.w3.org/ns/ttml\"><body><div><p begin=\"00:00:01.000\" end=\"00:00:03.000\">Hello</p></div></body></tt>");

            var ocrSubtitle = BinaryEditViewModel.LoadXmlImageSubtitle(fileName);

            Assert.Null(ocrSubtitle);
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
