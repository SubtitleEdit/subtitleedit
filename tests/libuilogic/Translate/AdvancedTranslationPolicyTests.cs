using System.Collections.ObjectModel;
using System.Net;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate.Llm;

namespace LibUiLogicTests.Translate;

public class AdvancedTranslationPolicyTests
{
    private static ObservableCollection<TranslateRow> Rows() => new()
    {
        new() { Text = "Where were you last night?" }, new() { Text = "I was working until late." },
    };

    [Fact]
    public async Task InvalidBatchShrinksWhileKeepingAdvancedWireProtocol()
    {
        var client = new FakeClient((_, user) => user.Contains("I was working") ? "invalid" : "{\"1\":\"Wo warst du letzte Nacht?\"}");
        using var translator = new TestTranslator(new() { BatchSize = 2 }, client);
        translator.Initialize();
        var rows = Rows();
        var count = await translator.TranslateBatchAsync(rows, 0, "English", "German", TestContext.Current.CancellationToken);
        Assert.Equal(1, count);
        Assert.Equal("Wo warst du letzte Nacht?", rows[0].TranslatedText);
        Assert.True(string.IsNullOrEmpty(rows[1].TranslatedText));
        Assert.Equal(3, client.Calls);
        Assert.True(client.AlwaysStructured);
    }

    [Fact]
    public async Task EmptySingleLineFailsWithoutSwitchingToPlainTranslation()
    {
        var client = new FakeClient((_, _) => "{\"1\":\"\"}");
        using var translator = new TestTranslator(new(), client);
        translator.Initialize();
        await Assert.ThrowsAsync<HttpRequestException>(() => translator.TranslateBatchAsync(Rows(), 1, "English", "German", TestContext.Current.CancellationToken));
        Assert.Equal(2, client.Calls);
        Assert.True(client.AlwaysStructured);
    }

    [Fact]
    public async Task AuthenticationErrorIsNotRetriedByPolicy()
    {
        var client = new FakeClient((_, _) => throw new HttpRequestException("denied", null, HttpStatusCode.Unauthorized));
        using var translator = new TestTranslator(new(), client);
        translator.Initialize();
        await Assert.ThrowsAsync<HttpRequestException>(() => translator.TranslateBatchAsync(Rows(), 0, "English", "German", TestContext.Current.CancellationToken));
        Assert.Equal(1, client.Calls);
    }

    [Theory]
    [InlineData("{\"1\":\"a\",\"1\":\"b\"}")]
    [InlineData("{\"01\":\"a\"}")]
    [InlineData("{\"1\":\"a\",\"note\":\"extra\"}")]
    public void InvalidIdentifiersCannotBeAcceptedAsAlignedTranslations(string reply)
    {
        Assert.Empty(AdvancedTranslationProtocol.ParseTranslations(reply));
    }

    private sealed class TestTranslator(AdvancedTranslationSettings settings, ILlmClient client) : AdvancedTranslatorBase
    {
        public override string Name => "Test";
        public override string Url => "https://example.test";
        protected override AdvancedTranslationSettings Settings => settings;
        protected override string GetApiUrl() => Url;
        protected override ILlmClient CreateClient() => client;
    }

    private sealed class FakeClient(Func<string, string, string> reply) : ILlmClient
    {
        public int Calls { get; private set; }
        public bool AlwaysStructured { get; private set; } = true;
        public string Error => "invalid reply";
        public bool ReplyFromReasoning => false;
        public bool LastRequestDisabledThinking => false;
        public Task<string> ChatAsync(string url, string systemPrompt, string userContent, string? responseFormatJson, CancellationToken cancellationToken, string? model = null, int defaultMaxTokens = -1, double temperatureBump = 0)
        {
            Calls++; AlwaysStructured &= responseFormatJson != null;
            return Task.FromResult(reply(systemPrompt, userContent));
        }
        public void Dispose() { }
    }
}
