using Nikse.SubtitleEdit.Features.Ocr;

namespace UITests.Features.Ocr.Engines;

public class TesseractOcrAvailabilityTests
{
    [Fact]
    public void HasLanguageModel_MissingFolder_IsFalse()
    {
        Assert.False(TesseractOcr.HasLanguageModel(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));
    }

    [Fact]
    public void HasLanguageModel_OnlyOsd_IsFalse()
    {
        var dir = Directory.CreateTempSubdirectory("se-tessdata-test");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "osd.traineddata"), "x");
            File.WriteAllText(Path.Combine(dir.FullName, "readme.txt"), "x");

            Assert.False(TesseractOcr.HasLanguageModel(dir.FullName));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    [Fact]
    public void HasLanguageModel_WithLanguage_IsTrue()
    {
        var dir = Directory.CreateTempSubdirectory("se-tessdata-test");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "eng.traineddata"), "x");

            Assert.True(TesseractOcr.HasLanguageModel(dir.FullName));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
