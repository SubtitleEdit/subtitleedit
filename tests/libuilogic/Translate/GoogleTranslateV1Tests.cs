using System.Reflection;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.Translate;

public class GoogleTranslateV1Tests
{
    // ConvertJsonObjectToStringLines is private; invoke it directly so the response
    // parser can be tested without a live network call.
    private static List<string> ConvertJsonObjectToStringLines(string json)
    {
        var method = typeof(GoogleTranslateV1).GetMethod(
            "ConvertJsonObjectToStringLines",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return (List<string>)method.Invoke(null, new object[] { json })!;
    }

    [Fact]
    public void SegmentWithTrailingEscapedNewline_DoesNotProduceBlankLine()
    {
        // Google returns a two-line dialogue cue as two segments; the first segment keeps
        // the source line break as an escaped trailing "\n". Before the fix for #13614 the
        // trailing newline survived and AppendLine doubled it into a blank middle line.
        var json = "[[[\"- Ne.\\n\",\"- No.\\n\",null,null,3],[\"- Přijdeme pozdě.\",\"- We're gonna be late.\",null,null,3]],null,\"en\"]";

        var lines = ConvertJsonObjectToStringLines(json);

        Assert.Equal(new[] { "- Ne.", "- Přijdeme pozdě." }, lines);
        Assert.DoesNotContain(lines, string.IsNullOrWhiteSpace);
    }

    [Fact]
    public void SegmentWithTrailingEscapedCrLf_DoesNotProduceBlankLine()
    {
        var json = "[[[\"- Ne.\\r\\n\",\"- No.\\r\\n\",null,null,3],[\"- Přijdeme pozdě.\",\"- We're gonna be late.\",null,null,3]],null,\"en\"]";

        var lines = ConvertJsonObjectToStringLines(json);

        Assert.Equal(new[] { "- Ne.", "- Přijdeme pozdě." }, lines);
    }

    [Fact]
    public void SingleSegment_IsReturnedUnchanged()
    {
        var json = "[[[\"Ahoj.\",\"Hello.\",null,null,3]],null,\"en\"]";

        var lines = ConvertJsonObjectToStringLines(json);

        Assert.Equal(new[] { "Ahoj." }, lines);
    }
}
