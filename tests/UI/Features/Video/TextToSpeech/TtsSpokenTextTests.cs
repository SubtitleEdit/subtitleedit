using System.Text.Json;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech;

public class TtsSpokenTextTests
{
    [Fact]
    public void KanaAndStyleAreSentToSpeechWithoutChangingTheSubtitle()
    {
        var paragraph = new Paragraph("<i>御茶ノ水まで迎えに来てくれる？</i>", 0, 3000);
        const string spoken = "👂おちゃのみずまで迎えに来てくれる？";

        Assert.Equal(spoken, TtsSpokenText.Resolve(paragraph.Text, spoken));
        Assert.Equal("<i>御茶ノ水まで迎えに来てくれる？</i>", paragraph.Text);
        Assert.Equal("御茶ノ水まで迎えに来てくれる？", TtsSpokenText.Resolve(paragraph.Text, " \n "));
    }

    [Fact]
    public void SessionJsonPreservesBothTextsAndAcceptsOlderSessions()
    {
        var item = new TtsImportExportItem { Text = "御茶ノ水", SpokenText = "⏸️おちゃのみず" };

        var restored = JsonSerializer.Deserialize<TtsImportExportItem>(JsonSerializer.Serialize(item))!;
        Assert.Equal(item.Text, restored.Text);
        Assert.Equal(item.SpokenText, restored.SpokenText);

        var older = JsonSerializer.Deserialize<TtsImportExportItem>("""{"Text":"御茶ノ水"}""")!;
        Assert.Null(older.SpokenText);
        Assert.Equal(older.Text, TtsSpokenText.Resolve(older.Text, older.SpokenText));
    }

    [Fact]
    public void OverridesSurviveMergeContinuationLines()
    {
        var first = new Paragraph("御茶ノ水まで", 0, 1000);
        var second = new Paragraph("<i>迎えに来て</i>", 1100, 2000);
        var third = new Paragraph("烏丸通り", 3000, 4000);
        var fourth = new Paragraph("ありがとう", 5000, 6000);
        var overrides = new Dictionary<Paragraph, string>(ReferenceEqualityComparer.Instance)
        {
            [first] = "おちゃのみずまで",
            [third] = "からすま通り",
        };

        // The merge builds every paragraph anew - even the ones it did not merge.
        var merged = new Paragraph("御茶ノ水まで 迎えに来て", 0, 2000);
        var thirdCopy = new Paragraph(third);
        var fourthCopy = new Paragraph(fourth);

        var remapped = TtsSpokenText.RemapAfterMerge(
            new[] { first, second, third, fourth }, overrides, new[] { merged, thirdCopy, fourthCopy });

        Assert.Equal("おちゃのみずまで 迎えに来て", remapped[merged]);
        Assert.Equal("からすま通り", remapped[thirdCopy]);
        Assert.False(remapped.ContainsKey(fourthCopy));
    }

    [Fact]
    public void PickerIncludesIrodoriAsACloneCapableJapaneseEngine()
    {
        var engine = Assert.Single(TtsEngineCatalog.CreateAll(null!).OfType<IrodoriTtsCrispAsr>());
        Assert.False(engine.HasLanguageParameter);
        Assert.True(engine.SupportsVoiceCloning);
        Assert.Contains(TtsEngineCatalog.CreateVoiceCloningEngines(), candidate => candidate is IrodoriTtsCrispAsr);
    }
}
