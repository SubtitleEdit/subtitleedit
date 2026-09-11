using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace UITests.Features.Video.TextToSpeech.Engines;

/// <summary>
/// "Rename voice..." on the TTS voice combo: the clone engines list voices straight from their
/// voices folder, so a rename has to move the reference WAV, keep its sidecars with it, and drop
/// the cached prepared copy that is keyed by the old file name.
/// </summary>
public class VoiceFileRenameTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "se-voice-rename-" + Guid.NewGuid().ToString("N"));

    public VoiceFileRenameTests()
    {
        Directory.CreateDirectory(_folder);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch
        {
            // best effort
        }
    }

    private Voice MakeVoice(string baseName)
    {
        var wav = Path.Combine(_folder, baseName + ".wav");
        File.WriteAllBytes(wav, new byte[64]);
        return new Voice(new Qwen3TtsVoice(baseName.Replace('_', ' '), wav));
    }

    [Fact]
    public void Rename_MovesWavSidecarsAndDropsPreparedCopy()
    {
        var voice = MakeVoice("Old_Name");
        File.WriteAllText(Path.Combine(_folder, "Old_Name.txt"), "transcript");
        var prepared = CloneReferenceTail.GetPreparedFileName(Path.Combine(_folder, "Old_Name.wav"));
        Directory.CreateDirectory(Path.GetDirectoryName(prepared)!);
        File.WriteAllBytes(prepared, new byte[8]);
        File.WriteAllText(prepared + ".stamp", "v1");

        var result = VoiceFileRename.Rename(voice, "New Name", out var error);

        Assert.Equal(string.Empty, error);
        Assert.Equal(Path.Combine(_folder, "New_Name.wav"), result);
        Assert.True(File.Exists(Path.Combine(_folder, "New_Name.wav")));
        Assert.Equal("transcript", File.ReadAllText(Path.Combine(_folder, "New_Name.txt")));
        Assert.False(File.Exists(Path.Combine(_folder, "Old_Name.wav")));
        Assert.False(File.Exists(Path.Combine(_folder, "Old_Name.txt")));
        Assert.False(File.Exists(prepared));
        Assert.False(File.Exists(prepared + ".stamp"));
    }

    [Fact]
    public void Rename_RefusesExistingNameAndLeavesFilesAlone()
    {
        var voice = MakeVoice("A");
        MakeVoice("B");

        var result = VoiceFileRename.Rename(voice, "B", out var error);

        Assert.Null(result);
        Assert.NotEqual(string.Empty, error);
        Assert.True(File.Exists(Path.Combine(_folder, "A.wav")));
        Assert.True(File.Exists(Path.Combine(_folder, "B.wav")));
    }

    [Fact]
    public void Rename_RefusesEmptyAndInvalidNames()
    {
        var voice = MakeVoice("A");

        Assert.Null(VoiceFileRename.Rename(voice, "   ", out _));
        Assert.Null(VoiceFileRename.Rename(voice, "bad/name", out _));
        Assert.Null(VoiceFileRename.Rename(voice, PerLineReferenceStaging.Prefix + "x", out _));
        Assert.True(File.Exists(Path.Combine(_folder, "A.wav")));
    }

    [Fact]
    public void CanRename_IsFalseForPresetsDefaultAndPerLineMarker()
    {
        Assert.False(VoiceFileRename.CanRename(null));
        Assert.False(VoiceFileRename.CanRename(new Voice(new Qwen3TtsVoice("Default", string.Empty))));
        Assert.False(VoiceFileRename.CanRename(new Voice(new CosyVoice3Voice("Preset", "preset"))));
        Assert.False(VoiceFileRename.CanRename(new Voice(new PerLineCloneVoice())));

        var staged = Path.Combine(_folder, PerLineReferenceStaging.Prefix + "clip.wav");
        File.WriteAllBytes(staged, new byte[8]);
        Assert.False(VoiceFileRename.CanRename(new Voice(new Qwen3TtsVoice("clip", staged))));

        Assert.True(VoiceFileRename.CanRename(MakeVoice("Real")));
    }
}
