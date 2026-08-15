using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAlpha;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustBrightness;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustColor;
using Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryResizeImages;
using Nikse.SubtitleEdit.Features.Tools.FixNetflixErrors;
using Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;
using Nikse.SubtitleEdit.Features.Tools.SplitSubtitle;
using Nikse.SubtitleEdit.Features.Video.TransparentSubtitles;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
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

    /// <summary>
    /// A failing request has to leave something in <c>Error</c>. The OCR loops fail fast on the
    /// first frame that comes back empty *with* an error set, and otherwise grind through the
    /// whole job to report "no text found" - so an engine that logged and returned an empty
    /// string turned a broken server into a silent, apparently textless result.
    /// </summary>
    [Fact]
    public async Task OcrEngines_RecordAnErrorWhenTheRequestFails()
    {
        using var bitmap = new SKBitmap(8, 8);

        // Port 1 on loopback: nothing listens, so the request fails immediately.
        using var llamaCpp = new LlamaCppOcr(1);
        var text = await llamaCpp.Ocr(bitmap, "http://127.0.0.1:1/v1/chat/completions", "m", "English", string.Empty, CancellationToken.None);

        Assert.Equal(string.Empty, text);
        Assert.False(string.IsNullOrEmpty(llamaCpp.Error));
    }

    /// <summary>
    /// Disposing the engine while a run is still using it (the OCR window started the run in a
    /// fire-and-forget task, so a "using" on the engine tore its HttpClient down immediately)
    /// left every frame empty. The engine must at least report that as an error, so the loop
    /// stops and says so instead of filling the grid with blank lines (#13633).
    /// </summary>
    [Fact]
    public async Task OcrEngines_RecordAnErrorWhenUsedAfterDisposal()
    {
        using var bitmap = new SKBitmap(8, 8);

        var llamaCpp = new LlamaCppOcr(1);
        llamaCpp.Dispose();
        var text = await llamaCpp.Ocr(bitmap, "http://127.0.0.1:1/v1/chat/completions", "m", "English", string.Empty, CancellationToken.None);

        Assert.Equal(string.Empty, text);
        Assert.False(string.IsNullOrEmpty(llamaCpp.Error));

        var ollama = new OllamaOcr(1);
        ollama.Dispose();
        var ollamaText = await ollama.Ocr(bitmap, "http://127.0.0.1:1/api/chat", "m", "English", CancellationToken.None);

        Assert.Equal(string.Empty, ollamaText);
        Assert.False(string.IsNullOrEmpty(ollama.Error));
    }

    /// <summary>
    /// These dialogs own a preview timer (or a preview bitmap) that only the close hook in
    /// <c>UiUtil.InitializeWindow</c> can release - and that hook only knows
    /// <see cref="IClosingCleanup"/>. Implementing plain <see cref="IDisposable"/> was not enough:
    /// nothing called it, so every visit left a 500 ms timer ticking over a closed window, or
    /// leaked the preview bitmap.
    /// </summary>
    [Theory]
    [InlineData(typeof(RemoveTextForHearingImpairedViewModel))]
    [InlineData(typeof(FixNetflixErrorsViewModel))]
    [InlineData(typeof(BinaryAdjustColorViewModel))]
    [InlineData(typeof(BinaryAdjustAlphaViewModel))]
    [InlineData(typeof(BinaryAdjustBrightnessViewModel))]
    [InlineData(typeof(BinaryResizeImagesViewModel))]
    public void DialogsOwningTimersOrBitmaps_CleanUpOnClose(Type viewModelType)
    {
        Assert.True(typeof(IClosingCleanup).IsAssignableFrom(viewModelType),
            $"{viewModelType.Name} must implement IClosingCleanup - nothing else releases what it owns");
    }

    /// <summary>
    /// The preview timer must not tick after the window is gone, and the cleanup has to survive
    /// being called twice (the close hook guards against it, a Cancel path may not).
    /// </summary>
    [Fact]
    public void PreviewTimerDialogs_StopTheirTimerOnCleanup()
    {
        var removeTextForHi = new RemoveTextForHearingImpairedViewModel(null!);
        StartTimer(removeTextForHi);
        removeTextForHi.OnClosingCleanup();
        removeTextForHi.OnClosingCleanup();
        Assert.False(IsTimerEnabled(removeTextForHi));

        var netflix = new FixNetflixErrorsViewModel(null!, null!);
        StartTimer(netflix);
        netflix.OnClosingCleanup();
        netflix.OnClosingCleanup();
        Assert.False(IsTimerEnabled(netflix));
    }

    private static System.Timers.Timer GetTimer(object viewModel) =>
        (System.Timers.Timer)viewModel.GetType()
            .GetField("_timer", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(viewModel)!;

    private static void StartTimer(object viewModel) => GetTimer(viewModel).Start();

    private static bool IsTimerEnabled(object viewModel)
    {
        try
        {
            return GetTimer(viewModel).Enabled;
        }
        catch (ObjectDisposedException)
        {
            return false; // disposed is as stopped as it gets
        }
    }
}
