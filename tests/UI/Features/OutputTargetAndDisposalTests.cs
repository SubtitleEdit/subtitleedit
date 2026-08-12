using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic.Config;
using System.Reflection;

namespace UITests.Features;

public class OutputTargetAndDisposalTests
{
    private static void Invoke(object vm, string method) =>
        vm.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(vm, null);

    private static void Set(object vm, string property, object value) =>
        vm.GetType().GetProperty(property)!.SetValue(vm, value);

    private static object? Get(object vm, string property) =>
        vm.GetType().GetProperty(property)!.GetValue(vm);

    /// <summary>
    /// The transparent-subtitles settings dialog saved to Video.Transparent.OutputFolder but loaded
    /// Video.BurnIn.OutputFolder - and nothing read the value it saved, so the folder picked here
    /// had no effect at all and the generated files went to the burn-in folder instead.
    /// </summary>
    [Fact]
    public void TransparentSettings_LoadsAndSavesItsOwnOutputFolder()
    {
        using var _ = new SettingsScope("Video.Transparent.OutputFolder", "Video.BurnIn.OutputFolder");

        Se.Settings.Video.Transparent.OutputFolder = "/tmp/transparent-out";
        Se.Settings.Video.BurnIn.OutputFolder = "/tmp/burnin-out";

        var vm = new TransparentSettingsViewModel(null!);
        Invoke(vm, "LoadSettings");
        Assert.Equal("/tmp/transparent-out", Get(vm, "OutputFolder"));

        Set(vm, "OutputFolder", "/tmp/changed");
        Invoke(vm, "SaveSettings");

        Assert.Equal("/tmp/changed", Se.Settings.Video.Transparent.OutputFolder);
        Assert.Equal("/tmp/burnin-out", Se.Settings.Video.BurnIn.OutputFolder); // untouched
    }

    /// <summary>
    /// Split subtitle wrote the chosen format and encoding on OK but never read them back, so the
    /// dialog always reopened on the first entry of each list.
    /// </summary>
    [Fact]
    public void SplitSubtitle_ReopensOnTheLastUsedFormatAndEncoding()
    {
        using var _ = new SettingsScope("Tools.SplitSubtitleFormat", "Tools.SplitSubtitleEncoding");

        var first = new SplitSubtitleViewModel(null!, null!);
        var otherFormat = first.Formats.Skip(3).First();
        var otherEncoding = first.Encodings.Skip(2).First();

        Se.Settings.Tools.SplitSubtitleFormat = otherFormat.Name;
        Se.Settings.Tools.SplitSubtitleEncoding = otherEncoding.DisplayName;

        var reopened = new SplitSubtitleViewModel(null!, null!);

        Assert.Equal(otherFormat.Name, reopened.SelectedSubtitleFormat?.Name);
        Assert.Equal(otherEncoding.DisplayName, reopened.SelectedEncoding?.DisplayName);
    }

    // An unknown saved name must not leave the dialog with nothing selected.
    [Fact]
    public void SplitSubtitle_FallsBackWhenTheSavedNamesAreGone()
    {
        using var _ = new SettingsScope("Tools.SplitSubtitleFormat", "Tools.SplitSubtitleEncoding");

        Se.Settings.Tools.SplitSubtitleFormat = "no such format";
        Se.Settings.Tools.SplitSubtitleEncoding = "no such encoding";

        var vm = new SplitSubtitleViewModel(null!, null!);

        Assert.Equal(vm.Formats[0].Name, vm.SelectedSubtitleFormat?.Name);
        Assert.Equal(vm.Encodings[0].DisplayName, vm.SelectedEncoding?.DisplayName);
    }

    /// <summary>
    /// The output folder is only usable if it exists, and that has to hold for the collision loop
    /// too: testing Directory.Exists at the first use but not when resolving "_2", "_3", ... builds
    /// a path into a missing directory for the second file of a run, which then fails at write time.
    /// </summary>
    [Theory]
    [InlineData(false)] // first file
    [InlineData(true)]  // name already taken, so the collision loop picks the folder again
    public void TransparentSubtitles_MissingOutputFolder_FallsBackToTheSourceFolder(bool nameTaken)
    {
        using var _ = new SettingsScope(
            "Video.Transparent.OutputFolder",
            "Video.Transparent.UseOutputFolder",
            "Video.BurnIn.BurnInSuffix");

        var dir = Directory.CreateTempSubdirectory("se-transparent-output");
        try
        {
            var missing = Path.Combine(dir.FullName, "does", "not", "exist");
            Se.Settings.Video.Transparent.UseOutputFolder = true;
            Se.Settings.Video.Transparent.OutputFolder = missing;
            Se.Settings.Video.BurnIn.BurnInSuffix = "_new";

            var videoFileName = Path.Combine(dir.FullName, "clip.mp4");
            File.WriteAllText(videoFileName, string.Empty);

            var vm = new TransparentSubtitlesViewModel(null!, null!, null!);
            var ext = (string)Get(vm, "SelectedVideoExtension")!;
            if (nameTaken)
            {
                File.WriteAllText(Path.Combine(dir.FullName, "clip_new" + ext), string.Empty);
            }

            var method = typeof(TransparentSubtitlesViewModel)
                .GetMethod("MakeOutputFileName", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var result = (string)method.Invoke(vm, new object[] { videoFileName })!;

            Assert.False(result.StartsWith(missing, StringComparison.Ordinal),
                $"resolved into the missing output folder: {result}");
            Assert.Equal(dir.FullName, Path.GetDirectoryName(result));
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }

    // A new engine - and so a new HttpClient, handler and connection pool - is built per OCR run
    // from three call sites each; without disposal those accumulate until sockets run out.
    [Fact]
    public void OcrEnginesOwningAnHttpClient_AreDisposable()
    {
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(OllamaOcr)));
        Assert.True(typeof(IDisposable).IsAssignableFrom(typeof(LlamaCppOcr)));

        // Idempotent: the call sites use "using", and a double dispose must not throw.
        var ollama = new OllamaOcr();
        ollama.Dispose();
        ollama.Dispose();

        var llamaCpp = new LlamaCppOcr();
        llamaCpp.Dispose();
        llamaCpp.Dispose();
    }
}
