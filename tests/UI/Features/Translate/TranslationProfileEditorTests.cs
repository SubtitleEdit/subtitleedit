using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia;
using Avalonia.Interactivity;
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
        Assert.Equal("custom", Labelled<TextBox>(window, Se.Language.General.Model).Text);
        Assert.Contains("openai.com", Labelled<TextBox>(window, Se.Language.General.Url).Text);
        Labelled<TextBox>(window, Se.Language.General.ApiKey).Text = "key-b";
        Assert.Equal("key-a", profile.ApiKey);
        Assert.Null(window.Result);
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
