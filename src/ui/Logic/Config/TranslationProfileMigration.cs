using System;
using System.Linq;
using System.Text.Json;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate.Llm;

namespace Nikse.SubtitleEdit.Logic.Config;

internal static class TranslationProfileMigration
{
    internal static void EnsureMigrated(Se settings)
    {
        var old = settings.AutoTranslate;
        old.Profiles ??= new();
        if (old.ProfileMigrationVersion >= 1) return;
        var defaults = new SeAutoTranslate();
        var batchName = settings.Tools.BatchConvert.AutoTranslateEngine;
        var batchWasSelected = !string.IsNullOrEmpty(batchName) && batchName != new SeBatchConvert().AutoTranslateEngine;
        void Add(TranslationProfile profile, string oldName)
        {
            profile.Name = TranslationProfileFactory.UniqueName(old.Profiles, profile.Name);
            old.Profiles.Add(profile);
            if (oldName == old.AutoTranslateLastName) old.SelectedProfileId = profile.Id;
            if (oldName == batchName) old.BatchProfileId = profile.Id;
        }
        foreach (var engine in TranslationProfileFactory.TraditionalEngines)
        {
            var translator = engine.Create(new());
            var profile = new TranslationProfile { Name = engine.Name, EngineId = engine.Id };
            var configured = false;
            if (old.EngineStrategies.TryGetValue(translator.Name, out var strategy)) profile.Parameters["TranslateStrategy"] = strategy;
            foreach (var field in engine.Fields)
            {
                var property = typeof(SeAutoTranslate).GetProperty(field.LegacyProperty)!;
                var value = (string?)property.GetValue(old) ?? "";
                profile.Parameters[field.Property] = value;
                configured |= value != ((string?)property.GetValue(defaults) ?? "");
            }
            if (configured || translator.Name == old.AutoTranslateLastName || (batchWasSelected && translator.Name == batchName))
                Add(profile, translator.Name);
        }
        var candidates = new[]
        {
            ("ChatGpt", new ChatGptTranslate().Name, ChatGptTranslate.DefaultUrl),
            ("OpenAiCompatible", new OpenAiCompatibleTranslate().Name, OpenAiCompatibleTranslate.DefaultUrl),
            ("Groq", new GroqTranslate().Name, GroqTranslate.DefaultUrl),
            ("OpenRouter", new OpenRouterTranslate().Name, OpenRouterTranslate.DefaultUrl),
            ("ApiRoute", new ApiRouteTranslate().Name, ApiRouteTranslate.DefaultUrl),
            ("DeepSeek", new DeepSeekTranslate().Name, DeepSeekTranslate.DefaultUrl),
            ("Nvidia", new NvidiaTranslate().Name, NvidiaTranslate.DefaultUrl),
            ("Mistral", new MistralTranslate().Name, MistralTranslate.DefaultUrl),
            ("Perplexity", new PerplexityTranslate().Name, PerplexityTranslate.DefaultUrl),
            ("Anthropic", new AnthropicTranslate().Name, AnthropicTranslate.DefaultUrl),
            ("Gemini", new GeminiTranslate().Name, "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions"),
            ("Ollama", new OllamaTranslate().Name, "http://localhost:11434/v1/chat/completions"),
            ("LmStudio", new LmStudioTranslate().Name, "http://localhost:1234/v1/chat/completions"),
            ("LlamaCpp", new LlamaCppTranslate().Name, "http://localhost:8080/v1/chat/completions"),
            ("OllamaAdvanced", "Ollama advanced (local LLM)", "http://localhost:11434/v1/chat/completions"),
            ("LmStudioAdvanced", "LM Studio advanced (local LLM)", "http://localhost:1234/v1/chat/completions"),
            ("LlamaCppAdvanced", "llama.cpp advanced (local LLM)", "http://localhost:8080/v1/chat/completions"),
        };
        foreach (var (prefix, name, endpoint) in candidates)
        {
            string Read(SeAutoTranslate source, string suffix)
            {
                var propertyName = prefix + suffix;
                if (prefix == "Anthropic") propertyName = suffix switch { "Url" => "AnthropicApiUrl", "Model" => "AnthropicApiModel", _ => propertyName };
                if (prefix == "Gemini" && suffix == "ApiKey") propertyName = "GeminiProApiKey";
                if ((prefix == "LmStudio" || prefix == "LlamaCpp") && suffix == "Url") propertyName = prefix + "ApiUrl";
                if (prefix == "LlamaCppAdvanced") propertyName = suffix == "Url" ? "LlamaCppApiUrl" : suffix == "Model" ? "LlamaCppModel" : "";
                return typeof(SeAutoTranslate).GetProperty(propertyName)?.GetValue(source) as string ?? "";
            }
            var key = prefix switch { "OpenRouter" => settings.Providers.OpenRouterApiKey, "Mistral" => settings.Providers.MistralApiKey, _ => Read(old, "ApiKey") };
            var url = Read(old, "Url");
            var model = Read(old, "Model");
            var prompt = Read(old, "Prompt");
            if (name != old.AutoTranslateLastName && (!batchWasSelected || name != batchName) && string.IsNullOrEmpty(key) &&
                url == Read(defaults, "Url") && model == Read(defaults, "Model") && prompt == Read(defaults, "Prompt")) continue;
            var local = prefix.StartsWith("Ollama") || prefix.StartsWith("LmStudio") || prefix.StartsWith("LlamaCpp");
            var engineId = local ? prefix.Replace("Advanced", "") : "Custom";
            var needsReview = false;
            if (local && !prefix.EndsWith("Advanced"))
            {
                var defaultUrl = Read(defaults, "Url");
                if (url == defaultUrl || string.IsNullOrWhiteSpace(url)) url = endpoint;
                else if (url.EndsWith("/api/generate", StringComparison.OrdinalIgnoreCase)) url = url[..^13] + "/v1/chat/completions";
                else if (url.EndsWith("/completion", StringComparison.OrdinalIgnoreCase)) needsReview = true;
            }
            if (model.Contains("MiLMMT", StringComparison.OrdinalIgnoreCase)) needsReview = true;
            var profile = new TranslationProfile
            {
                Name = name, Kind = TranslationEngineKind.Llm, EngineId = engineId,
                Location = local ? LlmLocation.Local : LlmLocation.Cloud,
                Protocol = prefix == "Anthropic" ? LlmApiProtocol.Anthropic : LlmApiProtocol.OpenAi,
                ApiUrl = AutoTranslateUrl.Complete(string.IsNullOrWhiteSpace(url) ? endpoint : url, endpoint),
                ApiKey = key, Model = model, NeedsReview = needsReview,
                Policy = JsonSerializer.Deserialize<AdvancedTranslationSettings>(JsonSerializer.Serialize(old.LlamaCppAdvanced))!,
            };
            if (!string.IsNullOrWhiteSpace(prompt)) profile.LinePrompt = prompt;
            if (engineId == "LlamaCpp") profile.Parameters["UseRemoteServer"] = old.LlamaCppUseRemoteServer.ToString();
            Add(profile, name);
        }
        if (old.Profiles.Count == 0 || (string.IsNullOrEmpty(old.AutoTranslateLastName) && string.IsNullOrEmpty(old.SelectedProfileId)))
        {
            var google = old.Profiles.FirstOrDefault(p => p.EngineId == "GoogleTranslateV1");
            if (google == null) { Add(new TranslationProfile { Name = "Google Translate" }, ""); google = old.Profiles[^1]; }
            old.SelectedProfileId = google.Id;
        }
        if (!batchWasSelected && string.IsNullOrEmpty(old.BatchProfileId)) old.BatchProfileId = old.SelectedProfileId;
        // Unknown legacy selections require a new choice instead of silently changing the engine.
        if (string.IsNullOrEmpty(old.AutoTranslateLastName) && string.IsNullOrEmpty(old.SelectedProfileId)) old.SelectedProfileId = old.Profiles[0].Id;
        old.ProfileMigrationVersion = 1;
    }
}
