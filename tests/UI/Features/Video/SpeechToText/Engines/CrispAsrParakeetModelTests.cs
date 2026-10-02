using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText.Engines;

public class CrispAsrParakeetModelTests
{
    [Theory]
    [InlineData("phonon2-q4_k.gguf", "402 MB")]
    [InlineData("phonon2-q8_0.gguf", "674 MB")]
    [InlineData("phonon2-f16.gguf", "1.26 GB")]
    public void Models_IncludePhonon2FromCstrGguf(string name, string size)
    {
        var model = Assert.Single(new CrispAsrParakeet().Models, m => m.Name == name);

        Assert.Equal(size, model.Size);
        Assert.Equal("https://huggingface.co/cstr/phonon2-GGUF/resolve/main/" + name, Assert.Single(model.Urls));
    }

    [Fact]
    public void Models_DownloadToTheirOwnName()
    {
        // GetWhisperModelDownloadFileName saves under the URL's file name while IsModelInstalled
        // looks for Name - a mismatch would download a model that never reads as installed.
        foreach (var model in new CrispAsrParakeet().Models)
        {
            foreach (var url in model.Urls)
            {
                Assert.Equal(model.Name, Path.GetFileName(url));
            }
        }
    }

    [Fact]
    public void Models_HaveUniqueNames()
    {
        var names = new CrispAsrParakeet().Models.Select(m => m.Name).ToList();

        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void IsModelEnglishOnly_FlagsExactlyTheEnglishOnlyParakeetModels()
    {
        var englishOnly = new CrispAsrParakeet().Models
            .Where(SpeechToTextViewModel.IsModelEnglishOnly)
            .Select(m => m.Name)
            .ToList();

        Assert.Equal(new[]
        {
            "phonon2-q4_k.gguf", "phonon2-q8_0.gguf", "phonon2-f16.gguf",
            "parakeet-rnnt-0.6b-q4_k.gguf", "parakeet-rnnt-0.6b-f16.gguf",
            "parakeet-rnnt-1.1b-q4_k.gguf", "parakeet-rnnt-1.1b-f16.gguf",
            "parakeet-tdt-1.1b-q4_k.gguf", "parakeet-tdt-1.1b-q8_0.gguf", "parakeet-tdt-1.1b.gguf",
            "parakeet-tdt-0.6b-v2-q4_k.gguf", "parakeet-tdt-0.6b-v2-q8_0.gguf", "parakeet-tdt-0.6b-v2.gguf",
            "parakeet-tdt_ctc-110m-q4_k.gguf", "parakeet-tdt_ctc-110m-q8_0.gguf", "parakeet-tdt_ctc-110m.gguf",
            "parakeet-tdt_ctc-1.1b-q4_k.gguf", "parakeet-tdt_ctc-1.1b-q8_0.gguf", "parakeet-tdt_ctc-1.1b.gguf",
        }, englishOnly);
    }

    [Theory]
    [InlineData("phonon2-q8_0.gguf", "de", true)]
    [InlineData("parakeet-rnnt-0.6b-q4_k.gguf", "fr", true)]
    [InlineData("phonon2-q8_0.gguf", "en", false)]
    [InlineData("phonon2-q8_0.gguf", "auto", false)]
    [InlineData("parakeet-rnnt-0.6b-q4_k.gguf", "auto", false)]
    [InlineData("parakeet-tdt-0.6b-v3-q4_k.gguf", "de", false)]
    public void ShouldWarnEnglishOnlyModel_SkipsEnglishAndAutoDetect(string name, string language, bool expected)
    {
        var model = Assert.Single(new CrispAsrParakeet().Models, m => m.Name == name);

        Assert.Equal(expected, SpeechToTextViewModel.ShouldWarnEnglishOnlyModel(model, language));
    }
}
