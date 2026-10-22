using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Translate;

public class TranslationProfileEditorTests
{
    private static T Named<T>(Control root, string name) where T : Control => root.GetLogicalDescendants().OfType<T>().Single(c => c.Name == name);
    private static T Labelled<T>(Control root, string label) where T : Control => root.GetLogicalDescendants().OfType<T>().Single(c => AutomationProperties.GetName(c) == label);

    [AvaloniaFact]
    public void ProviderOnlyFillsUrlAndCancelPreservesOriginal()
    {
        var profile = new TranslationProfile { Name = "My account", Kind = TranslationEngineKind.Llm, Protocol = LlmApiProtocol.Anthropic, ApiUrl = "https://proxy.test/messages", ApiKey = "key-a", Model = "custom" };
        var window = new TranslationProfileEditorWindow(profile);
        Named<ComboBox>(window, "ProviderPreset").SelectedIndex = 1;
        Assert.Equal(1, Named<ComboBox>(window, "ApiProtocol").SelectedIndex);
        Assert.Equal("key-a", Labelled<TextBox>(window, Se.Language.General.ApiKey).Text);
        Assert.Equal("custom", Labelled<ComboBox>(window, Se.Language.General.Model).Text);
        Assert.Contains("openai.com", Labelled<TextBox>(window, Se.Language.General.Url).Text);
        Labelled<TextBox>(window, Se.Language.General.ApiKey).Text = "key-b";
        Assert.Equal("key-a", profile.ApiKey);
        Assert.Null(window.Result);
    }

    [AvaloniaFact]
    public void EngineCategoriesShowOnlyMatchingChoicesAndSaveCloudSettings()
    {
        var window = new TranslationProfileEditorWindow(new() { Name = "Account", Model = "custom-model" });
        var engines = Named<ComboBox>(window, "TranslationEngine");
        var services = ((System.Collections.IEnumerable)engines.ItemsSource!).Cast<object>().ToArray();
        Assert.Contains(services, c => c.ToString() == "DeepL");
        Assert.DoesNotContain(services, c => c.ToString() == "Ollama");
        Assert.True(Named<RadioButton>(window, "TranslationServices").IsChecked);
        Named<RadioButton>(window, "LanguageModels").IsChecked = true;
        Assert.False(Named<RadioButton>(window, "TranslationServices").IsChecked);
        Assert.True(Named<RadioButton>(window, "CloudModels").IsChecked);
        Assert.False(((Control)engines.Parent!).IsVisible);
        Assert.Equal(Se.Language.Translate.CloudApi, engines.SelectedItem!.ToString());
        window.GetLogicalDescendants().OfType<Button>().Single(b => b.IsDefault).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.NotNull(window.Result);
        Assert.Equal(TranslationEngineKind.Llm, window.Result.Kind);
        Assert.Equal(LlmLocation.Cloud, window.Result.Location);
        Assert.Equal("Custom", window.Result.EngineId);
    }

    [AvaloniaFact]
    public void SegmentsSupportArrowKeysAndKeepConnectionDraftsWhenSwitching()
    {
        var profile = new TranslationProfile
        {
            Kind = TranslationEngineKind.Llm, ApiUrl = "https://proxy.test/v1/chat/completions", Model = "manual-model",
        };
        var window = new TranslationProfileEditorWindow(profile);
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            var url = Labelled<TextBox>(window, Se.Language.General.Url);
            url.Text = "https://draft.test/v1/chat/completions";
            var cloud = Named<RadioButton>(window, "CloudModels");
            cloud.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Left });
            Assert.True(Named<RadioButton>(window, "LocalModels").IsChecked);
            var engines = Named<ComboBox>(window, "TranslationEngine");
            var local = ((System.Collections.IEnumerable)engines.ItemsSource!).Cast<object>().ToArray();
            Assert.Contains(local, c => c.ToString() == "Ollama");
            Assert.DoesNotContain(local, c => c.ToString() == "DeepL");
            Named<RadioButton>(window, "LocalModels").RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Right });
            Assert.True(cloud.IsChecked);
            Assert.Equal("https://draft.test/v1/chat/completions", url.Text);
            Named<RadioButton>(window, "TranslationServices").IsChecked = true;
            Named<RadioButton>(window, "LanguageModels").IsChecked = true;
            Assert.Equal("https://draft.test/v1/chat/completions", url.Text);
            Assert.Equal("manual-model", Named<ComboBox>(window, "TranslationModel").Text);
            Assert.Equal("https://proxy.test/v1/chat/completions", profile.ApiUrl);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void SelectingLocalOrTraditionalEngineSavesMatchingInternalSettings()
    {
        var window = new TranslationProfileEditorWindow(new() { Name = "Local account", ApiUrl = "https://proxy.test/v1/chat/completions", Model = "qwen3" });
        Named<RadioButton>(window, "LanguageModels").IsChecked = true;
        Named<RadioButton>(window, "LocalModels").IsChecked = true;
        var engines = Named<ComboBox>(window, "TranslationEngine");
        var choices = ((System.Collections.IEnumerable)engines.ItemsSource!).Cast<object>().ToArray();
        engines.SelectedItem = choices.Single(c => c.ToString() == "Ollama");
        window.GetLogicalDescendants().OfType<Button>().Single(b => b.IsDefault).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.NotNull(window.Result);
        Assert.Equal(TranslationEngineKind.Llm, window.Result.Kind);
        Assert.Equal(LlmLocation.Local, window.Result.Location);
        Assert.Equal("Ollama", window.Result.EngineId);
        Assert.Equal("http://localhost:11434/v1/chat/completions", window.Result.ApiUrl);

        var traditional = new TranslationProfileEditorWindow(window.Result.Copy("DeepL account"));
        Named<RadioButton>(traditional, "TranslationServices").IsChecked = true;
        var services = Named<ComboBox>(traditional, "TranslationEngine");
        services.SelectedItem = ((System.Collections.IEnumerable)services.ItemsSource!).Cast<object>().Single(c => c.ToString() == "DeepL");
        traditional.GetLogicalDescendants().OfType<Button>().Single(b => b.IsDefault).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.NotNull(traditional.Result);
        Assert.Equal(TranslationEngineKind.Traditional, traditional.Result.Kind);
        Assert.Equal("DeepLTranslate", traditional.Result.EngineId);
        Assert.Contains("AutoTranslateDeepLApiKey", traditional.Result.Parameters.Keys);
    }

    [AvaloniaFact]
    public void SelectingProviderOffersModelsWithoutLosingCustomModel()
    {
        var window = new TranslationProfileEditorWindow(new() { Kind = TranslationEngineKind.Llm, ApiUrl = "https://proxy.test/v1/chat/completions", Model = "my-custom-model" });
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            Named<ComboBox>(window, "ProviderPreset").SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            var models = Named<ComboBox>(window, "TranslationModel");
            Assert.True(models.IsEditable);
            Assert.Equal("my-custom-model", models.Text);
            Assert.Contains(Nikse.SubtitleEdit.UiLogic.AutoTranslate.ChatGptTranslate.DefaultModel, ((IEnumerable<string>)models.ItemsSource!));
            models.SelectedItem = Nikse.SubtitleEdit.UiLogic.AutoTranslate.ChatGptTranslate.DefaultModel;
            Assert.Equal(Nikse.SubtitleEdit.UiLogic.AutoTranslate.ChatGptTranslate.DefaultModel, models.Text);
            Named<ComboBox>(window, "ProviderPreset").SelectedIndex = 2;
            Dispatcher.UIThread.RunJobs();
            Assert.Contains(Nikse.SubtitleEdit.UiLogic.AutoTranslate.AnthropicTranslate.Models[0], ((IEnumerable<string>)models.ItemsSource!));
            models.Text = "manual-anthropic-model";
            Assert.Equal("manual-anthropic-model", models.Text);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ReopeningProviderRestoresItsModelChoices()
    {
        var window = new TranslationProfileEditorWindow(new()
        {
            Kind = TranslationEngineKind.Llm,
            ApiUrl = Nikse.SubtitleEdit.UiLogic.AutoTranslate.AnthropicTranslate.DefaultUrl,
            Protocol = LlmApiProtocol.Anthropic,
            Model = "saved-custom-model",
        });
        var models = Named<ComboBox>(window, "TranslationModel");
        Assert.NotEmpty((IEnumerable<string>)models.ItemsSource!);
        Assert.Equal("saved-custom-model", models.Text);
        Assert.Equal(2, Named<ComboBox>(window, "ProviderPreset").SelectedIndex);
    }

    [AvaloniaFact]
    public void SavingLlmProfileKeepsProtocolAndManualLineMode()
    {
        var profile = new TranslationProfile { Name = "Independent account", Kind = TranslationEngineKind.Llm, ApiUrl = "https://proxy.test/v1", ApiKey = "secret", Model = "test-model" };
        var window = new TranslationProfileEditorWindow(profile);
        Named<ComboBox>(window, "ApiProtocol").SelectedIndex = 0;
        Named<ComboBox>(window, "TranslationMode").SelectedIndex = 1;
        window.GetLogicalDescendants().OfType<Button>().Single(b => b.IsDefault).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.NotNull(window.Result);
        Assert.EndsWith("/v1/chat/completions", window.Result.ApiUrl);
        Assert.Equal(LlmTranslationMode.LineByLine, window.Result.Mode);
        Assert.Equal(LlmTranslationMode.Advanced, profile.Mode);
    }

    [AvaloniaFact]
    public void SmallEditorKeepsSaveButtonReachable()
    {
        var window = new TranslationProfileEditorWindow(new() { Kind = TranslationEngineKind.Llm, ApiUrl = "https://example.test/v1/chat/completions", Model = "subtitle-model" });
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            using (var frame = window.CaptureRenderedFrame())
            {
                Assert.NotNull(frame);
                var directory = Environment.GetEnvironmentVariable("SE_UI_SCREENSHOT_DIR");
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                    frame.Save(Path.Combine(directory, "translation-profile-editor.png"));
                }
            }
            window.Height = window.MinHeight;
            window.UpdateLayout();
            var save = window.GetLogicalDescendants().OfType<Button>().Single(b => b.IsDefault);
            var position = save.TranslatePoint(new Point(), window);
            Assert.NotNull(position);
            Assert.True(position.Value.Y + save.Bounds.Height <= window.ClientSize.Height + 1);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void DuplicateNameDoesNotSave()
    {
        var before = Se.Settings.AutoTranslate.Profiles;
        try
        {
            Se.Settings.AutoTranslate.Profiles = new() { new() { Name = "Taken" } };
            var window = new TranslationProfileEditorWindow(new() { Name = "taken" });
            window.GetLogicalDescendants().OfType<Button>().Single(b => b.IsDefault).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Null(window.Result);
        }
        finally { Se.Settings.AutoTranslate.Profiles = before; }
    }
}
