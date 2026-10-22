using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>Commits a management session once, preserving selection IDs and rolling memory back on failure.</summary>
internal static class TranslationProfileStore
{
    internal static event Action? Changed;

    internal static void Save(IEnumerable<TranslationProfile> profiles, Action? persist = null)
    {
        var settings = Se.Settings.AutoTranslate;
        var previous = settings.Profiles;
        var selected = settings.SelectedProfileId;
        var batch = settings.BatchProfileId;
        var next = profiles.Select(p => p.Snapshot()).ToList();
        var ids = next.Select(p => p.Id).ToHashSet(StringComparer.Ordinal);
        try
        {
            settings.Profiles = next;
            if (!ids.Contains(selected)) settings.SelectedProfileId = "";
            if (!ids.Contains(batch)) settings.BatchProfileId = "";
            (persist ?? Se.SaveSettings)();
        }
        catch
        {
            settings.Profiles = previous;
            settings.SelectedProfileId = selected;
            settings.BatchProfileId = batch;
            throw;
        }
        // Notification failures must not turn a successful disk write into a failed management save.
        if (Changed == null) return;
        foreach (Action listener in Changed.GetInvocationList())
        {
            try { listener(); }
            catch (Exception ex) { Se.LogError(ex, "Failed to refresh translation profiles"); }
        }
    }
}
