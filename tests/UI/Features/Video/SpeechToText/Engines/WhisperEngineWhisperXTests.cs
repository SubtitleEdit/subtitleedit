using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace UITests.Features.Video.SpeechToText.Engines;

public class WhisperEngineWhisperXTests
{
    [Fact]
    public void FactoryCreatesWhisperXEngine()
    {
        var engine = WhisperEngineFactory.MakeEngineFromStaticName(WhisperEngineWhisperX.StaticName);

        Assert.IsType<WhisperEngineWhisperX>(engine);
        Assert.Equal(WhisperChoice.WhisperX, engine.Choice);
        Assert.True(engine.CanBeDownloaded());
    }

    [Fact]
    public void RepoIdComesFromTheModelUrl()
    {
        var model = new WhisperModel
        {
            Name = "large-v1",
            Urls = new[] { "https://huggingface.co/Systran/faster-whisper-large-v1/resolve/main/model.bin" },
        };

        Assert.Equal("Systran/faster-whisper-large-v1", WhisperEngineWhisperX.GetHuggingFaceRepoId(model));
    }

    [Fact]
    public void RepoIdFollowsFasterWhispersNameMapWhereItDiffersFromTheDownloadUrl()
    {
        // SE downloads distil-large-v3.5 from Purfview's repo for the other engines, but
        // faster-whisper maps the name to distil-whisper's - that is where whisperx caches it.
        var model = new WhisperEngineWhisperX().Models.Single(m => m.Name == "distil-large-v3.5");

        Assert.Equal("distil-whisper/distil-large-v3.5-ct2", WhisperEngineWhisperX.GetHuggingFaceRepoId(model));
    }

    [Fact]
    public void ModelsFasterWhisperHasNoNameForGoOnTheCommandLineAsRepoIds()
    {
        // faster-whisper rejects "anime.ja" ("Invalid model size") but accepts a repo id.
        var engine = new WhisperEngineWhisperX();
        var emptyModelsFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "se-whisperx-no-models-" + Guid.NewGuid());

        Assert.Equal("large-v3", engine.GetModelForCmdLine("large-v3", emptyModelsFolder));
        Assert.Equal("distil-large-v3.5", engine.GetModelForCmdLine("distil-large-v3.5", emptyModelsFolder));
        Assert.Equal("quantumcookie/anime-whisper-ct2-int8", engine.GetModelForCmdLine("anime.ja", emptyModelsFolder));
        Assert.Equal("my-custom-model", engine.GetModelForCmdLine("my-custom-model", emptyModelsFolder));

        // Guards the next model added to the shared list: every entry must reach faster-whisper
        // as a name it knows or as a repo id.
        Assert.All(engine.Models, m =>
        {
            var arg = engine.GetModelForCmdLine(m.Name, emptyModelsFolder);
            Assert.True(WhisperEngineWhisperX.FasterWhisperModelNames.Contains(arg) || arg.Contains('/'), m.Name);
        });
    }

    [Fact]
    public void CustomModelsWithoutUrlHaveNoRepoId()
    {
        // Custom models found in Purfview's model folder carry no URL - no repo, no green dot.
        var custom = new WhisperModel { Name = "my-model", Urls = System.Array.Empty<string>(), Folder = "my-model" };
        var other = new WhisperModel { Name = "x", Urls = new[] { "https://example.com/model.bin" } };

        Assert.Null(WhisperEngineWhisperX.GetHuggingFaceRepoId(custom));
        Assert.Null(WhisperEngineWhisperX.GetHuggingFaceRepoId(other));
    }

    [Fact]
    public void HubCacheDirFollowsHuggingFaceResolutionOrder()
    {
        static string Resolve(params (string Key, string Value)[] env) =>
            WhisperEngineWhisperX.GetHuggingFaceHubCacheDir(key => env.FirstOrDefault(e => e.Key == key).Value);

        var home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        var sep = System.IO.Path.DirectorySeparatorChar;

        Assert.Equal($"{sep}hub-cache", Resolve(("HF_HUB_CACHE", $"{sep}hub-cache"), ("HF_HOME", $"{sep}hf-home")));
        Assert.Equal($"{sep}hub-cache", Resolve(("HF_HUB_CACHE", $"{sep}hub-cache"), ("HUGGINGFACE_HUB_CACHE", $"{sep}legacy")));
        // The legacy variable is the default for HF_HUB_CACHE in huggingface_hub, so it beats HF_HOME.
        Assert.Equal($"{sep}legacy", Resolve(("HF_HOME", $"{sep}hf-home"), ("HUGGINGFACE_HUB_CACHE", $"{sep}legacy")));
        Assert.Equal(System.IO.Path.Combine($"{sep}hf-home", "hub"), Resolve(("HF_HOME", $"{sep}hf-home"), ("XDG_CACHE_HOME", $"{sep}xdg")));
        Assert.Equal(System.IO.Path.Combine(home, "hf", "hub"), Resolve(("HF_HOME", "~/hf")));
        Assert.Equal(System.IO.Path.Combine(home, "hub-cache"), Resolve(("HF_HUB_CACHE", "~/hub-cache")));
        Assert.Equal(System.IO.Path.Combine($"{sep}xdg", "huggingface", "hub"), Resolve(("XDG_CACHE_HOME", $"{sep}xdg")));
        Assert.Equal(System.IO.Path.Combine(home, ".cache", "huggingface", "hub"), Resolve());
        Assert.Equal(System.IO.Path.Combine(home, ".cache", "huggingface", "hub"), Resolve(("HF_HOME", "  ")));
    }

    [Fact]
    public void ModelIsInstalledOnlyWhenASnapshotHoldsTheWeights()
    {
        // #15170: every model showed a green dot because IsModelInstalled was hardcoded true.
        // "Downloaded" means a complete faster-whisper snapshot in the Hugging Face hub cache;
        // an interrupted download leaves the repo folder without model.bin in any snapshot.
        var cache = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "se-hf-cache-" + System.Guid.NewGuid().ToString("N"));
        try
        {
            var tiny = new WhisperModel { Name = "tiny", Urls = new[] { "https://huggingface.co/Systran/faster-whisper-tiny/resolve/main/model.bin" } };
            var baseModel = new WhisperModel { Name = "base", Urls = new[] { "https://huggingface.co/Systran/faster-whisper-base/resolve/main/model.bin" } };
            var small = new WhisperModel { Name = "small", Urls = new[] { "https://huggingface.co/Systran/faster-whisper-small/resolve/main/model.bin" } };

            var tinySnapshot = System.IO.Path.Combine(cache, "models--Systran--faster-whisper-tiny", "snapshots", "d90ca5fe");
            System.IO.Directory.CreateDirectory(tinySnapshot);
            System.IO.File.WriteAllText(System.IO.Path.Combine(tinySnapshot, "model.bin"), "weights");

            // Interrupted: repo folder and blobs exist, no snapshot with weights.
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(cache, "models--Systran--faster-whisper-base", "blobs"));
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(cache, "models--Systran--faster-whisper-base", "snapshots", "abc"));

            Assert.True(WhisperEngineWhisperX.IsModelInHubCache(tiny, cache));
            Assert.False(WhisperEngineWhisperX.IsModelInHubCache(baseModel, cache));
            Assert.False(WhisperEngineWhisperX.IsModelInHubCache(small, cache));
            Assert.False(WhisperEngineWhisperX.IsModelInHubCache(tiny, System.IO.Path.Combine(cache, "missing")));
        }
        finally
        {
            if (System.IO.Directory.Exists(cache))
            {
                System.IO.Directory.Delete(cache, true);
            }
        }
    }

    [Fact]
    public void ModelListExcludesNamesWhisperXCannotResolve()
    {
        // The NbAiLab "*.nb" names only work when SE downloads the model and passes a local
        // folder; whisperx gets the bare name, which is not a size or a Hugging Face repo id.
        var engine = new WhisperEngineWhisperX();

        Assert.DoesNotContain(engine.Models, m => m.Name.EndsWith(".nb"));
        Assert.Contains(engine.Models, m => m.Name == "large-v3");
    }

    [Fact]
    public void LanguageCatalogIncludesLanguagesBeyondTheOriginalWhisperXSubset()
    {
        var engine = new WhisperEngineWhisperX();
        var languageCodes = engine.Languages.Select(p => p.Code).ToHashSet();

        Assert.Contains("tr", languageCodes);
        Assert.Contains("ar", languageCodes);
        Assert.Contains("ru", languageCodes);
        Assert.Contains("fa", languageCodes);
        Assert.True(languageCodes.Count > 50);
    }

    [Fact]
    public void ParametersCanSelectReliableMacCpuMode()
    {
        var engine = new WhisperEngineWhisperX();
        var original = engine.CommandLineParameter;

        try
        {
            engine.CommandLineParameter = "--device cpu --compute_type int8";
            Assert.Contains("--device cpu", engine.CommandLineParameter);
            Assert.Contains("--compute_type int8", engine.CommandLineParameter);
        }
        finally
        {
            engine.CommandLineParameter = original;
        }
    }

    [Fact]
    public void ExecutableLivesDirectlyInsideTheWhisperXFolder()
    {
        // The standalone build's zip is unpacked flat (Unpack(dir, string.Empty)), so the
        // executable and its "_internal" PyInstaller payload land directly in the engine
        // folder - same layout as WhisperEngineCTranslate2, not a nested subfolder.
        var engine = new WhisperEngineWhisperX();

        var executable = engine.GetExecutable();
        var folder = engine.GetAndCreateWhisperFolder();

        Assert.Equal(folder, System.IO.Path.GetDirectoryName(executable));
    }

    [Fact]
    public void ModelSeDownloadedGoesOnTheCommandLineAsItsFolder()
    {
        // SE's downloader fills the shared faster-whisper "_models" folder; whisperx must get
        // that folder, not the bare name, or it downloads a second copy into the hub cache.
        var engine = new WhisperEngineWhisperX();
        var modelsFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "se-whisperx-models-" + Guid.NewGuid());
        try
        {
            var small = engine.Models.Single(m => m.Name == "small");
            var smallFolder = System.IO.Path.Combine(modelsFolder, "faster-whisper-small");
            System.IO.Directory.CreateDirectory(smallFolder);
            System.IO.File.WriteAllText(System.IO.Path.Combine(smallFolder, "config.json"), "{}");

            // Still downloading: only the partial file is there.
            using (var partial = System.IO.File.Create(System.IO.Path.Combine(smallFolder, "model.bin.$$$")))
            {
                partial.SetLength(20_000_000);
            }

            Assert.Null(WhisperEngineWhisperX.GetLocalModelFolder(small, modelsFolder));
            Assert.Equal("small", engine.GetModelForCmdLine("small", modelsFolder));

            System.IO.File.Move(System.IO.Path.Combine(smallFolder, "model.bin.$$$"), System.IO.Path.Combine(smallFolder, "model.bin"));

            Assert.Equal(smallFolder, WhisperEngineWhisperX.GetLocalModelFolder(small, modelsFolder));
            Assert.Equal(smallFolder, engine.GetModelForCmdLine("small", modelsFolder));
            Assert.Equal("base", engine.GetModelForCmdLine("base", modelsFolder));
        }
        finally
        {
            if (System.IO.Directory.Exists(modelsFolder))
            {
                System.IO.Directory.Delete(modelsFolder, true);
            }
        }
    }

    [Fact]
    public void TruncatedModelBinIsNotALocalModel()
    {
        var engine = new WhisperEngineWhisperX();
        var modelsFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "se-whisperx-models-" + Guid.NewGuid());
        try
        {
            var tiny = engine.Models.Single(m => m.Name == "tiny");
            var tinyFolder = System.IO.Path.Combine(modelsFolder, "faster-whisper-tiny");
            System.IO.Directory.CreateDirectory(tinyFolder);
            System.IO.File.WriteAllText(System.IO.Path.Combine(tinyFolder, "config.json"), "{}");
            System.IO.File.WriteAllBytes(System.IO.Path.Combine(tinyFolder, "model.bin"), new byte[1000]);

            Assert.Null(WhisperEngineWhisperX.GetLocalModelFolder(tiny, modelsFolder));
            Assert.Equal("tiny", engine.GetModelForCmdLine("tiny", modelsFolder));
        }
        finally
        {
            if (System.IO.Directory.Exists(modelsFolder))
            {
                System.IO.Directory.Delete(modelsFolder, true);
            }
        }
    }
}
