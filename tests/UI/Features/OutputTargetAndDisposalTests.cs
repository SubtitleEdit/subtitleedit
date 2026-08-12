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
        var beforeTransparent = Se.Settings.Video.Transparent.OutputFolder;
        var beforeBurnIn = Se.Settings.Video.BurnIn.OutputFolder;
        try
        {
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
        finally
        {
            Se.Settings.Video.Transparent.OutputFolder = beforeTransparent;
            Se.Settings.Video.BurnIn.OutputFolder = beforeBurnIn;
        }
    }

    /// <summary>
    /// Split subtitle wrote the chosen format and encoding on OK but never read them back, so the
    /// dialog always reopened on the first entry of each list.
    /// </summary>
    [Fact]
    public void SplitSubtitle_ReopensOnTheLastUsedFormatAndEncoding()
    {
        var beforeFormat = Se.Settings.Tools.SplitSubtitleFormat;
        var beforeEncoding = Se.Settings.Tools.SplitSubtitleEncoding;
        try
        {
            var first = new SplitSubtitleViewModel(null!, null!);
            var otherFormat = first.Formats.Skip(3).First();
            var otherEncoding = first.Encodings.Skip(2).First();

            Se.Settings.Tools.SplitSubtitleFormat = otherFormat.Name;
            Se.Settings.Tools.SplitSubtitleEncoding = otherEncoding.DisplayName;

            var reopened = new SplitSubtitleViewModel(null!, null!);

            Assert.Equal(otherFormat.Name, reopened.SelectedSubtitleFormat?.Name);
            Assert.Equal(otherEncoding.DisplayName, reopened.SelectedEncoding?.DisplayName);
        }
        finally
        {
            Se.Settings.Tools.SplitSubtitleFormat = beforeFormat;
            Se.Settings.Tools.SplitSubtitleEncoding = beforeEncoding;
        }
    }

    // An unknown saved name must not leave the dialog with nothing selected.
    [Fact]
    public void SplitSubtitle_FallsBackWhenTheSavedNamesAreGone()
    {
        var beforeFormat = Se.Settings.Tools.SplitSubtitleFormat;
        var beforeEncoding = Se.Settings.Tools.SplitSubtitleEncoding;
        try
        {
            Se.Settings.Tools.SplitSubtitleFormat = "no such format";
            Se.Settings.Tools.SplitSubtitleEncoding = "no such encoding";

            var vm = new SplitSubtitleViewModel(null!, null!);

            Assert.Equal(vm.Formats[0].Name, vm.SelectedSubtitleFormat?.Name);
            Assert.Equal(vm.Encodings[0].DisplayName, vm.SelectedEncoding?.DisplayName);
        }
        finally
        {
            Se.Settings.Tools.SplitSubtitleFormat = beforeFormat;
            Se.Settings.Tools.SplitSubtitleEncoding = beforeEncoding;
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
