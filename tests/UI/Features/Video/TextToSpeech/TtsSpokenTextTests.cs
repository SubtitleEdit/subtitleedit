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
    public void PickerIncludesIrodoriAsACloneCapableJapaneseEngine()
    {
        var engine = Assert.Single(TtsEngineCatalog.CreateAll(null!).OfType<IrodoriTtsCrispAsr>());
        Assert.False(engine.HasLanguageParameter);
        Assert.True(engine.SupportsVoiceCloning);
        Assert.Contains(TtsEngineCatalog.CreateVoiceCloningEngines(), candidate => candidate is IrodoriTtsCrispAsr);
    }
}
