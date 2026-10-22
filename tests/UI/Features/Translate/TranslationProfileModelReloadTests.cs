using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Translate;

public class TranslationProfileModelReloadTests
{
    private static TranslationProfile Profile() => new()
    {
        Kind = TranslationEngineKind.Llm,
        ApiUrl = "https://api.openai.com/v1/chat/completions",
        Model = "manual-model",
    };

    private static ComboBox Models(Control editor) => editor.GetLogicalDescendants().OfType<ComboBox>().Single(c => c.Name == "TranslationModel");
    private static TextBlock Status(Control editor) => editor.GetLogicalDescendants().OfType<TextBlock>().Single(c => c.Name == "ModelListStatus");
    private static async Task Until(Func<bool> condition)
    {
        var timeout = Stopwatch.StartNew();
        while (!condition() && timeout.Elapsed < TimeSpan.FromSeconds(5))
        {
            await Task.Delay(10);
            Dispatcher.UIThread.RunJobs();
        }
        Assert.True(condition(), "Model reload did not complete.");
    }

    [AvaloniaFact]
    public async Task OpeningProfileAndEveryDropdownOpeningReloadWithoutChangingManualModel()
    {
        var calls = 0;
        using var editor = new TranslationProfileEditorControl(Profile(), _ => Task.FromResult(new[] { "model-" + ++calls }));
        var host = new Window { Content = editor };
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            var models = Models(editor);
            await Until(() => calls == 1);
            Assert.Equal(new[] { "model-1" }, models.ItemsSource);
            models.IsDropDownOpen = true;
            await Until(() => calls == 2);
            Assert.Equal(new[] { "model-2" }, models.ItemsSource);
            models.IsDropDownOpen = false;
            models.IsDropDownOpen = true;
            await Until(() => calls == 3);
            Assert.Equal(new[] { "model-3" }, models.ItemsSource);
            Assert.Equal("manual-model", models.Text);
            Assert.DoesNotContain(editor.GetLogicalDescendants().OfType<Button>(), b => Equals(b.Content, Se.Language.General.Refresh));
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public async Task ConnectionEditsWaitForFocusToLeaveBeforeReloading()
    {
        var calls = 0;
        using var editor = new TranslationProfileEditorControl(Profile(), _ => Task.FromResult(new[] { "model-" + ++calls }));
        var host = new Window { Content = editor };
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            await Until(() => calls == 1);
            var url = editor.GetLogicalDescendants().OfType<TextBox>().Single(c => c.Text == Profile().ApiUrl);
            url.Focus();
            url.Text = "https://proxy.test/v1/chat/completions";
            await Task.Delay(500);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, calls);
            Models(editor).Focus();
            await Until(() => calls == 2);
            Assert.Equal("manual-model", Models(editor).Text);
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public async Task ObsoleteResponsesAndClosedEditorsCannotReplaceCurrentModels()
    {
        var responses = new List<TaskCompletionSource<string[]>>();
        var tokens = new List<CancellationToken>();
        using var editor = new TranslationProfileEditorControl(Profile(), token =>
        {
            tokens.Add(token);
            var response = new TaskCompletionSource<string[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            responses.Add(response);
            return response.Task; // Deliberately ignore cancellation to exercise stale response protection.
        });
        var host = new Window { Content = editor };
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            var models = Models(editor);
            models.IsDropDownOpen = true;
            await Until(() => responses.Count == 1);
            models.IsDropDownOpen = false;
            models.IsDropDownOpen = true;
            await Until(() => responses.Count == 2);
            Assert.True(tokens[0].IsCancellationRequested);
            responses[1].SetResult(new[] { "current-model" });
            await Until(() => models.ItemsSource!.Cast<string>().Contains("current-model"));
            responses[0].SetResult(new[] { "obsolete-model" });
            await Task.Delay(30); Dispatcher.UIThread.RunJobs();
            Assert.Equal(new[] { "current-model" }, models.ItemsSource);
            models.IsDropDownOpen = false;
            models.IsDropDownOpen = true;
            await Until(() => responses.Count == 3);
            host.Close();
            Assert.True(tokens[2].IsCancellationRequested);
            responses[2].SetResult(new[] { "closed-model" });
            await Task.Delay(30); Dispatcher.UIThread.RunJobs();
            Assert.Equal(new[] { "current-model" }, models.ItemsSource);
        }
        finally
        {
            host.Close();
            foreach (var response in responses) response.TrySetCanceled();
        }
    }

    [AvaloniaFact]
    public async Task FailedReloadKeepsProviderSuggestionsAndManualInput()
    {
        using var editor = new TranslationProfileEditorControl(Profile(), _ => throw new IOException("Model endpoint unavailable"));
        var host = new Window { Content = editor };
        try
        {
            host.Show(); Dispatcher.UIThread.RunJobs();
            var models = Models(editor);
            models.IsDropDownOpen = true;
            await Until(() => Status(editor).Text == "Model endpoint unavailable");
            Assert.True(Status(editor).IsVisible);
            Assert.NotEmpty(models.ItemsSource!.Cast<string>());
            Assert.Equal("manual-model", models.Text);
            models.Text = "another-manual-model";
            Assert.Equal("another-manual-model", editor.CaptureDraft().Model);
        }
        finally { host.Close(); }
    }
}
