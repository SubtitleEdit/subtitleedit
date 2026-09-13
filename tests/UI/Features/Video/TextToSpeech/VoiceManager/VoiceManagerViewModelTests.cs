using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Media;
using System.Text;

namespace UITests.Features.Video.TextToSpeech.VoiceManager;

/// <summary>
/// The voice manager lists each engine's voices, tells file-backed clones from the rest, reads
/// the transcript sidecar and reflects a saved edit back into the row. Uses a fake engine that
/// hands out CosyVoice3-shaped voices pointing at real WAV files in a temp folder, so the
/// file-backed paths (rename/delete/transcript) run against disk without any engine binary.
/// </summary>
public class VoiceManagerViewModelTests
{
    private sealed class FakeCloneEngine : ITtsEngine
    {
        private readonly string _folder;
        public FakeCloneEngine(string folder) => _folder = folder;
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
        public Task<Voice[]> RefreshVoices(string language, CancellationToken cancellationToken) => GetVoices(language);
        public Task<TtsResult> Speak(string text, string outputFolder, Voice voice, TtsLanguage? language, string? region, string? model, CancellationToken cancellationToken) => throw new NotSupportedException();
        public bool ImportVoice(string fileName) => false;

        public Task<Voice[]> GetVoices(string languageCode)
        {
            var voices = new List<Voice> { new(new CosyVoice3Voice("Preset A", "preset-a"), "Preset: Preset A") };
            foreach (var file in Directory.GetFiles(_folder, "*.wav").OrderBy(f => f, StringComparer.Ordinal))
            {
                var name = Path.GetFileNameWithoutExtension(file).Replace('_', ' ');
                voices.Add(new Voice(new CosyVoice3Voice(name, file, string.Empty), "Clone: " + name));
            }

            return Task.FromResult(voices.ToArray());
        }
    }

    private sealed class FakeFolderHelper : IFolderHelper
    {
        public Task<string> PickFolderAsync(Window window, string title, string? suggestedStartFolder = null) => Task.FromResult(string.Empty);
        public Task OpenFolder(Window window, string folder) => Task.CompletedTask;
        public Task OpenFolderWithFileSelected(Window window, string selectedFile) => Task.CompletedTask;
    }

    /// <summary>A 0.5 s 16 kHz mono 16-bit PCM WAV of silence - enough for a header and peaks.</summary>
    private static void WriteWav(string fileName)
    {
        const int sampleRate = 16000;
        const int samples = sampleRate / 2;
        var data = new byte[samples * 2];
        using var stream = File.Create(fileName);
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

    private static string CreateVoicesFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "SeVoiceManagerTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        WriteWav(Path.Combine(folder, "Alice_Smith.wav"));
        File.WriteAllText(Path.Combine(folder, "Alice_Smith.txt"), "Hello there, this is Alice speaking.");
        WriteWav(Path.Combine(folder, "Bob.wav"));
        return folder;
    }

    private static VoiceManagerViewModel MakeViewModel() =>
        new(new StubWindowService(), new StubFileHelper(), new FakeFolderHelper());

    [AvaloniaFact]
    public async Task ListsFileVoicesAndPresetsWithTheirKindsAndTranscripts()
    {
        var folder = CreateVoicesFolder();
        try
        {
            var vm = MakeViewModel();
            var engine = new FakeCloneEngine(folder);
            vm.Initialize(new ITtsEngine[] { engine }, engine);
            await vm.VoicesLoaded;
            await vm.DetailsLoaded;

            Assert.Equal(3, vm.Voices.Count);
            Assert.Equal("3", vm.Engines[0].CountText);

            var preset = Assert.Single(vm.Voices, v => v.Kind == VoiceKind.Preset);
            Assert.False(preset.IsFileVoice);

            var alice = Assert.Single(vm.Voices, v => v.Name == "Alice Smith");
            Assert.Equal(VoiceKind.Clone, alice.Kind);
            Assert.True(alice.HasTranscript);
            Assert.Equal(0.5, alice.DurationSeconds, 2);
            Assert.Contains("16 kHz", alice.Format);

            var bob = Assert.Single(vm.Voices, v => v.Name == "Bob");
            Assert.False(bob.HasTranscript);

            // The engine's own folder is what "open folder" targets - taken from the first clone.
            Assert.True(vm.IsOpenFolderEnabled);
            Assert.Equal(folder, vm.StatusText);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [AvaloniaFact]
    public async Task SelectingACloneLoadsPeaksAndTranscriptAndSavingWritesTheSidecar()
    {
        var folder = CreateVoicesFolder();
        try
        {
            var vm = MakeViewModel();
            var engine = new FakeCloneEngine(folder);
            vm.Initialize(new ITtsEngine[] { engine }, engine);
            await vm.VoicesLoaded;

            vm.SelectedVoice = vm.Voices.Single(v => v.Name == "Alice Smith");
            await vm.DetailsLoaded;

            Assert.NotNull(vm.WavePeakData);
            Assert.True(vm.IsFileVoiceSelected);
            Assert.True(vm.IsPlayEnabled);
            // The fake engine does not read ref-text, so the editor stays hidden - the sidecar is
            // still loaded because it travels with the voice on copy.
            Assert.False(vm.IsTranscriptVisible);
            Assert.Equal("Hello there, this is Alice speaking.", vm.Transcript);
            Assert.False(vm.IsTranscriptDirty);

            vm.Transcript = "Corrected transcript.";
            Assert.True(vm.IsTranscriptDirty);
            await vm.SaveTranscriptCommand.ExecuteAsync(null);

            Assert.False(vm.IsTranscriptDirty);
            Assert.Equal("Corrected transcript.", File.ReadAllText(Path.Combine(folder, "Alice_Smith.txt")));
            Assert.Contains(engine.Name, vm.ChangedEngineNames);

            // A preset has no file, so nothing to play, rename or transcribe.
            vm.SelectedVoice = vm.Voices.Single(v => v.Kind == VoiceKind.Preset);
            await vm.DetailsLoaded;
            Assert.False(vm.IsFileVoiceSelected);
            Assert.False(vm.IsPlayEnabled);
            Assert.Null(vm.WavePeakData);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [AvaloniaFact]
    public async Task FilterNarrowsTheListAndKeepsTheSelectionWhenItStillMatches()
    {
        var folder = CreateVoicesFolder();
        try
        {
            var vm = MakeViewModel();
            var engine = new FakeCloneEngine(folder);
            vm.Initialize(new ITtsEngine[] { engine }, engine);
            await vm.VoicesLoaded;

            vm.SelectedVoice = vm.Voices.Single(v => v.Name == "Bob");
            vm.FilterText = "bo";
            Assert.Single(vm.Voices);
            Assert.Equal("Bob", vm.SelectedVoice?.Name);

            vm.FilterText = "zzz";
            Assert.Empty(vm.Voices);
            Assert.True(vm.IsEmptyTextVisible);

            vm.FilterText = string.Empty;
            Assert.Equal(3, vm.Voices.Count);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}

public class VoiceReferenceTranscriptTests
{
    [Fact]
    public void RequirementFollowsTheEngine()
    {
        Assert.Equal(TranscriptRequirement.Required, VoiceReferenceTranscript.GetRequirement(new CosyVoice3CrispAsr()));
        Assert.Equal(TranscriptRequirement.Optional, VoiceReferenceTranscript.GetRequirement(new VoxCPM2CrispAsr()));
        Assert.Equal(TranscriptRequirement.NotUsed, VoiceReferenceTranscript.GetRequirement(new DotsTtsCrispAsr()));
        Assert.Equal(TranscriptRequirement.NotUsed, VoiceReferenceTranscript.GetRequirement(null));
    }

    [Fact]
    public void ReadRejectsAttributionBlurbsAndMarkup()
    {
        var folder = Path.Combine(Path.GetTempPath(), "SeVoiceTranscriptTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var wav = Path.Combine(folder, "voice.wav");
            File.WriteAllText(wav, string.Empty);
            Assert.Null(VoiceReferenceTranscript.Read(wav));

            File.WriteAllText(Path.ChangeExtension(wav, ".txt"), "This <i>is</i> karaoke <u>markup</u>");
            Assert.Null(VoiceReferenceTranscript.Read(wav));

            Assert.True(VoiceReferenceTranscript.Write(wav, "  Plain spoken words.  ", out _));
            Assert.Equal("Plain spoken words.", VoiceReferenceTranscript.Read(wav));

            // Empty removes the sidecar instead of leaving an empty one behind.
            Assert.True(VoiceReferenceTranscript.Write(wav, "   ", out _));
            Assert.False(File.Exists(Path.ChangeExtension(wav, ".txt")));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}

public class VoicePackCatalogTests
{
    [Fact]
    public void EveryPackIsWellFormed()
    {
        Assert.NotEmpty(VoicePackCatalog.All);
        Assert.Equal(VoicePackCatalog.All.Count, VoicePackCatalog.All.Select(p => p.Id).Distinct().Count());
        foreach (var pack in VoicePackCatalog.All)
        {
            Assert.StartsWith("https://github.com/SubtitleEdit/support-files/releases/download/", pack.Url);
            Assert.EndsWith(".zip", pack.Url);
            Assert.Equal(64, pack.Sha256.Length);
            Assert.True(pack.VoiceCount > 0);
            Assert.True(pack.SizeBytes > 0);
            Assert.False(string.IsNullOrWhiteSpace(pack.License));
        }
    }
}
