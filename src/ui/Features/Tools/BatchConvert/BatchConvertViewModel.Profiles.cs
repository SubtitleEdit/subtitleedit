using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Tools.BatchConvert;

public partial class BatchConvertViewModel
{
    public TranslationProfile? SelectedTranslationProfile { get; private set; }
    public bool UsesTranslationProfiles { get; private set; }
    private bool _applyingTranslationProfile;
    internal void SelectTranslationProfile(TranslationProfile? profile)
    {
        UsesTranslationProfiles = true; SelectedTranslationProfile = profile;
        Se.Settings.AutoTranslate.BatchProfileId = profile?.Id ?? "";
        if (profile == null) return;
        var engineId = profile.Kind == TranslationEngineKind.Traditional ? profile.EngineId : profile.EngineId switch
        {
            "LlamaCpp" when profile.Location == LlmLocation.Local => nameof(LlamaCppAdvancedTranslate),
            "Ollama" when profile.Location == LlmLocation.Local => nameof(OllamaAdvancedTranslate),
            "LmStudio" when profile.Location == LlmLocation.Local => nameof(LmStudioAdvancedTranslate),
            _ => nameof(OpenAiCompatibleTranslate),
        };
        var engine = AutoTranslators.FirstOrDefault(t => t.GetType().Name == engineId);
        if (engine == null && profile.Kind == TranslationEngineKind.Traditional)
        {
            engine = TranslationProfileFactory.TraditionalEngines.FirstOrDefault(e => e.Id == engineId)?.Create(new());
            if (engine != null) AutoTranslators.Add(engine);
        }
        if (engine == null) return;
        var source = SelectedSourceLanguage;
        var target = SelectedTargetLanguage;
        _applyingTranslationProfile = true;
        SelectedAutoTranslator = engine;
        OnAutoTranslatorChanged();
        SelectedSourceLanguage = SourceLanguages.FirstOrDefault(p => p.TwoLetterIsoLanguageName == source?.TwoLetterIsoLanguageName) ?? SelectedSourceLanguage;
        SelectedTargetLanguage = TargetLanguages.FirstOrDefault(p => p.TwoLetterIsoLanguageName == target?.TwoLetterIsoLanguageName) ?? SelectedTargetLanguage;
        AutoTranslateUrl = profile.ApiUrl; AutoTranslateApiKey = profile.ApiKey; AutoTranslateModel = profile.Model;
        if (engine is LlamaCppAdvancedTranslate)
        {
            _suppressLlamaCppRemoteToggle = true;
            LlamaCppUseRemoteServer = profile.Parameters.GetValueOrDefault("UseRemoteServer") == "True";
            _suppressLlamaCppRemoteToggle = false;
            PopulateLlamaCppModels();
            if (!LlamaCppUseRemoteServer) SelectedLlamaCppModel = LlamaCppDownloadHelper.PopulateModels(LlamaCppModels, GetLlamaCppModelsForEngine(), Path.GetFileName(profile.Model));
        }
        if (engine is CrispAsrMadladTranslate)
            SelectedCrispAsrModel = CrispAsrTranslateDownloadHelper.PopulateModels(CrispAsrModels, Path.GetFileName(profile.Parameters.GetValueOrDefault("AutoTranslateCrispAsrModel")));
        AutoTranslateUrlIsVisible = AutoTranslateApiKeyIsVisible = AutoTranslateModelIsVisible = AutoTranslateModelBrowseIsVisible = false;
        LlamaCppAdvancedButtonIsVisible = LlamaCppSettingsButtonIsVisible = LlamaCppRemoteToggleIsVisible = false;
        _applyingTranslationProfile = false;
    }

    private void ApplyTranslationProfile(BatchConvertConfig config)
    {
        var profile = config.AutoTranslate.Profile;
        if (profile == null || !config.AutoTranslate.IsActive) return;
        if (profile.Kind == TranslationEngineKind.Llm && profile.EngineId == "LlamaCpp" && profile.Location == LlmLocation.Local && profile.Parameters.GetValueOrDefault("UseRemoteServer") != "True")
        {
            profile.ApiUrl = UiLogic.AutoTranslate.AutoTranslateUrl.Complete(Configuration.Settings.Tools.LlamaCppApiUrl, "http://localhost:8080/v1/chat/completions");
            profile.Model = "";
        }
        config.AutoTranslate.Translator = TranslationProfileFactory.Create(profile);
    }
}
