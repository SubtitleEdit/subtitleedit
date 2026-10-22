using System.Text.Json;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Translate;

public class TranslationProfileManagerTests
{
    private static TranslationProfile[] Seed()
    {
        var profiles = new[] { new TranslationProfile { Name = "Default" }, new TranslationProfile { Name = "Other account" } };
        Se.Settings.AutoTranslate = new SeAutoTranslate
        {
            ProfileMigrationVersion = 2, Profiles = profiles.ToList(),
            SelectedProfileId = profiles[0].Id, BatchProfileId = profiles[1].Id,
        };
        return profiles;
    }

    private static T Named<T>(Control root, string name) where T : Control => root.GetLogicalDescendants().OfType<T>().Single(c => c.Name == name);
    private static TextBox NameField(Control root) => root.GetLogicalDescendants().OfType<TextBox>().Single(c => AutomationProperties.GetName(c) == Se.Language.General.Name);
    private static void Click(Control root, string name) => Named<Button>(root, name).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

    [Fact]
    public void ManagementDraftsDoNotMutateOriginalsAndCopyOwnsPolicyAndCredentials()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        source[0].ApiKey = "account-a";
        source[0].Policy.Glossary = "Alice = 艾丽丝";
        var manager = new TranslationProfileManagerViewModel(source);
        var copy = manager.Copy(manager.Profiles[0]);
        copy.Profile.ApiKey = "account-b";
        copy.Profile.Policy.Glossary = "another glossary";
        manager.Profiles[0].Profile.Name = "Renamed draft";
        manager.Remove(new[] { manager.Profiles[1] });
        Assert.Equal("Default (2)", copy.Name);
        Assert.NotEqual(source[0].Id, copy.Profile.Id);
        Assert.Equal("Default", source[0].Name);
        Assert.Equal("account-a", source[0].ApiKey);
        Assert.Equal("Alice = 艾丽丝", manager.Profiles[0].Profile.Policy.Glossary);
        Assert.Equal(2, Se.Settings.AutoTranslate.Profiles.Count);
    }

    [Fact]
    public void SaveIsOneWritePreservesActiveIdAndClearsDeletedBatchId()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        var manager = new TranslationProfileManagerViewModel(source);
        manager.Profiles[0].Profile.Name = "Renamed";
        manager.Remove(new[] { manager.Profiles[1] });
        var calls = 0;
        Assert.True(manager.TrySave(out _, out _, () => calls++));
        Assert.Equal(1, calls);
        Assert.Equal(source[0].Id, Se.Settings.AutoTranslate.SelectedProfileId);
        Assert.Equal("", Se.Settings.AutoTranslate.BatchProfileId);
        Assert.Equal("Renamed", Assert.Single(Se.Settings.AutoTranslate.Profiles).Name);
    }

    [Fact]
    public void FailedWriteRollsBackMemoryAndAllowsRetryWithDraftsIntact()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        var previous = Se.Settings.AutoTranslate.Profiles;
        var manager = new TranslationProfileManagerViewModel(source);
        manager.Profiles[0].Profile.Name = "Draft name";
        manager.Remove(new[] { manager.Profiles[1] });
        Assert.False(manager.TrySave(out var error, out var invalidId, () => throw new IOException("disk unavailable")));
        Assert.Equal("disk unavailable", error);
        Assert.Null(invalidId);
        Assert.Same(previous, Se.Settings.AutoTranslate.Profiles);
        Assert.Equal(source[0].Id, Se.Settings.AutoTranslate.SelectedProfileId);
        Assert.Equal(source[1].Id, Se.Settings.AutoTranslate.BatchProfileId);
        Assert.Equal("Draft name", manager.Profiles[0].Name);
        Assert.True(manager.TrySave(out _, out _, () => { }));
        Assert.Equal("Draft name", Assert.Single(Se.Settings.AutoTranslate.Profiles).Name);
    }

    [AvaloniaFact]
    public void DuplicateOrInvalidDraftIsLocatedAndDeletingItAllowsSave()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        var manager = new TranslationProfileManagerViewModel(source);
        var added = manager.Add();
        Assert.Equal("Default (2)", added.Name);
        added.Profile.Name = " default ";
        Assert.False(manager.TrySave(out _, out var invalidId, () => throw new Exception("must not persist")));
        Assert.Equal(added.Profile.Id, invalidId);
        added.Profile.Name = "Cloud";
        added.Profile.Kind = TranslationEngineKind.Llm;
        added.Profile.ApiUrl = "https://proxy.test/v1/chat/completions";
        Assert.False(manager.TrySave(out _, out invalidId, () => throw new Exception("must not persist")));
        Assert.Equal(added.Profile.Id, invalidId);
        manager.Remove(new[] { added });
        Assert.True(manager.TrySave(out _, out _, () => { }));
        Assert.Equal(2, Se.Settings.AutoTranslate.Profiles.Count);
    }

    [AvaloniaFact]
    public void ViewingUntouchedLegacyConnectionDoesNotBlockUnrelatedManagement()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        source[0].Kind = TranslationEngineKind.Llm;
        source[0].ApiUrl = "bad legacy route";
        source[0].Model = "";
        source[0].NeedsReview = true;
        var manager = new TranslationProfileManagerViewModel(source);
        using var editor = new TranslationProfileEditorControl(manager.Profiles[0].Profile);
        manager.Profiles[0].Update(editor.CaptureDraft());
        manager.Add();
        Assert.True(manager.TrySave(out _, out _, () => { }));
        Assert.True(Se.Settings.AutoTranslate.Profiles[0].NeedsReview);
        Assert.Equal("bad legacy route", Se.Settings.AutoTranslate.Profiles[0].ApiUrl);
    }

    [Fact]
    public void UntouchedLegacyBlankNameDoesNotBlockOtherEditsButNewBlankNameDoes()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        source[0].Name = "";
        var manager = new TranslationProfileManagerViewModel(source);
        manager.Profiles[1].Profile.Name = "Updated account";
        Assert.True(manager.TrySave(out _, out _, () => { }));
        var added = manager.Add();
        added.Profile.Name = " ";
        Assert.False(manager.TrySave(out _, out var invalidId, () => { }));
        Assert.Equal(added.Profile.Id, invalidId);
    }

    [Fact]
    public void DeletingAllProfilesIsAllowedAndDoesNotRegenerateDefault()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var manager = new TranslationProfileManagerViewModel(Seed());
        manager.Remove(manager.Profiles.ToArray());
        Assert.True(manager.TrySave(out _, out _, () => { }));
        TranslationProfileMigration.EnsureMigrated(Se.Settings);
        Assert.Empty(Se.Settings.AutoTranslate.Profiles);
        Assert.Equal("", Se.Settings.AutoTranslate.SelectedProfileId);
        Assert.Equal("", Se.Settings.AutoTranslate.BatchProfileId);
    }

    [Fact]
    public void SavedProfilesRoundTripThroughSettingsFile()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        var manager = new TranslationProfileManagerViewModel(source);
        manager.Profiles[0].Profile.Name = "Persisted account";
        var file = Path.Combine(Path.GetTempPath(), "se-profile-manager-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            Assert.True(manager.TrySave(out _, out _, () => Se.SaveSettings(file)));
            var restored = JsonSerializer.Deserialize(File.ReadAllText(file), SeJsonContext.Default.Se)!;
            Assert.Equal("Persisted account", restored.AutoTranslate.Profiles[0].Name);
            Assert.Equal(source[0].Id, restored.AutoTranslate.SelectedProfileId);
            Assert.Equal(source[1].Id, restored.AutoTranslate.BatchProfileId);
        }
        finally { File.Delete(file); File.Delete(file + ".tmp"); }
    }

    [AvaloniaFact]
    public void SelectorContainsOnlyDropdownAndManagementButtonEvenWhenEmpty()
    {
        using var scope = new SettingsScope("AutoTranslate");
        Seed(); Se.Settings.AutoTranslate.Profiles.Clear();
        var selector = new TranslationProfileSelector(() => null, "", _ => { });
        Assert.Equal(2, selector.Children.Count);
        Assert.IsType<ComboBox>(selector.Children[0]);
        var button = Assert.IsType<Button>(selector.Children[1]);
        Assert.Equal("…", button.Content);
        Assert.Equal(Se.Language.Translate.ManageProfiles, AutomationProperties.GetName(button));
        Assert.Null(button.Flyout);
        Assert.True(button.IsEnabled);
    }

    [AvaloniaFact]
    public void SelectionSwitchingKeepsIncompleteDraftAndCancelDiscardsSession()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        var window = new TranslationProfileManagerWindow(source[1].Id, () => throw new Exception("Cancel must not save"));
        var list = Named<ListBox>(window, "ManagedProfiles");
        Assert.Equal(source[1].Id, ((TranslationProfileDraft)list.SelectedItem!).Profile.Id);
        NameField(window).Text = "Unsaved name";
        list.SelectedItems!.Clear(); list.SelectedItem = window.ViewModel.Profiles[0];
        list.SelectedItems.Clear(); list.SelectedItem = window.ViewModel.Profiles[1];
        Assert.Equal("Unsaved name", NameField(window).Text);
        Click(window, "CancelProfiles");
        Assert.Equal("Other account", Se.Settings.AutoTranslate.Profiles[1].Name);
        Assert.False(window.Saved);
        Assert.Equal(source[0].Id, Se.Settings.AutoTranslate.SelectedProfileId);
    }

    [AvaloniaFact]
    public void MultiSelectionShowsSummaryAndSupportsBatchDeleteWithoutImmediateWrite()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        var window = new TranslationProfileManagerWindow(source[0].Id);
        var list = Named<ListBox>(window, "ManagedProfiles");
        Assert.True(Named<Button>(window, "CopyProfile").IsEnabled);
        list.SelectedItems!.Add(window.ViewModel.Profiles[1]);
        Assert.False(Named<Button>(window, "CopyProfile").IsEnabled);
        Assert.True(Named<Button>(window, "DeleteProfiles").IsEnabled);
        Assert.DoesNotContain(window.GetLogicalDescendants(), c => c is TranslationProfileEditorControl);
        Assert.Contains("2", Named<TextBlock>(window, "ProfileSelectionSummary").Text);
        Click(window, "DeleteProfiles");
        Assert.Empty(window.ViewModel.Profiles);
        Assert.Equal(2, Se.Settings.AutoTranslate.Profiles.Count);
        Assert.False(Named<Button>(window, "DeleteProfiles").IsEnabled);
        Click(window, "CancelProfiles");
    }

    [AvaloniaFact]
    public void AddCopyAndValidationKeepTheManagerOpenForCorrections()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        var calls = 0;
        var window = new TranslationProfileManagerWindow(source[0].Id, () => calls++);
        Click(window, "AddProfile");
        Assert.Equal("Default (2)", NameField(window).Text);
        Click(window, "CopyProfile");
        Assert.Equal("Default (2) (2)", NameField(window).Text);
        NameField(window).Text = "default";
        Click(window, "SaveProfiles");
        Assert.False(window.Saved);
        Assert.Equal(0, calls);
        Assert.Equal("default", NameField(window).Text);
        NameField(window).Text = "Corrected";
        Click(window, "SaveProfiles");
        Assert.True(window.Saved);
        Assert.Equal(1, calls);
        Assert.Equal(source[0].Id, Se.Settings.AutoTranslate.SelectedProfileId);
        Assert.Equal(4, Se.Settings.AutoTranslate.Profiles.Count);
    }

    [AvaloniaFact]
    public async Task AdvancedPolicyStaysInDraftAcrossSelectionAndOuterCancel()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        source[0].Kind = TranslationEngineKind.Llm;
        source[0].ApiUrl = "https://api.openai.com/v1/chat/completions";
        source[0].Model = "manual-model";
        source[0].Policy.Glossary = "Original";
        var window = new TranslationProfileManagerWindow(source[0].Id);
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs();
            window.GetLogicalDescendants().OfType<Button>()
                .Single(b => Equals(b.Content, Se.Language.General.AdvancedDotDotDot))
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            var advanced = Assert.Single(window.OwnedWindows.OfType<LlamaCppAdvancedSettingsWindow>());
            var vm = Assert.IsType<LlamaCppAdvancedSettingsViewModel>(advanced.DataContext);
            vm.Glossary = "Draft glossary";
            vm.OkCommand.Execute(null);
            await Task.Yield();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Original", source[0].Policy.Glossary);
            var list = Named<ListBox>(window, "ManagedProfiles");
            list.SelectedItems!.Clear(); list.SelectedItem = window.ViewModel.Profiles[1];
            list.SelectedItems.Clear(); list.SelectedItem = window.ViewModel.Profiles[0];
            Assert.Equal("Draft glossary", window.ViewModel.Profiles[0].Profile.Policy.Glossary);
            Assert.Equal("manual-model", Named<ComboBox>(window, "TranslationModel").Text);
            Click(window, "CancelProfiles");
            Assert.Equal("Original", Se.Settings.AutoTranslate.Profiles[0].Policy.Glossary);
        }
        finally { foreach (var owned in window.OwnedWindows.ToArray()) owned.Close(); window.Close(); }
    }

    [AvaloniaFact]
    public void FailedSaveLeavesWindowAndDraftAvailableForRetry()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        var fail = true;
        var window = new TranslationProfileManagerWindow(source[0].Id, () => { if (fail) throw new IOException("disk unavailable"); });
        try
        {
            window.Show();
            NameField(window).Text = "Retry account";
            Click(window, "SaveProfiles");
            Assert.True(window.IsVisible);
            Assert.False(window.Saved);
            Assert.Equal("Default", Se.Settings.AutoTranslate.Profiles[0].Name);
            Assert.Equal("Retry account", NameField(window).Text);
            fail = false;
            Click(window, "SaveProfiles");
            Assert.True(window.Saved);
            Assert.Equal("Retry account", Se.Settings.AutoTranslate.Profiles[0].Name);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void BothOpenSelectorsRefreshByIdAndUnsubscribeWhenClosed()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        TranslationProfile? active = null, batch = null;
        var updates = 0;
        var left = new TranslationProfileSelector(() => null, source[0].Id, p => { active = p; updates++; });
        var right = new TranslationProfileSelector(() => null, source[1].Id, p => batch = p, batch: true);
        var host = new Window { Content = new StackPanel { Children = { left, right } } };
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            var manager = new TranslationProfileManagerViewModel(Se.Settings.AutoTranslate.Profiles);
            manager.Profiles[0].Profile.Name = "Updated";
            manager.Remove(new[] { manager.Profiles[1] });
            Assert.True(manager.TrySave(out _, out _, () => { }));
            Assert.Equal("Updated", active?.Name);
            Assert.Equal(source[0].Id, active?.Id);
            Assert.Null(batch);
        }
        finally { host.Close(); Dispatcher.UIThread.RunJobs(); }
        var before = updates;
        TranslationProfileStore.Save(Se.Settings.AutoTranslate.Profiles, () => { });
        Assert.Equal(before, updates);
    }

    [AvaloniaFact]
    public void SmallManagerKeepsButtonsReachableAndSupportsSelectAll()
    {
        using var scope = new SettingsScope("AutoTranslate");
        var source = Seed();
        source[0].Kind = TranslationEngineKind.Llm;
        source[0].ApiUrl = "https://api.openai.com/v1/chat/completions";
        source[0].Model = "subtitle-model";
        var window = new TranslationProfileManagerWindow(source[0].Id);
        try
        {
            window.Show(); Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            using (var frame = window.CaptureRenderedFrame())
            {
                Assert.NotNull(frame);
                var directory = Environment.GetEnvironmentVariable("SE_UI_SCREENSHOT_DIR");
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                    frame.Save(Path.Combine(directory, "translation-profile-manager.png"));
                }
            }
            window.Width = window.MinWidth; window.Height = window.MinHeight;
            Dispatcher.UIThread.RunJobs(); window.UpdateLayout();
            using var resizedFrame = window.CaptureRenderedFrame();
            var formScroll = window.GetLogicalDescendants().OfType<TranslationProfileEditorControl>()
                .Single().GetLogicalDescendants().OfType<ScrollViewer>().Single();
            Assert.True(formScroll.Extent.Height > formScroll.Viewport.Height, $"Client: {window.ClientSize}; form extent: {formScroll.Extent}; viewport: {formScroll.Viewport}");
            var screenshotDirectory = Environment.GetEnvironmentVariable("SE_UI_SCREENSHOT_DIR");
            if (!string.IsNullOrEmpty(screenshotDirectory))
            {
                using var smallFrame = window.CaptureRenderedFrame();
                smallFrame!.Save(Path.Combine(screenshotDirectory, "translation-profile-manager-small.png"));
            }
            var button = Named<Button>(window, "SaveProfiles");
            var position = button.TranslatePoint(new Point(), window);
            Assert.True(position.HasValue);
            Assert.True(position.Value.Y + button.Bounds.Height <= window.ClientSize.Height + 1);
            var list = Named<ListBox>(window, "ManagedProfiles");
            list.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.A, KeyModifiers = KeyModifiers.Control });
            Assert.Equal(2, list.SelectedItems!.Count);
        }
        finally { window.Close(); }
    }
}
