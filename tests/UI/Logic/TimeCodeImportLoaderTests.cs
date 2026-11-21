using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.UiLogic.SubtitleLoading;
using SkiaSharp;
using System.Text;

namespace UITests.Logic;

/// <summary>
/// File > Import > Time codes from a Blu-ray sup said "Unknown subtitle format" in SE 5 - SE 4
/// read the picture time codes directly, no OCR needed (issue #17089).
/// </summary>
public class TimeCodeImportLoaderTests
{
    public TimeCodeImportLoaderTests()
    {
        // binary formats like Chk use code page 850, as in the app, where Program registers the provider
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private static string TempFileName(string extension) =>
        Path.Combine(Path.GetTempPath(), "se-time-code-import-" + Guid.NewGuid() + extension);

    private static byte[] MakeBluRaySup(int count)
    {
        var sup = new List<byte>();
        for (var i = 0; i < count; i++)
        {
            using var bitmap = new SKBitmap(64, 32);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Transparent);
                canvas.DrawRect(new SKRect(4, 4, 60, 28), new SKPaint { Color = SKColors.White });
            }

            var pic = new BluRaySupPicture
            {
                StartTime = 1000 + i * 4000,
                EndTime = 3000 + i * 4000,
                Width = 720,
                Height = 480,
                CompositionNumber = i * 2,
            };
            sup.AddRange(BluRaySupPicture.CreateSupFrame(pic, bitmap, SKColors.White, 25, 10, 10, BluRayContentAlignment.BottomCenter));
        }

        return sup.ToArray();
    }

    [Theory]
    [InlineData(".sup")]
    [InlineData(".sub")] // a Blu-ray sup with the wrong extension - detected by content, like File > Open
    public void TryLoad_BluRaySup_ReturnsPictureTimeCodes(string extension)
    {
        var fileName = TempFileName(extension);
        try
        {
            File.WriteAllBytes(fileName, MakeBluRaySup(3));

            var subtitle = TimeCodeImportLoader.TryLoad(fileName);

            Assert.NotNull(subtitle);
            Assert.Equal(3, subtitle.Paragraphs.Count);
            for (var i = 0; i < 3; i++)
            {
                Assert.Equal(1000 + i * 4000, subtitle.Paragraphs[i].StartTime.TotalMilliseconds, 1);
                Assert.Equal(3000 + i * 4000, subtitle.Paragraphs[i].EndTime.TotalMilliseconds, 1);
            }
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void TryLoad_SubRip_StillWorks()
    {
        var fileName = TempFileName(".srt");
        try
        {
            File.WriteAllText(fileName, "1\r\n00:00:01,000 --> 00:00:02,500\r\nHello\r\n\r\n2\r\n00:00:04,000 --> 00:00:05,000\r\nWorld\r\n", new UTF8Encoding(false));

            var subtitle = TimeCodeImportLoader.TryLoad(fileName);

            Assert.NotNull(subtitle);
            Assert.Equal(2, subtitle.Paragraphs.Count);
            Assert.Equal(4000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void TryLoad_MicroDvdSub_IsNotTakenForVobSub()
    {
        var fileName = TempFileName(".sub");
        try
        {
            File.WriteAllText(fileName, "{25}{50}Hello\r\n{100}{150}World\r\n", new UTF8Encoding(false));

            Assert.Null(TimeCodeImportLoader.TryLoadVobSub(fileName));
            var subtitle = TimeCodeImportLoader.TryLoad(fileName);

            Assert.NotNull(subtitle);
            Assert.Equal(2, subtitle.Paragraphs.Count);
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void TryLoad_UnknownFile_ReturnsNull()
    {
        var fileName = TempFileName(".sup");
        try
        {
            File.WriteAllBytes(fileName, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });

            Assert.Null(TimeCodeImportLoader.TryLoad(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }
}
