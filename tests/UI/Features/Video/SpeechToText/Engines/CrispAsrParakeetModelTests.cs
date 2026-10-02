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

    [Theory]
    [InlineData("phonon2-q4_k.gguf", true)]
    [InlineData("phonon2-q8_0.gguf", true)]
    [InlineData("phonon2-f16.gguf", true)]
    [InlineData("parakeet-ultra-q8_0.gguf", false)]
    [InlineData("parakeet-tdt-0.6b-v3-q4_k.gguf", false)]
    public void IsModelEnglishOnly_WarnsForPhonon2(string name, bool expected)
    {
        var model = Assert.Single(new CrispAsrParakeet().Models, m => m.Name == name);

        Assert.Equal(expected, SpeechToTextViewModel.IsModelEnglishOnly(model));
    }
}
