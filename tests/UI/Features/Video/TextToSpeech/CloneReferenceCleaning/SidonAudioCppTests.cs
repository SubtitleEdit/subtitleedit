using Nikse.SubtitleEdit.Features.Video.TextToSpeech.CloneReferenceCleaning;
using Nikse.SubtitleEdit.Logic.Download;

namespace UITests.Features.Video.TextToSpeech.CloneReferenceCleaning;

public class SidonAudioCppTests
{
    [Fact]
    public void BuildFileArguments_RunsSpeechToSpeechWithTheSidonFamily()
    {
        var args = SidonAudioCpp.BuildFileArguments("in.wav", "out.wav", "sidon.gguf", "metal");

        Assert.Equal(
            new[] { "--task", "s2s", "--family", "sidon", "--model", "sidon.gguf", "--backend", "metal", "--audio", "in.wav", "--out", "out.wav" },
            args);
    }

    /// <summary>One model load for a whole folder of per-line clips, not one per clip.</summary>
    [Fact]
    public void BuildFolderArguments_UsesOneBatchRun()
    {
        var args = SidonAudioCpp.BuildFolderArguments("clips", "clips/sidon", "sidon.gguf", "cpu");

        Assert.Equal(
            new[] { "--task", "s2s", "--family", "sidon", "--model", "sidon.gguf", "--backend", "cpu", "--batch-audio-dir", "clips", "--out-dir", "clips/sidon" },
            args);
    }

    /// <summary>Per-line clips go back to the rate they were cut at; Sidon writes 48 kHz.</summary>
    [Fact]
    public void ResampleParameters_WritesMono16BitAtTheRequestedRate()
    {
        var parameters = CloneReferenceCleaner.ResampleParameters("cleaned.wav", "line-0001.wav", 24000);

        Assert.Equal("-y -i \"cleaned.wav\" -ar 24000 -ac 1 -c:a pcm_s16le \"line-0001.wav\"", parameters);
    }

    [Fact]
    public void IsValidLocalModelFile_RejectsMissingAndTruncatedFiles()
    {
        var fileName = Path.Combine(Path.GetTempPath(), "sidon-test-" + Guid.NewGuid().ToString("N") + ".gguf");
        Assert.False(SidonAudioCpp.IsValidLocalModelFile(fileName));

        try
        {
            File.WriteAllBytes(fileName, new byte[1024]);
            Assert.False(SidonAudioCpp.IsValidLocalModelFile(fileName));
        }
        finally
        {
            File.Delete(fileName);
        }
    }

    [Fact]
    public void ModelHash_IsRegistered()
    {
        Assert.Equal(
            "5af689683a8142764dd69c956a87455c34f8b685cce90ac4bcb722037bc8e360",
            DownloadHashManager.GetLatestKnownHash(DownloadHashManager.SidonAudioCpp.ModelF32));
    }
}
