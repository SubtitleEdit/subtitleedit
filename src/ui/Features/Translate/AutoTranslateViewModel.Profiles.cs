using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using Nikse.SubtitleEdit.Logic.Download;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class AutoTranslateViewModel
{
    public TranslationProfile? SelectedProfile { get; private set; }
    private bool _applyingProfile;
    public bool UsesProfiles { get; private set; }
    internal void SelectProfile(TranslationProfile? profile)
    {
        UsesProfiles = true; SelectedProfile = profile;
        Se.Settings.AutoTranslate.SelectedProfileId = profile?.Id ?? "";
        if (profile == null) { IsTranslateEnabled = false; _autoStart = false; return; }
        IsTranslateEnabled = true;
        var engineId = profile.Kind == TranslationEngineKind.Traditional ? profile.EngineId : profile.EngineId switch
        {
            "LlamaCpp" when profile.Location == LlmLocation.Local => nameof(LlamaCppAdvancedTranslate),
            "Ollama" when profile.Location == LlmLocation.Local => nameof(OllamaAdvancedTranslate),
            "LmStudio" when profile.Location == LlmLocation.Local => nameof(LmStudioAdvancedTranslate),
            _ => nameof(OpenAiCompatibleTranslate),
        };
        var engine = AutoTranslators.FirstOrDefault(t => t.GetType().Name == engineId);
        if (engine == null) { IsTranslateEnabled = false; return; }
        _applyingProfile = true;
        SelectedAutoTranslator = engine;
        AutoTranslatorChanged(Window!);
        ApiUrlText = profile.ApiUrl; ApiKeyText = profile.ApiKey; ModelText = profile.Model;
        if (engine is LlamaCppAdvancedTranslate)
        {
            _suppressLlamaCppRemoteToggle = true;
            LlamaCppUseRemoteServer = profile.Parameters.GetValueOrDefault("UseRemoteServer") == "True";
            _suppressLlamaCppRemoteToggle = false;
            LlamaCppModelComboIsVisible = !LlamaCppUseRemoteServer;
            LlamaCppButtonsAreVisible = !LlamaCppUseRemoteServer;
            if (!LlamaCppUseRemoteServer)
                SelectedLlamaCppModel = LlamaCppDownloadHelper.PopulateModels(LlamaCppModels, GetLlamaCppModelsForEngine(), Path.GetFileName(profile.Model));
        }
        if (engine is CrispAsrMadladTranslate)
            PopulateCrispAsrModels(Path.GetFileName(profile.Parameters.GetValueOrDefault("AutoTranslateCrispAsrModel")));
        // Connection and policy editing belongs to the profile dialog. Keep local install/model controls.
        ApiKeyIsVisible = ApiIdIsVisible = ApiSecretIsVisible = ApiUrlIsVisible = ButtonApiUrlIsVisible = false;
        ModelIsVisible = ModelBrowseIsVisible = ModelComboIsVisible = ModelTextBoxIsVisible = false;
        FormalityIsVisible = LlamaCppAdvancedButtonIsVisible = LlamaCppRemoteToggleIsVisible = false;
        AutoTranslatorLinkText = profile.Name;
        _applyingProfile = false;
    }

    partial void OnSelectedAutoTranslatorChanged(IAutoTranslator value)
    {
        if (!_applyingProfile && Window != null)
        {
            UsesProfiles = false;
            SelectedProfile = null;
            AutoTranslatorChanged(Window);
        }
    }

    private IAutoTranslator CreateProfileTranslator()
    {
        var profile = SelectedProfile!.Snapshot();
        if (profile.Kind == TranslationEngineKind.Llm && profile.EngineId == "LlamaCpp" && profile.Location == LlmLocation.Local && !LlamaCppUseRemoteServer)
        {
            profile.ApiUrl = UiLogic.AutoTranslate.AutoTranslateUrl.Complete(Configuration.Settings.Tools.LlamaCppApiUrl, "http://localhost:8080/v1/chat/completions");
            TranslationProfileFactory.ApplyModelDefaults(profile.Policy, SelectedLlamaCppModel?.Model);
            profile.Model = ""; // llama-server already serves the selected GGUF.
        }
        return TranslationProfileFactory.Create(profile, action => Dispatcher.UIThread.Invoke(action));
    }
}
