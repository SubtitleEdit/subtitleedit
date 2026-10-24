using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate;

public sealed class TranslationProfileDraft : ObservableObject
{
    public TranslationProfile Profile { get; private set; }
    public string Name => Profile.Name;
    public string EngineName => Profile.Kind == TranslationEngineKind.Traditional
        ? TranslationProfileFactory.TraditionalEngines.FirstOrDefault(e => e.Id == Profile.EngineId)?.Name ?? Profile.EngineId
        : Profile.EngineId switch
        {
            "LlamaCpp" => "llama.cpp", "Ollama" => "Ollama", "LmStudio" => "LM Studio",
            _ => Profile.Location == LlmLocation.Local ? Se.Language.Translate.LocalApi : Se.Language.Translate.CloudApi,
        };

    public TranslationProfileDraft(TranslationProfile profile) => Profile = profile.Snapshot();
    public void Update(TranslationProfile profile)
    {
        Profile = profile.Snapshot();
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(EngineName));
    }
}

public sealed class TranslationProfileManagerViewModel
{
    private readonly Dictionary<string, string> _originals;
    public ObservableCollection<TranslationProfileDraft> Profiles { get; }

    public TranslationProfileManagerViewModel(IEnumerable<TranslationProfile> profiles)
    {
        Profiles = new(profiles.Select(p => new TranslationProfileDraft(p)));
        _originals = Profiles.ToDictionary(p => p.Profile.Id, p => JsonSerializer.Serialize(p.Profile));
    }

    public TranslationProfileDraft Add()
    {
        var profile = new TranslationProfile();
        profile.Name = UniqueName(profile.Name);
        var item = new TranslationProfileDraft(profile);
        Profiles.Add(item);
        return item;
    }

    public TranslationProfileDraft Copy(TranslationProfileDraft source)
    {
        var profile = source.Profile.Copy(UniqueName(source.Name));
        var item = new TranslationProfileDraft(profile);
        Profiles.Add(item);
        return item;
    }

    private string UniqueName(string name)
    {
        name = name.Trim();
        var used = Profiles.Select(p => p.Name.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = name;
        for (var i = 2; used.Contains(result); i++) result = name + " (" + i + ")";
        return result;
    }

    public void Remove(IEnumerable<TranslationProfileDraft> selected)
    {
        foreach (var item in selected.ToArray()) Profiles.Remove(item);
    }

    public bool TrySave(out string error, out string? invalidId, Action? persist = null)
    {
        error = "";
        invalidId = null;
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in Profiles)
        {
            if (!names.Add(item.Name.Trim()))
            {
                error = Se.Language.Translate.UniqueProfileNameRequired;
                invalidId = item.Profile.Id;
                return false;
            }
        }
        var drafts = Profiles.Select(p => p.Profile).ToArray();
        var validated = new List<TranslationProfile>();
        foreach (var profile in drafts)
        {
            var changed = !_originals.TryGetValue(profile.Id, out var original) || original != JsonSerializer.Serialize(profile);
            if (changed)
            {
                if (!TranslationProfileEditorControl.TryValidate(profile, drafts, out var result, out error))
                {
                    invalidId = profile.Id;
                    return false;
                }
                validated.Add(result);
            }
            else validated.Add(profile.Snapshot());
        }
        try { TranslationProfileStore.Save(validated, persist); }
        catch (Exception ex) { error = ex.Message; return false; }
        return true;
    }
}
