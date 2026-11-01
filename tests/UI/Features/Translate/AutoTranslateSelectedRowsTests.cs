using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Translate;
using Nikse.SubtitleEdit.Logic;
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

            return Task.FromResult(text.ToUpperInvariant());
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
