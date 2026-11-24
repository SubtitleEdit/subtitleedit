using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace UITests.Features.Translate;

/// <summary>
/// "Translate rows" (#16548): the rows selected in the grid are translated together - a sentence
/// spread over adjacent rows reaches the engine as one text - while unselected rows stay untouched
/// and the selection survives, so a single grid is enough to work through a subtitle in parts.
/// </summary>
public class AutoTranslateSelectedRowsTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class RecordingTranslator : IAutoTranslator
    {
        public List<string> Texts { get; } = new();
        public Func<string, string> TranslateText { get; set; } = text => text.ToUpperInvariant();
        public string Name => "Recording";
        public string Url => string.Empty;
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1000;
        public void Initialize() { }
        public List<TranslationPair> GetSupportedSourceLanguages() => new() { new TranslationPair("English", "en") };
        public List<TranslationPair> GetSupportedTargetLanguages() => new() { new TranslationPair("Danish", "da") };

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            lock (Texts)
            {
                Texts.Add(text);
            }

            return Task.FromResult(TranslateText(text));
        }
    }

    [AvaloniaFact]
    public async Task DictionaryHiddenInOptions_HidesToolbar_AndAppliesNoFixes()
    {
        var originalShow = Se.Settings.AutoTranslate.ShowPostProcessDictionary;
        var originalUse = Se.Settings.AutoTranslate.UsePostProcessDictionary;
        Se.Settings.AutoTranslate.ShowPostProcessDictionary = false;
        Se.Settings.AutoTranslate.UsePostProcessDictionary = true;
        var (vm, window, grid, translator) = OpenWindow("It's no big deal.", "Thank you.", "You got it.");
        try
        {
            var language = Se.Language.Translate;
            var toggle = window.GetVisualDescendants().OfType<ToggleSwitch>()
                .Single(control => AutomationProperties.GetName(control) == language.PostProcessDictionary);
            Assert.False(toggle.IsEffectivelyVisible);

            translator.TranslateText = text => text is "It's no big deal." or "It is no big deal." ? "Det er ikke noget stort." : text;
            Select(vm, grid, 0);
            await vm.TranslateRowsCommand.ExecuteAsync(null);
            await WaitUntilDone(vm);
            Assert.Equal("Det er ikke noget stort.", vm.Rows[0].TranslatedText);
            Assert.False(vm.HasDictionaryMatches);
        }
        finally
        {
            Se.Settings.AutoTranslate.ShowPostProcessDictionary = originalShow;
            Se.Settings.AutoTranslate.UsePostProcessDictionary = originalUse;
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task DictionarySwitch_IsLive_AppliesAndRevertsFixesOnExistingTranslations()
    {
        var (vm, window, grid, translator) = OpenWindow("It's no big deal.", "You got it.", "Fair enough, Major.");
        var originalSetting = Se.Settings.AutoTranslate.UsePostProcessDictionary;
        try
        {
            vm.UsePostProcessDictionary = false;
            // Neighbouring lines reach the engine merged, so translate line by line.
            translator.TranslateText = text => string.Join("\n", text.Split('\n').Select(line => line.Trim() switch
            {
                "It's no big deal." or "It is no big deal." => "Det er ikke noget stort.",
                "You got it." => "Du har det.",
                "Fair enough, Major." => "Det er nok, Major.",
                var other => other,
            }));
            Select(vm, grid, 0, 1, 2);
            await vm.TranslateRowsCommand.ExecuteAsync(null);
            await WaitUntilDone(vm);
            Assert.Equal("Det er ikke noget stort.", vm.Rows[0].TranslatedText);
            Assert.False(vm.HasDictionaryMatches);

            // On: the lines already translated are fixed straight away.
            vm.UsePostProcessDictionary = true;
            Assert.Equal("Det er ikke noget særligt.", vm.Rows[0].TranslatedText);
            Assert.Equal("Det gør jeg.", vm.Rows[1].TranslatedText);
            Assert.Equal("Fair nok, major.", vm.Rows[2].TranslatedText);
            Assert.Equal("Det er ikke noget stort.", vm.Rows[0].PostProcessOriginal);

            // A line the user edits keeps the edit when the switch goes off; the others go back.
            vm.OnTranslationEdited(vm.Rows[2], "Fair nok, hr. major.");
            vm.UsePostProcessDictionary = false;
            Assert.Equal("Det er ikke noget stort.", vm.Rows[0].TranslatedText);
            Assert.Equal("Du har det.", vm.Rows[1].TranslatedText);
            Assert.Equal("Fair nok, hr. major.", vm.Rows[2].TranslatedText);
            Assert.False(vm.HasDictionaryMatches);
            Assert.All(vm.Rows, row => Assert.Null(row.PostProcessOriginal));
        }
        finally
        {
            Se.Settings.AutoTranslate.UsePostProcessDictionary = originalSetting;
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task DictionarySwitch_ReviewsSelectedRows_AndCanEditAcceptOrRevert()
    {
        var (vm, window, grid, translator) = OpenWindow("It's no big deal.", "Thank you.", "You got it.");
        var originalSetting = Se.Settings.AutoTranslate.UsePostProcessDictionary;
        try
        {
            var language = Se.Language.Translate;
            var toggle = window.GetVisualDescendants().OfType<ToggleSwitch>()
                .Single(control => AutomationProperties.GetName(control) == language.PostProcessDictionary);
            toggle.SetCurrentValue(ToggleSwitch.IsCheckedProperty, true);
            Dispatcher.UIThread.RunJobs();
            Assert.True(vm.UsePostProcessDictionary);

            translator.TranslateText = text => text switch
            {
                "It's no big deal." or "It is no big deal." => "Det er ikke noget stort.",
                "You got it." => "Du har det.",
                _ => text,
            };
            vm.Rows[1].TranslatedText = "Thank you."; // Existing translation outside the current run.
            Select(vm, grid, 0, 2);
            await vm.TranslateRowsCommand.ExecuteAsync(null);
            await WaitUntilDone(vm);

            Assert.Equal("Det er ikke noget særligt.", vm.Rows[0].TranslatedText);
            Assert.Equal("Thank you.", vm.Rows[1].TranslatedText);
            Assert.Equal("Det gør jeg.", vm.Rows[2].TranslatedText); // first suggestion applied
            Assert.Equal(string.Format(language.PostProcessSummary, 2, 1), vm.PostProcessSummary);
            Assert.Equal("Det er ikke noget stort.", vm.Rows[0].PostProcessOriginal);
            Assert.Null(vm.Rows[1].PostProcessOriginal);
            Assert.Equal("Du har det.", vm.Rows[2].PostProcessOriginal);
            Assert.True(vm.Rows[2].PostProcessNeedsCheck);
            Assert.False(vm.Rows[0].PostProcessNeedsCheck);
            Assert.Equal(string.Format(language.PostProcessToCheckX, 1), vm.DictionaryToCheckText);

            vm.SelectedTranslateRow = vm.Rows[0];
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            var original = window.GetVisualDescendants().OfType<SelectableTextBlock>()
                .Single(control => AutomationProperties.GetName(control) == language.PostProcessOriginal);
            var current = window.GetVisualDescendants().OfType<TextBox>()
                .Single(control => AutomationProperties.GetName(control) == language.PostProcessCurrent);
            Assert.True(original.IsEffectivelyVisible);
            Assert.Equal("Det er ikke noget stort.", original.Text);
            Assert.Equal("Det er ikke noget særligt.", current.Text);
            current.SetCurrentValue(TextBox.TextProperty, "Det betyder ikke det store.");
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("Det betyder ikke det store.", vm.Rows[0].TranslatedText);
            vm.RevertDictionaryTextCommand.Execute(null);
            Assert.Equal("Det er ikke noget stort.", vm.Rows[0].TranslatedText);
            Assert.Null(vm.Rows[0].PostProcessOriginal);

            vm.SelectedTranslateRow = vm.Rows[2];
            Assert.True(vm.SelectedDictionaryMatchNeedsCheck);
            Assert.True(Assert.Single(vm.DictionaryChoices, choice => choice.Text == "Det gør jeg.").IsCurrent);
            vm.UseDictionaryTextCommand.Execute("Lige præcis.");
            Assert.Equal("Lige præcis.", vm.Rows[2].TranslatedText);
            Assert.False(vm.SelectedDictionaryMatchNeedsCheck);
            Assert.Equal(string.Format(language.PostProcessSummary, 1, 0), vm.PostProcessSummary);
            Assert.False(vm.Rows[2].PostProcessNeedsCheck);
            Assert.False(vm.HasDictionaryToCheck);

            toggle.SetCurrentValue(ToggleSwitch.IsCheckedProperty, false);
            Select(vm, grid, 2);
            Assert.False(vm.UsePostProcessDictionary);
            await vm.TranslateRowsCommand.ExecuteAsync(null);
            await WaitUntilDone(vm);
            Assert.Equal("Du har det.", vm.Rows[2].TranslatedText);
            vm.SelectedTranslateRow = vm.Rows[2];
            Assert.False(vm.HasSelectedDictionaryMatch);
        }
        finally
        {
            Se.Settings.AutoTranslate.UsePostProcessDictionary = originalSetting;
            window.Close();
        }
    }

    private static async Task WaitUntilDone(AutoTranslateViewModel vm)
    {
        for (var i = 0; i < 500 && !vm.IsTranslateEnabled; i++)
        {
            await Task.Delay(10);
            Dispatcher.UIThread.RunJobs();
        }

        Dispatcher.UIThread.RunJobs();
        Assert.True(vm.IsTranslateEnabled);
    }

    private static (AutoTranslateViewModel Vm, AutoTranslateWindow Window, TableView Grid, RecordingTranslator Translator) OpenWindow(params string[] lines)
    {
        var vm = new AutoTranslateViewModel(new WindowService(new NullServiceProvider()), new FolderHelper());
        var subtitle = new Subtitle();
        for (var i = 0; i < lines.Length; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph(lines[i], i * 2000, i * 2000 + 1500));
        }

        vm.Initialize(subtitle);
        var window = new AutoTranslateWindow(vm) { Width = 1000, Height = 700 };
        window.Show();
        vm.OnLoaded();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        var translator = new RecordingTranslator();
        vm.AutoTranslators.Add(translator);
        vm.SelectedAutoTranslator = translator;
        vm.SelectedSourceLanguage = translator.GetSupportedSourceLanguages()[0];
        vm.SelectedTargetLanguage = translator.GetSupportedTargetLanguages()[0];
        Dispatcher.UIThread.RunJobs();
        return (vm, window, window.GetVisualDescendants().OfType<TableView>().First(), translator);
    }

    private static void Select(AutoTranslateViewModel vm, TableView grid, params int[] indices)
    {
        grid.SelectedItems!.Clear();
        foreach (var index in indices)
        {
            grid.SelectedItems.Add(vm.Rows[index]);
        }

        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public async Task AdjacentRows_AreSentTogether_AndUnselectedRowsStayEmpty()
    {
        var (vm, window, grid, translator) = OpenWindow("Before.", "This sentence is", "spread over rows.", "After.");
        try
        {
            Select(vm, grid, 1, 2);

            await vm.TranslateRowsCommand.ExecuteAsync(null);
            await WaitUntilDone(vm);

            Assert.Equal("This sentence is spread over rows.", Assert.Single(translator.Texts));
            Assert.True(string.IsNullOrEmpty(vm.Rows[0].TranslatedText));
            Assert.False(string.IsNullOrEmpty(vm.Rows[1].TranslatedText));
            Assert.False(string.IsNullOrEmpty(vm.Rows[2].TranslatedText));
            Assert.True(string.IsNullOrEmpty(vm.Rows[3].TranslatedText));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task SeparateRuns_AreNotMergedAcrossTheGap()
    {
        var (vm, window, grid, translator) = OpenWindow("First part", "gap in between", "last part.");
        try
        {
            Select(vm, grid, 0, 2);

            await vm.TranslateRowsCommand.ExecuteAsync(null);
            await WaitUntilDone(vm);

            Assert.Equal(new[] { "First part", "last part." }, translator.Texts);
            Assert.Equal("FIRST PART", vm.Rows[0].TranslatedText);
            Assert.True(string.IsNullOrEmpty(vm.Rows[1].TranslatedText));
            Assert.Equal("LAST PART.", vm.Rows[2].TranslatedText);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task Selection_SurvivesTheTranslation()
    {
        var (vm, window, grid, _) = OpenWindow("One.", "Two.", "Three.", "Four.");
        try
        {
            Select(vm, grid, 1, 3);

            await vm.TranslateRowsCommand.ExecuteAsync(null);
            await WaitUntilDone(vm);

            Assert.Equal(new[] { vm.Rows[1], vm.Rows[3] }, grid.SelectedItems!.OfType<object>());
            Assert.True(vm.IsOkEnabled);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(new[] { 4 }, "4-4")]
    [InlineData(new[] { 1, 2, 3 }, "1-3")]
    [InlineData(new[] { 0, 1, 5, 7, 8 }, "0-1 5-5 7-8")]
    public void GetRowRuns_GroupsAdjacentRows(int[] indices, string expected)
    {
        var runs = AutoTranslateViewModel.GetRowRuns(indices);

        Assert.Equal(expected, string.Join(" ", runs.Select(r => $"{r.First}-{r.Last}")));
    }
}
