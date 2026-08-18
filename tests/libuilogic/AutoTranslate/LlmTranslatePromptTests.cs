using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// The two prompt shapes the local-LLM translate engines send. Completion-format models
/// (MiLMMT-46) need the text inside the template and a trailing target-language cue, and issue
/// #13803 is that only the built-in llama.cpp engine could produce it - the same model behind LM
/// Studio, KoboldCpp, Ollama or an OpenAI-compatible server got the chat shape instead.
/// </summary>
public class LlmTranslatePromptTests
{
    private const string CompletionTemplate = "Translate this from {0} to {1}:\n{0}: {2}\n{1}:";
    private const string ChatTemplate = "Translate from {0} to {1}, keep the meaning:";

    [Fact]
    public void FillCompletionTemplate_EmbedsTextInsideTemplate()
    {
        var result = LlmTranslatePrompt.FillCompletionTemplate(
            CompletionTemplate, "English", "Danish",
            "We've lost the signal.\nCheck the antenna on the roof.");

        Assert.Equal(
            "Translate this from English to Danish:\nEnglish: We've lost the signal.\nCheck the antenna on the roof.\nDanish:",
            result);
    }

    // The text is substituted last, so brace sequences in subtitle text (ASSA override tags)
    // must survive as-is and can never be treated as a placeholder.
    [Fact]
    public void FillCompletionTemplate_TextWithBraces_IsNotReSubstituted()
    {
        var result = LlmTranslatePrompt.FillCompletionTemplate(
            CompletionTemplate, "English", "Danish", @"{\an8}Look {0} up there.");

        Assert.Equal(
            "Translate this from English to Danish:\nEnglish: {\\an8}Look {0} up there.\nDanish:",
            result);
    }

    [Theory]
    [InlineData(CompletionTemplate, true)]
    [InlineData(ChatTemplate, false)]
    [InlineData(null, false)]
    public void IsCompletionTemplate_DecidesOnTheTextPlaceholder(string? template, bool expected)
    {
        Assert.Equal(expected, LlmTranslatePrompt.IsCompletionTemplate(template!));
    }

    // Completion prompts go on the wire with real line breaks: under the "<br />" placeholder
    // encoding MiLMMT-46 still translates but mirrors placeholder fragments into its output.
    [Fact]
    public void BuildEncodedUserMessage_CompletionTemplate_IsOneBlockWithRealNewlines()
    {
        var result = LlmTranslatePrompt.BuildEncodedUserMessage(
            CompletionTemplate, "English", "Danish", "Line one.\nLine two.");

        Assert.Equal(
            "Translate this from English to Danish:\\nEnglish: Line one.\\nLine two.\\nDanish:",
            result);
        Assert.DoesNotContain("<br />", result);
    }

    // The historical chat wire format is unchanged: prompt, a real blank line, then the text,
    // with line breaks inside either part as the "<br />" placeholder the engines decode back.
    [Fact]
    public void BuildEncodedUserMessage_ChatTemplate_KeepsTheHistoricalWireFormat()
    {
        var result = LlmTranslatePrompt.BuildEncodedUserMessage(
            ChatTemplate, "English", "Danish", "Line one.\nLine two.");

        Assert.Equal(
            "Translate from English to Danish, keep the meaning:\\n\\nLine one.<br />Line two.",
            result);
    }

    // The engines used to string.Format the prompt, which throws on any brace the user leaves in
    // it - including the "{2}" this feature asks them to type.
    [Theory]
    [InlineData("Translate {0} to {1} and keep {curly} tags:")]
    [InlineData("Translate {0} to {1}, answer as {\"text\": \"...\"}:")]
    public void BuildEncodedUserMessage_BracesInTheUserPrompt_DoNotThrow(string template)
    {
        var result = LlmTranslatePrompt.BuildEncodedUserMessage(template, "English", "Danish", "Hello.");

        Assert.Contains("English", result);
        Assert.Contains("Danish", result);
        Assert.EndsWith("Hello.", result);
    }
}
