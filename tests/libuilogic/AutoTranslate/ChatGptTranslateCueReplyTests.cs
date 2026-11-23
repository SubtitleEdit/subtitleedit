using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// Batch replies are often wrapped in a ```json fence or come with a line around the array;
/// those used to fail the parse and cost the halving retries before single-line mode.
/// </summary>
public class ChatGptTranslateCueReplyTests
{
    [Theory]
    [InlineData("[{\"id\":1,\"text\":\"Hej\"},{\"id\":2,\"text\":\"Farvel\"}]")]
    [InlineData("```json\n[{\"id\":1,\"text\":\"Hej\"},{\"id\":2,\"text\":\"Farvel\"}]\n```")]
    [InlineData("```\n[{\"id\":1,\"text\":\"Hej\"},{\"id\":2,\"text\":\"Farvel\"}]\n```")]
    [InlineData("Here is the translation:\n[{\"id\":1,\"text\":\"Hej\"},{\"id\":2,\"text\":\"Farvel\"}]\nHope this helps!")]
    [InlineData("[{\"Id\":1,\"Text\":\"Hej\"},{\"Id\":2,\"Text\":\"Farvel\"}]")]
    public void ParseCueReply_ReadsTheArray(string reply)
    {
        var cues = ChatGptTranslate.ParseCueReply(reply);

        Assert.NotNull(cues);
        Assert.Equal(2, cues!.Length);
        Assert.Equal(1, cues[0].Id);
        Assert.Equal("Hej", cues[0].Text);
        Assert.Equal(2, cues[1].Id);
        Assert.Equal("Farvel", cues[1].Text);
    }

    [Fact]
    public void ParseCueReply_KeepsBracketsInsideTheText()
    {
        var cues = ChatGptTranslate.ParseCueReply("```json\n[{\"id\":1,\"text\":\"[MUSIK] Hej\"}]\n```");

        Assert.NotNull(cues);
        Assert.Equal("[MUSIK] Hej", cues![0].Text);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Sorry, I cannot help with that.")]
    [InlineData("```json\n[{\"id\":1,\"text\":\"Hej\"}\n```")]
    public void ParseCueReply_NullWhenNoArray(string? reply)
    {
        Assert.Null(ChatGptTranslate.ParseCueReply(reply));
    }
}
