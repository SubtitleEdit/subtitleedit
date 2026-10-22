using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace Nikse.SubtitleEdit.Features.Translate;

public sealed class TranslationProfileSelector : StackPanel
{
    private readonly ComboBox _combo;
    private readonly ObservableCollection<TranslationProfile> _profiles = new();
    private readonly Func<Window?> _owner;
    private readonly Action<TranslationProfile?> _changed;
    private bool _refreshing;
    private bool _subscribed;

    public TranslationProfileSelector(Func<Window?> owner, string selectedId, Action<TranslationProfile?> changed, bool batch = false)
    {
        _owner = owner; _changed = changed;
        Orientation = Orientation.Horizontal; Spacing = 4;
        TranslationProfileMigration.EnsureMigrated(Se.Settings);
        _combo = new ComboBox { ItemsSource = _profiles, MinWidth = 180, MaxWidth = 240 }.WithAccessibleName(Se.Language.General.Profile);
        _combo.SelectionChanged += (_, _) => { if (!_refreshing) _changed(_combo.SelectedItem as TranslationProfile); };
        Children.Add(_combo);
        var manage = Tool("…", Se.Language.Translate.ManageProfiles);
        manage.Name = "ManageTranslationProfiles";
        manage.Click += async (_, _) => await Manage();
        Children.Add(manage);
        AttachedToVisualTree += (_, _) =>
        {
            if (!_subscribed) { TranslationProfileStore.Changed += ProfilesChanged; _subscribed = true; }
            ProfilesChanged();
        };
        DetachedFromVisualTree += (_, _) =>
        {
            if (_subscribed) { TranslationProfileStore.Changed -= ProfilesChanged; _subscribed = false; }
        };
        Refresh(string.IsNullOrEmpty(selectedId) ? (batch ? Se.Settings.AutoTranslate.BatchProfileId : Se.Settings.AutoTranslate.SelectedProfileId) : selectedId);
    }
    private static Button Tool(string text, string label)
    {
        var button = new Button { Content = text, MinWidth = 28 }.WithAccessibleName(label);
        ToolTip.SetTip(button, label); return button;
    }
    private async Task Manage()
    {
        if (_owner() is not Window owner) return;
        var selectedId = (_combo.SelectedItem as TranslationProfile)?.Id ?? "";
        var manager = new TranslationProfileManagerWindow(selectedId);
        await manager.ShowDialog(owner);
        if (manager.Saved && !_subscribed) Refresh(selectedId);
    }

    private void ProfilesChanged() => Refresh((_combo.SelectedItem as TranslationProfile)?.Id ?? "");

    private void Refresh(string id)
    {
        _refreshing = true;
        _profiles.Clear();
        foreach (var profile in Se.Settings.AutoTranslate.Profiles) _profiles.Add(profile);
        _combo.SelectedItem = _profiles.FirstOrDefault(p => p.Id == id);
        _refreshing = false;
        _changed(_combo.SelectedItem as TranslationProfile);
    }
}
