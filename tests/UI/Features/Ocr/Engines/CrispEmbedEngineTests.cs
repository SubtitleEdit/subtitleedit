using Nikse.SubtitleEdit.Features.Ocr;
using Nikse.SubtitleEdit.Features.Ocr.Engines;

namespace UITests.Features.Ocr.Engines;

public class CrispEmbedEngineTests
{
    [Fact]
    public void GetBackends_HasExpectedBackends()
    {
        var backends = CrispEmbedEngine.GetBackends();

        Assert.Equal(new[] { "GLM-OCR", "GOT-OCR2", "PaddleOCR-VL" }, backends.Select(p => p.Name));
    }

    [Fact]
    public void GetBackends_ModelsAreWellFormed()
    {
        foreach (var backend in CrispEmbedEngine.GetBackends())
        {
            Assert.NotEmpty(backend.Models);

            foreach (var model in backend.Models)
            {
                Assert.EndsWith(".gguf", model.Name);
                Assert.False(string.IsNullOrWhiteSpace(model.Size));

                // The model is downloaded to <models>/<Name>, so the URL must end in the
                // same file name or the install check would never see the download.
                Assert.StartsWith("https://huggingface.co/", model.Url);
                Assert.EndsWith("/" + model.Name, model.Url);
            }

            // No duplicate model file names within a backend - they share one models folder.
            Assert.Equal(backend.Models.Count, backend.Models.Select(p => p.Name).Distinct().Count());
        }
    }

    [Fact]
    public void OcrEngines_CrispEmbedListedOnSupportedPlatforms()
    {
        var engines = OcrEngineItem.GetOcrEngines();

        Assert.Equal(
            CrispEmbedEngine.CanBeDownloaded(),
            engines.Any(p => p.EngineType == OcrEngineType.CrispEmbed));
    }
}
