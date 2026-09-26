using Nikse.SubtitleEdit.Features.Tools.AiReview;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
