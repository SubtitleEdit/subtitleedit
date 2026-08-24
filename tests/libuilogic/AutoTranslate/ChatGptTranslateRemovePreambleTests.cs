using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace LibUiLogicTests.AutoTranslate;

/// <summary>
/// <see cref="ChatGptTranslate.RemovePreamble"/> is the shared post-processing step every
/// OpenAI-compatible engine (ChatGPT, DeepSeek, Groq, Ollama, llama.cpp, ...) runs a raw model
/// response through. A thinking model that closes its &lt;think&gt; block behaves fine; one that
/// gets cut off mid-thought (hits its token budget before ever reaching the translation - the
/// Qwen 3.5 leakage bug, see the note on the curated Qwen entries in LlamaCppServerManager) used
/// to pass the raw reasoning text straight through as if it were the translation.
/// </summary>
public class ChatGptTranslateRemovePreambleTests
{
    [Fact]
    public void ClosedThinkBlock_IsStrippedFromTranslation()
    {
        var result = ChatGptTranslate.RemovePreamble("Hello", "<think>reasoning here</think>Hej");

        Assert.Equal("Hej", result);
    }

    /// <summary>
    /// The model hit its token budget before closing the tag: what follows "&lt;think&gt;" is raw
    /// internal monologue, not a translation. Returning it unstripped ships garbage into the
    /// subtitle file (#Qwen 3.5 thinking-leakage bug); returning empty instead lets the caller's
    /// existing retry-on-no-progress logic (DoAutoTranslate) re-request the line.
    /// </summary>
    [Fact]
    public void UnterminatedThinkBlock_ReturnsEmpty()
    {
        var result = ChatGptTranslate.RemovePreamble("Hello",
            "<think>\nThinking Process:\n1. **Analyze the Request:**\n   *   Source Text:");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void NoThinkBlock_IsUnaffected()
    {
        var result = ChatGptTranslate.RemovePreamble("Hello", "Hej");

        Assert.Equal("Hej", result);
    }

    [Fact]
    public void HerePreamble_IsStrippedAfterThinkBlockRemoval()
    {
        var result = ChatGptTranslate.RemovePreamble("Hello", "<think>x</think>Here is the translation: Hej");

        Assert.Equal("Hej", result);
    }
}
