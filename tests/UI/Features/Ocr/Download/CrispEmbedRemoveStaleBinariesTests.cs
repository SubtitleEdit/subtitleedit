using Nikse.SubtitleEdit.Features.Ocr.Download;

namespace UITests.Features.Ocr.Download;

/// <summary>
/// Re-downloading the CrispEmbed engine (to switch CPU/Vulkan/CUDA) extracts over the existing
/// install, so the cleanup step must drop the old binaries without touching anything the user
/// would hate to lose - above all the multi-GB models in models/.
/// </summary>
public class CrispEmbedRemoveStaleBinariesTests
{
    private static string NewFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-crispembed-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    [Fact]
    public void RemoveStaleBinaries_KeepsDownloadedModels()
    {
        var folder = NewFolder();
        try
        {
            var models = Path.Combine(folder, "models");
            Directory.CreateDirectory(models);
            var model = Path.Combine(models, "glm-ocr-q4_k.gguf");
            File.WriteAllText(model, "not really a model");

            // A model folder can also hold a stray .dll/.so next to the GGUFs; the sweep is
            // top-level only, so nothing under models/ may be touched whatever its extension.
            var strayInModels = Path.Combine(models, "libggml.so");
            File.WriteAllText(strayInModels, "x");

            DownloadCrispEmbedViewModel.RemoveStaleBinaries(folder);

            Assert.True(File.Exists(model));
            Assert.True(File.Exists(strayInModels));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Theory]
    [InlineData("ggml-vulkan.dll")]
    [InlineData("libggml.so")]
    [InlineData("libggml.dylib")]
    [InlineData("crispembed-server.exe")]
    public void RemoveStaleBinaries_RemovesTopLevelBinaries(string fileName)
    {
        var folder = NewFolder();
        try
        {
            var path = Path.Combine(folder, fileName);
            File.WriteAllText(path, "x");

            DownloadCrispEmbedViewModel.RemoveStaleBinaries(folder);

            Assert.False(File.Exists(path));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void RemoveStaleBinaries_KeepsSidecarAndDocs()
    {
        var folder = NewFolder();
        try
        {
            // The sidecar records which build is installed; wiping it would make the engine look
            // up to date forever, so it has to survive the sweep.
            var sidecar = Path.Combine(folder, ".installed.sha256");
            var readme = Path.Combine(folder, "README.md");
            File.WriteAllText(sidecar, "abc");
            File.WriteAllText(readme, "docs");

            DownloadCrispEmbedViewModel.RemoveStaleBinaries(folder);

            Assert.True(File.Exists(sidecar));
            Assert.True(File.Exists(readme));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void RemoveStaleBinaries_MissingFolderDoesNotThrow()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-crispembed-missing-" + Guid.NewGuid().ToString("N"));

        DownloadCrispEmbedViewModel.RemoveStaleBinaries(folder);
    }
}
