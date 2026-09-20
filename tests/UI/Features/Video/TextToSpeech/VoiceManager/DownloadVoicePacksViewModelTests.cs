using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Download;
using System.IO.Compression;
using System.Text;

namespace UITests.Features.Video.TextToSpeech.VoiceManager;

/// <summary>
/// Installing a voice pack runs on a thread-pool thread while its progress is bound to the
/// window's progress bar, so every report has to land on the UI thread - otherwise Avalonia
/// throws "Call from invalid thread" after the first WAV and leaves a half-installed pack.
/// </summary>
public class DownloadVoicePacksViewModelTests
{
    private sealed class FakeDownloadService : IVoicePackDownloadService
    {
        public Task DownloadPack(VoicePack pack, Stream stream, IProgress<float>? progress, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeCloneEngine : ITtsEngine
    {
        public List<string> Imported { get; } = new();
        public string Name => "Fake clone engine";
        public string Description => string.Empty;
        public bool HasLanguageParameter => false;
        public bool HasApiKey => false;
        public bool HasRegion => false;
        public bool HasModel => false;
        public bool HasKeyFile => false;
        public bool SupportsVoiceCloning => true;
        public bool SupportsPerLineVoiceCloning => false;
        public Task<bool> IsInstalled(string? region) => Task.FromResult(true);
        public Task<string[]> GetRegions() => Task.FromResult(Array.Empty<string>());
        public Task<string[]> GetModels() => Task.FromResult(Array.Empty<string>());
        public Task<TtsLanguage[]> GetLanguages(Voice voice, string? model) => Task.FromResult(Array.Empty<TtsLanguage>());
        public bool IsVoiceInstalled(Voice voice) => true;
        public Task<Voice[]> GetVoices(string languageCode) => Task.FromResult(Array.Empty<Voice>());
        public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) => GetVoices(language);
        public Task<TtsResult> Speak(string text, string outputFolder, Voice voice, TtsLanguage? language, string? region, string? model, CancellationToken cancellationToken) => throw new NotSupportedException();

        public bool ImportVoice(string fileName)
        {
            Imported.Add(Path.GetFileName(fileName));
            return true;
        }
    }

    private sealed class RecordingProgress : IProgress<double>
    {
        public List<double> Values { get; } = new();
        public void Report(double value) => Values.Add(value);
    }

    private static DownloadVoicePacksViewModel MakeViewModel() => new(new FakeDownloadService(), new StubWindowService());

    [AvaloniaFact]
    public async Task InstallProgressReportedFromABackgroundThreadIsRaisedOnTheUiThread()
    {
        var vm = MakeViewModel();
        var progressBar = new ProgressBar { DataContext = vm };
        progressBar.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var raisedOnUiThread = new List<bool>();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.ProgressValue))
            {
                raisedOnUiThread.Add(Dispatcher.UIThread.CheckAccess());
            }
        };

        var progress = vm.CreateInstallProgress();

        // The install loop reports from Task.Run; a plain delegate raised the change on the
        // worker thread, where anything touching a control throws "Call from invalid thread".
        await Task.Run(() => progress.Report(42));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(new[] { true }, raisedOnUiThread);
        Assert.Equal(42, vm.ProgressValue);
        Assert.Equal(42, progressBar.Value);
    }

    [Fact]
    public void InstallPackImportsNewWavsSkipsExistingOnesAndEndsAtFullProgress()
    {
        using var zip = new MemoryStream();
        using (var archive = new ZipArchive(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddWav(archive, "Alice_Smith.wav");
            AddText(archive, "Alice_Smith.txt", "Hello there.");
            AddWav(archive, "Bob.wav");
        }

        var engine = new FakeCloneEngine();
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Bob" };
        var progress = new RecordingProgress();

        var (installed, skipped) = DownloadVoicePacksViewModel.InstallPack(engine, zip, existing, CancellationToken.None, progress);

        Assert.Equal(1, installed);
        Assert.Equal(1, skipped);
        Assert.Equal(new[] { "Alice_Smith.wav" }, engine.Imported);
        Assert.Contains("Alice Smith", existing);
        Assert.Equal(new[] { 50.0, 100.0 }, progress.Values);
    }

    private static void AddText(ZipArchive archive, string name, string text)
    {
        using var stream = archive.CreateEntry(name).Open();
        var bytes = Encoding.UTF8.GetBytes(text);
        stream.Write(bytes, 0, bytes.Length);
    }

    /// <summary>A 0.1 s 16 kHz mono 16-bit PCM WAV of silence.</summary>
    private static void AddWav(ZipArchive archive, string name)
    {
        const int sampleRate = 16000;
        var data = new byte[sampleRate / 10 * 2];
        using var stream = archive.CreateEntry(name).Open();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write("RIFF"u8);
        writer.Write(36 + data.Length);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(data.Length);
        writer.Write(data);
    }
}
