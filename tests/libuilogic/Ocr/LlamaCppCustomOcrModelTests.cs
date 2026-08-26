using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using System.Linq;

namespace LibUiLogicTests.Ocr;

/// <summary>
/// The OCR model list offered by the OCR window / video OCR / batch convert: curated entries plus
/// self-supplied vision <c>*.gguf</c> files. A custom entry only qualifies when its mmproj vision
/// projector sits next to it - a text-only model served to the OCR engines cannot see the image.
/// The tests point <see cref="LlamaCppServerManager.FolderOverride"/> at a temp folder so nothing
/// depends on a real llama.cpp install.
/// </summary>
public class LlamaCppCustomOcrModelTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "se-llamacpp-ocr-" + Guid.NewGuid().ToString("N"));

    public LlamaCppCustomOcrModelTests()
    {
        LlamaCppServerManager.FolderOverride = _folder;
        Directory.CreateDirectory(Path.Combine(_folder, "models"));
    }

    public void Dispose()
    {
        LlamaCppServerManager.FolderOverride = null;
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }
    }

    private void PlantModel(params string[] fileNames)
    {
        foreach (var name in fileNames)
        {
            File.WriteAllText(Path.Combine(_folder, "models", name), "fake gguf");
        }
    }

    [Theory]
    [InlineData("my-vision-model.gguf", "mmproj-my-vision-model.gguf")]
    [InlineData("my-vision-model.gguf", "my-vision-model-mmproj.gguf")]
    public void GetAllOcrModels_CustomModelWithSidecar_IsListedWithItsProjector(string model, string mmproj)
    {
        PlantModel(model, mmproj);

        var custom = LlamaCppServerManager.GetAllOcrModels().SingleOrDefault(m => m.FileName == model);

        Assert.NotNull(custom);
        Assert.Equal(mmproj, custom!.MmprojFileName);
        // No Url - already on disk, which is what marks it as a custom entry in the combo box.
        Assert.Equal(string.Empty, custom.Url);
    }

    [Fact]
    public void GetAllOcrModels_CustomModelWithoutSidecar_IsNotListed()
    {
        PlantModel("some-text-model.gguf");

        Assert.DoesNotContain(LlamaCppServerManager.GetAllOcrModels(), m => m.FileName == "some-text-model.gguf");
    }

    [Fact]
    public void GetAllOcrModels_ProjectorsThemselves_AreNeverListed()
    {
        PlantModel("my-vision-model.gguf", "mmproj-my-vision-model.gguf", "other-model-mmproj.gguf");

        var models = LlamaCppServerManager.GetAllOcrModels();

        Assert.DoesNotContain(models, m => m.FileName == "mmproj-my-vision-model.gguf");
        Assert.DoesNotContain(models, m => m.FileName == "other-model-mmproj.gguf");
    }

    [Fact]
    public void GetAllOcrModels_KeepsCuratedModelsFirstAndDoesNotDuplicateThem()
    {
        var curated = LlamaCppServerManager.OcrModels[0];
        PlantModel(curated.FileName, curated.MmprojFileName!, "my-vision-model.gguf", "mmproj-my-vision-model.gguf");

        var models = LlamaCppServerManager.GetAllOcrModels();

        Assert.Equal(LlamaCppServerManager.OcrModels.Count + 1, models.Count);
        Assert.Equal(1, models.Count(m => m.FileName == curated.FileName));
        Assert.Equal(curated.FileName, models[0].FileName);
    }

    /// <summary>
    /// A multimodal GGUF's embedded chat template is what encodes the image placeholder, so the
    /// family guess that serves the translate list (Gemma =&gt; "gemma" + --no-jinja) must not be
    /// applied here - it would drop the image from the prompt.
    /// </summary>
    [Fact]
    public void GetAllOcrModels_GemmaNamedVisionModel_KeepsItsEmbeddedChatTemplate()
    {
        PlantModel("gemma-3-vision-Q4_K_M.gguf", "mmproj-gemma-3-vision-Q4_K_M.gguf");

        var custom = LlamaCppServerManager.GetAllOcrModels().Single(m => m.FileName == "gemma-3-vision-Q4_K_M.gguf");

        Assert.Null(custom.ChatTemplate);
        Assert.False(custom.NoJinja);
    }

    [Fact]
    public void GetAllTranslateModels_VisionProjectors_AreNotOfferedAsTranslateModels()
    {
        PlantModel("my-vision-model.gguf", "my-vision-model-mmproj.gguf");

        var models = LlamaCppServerManager.GetAllTranslateModels();

        Assert.DoesNotContain(models, m => m.FileName == "my-vision-model-mmproj.gguf");
        // The vision model itself is still a plain *.gguf as far as the translate list is
        // concerned - only its projector is filtered out.
        Assert.Contains(models, m => m.FileName == "my-vision-model.gguf");
    }
}
