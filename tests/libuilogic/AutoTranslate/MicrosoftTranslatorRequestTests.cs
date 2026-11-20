using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

public class MicrosoftTranslatorRequestTests
{
    [Fact]
    public void BuildRequestJson_OneElementPerLine_SkipsBlankLines()
    {
        var json = MicrosoftTranslator.BuildRequestJson(["Конечно.", "", "Привет \"ты\"."]);

        Assert.Equal("[{\"Text\":\"Конечно.\"},{\"Text\":\"Привет \\\"ты\\\".\"}]", json);
    }

    [Fact]
    public void ParseResponse_ReturnsTranslationsInOrder()
    {
        const string reply = "[{\"translations\":[{\"text\":\"Por supuesto.\",\"to\":\"es\"}]},{\"translations\":[{\"text\":\"Hola.\",\"to\":\"es\"}]}]";

        Assert.Equal(["Por supuesto.", "Hola."], MicrosoftTranslator.ParseResponse(reply));
    }

    [Fact]
    public void MapResponseToLines_PutsBlankLinesBack()
    {
        var result = MicrosoftTranslator.MapResponseToLines(["Конечно.", "", "Привет."], ["Por supuesto.", "Hola."]);

        Assert.Equal(["Por supuesto.", "", "Hola."], result);
    }

    [Fact]
    public void MapResponseToLines_ShortReply_LeavesMissingLineEmptyNotSource()
    {
        var result = MicrosoftTranslator.MapResponseToLines(["Конечно.", "Привет."], ["Por supuesto."]);

        Assert.Equal(["Por supuesto.", ""], result);
    }
}
