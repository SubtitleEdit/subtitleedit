using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using SkiaSharp;

namespace LibSETests.VobSub;

public class VobSubWriterRawTest : IDisposable
{
    private readonly string _folder;

    public VobSubWriterRawTest()
    {
        _folder = Path.Combine(Path.GetTempPath(), "se-vobsub-raw-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, true);
        }
        catch (IOException)
        {
            // ignore
        }
    }

    [Fact]
    public void WriteSubPictureUnit_CopiesUnitsAndPalette()
    {
        // make some sub picture units the normal way (one big enough to span several packs)
        var source = Path.Combine(_folder, "source.sub");
        using (var writer = new VobSubWriter(source, 720, 576, 0, 0, 0x20, SKColors.White, SKColors.Black, false, DvdSubtitleLanguage.English))
        {
            writer.WriteParagraph(new Paragraph("a", 1000, 2500), MakeBitmap(200, 40), BluRayContentAlignment.BottomCenter);
            writer.WriteParagraph(new Paragraph("b", 61000, 63000), MakeBitmap(700, 120), BluRayContentAlignment.BottomCenter);
            writer.WriteIdxFile();
        }

        var sourcePacks = Read(source);
        Assert.Equal(2, sourcePacks.Count);

        var palette = Enumerable.Range(0, 16).Select(i => new SKColor((byte)(i * 16), 0x40, 0x80)).ToList();
        var copy = Path.Combine(_folder, "copy.sub");
        using (var writer = new VobSubWriter(copy, 720, 576, 0x21, DvdSubtitleLanguage.GetLanguageOrNull("da"), palette))
        {
            foreach (var pack in sourcePacks)
            {
                writer.WriteSubPictureUnit(pack.StartTimeCode, pack.SubPictureData);
            }

            writer.WriteIdxFile();
        }

        var parser = new VobSubParser(true);
        parser.OpenSubIdx(copy, Path.ChangeExtension(copy, ".idx"));
        var copyPacks = parser.MergeVobSubPacks();

        Assert.Equal(2, copyPacks.Count);
        for (var i = 0; i < copyPacks.Count; i++)
        {
            Assert.Equal(sourcePacks[i].StartTime, copyPacks[i].StartTime);
            Assert.Equal(sourcePacks[i].EndTime, copyPacks[i].EndTime);
            Assert.Equal(sourcePacks[i].SubPicture.ImageDisplayArea, copyPacks[i].SubPicture.ImageDisplayArea);
            Assert.Equal(0x21, copyPacks[i].StreamId);
        }

        Assert.Equal(palette, parser.IdxPalette);
        Assert.Contains("id: da", File.ReadAllText(Path.ChangeExtension(copy, ".idx")));
    }

    private static List<VobSubMergedPack> Read(string subFileName)
    {
        var parser = new VobSubParser(true);
        parser.OpenSubIdx(subFileName, Path.ChangeExtension(subFileName, ".idx"));
        return parser.MergeVobSubPacks();
    }

    private static SKBitmap MakeBitmap(int width, int height)
    {
        var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Transparent);
        for (var x = 2; x < width - 2; x += 3)
        {
            for (var y = 2; y < height - 2; y++)
            {
                bitmap.SetPixel(x, y, SKColors.White);
            }
        }

        return bitmap;
    }
}
