using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Every engine used to ignore the output folder its caller passed to
/// <see cref="ITtsEngine.Speak"/> and write into its own <c>TextToSpeech/&lt;Engine&gt;/</c>
/// folder instead, where nothing ever deleted the clips again (#13332). The last test here is
/// the guard against that coming back with the next engine.
/// </summary>
public class TtsOutputFolderTests
{
    [Fact]
    public void UsesTheCallersFolderWhenItExists()
    {
        var callerFolder = CreateTempFolder();
        try
        {
            Assert.Equal(callerFolder, TtsOutputFolder.Resolve(callerFolder, () => "engine-folder"));
        }
        finally
        {
            Directory.Delete(callerFolder, true);
        }
    }

    [Fact]
    public void CreatesTheCallersFolderWhenItIsMissing()
    {
        var callerFolder = Path.Combine(Path.GetTempPath(), "SeTtsOutputFolderTests_" + Guid.NewGuid());
        try
        {
            Assert.Equal(callerFolder, TtsOutputFolder.Resolve(callerFolder, () => "engine-folder"));
            Assert.True(Directory.Exists(callerFolder));
        }
        finally
        {
            if (Directory.Exists(callerFolder))
            {
                Directory.Delete(callerFolder, true);
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FallsBackToTheEngineFolderWhenNothingUsableIsPassed(string? outputFolder)
    {
        Assert.Equal("engine-folder", TtsOutputFolder.Resolve(outputFolder, () => "engine-folder"));
    }

    /// <summary>
    /// A bad path must not fail the whole generation run - synthesis falls back to the engine's
    /// own folder, which is exactly where it went before.
    /// </summary>
    [Fact]
    public void FallsBackToTheEngineFolderWhenTheFolderCannotBeCreated()
    {
        var blockingFile = Path.Combine(Path.GetTempPath(), "SeTtsOutputFolderTests_" + Guid.NewGuid() + ".tmp");
        File.WriteAllText(blockingFile, "not a folder");
        try
        {
            var impossibleFolder = Path.Combine(blockingFile, "output");
            Assert.Equal("engine-folder", TtsOutputFolder.Resolve(impossibleFolder, () => "engine-folder"));
        }
        finally
        {
            File.Delete(blockingFile);
        }
    }

    /// <summary>
    /// Reflection finds the engines, the source scan finds the idiom: a new engine that writes to
    /// its own folder instead of the caller's fails here rather than in a bug report six months on.
    /// </summary>
    [Fact]
    public void EveryEngineRoutesItsOutputThroughResolve()
    {
        var enginesFolder = GetEnginesSourceFolder();
        var engineTypes = typeof(ITtsEngine).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ITtsEngine).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToList();

        Assert.NotEmpty(engineTypes);

        foreach (var engineType in engineTypes)
        {
            var sourceFile = Path.Combine(enginesFolder, engineType.Name + ".cs");
            Assert.True(File.Exists(sourceFile), $"No source file found for TTS engine '{engineType.Name}' at '{sourceFile}'.");

            var source = File.ReadAllText(sourceFile);
            Assert.True(
                source.Contains("TtsOutputFolder.Resolve(outputFolder", StringComparison.Ordinal),
                $"{engineType.Name}.Speak must write into the caller's output folder - "
                + "use TtsOutputFolder.Resolve(outputFolder, <engine folder getter>) instead of the engine folder.");
        }
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "SeTtsOutputFolderTests_" + Guid.NewGuid());
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static string GetEnginesSourceFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ui", "Features", "Video", "TextToSpeech", "Engines");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the TTS engines source folder walking up from '{AppContext.BaseDirectory}'.");
    }
}
