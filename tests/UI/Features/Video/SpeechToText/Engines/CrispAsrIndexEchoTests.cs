using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

namespace UITests.Features.Video.SpeechToText.Engines;

public class CrispAsrIndexEchoTests
{
    /// <summary>Real crispasr v0.8.41 output: Chinese transcript line, then the translation.</summary>
    [Fact]
    public void GetTranslation_KeepsTheTranslationLine()
    {
        var text = "今天天气很好，我们一起去公园散步吧。" + Environment.NewLine +
                   "The weather is great today. Let's go for a walk in the park.";

        Assert.Equal("The weather is great today. Let's go for a walk in the park.", CrispAsrIndexEcho.GetTranslation(text));
    }

    /// <summary>The Japanese target wraps every translation in corner brackets.</summary>
    [Fact]
    public void GetTranslation_StripsEnclosingCornerBrackets()
    {
        var text = "下午三点，我在图书馆门口等你。" + Environment.NewLine + "「午後3時に図書館の入り口で待ちますね。」";

        Assert.Equal("午後3時に図書館の入り口で待ちますね。", CrispAsrIndexEcho.GetTranslation(text));
    }

    [Fact]
    public void GetTranslation_KeepsInnerQuotations()
    {
        var text = "他说：“好”。" + Environment.NewLine + "「彼は」と「良い」と言った";

        Assert.Equal("「彼は」と「良い」と言った", CrispAsrIndexEcho.GetTranslation(text));
    }

    /// <summary>An unparsed window comes back as one whole-window line.</summary>
    [Fact]
    public void GetTranslation_LeavesSingleLineCuesAlone()
    {
        Assert.Equal("Hello there", CrispAsrIndexEcho.GetTranslation("Hello there"));
    }

    [Theory]
    [InlineData("index-echo-2b-q8_0.gguf", "index-echo-2b-decoder-q8_0.gguf")]
    [InlineData("index-echo-2b-f16.gguf", "index-echo-2b-decoder-f16.gguf")]
    public void GetDecoderFileName_PairsTowerWithDecoder(string tower, string decoder)
    {
        Assert.Equal(decoder, CrispAsrIndexEcho.GetDecoderFileName(tower));
    }

    /// <summary>Each model downloads its tower and the decoder crispasr loads beside it.</summary>
    [Fact]
    public void Models_DownloadTowerAndDecoder()
    {
        foreach (var model in new CrispAsrIndexEcho().Models)
        {
            Assert.Equal(2, model.Urls.Length);
            Assert.EndsWith("/" + model.Name, model.Urls[0]);
            Assert.EndsWith("/" + CrispAsrIndexEcho.GetDecoderFileName(model.Name), model.Urls[1]);
        }
    }
}
