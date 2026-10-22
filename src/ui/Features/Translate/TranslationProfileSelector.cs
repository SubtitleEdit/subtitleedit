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

    public TranslationProfileSelector(Func<Window?> owner, string selectedId, Action<TranslationProfile?> changed, bool batch = false)
    {
        _owner = owner; _changed = changed;
        Orientation = Orientation.Horizontal; Spacing = 4;
        TranslationProfileMigration.EnsureMigrated(Se.Settings);
        _combo = new ComboBox { ItemsSource = _profiles, MinWidth = 180, MaxWidth = 240 }.WithAccessibleName(Se.Language.General.Profile);
        _combo.SelectionChanged += (_, _) => { if (!_refreshing) _changed(_combo.SelectedItem as TranslationProfile); };
        Children.Add(_combo);
        var add = Tool("+", Se.Language.General.NewProfile);
        add.Click += async (_, _) => await Edit(new TranslationProfile { Name = TranslationProfileFactory.UniqueName(_profiles, Se.Language.General.NewProfile) });
        var edit = Tool("…", Se.Language.General.Edit);
        edit.Click += async (_, _) => { if (_combo.SelectedItem is TranslationProfile p) await Edit(p); };
        var more = Tool("▾", Se.Language.General.Profiles);
        var copy = new MenuItem { Header = Se.Language.General.Copy };
        copy.Click += async (_, _) => { if (_combo.SelectedItem is TranslationProfile p) await Edit(p.Copy(TranslationProfileFactory.UniqueName(_profiles, p.Name))); };
        var rename = new MenuItem { Header = Se.Language.General.Rename };
        rename.Click += async (_, _) => { if (_combo.SelectedItem is TranslationProfile p) await Edit(p); };
        var delete = new MenuItem { Header = Se.Language.General.Delete };
        delete.Click += (_, _) =>
        {
            if (_combo.SelectedItem is not TranslationProfile p) return;
            Se.Settings.AutoTranslate.Profiles.RemoveAll(x => x.Id == p.Id);
            if (Se.Settings.AutoTranslate.SelectedProfileId == p.Id) Se.Settings.AutoTranslate.SelectedProfileId = "";
            if (Se.Settings.AutoTranslate.BatchProfileId == p.Id) Se.Settings.AutoTranslate.BatchProfileId = "";
            Se.SaveSettings(); Refresh("");
        };
        more.Flyout = new MenuFlyout { Items = { copy, rename, delete } };
        Children.Add(add); Children.Add(edit); Children.Add(more);
        Refresh(string.IsNullOrEmpty(selectedId) ? (batch ? Se.Settings.AutoTranslate.BatchProfileId : Se.Settings.AutoTranslate.SelectedProfileId) : selectedId);
    }
    private static Button Tool(string text, string label)
    {
        var button = new Button { Content = text, MinWidth = 28 }.WithAccessibleName(label);
        ToolTip.SetTip(button, label); return button;
    }
    private async Task Edit(TranslationProfile profile)
    {
        if (_owner() is not Window owner) return;
        var editor = new TranslationProfileEditorWindow(profile);
        await editor.ShowDialog(owner);
        if (editor.Result is not TranslationProfile result) return;
        var store = Se.Settings.AutoTranslate.Profiles;
        var index = store.FindIndex(p => p.Id == result.Id);
        if (index >= 0) store[index] = result; else store.Add(result);
        Se.SaveSettings(); Refresh(result.Id);
    }
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
