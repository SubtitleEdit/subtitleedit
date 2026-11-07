using Nikse.SubtitleEdit.Features.Tools.AiReview;

namespace UITests.Features.Tools.AiReview;

public class AiReviewContextGeneratorTests
{
    [Fact]
    public void BuildSystemPrompt_InsertsContextBeforeProtocol()
    {
        var prompt = AiReviewProtocol.BuildSystemPrompt("Proofread {language}.", "English", "Names: Kaito");

        Assert.StartsWith("Proofread English.", prompt);
        var contextIndex = prompt.IndexOf("Names: Kaito", System.StringComparison.Ordinal);
        Assert.True(contextIndex > 0);
        Assert.True(contextIndex < prompt.IndexOf(AiReviewProtocol.ProtocolText, System.StringComparison.Ordinal));
        Assert.EndsWith(AiReviewProtocol.ProtocolText, prompt);
    }

    [Fact]
    public void BuildSystemPrompt_EmptyContext_AddsNoHeading()
    {
        var prompt = AiReviewProtocol.BuildSystemPrompt("Proofread.", "English", "  ");

        Assert.Equal("Proofread." + AiReviewProtocol.ProtocolText, prompt);
    }

    [Theory]
    [InlineData(2048)]
    [InlineData(4096)]
    [InlineData(8192)]
    [InlineData(16384)]
    public void TokensPerPartForContextSize_PartPromptAndAnswerFitTheContext(int contextSize)
    {
        var part = AiReviewContextGenerator.TokensPerPartForContextSize(contextSize);

        // #16701: a 4096 context got 3000-token parts and an uncapped answer. Even if the estimate is
        // a quarter low, part + prompt + answer must still fit.
        Assert.True(part * 5 / 4 + 300 + AiReviewContextGenerator.MaxAnswerTokens <= contextSize);
        Assert.True(part <= AiReviewContextGenerator.MaxTokensPerPart);
        Assert.True(part >= 256);
    }

    [Fact]
    public void TokensPerPartForContextSize_LargeContextKeepsDefaultPartSize()
    {
        Assert.Equal(AiReviewContextGenerator.MaxTokensPerPart, AiReviewContextGenerator.TokensPerPartForContextSize(8192));
        Assert.Equal(AiReviewContextGenerator.MaxTokensPerPart, AiReviewContextGenerator.TokensPerPartForContextSize(262144));
    }

    [Fact]
    public async Task GenerateAsync_SmallContextUsesMoreParts()
    {
        var lines = Enumerable.Range(1, 1000).Select(i => $"Line number {i} of the subtitle with a few more words in it.").ToList();
        var calls = 0;
        AiChatFunc chat = (_, _, _, _) =>
        {
            calls++;
            return Task.FromResult("{\"names\":[],\"terms\":[],\"summary\":\"x\"}");
        };

        await AiReviewContextGenerator.GenerateAsync(lines, "English", chat, null, CancellationToken.None, null, AiReviewContextGenerator.MaxTokensPerPart);
        var defaultCalls = calls;
        calls = 0;
        await AiReviewContextGenerator.GenerateAsync(lines, "English", chat, null, CancellationToken.None, null, AiReviewContextGenerator.TokensPerPartForContextSize(4096));

        Assert.True(calls > defaultCalls);
    }

    [Fact]
    public void BuildParts_SplitsOnTokenBudgetWithoutSplittingLines()
    {
        var lines = Enumerable.Range(0, 10).Select(i => new string('a', 40)).ToList(); // ~11 tokens each

        var parts = AiReviewContextGenerator.BuildParts(lines, 30);

        Assert.Equal(5, parts.Count);
        Assert.All(parts, p => Assert.Equal(2, p.Split('\n', System.StringSplitOptions.RemoveEmptyEntries).Length));
    }

    [Fact]
    public void EstimateTokens_CountsCjkPerCharacter()
    {
        Assert.Equal(4, AiReviewContextGenerator.EstimateTokens("東京に行く"[..4]));
        Assert.Equal(2, AiReviewContextGenerator.EstimateTokens("abcdefgh"));
    }

    [Fact]
    public void MergeEntries_DropsInventedAndPicksMostFrequentSpelling()
    {
        var text = "Kaito went home.\nKAITO!\nKaito, wait.\nAunt Mei\nwaved.";

        var merged = AiReviewContextGenerator.MergeEntries(new[] { "KAITO", "Kaito", "Hiroshi", "Aunt Mei waved" }, text, 10);

        Assert.Equal(new List<string> { "Kaito", "Aunt Mei waved" }, merged);
    }

    [Fact]
    public async Task GenerateAsync_MergesPartsAndWritesSynopsis()
    {
        var lines = new List<string> { "Kaito ran to the station.", "Mei checked the warp core." };
        var calls = new List<(string System, bool Json)>();
        var replies = new Queue<string>(new[]
        {
            "```json\n{\"names\":[\"Kaito\",\"Ghost\"],\"terms\":[],\"summary\":\"Part one.\"}\n```",
            "{\"names\":[\"Mei\"],\"terms\":[\"warp core\",\"kaito\"],\"summary\":\"Part two.\"}",
            "One synopsis.",
        });
        var progress = new List<int>();

        var result = await AiReviewContextGenerator.GenerateAsync(lines, "English",
            (system, user, json, ct) =>
            {
                calls.Add((system, json));
                return Task.FromResult(replies.Dequeue());
            },
            (part, _) => progress.Add(part), CancellationToken.None, maxTokensPerPart: 10);

        Assert.Equal("Names: Kaito, Mei\nTerms: warp core\nSynopsis: One synopsis.", result.Replace("\r\n", "\n"));
        Assert.Equal(new[] { true, true, false }, calls.Select(c => c.Json));
        Assert.Equal(new[] { 1, 2, 0 }, progress);
    }

    [Fact]
    public async Task GenerateAsync_InvalidReply_ReturnsEmpty()
    {
        var result = await AiReviewContextGenerator.GenerateAsync(new[] { "Hello there." }, "English",
            (_, _, _, _) => Task.FromResult("sorry, no"), null, CancellationToken.None);

        Assert.Equal(string.Empty, result);
    }
}
