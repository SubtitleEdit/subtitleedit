using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System.Text.Json;

namespace UITests.Features.Video.TextToSpeech.Engines;

public class OpenAiCompatibleSpeechTests
{
    [Fact]
    public void RequestBodyHasTheOpenAiShape()
    {
        var s = Se.Settings.Video.TextToSpeech;
        var (savedSpeed, savedInstructions) = (s.OpenAiCompatibleSpeed, s.OpenAiCompatibleInstructions);
        try
        {
            s.OpenAiCompatibleSpeed = 1.0;
            s.OpenAiCompatibleInstructions = string.Empty;

            using var doc = JsonDocument.Parse(OpenAiCompatibleSpeech.BuildRequestJson("Say \"hi\"", "gpt-4o-mini-tts", "alloy"));
            var root = doc.RootElement;
            Assert.Equal("gpt-4o-mini-tts", root.GetProperty("model").GetString());
            Assert.Equal("Say \"hi\"", root.GetProperty("input").GetString());
            Assert.Equal("alloy", root.GetProperty("voice").GetString());
            Assert.Equal("mp3", root.GetProperty("response_format").GetString());

            // Defaults are left out - not every compatible server knows these fields.
            Assert.False(root.TryGetProperty("speed", out _));
            Assert.False(root.TryGetProperty("instructions", out _));
        }
        finally
        {
            (s.OpenAiCompatibleSpeed, s.OpenAiCompatibleInstructions) = (savedSpeed, savedInstructions);
        }
    }

    [Fact]
    public void InstructionsAreNotSentToTts1()
    {
        var s = Se.Settings.Video.TextToSpeech;
        var (savedSpeed, savedInstructions) = (s.OpenAiCompatibleSpeed, s.OpenAiCompatibleInstructions);
        try
        {
            s.OpenAiCompatibleSpeed = 1.25;
            s.OpenAiCompatibleInstructions = "Calm and warm";

            using var mini = JsonDocument.Parse(OpenAiCompatibleSpeech.BuildRequestJson("x", "gpt-4o-mini-tts", "coral"));
            Assert.Equal("Calm and warm", mini.RootElement.GetProperty("instructions").GetString());
            Assert.Equal(1.25, mini.RootElement.GetProperty("speed").GetDouble());

            // OpenAI rejects "instructions" for tts-1 / tts-1-hd.
            using var tts1 = JsonDocument.Parse(OpenAiCompatibleSpeech.BuildRequestJson("x", "tts-1-hd", "coral"));
            Assert.False(tts1.RootElement.TryGetProperty("instructions", out _));
            Assert.Equal(1.25, tts1.RootElement.GetProperty("speed").GetDouble());
        }
        finally
        {
            (s.OpenAiCompatibleSpeed, s.OpenAiCompatibleInstructions) = (savedSpeed, savedInstructions);
        }
    }

    [Fact]
    public void ParsesTheOpenRouterModelList()
    {
        const string json = """
            {"data":[
              {"id":"x-ai/grok-voice-tts-1.0","name":"Grok Voice","supported_voices":["eve","ara"]},
              {"id":"fish-audio/s2-pro","name":"S2 Pro","supported_voices":null},
              {"name":"no id - skipped"}
            ]}
            """;

        var models = OpenAiCompatibleSpeech.ParseOpenRouterModels(json);

        Assert.Equal(["fish-audio/s2-pro", "x-ai/grok-voice-tts-1.0"], models.Select(m => m.Id));
        Assert.Empty(models[0].Voices);
        Assert.Equal(["eve", "ara"], models[1].Voices);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{\"error\":{\"message\":\"rate limited\"}}")]
    public void ABadModelListIsEmptyRatherThanAThrow(string json)
    {
        // RefreshVoices only overwrites the cached list when this finds models.
        Assert.Empty(OpenAiCompatibleSpeech.ParseOpenRouterModels(json));
    }

    [Fact]
    public async Task EachProviderKeepsItsOwnKeyAndModel()
    {
        var s = Se.Settings.Video.TextToSpeech;
        var saved = (s.OpenAiCompatibleProvider, s.OpenAiApiKey, s.OpenRouterTtsApiKey, s.OpenAiModel, s.OpenRouterTtsModel);
        try
        {
            OpenAiCompatibleSpeech.SetApiKey(OpenAiCompatibleSpeech.ProviderOpenAi, "sk-openai");
            OpenAiCompatibleSpeech.SetApiKey(OpenAiCompatibleSpeech.ProviderOpenRouter, "sk-or");
            OpenAiCompatibleSpeech.SetSavedModel(OpenAiCompatibleSpeech.ProviderOpenAi, "tts-1");

            Assert.Equal("sk-openai", OpenAiCompatibleSpeech.GetApiKey("OpenAI"));
            Assert.Equal("sk-or", OpenAiCompatibleSpeech.GetApiKey("openrouter"));
            Assert.Equal(OpenAiCompatibleSpeech.OpenRouterUrl, OpenAiCompatibleSpeech.GetUrl("OpenRouter"));

            // tts-1 doesn't know the newer voices.
            s.OpenAiCompatibleProvider = OpenAiCompatibleSpeech.ProviderOpenAi;
            var engine = new OpenAiCompatibleSpeech(null!);
            var voiceIds = (await engine.GetVoices(string.Empty))
                .Select(v => ((OpenAiCompatibleVoice)v.EngineVoice!).VoiceId)
                .ToList();
            Assert.Contains("alloy", voiceIds);
            Assert.DoesNotContain("marin", voiceIds);

            OpenAiCompatibleSpeech.SetSavedModel(OpenAiCompatibleSpeech.ProviderOpenAi, "gpt-4o-mini-tts");
            voiceIds = (await engine.GetVoices(string.Empty))
                .Select(v => ((OpenAiCompatibleVoice)v.EngineVoice!).VoiceId)
                .ToList();
            Assert.Contains("marin", voiceIds);
        }
        finally
        {
            (s.OpenAiCompatibleProvider, s.OpenAiApiKey, s.OpenRouterTtsApiKey, s.OpenAiModel, s.OpenRouterTtsModel) = saved;
        }
    }

    [Fact]
    public void UnknownProviderFallsBackToOpenAi()
    {
        Assert.Equal(OpenAiCompatibleSpeech.ProviderOpenAi, OpenAiCompatibleSpeech.ResolveProvider(null));
        Assert.Equal(OpenAiCompatibleSpeech.ProviderOpenAi, OpenAiCompatibleSpeech.ResolveProvider("westeurope"));
        Assert.Equal(OpenAiCompatibleSpeech.ProviderCustom, OpenAiCompatibleSpeech.ResolveProvider("custom"));
    }
}
