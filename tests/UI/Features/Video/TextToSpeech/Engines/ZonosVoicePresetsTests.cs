using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Download;
using System.Linq;

namespace UITests.Features.Video.TextToSpeech.Engines;

public class ZonosVoicePresetsTests
{
    [Fact]
    public void PresetsAreUniqueAndHashed()
    {
        var all = ZonosVoicePresets.All;

        Assert.Equal(27, all.Length);
        Assert.Equal(all.Length, all.Select(p => p.FileStem).Distinct().Count());
        Assert.Equal(all.Length, all.Select(p => p.DisplayName).Distinct().Count());
        Assert.All(all, p => Assert.Matches("^[0-9a-f]{64}$", p.Sha256));
        Assert.All(all, p => Assert.StartsWith(p.Name + "_", p.FileStem));
    }

    [Fact]
    public void ChineseVoicesLiveInZyphrasMandarinFolder()
    {
        var meilin = ZonosVoicePresets.FindByStem("Meilin_zh-energetic_F_1");

        Assert.NotNull(meilin);
        Assert.Equal(
            "https://huggingface.co/Zyphra/ZONOS3-Mini/resolve/" + ZonosTtsCrispAsr.RepoRevision
            + "/gguf/voices/Mandarin%20Chinese/Meilin_zh-energetic_F_1.embedding.npy",
            ZonosTtsCrispAsrDownloadService.GetVoiceUrl(meilin!));
    }

    [Fact]
    public void ModelUrlsArePinnedToTheRevision()
    {
        Assert.Equal(
            "https://huggingface.co/Zyphra/ZONOS3-Mini/resolve/" + ZonosTtsCrispAsr.RepoRevision + "/gguf/backbone-ptq.gguf",
            ZonosTtsCrispAsrDownloadService.GetModelUrl(ZonosTtsCrispAsr.TalkerFileName));
    }

    [Fact]
    public void UnknownStemIsNotAPreset()
    {
        Assert.Null(ZonosVoicePresets.FindByStem(string.Empty));
        Assert.Null(ZonosVoicePresets.FindByStem("imogen_en-british_f_1"));
    }
}
