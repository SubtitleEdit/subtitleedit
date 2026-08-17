using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace LibUiLogicTests.Translate;

public class DoAutoTranslateTests
{
    private sealed class FakeTranslator : IAutoTranslator
    {
        public List<string> ReceivedTexts { get; } = new();
        public Func<string, string> Translation { get; set; } = text => "X" + text;

        public string Name => "FakeTranslator";
        public string Url => "https://example.com";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        public void Initialize()
        {
        }

        public List<TranslationPair> GetSupportedSourceLanguages() => new() { new TranslationPair("English", "en") };

        public List<TranslationPair> GetSupportedTargetLanguages() => new() { new TranslationPair("Danish", "da") };

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            ReceivedTexts.Add(text);
            return Task.FromResult(Translation(text));
        }
    }

    /// <summary>
    /// Stands in for the "advanced" local-LLM engines: translates whole batches itself and must
    /// never see the plain per-line <see cref="IAutoTranslator.Translate"/> call.
    /// </summary>
    private sealed class FakeBatchTranslator : IAutoTranslator, IBatchContextTranslator
    {
        public int BatchSize { get; set; } = 4;
        public List<int> BatchStartIndexes { get; } = new();

        public string Name => "FakeBatchTranslator";
        public string Url => "https://example.com";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        public void Initialize()
        {
        }

        public List<TranslationPair> GetSupportedSourceLanguages() => new() { new TranslationPair("English", "en") };

        public List<TranslationPair> GetSupportedTargetLanguages() => new() { new TranslationPair("Danish", "da") };

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("The per-line path must not be used for a batch-context engine");
        }

        public Task<int> TranslateBatchAsync(ObservableCollection<TranslateRow> rows, int index, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken)
        {
            BatchStartIndexes.Add(index);
            var count = Math.Min(BatchSize, rows.Count - index);
            for (var i = 0; i < count; i++)
            {
                rows[index + i].TranslatedText = "X" + rows[index + i].Text;
            }

            return Task.FromResult(count);
        }
    }

    private static Subtitle MakeSubtitle(int lineCount)
    {
        var subtitle = new Subtitle();
        for (var i = 0; i < lineCount; i++)
        {
            subtitle.Paragraphs.Add(new Paragraph($"This is line number {i}.", i * 2000, i * 2000 + 1500));
        }

        subtitle.Renumber();
        return subtitle;
    }

    [Fact]
    public async Task TranslateEachLineSeparately_StaysSingleLine_ForWholeRun()
    {
        var translator = new FakeTranslator();
        var doAutoTranslate = new DoAutoTranslate { TranslateEachLineSeparately = true };
        const int lineCount = 12; // more than the 8 successes that used to flip merged mode back on

        var rows = await doAutoTranslate.DoTranslate(
            MakeSubtitle(lineCount),
            new TranslationPair("English", "en"),
            new TranslationPair("Danish", "da"),
            translator,
            CancellationToken.None);

        Assert.Equal(lineCount, rows.Count);
        Assert.All(rows, row => Assert.False(string.IsNullOrEmpty(row.TranslatedText)));

        // Every request must contain exactly one subtitle line - no merged batches.
        Assert.Equal(lineCount, translator.ReceivedTexts.Count);
        for (var i = 0; i < lineCount; i++)
        {
            Assert.Equal($"This is line number {i}.", translator.ReceivedTexts[i]);
        }
    }

    [Fact]
    public async Task MergedMode_TranslatesAllLines()
    {
        var translator = new FakeTranslator { Translation = text => text };
        var doAutoTranslate = new DoAutoTranslate();
        const int lineCount = 10;

        var rows = await doAutoTranslate.DoTranslate(
            MakeSubtitle(lineCount),
            new TranslationPair("English", "en"),
            new TranslationPair("Danish", "da"),
            translator,
            CancellationToken.None);

        Assert.Equal(lineCount, rows.Count);
        Assert.All(rows, row => Assert.False(string.IsNullOrEmpty(row.TranslatedText)));
    }

    [Fact]
    public async Task BatchContextTranslator_DrivesItsOwnBatchLoop()
    {
        var translator = new FakeBatchTranslator { BatchSize = 4 };
        // Single-line mode is a merge/split setting; the batch engine guarantees line alignment
        // on its own and keeps batching regardless (same as the Auto-translate window).
        var doAutoTranslate = new DoAutoTranslate { TranslateEachLineSeparately = true };
        const int lineCount = 10;

        var rows = await doAutoTranslate.DoTranslate(
            MakeSubtitle(lineCount),
            new TranslationPair("English", "en"),
            new TranslationPair("Danish", "da"),
            translator,
            CancellationToken.None);

        Assert.Equal(lineCount, rows.Count);
        Assert.All(rows, row => Assert.StartsWith("X", row.TranslatedText));

        // 4 + 4 + 2 - the last batch is trimmed to what is left, and no line is sent twice.
        Assert.Equal(new List<int> { 0, 4, 8 }, translator.BatchStartIndexes);
    }

    [Fact]
    public async Task EmptyTranslations_TerminateInsteadOfLoopingForever()
    {
        // An engine that always returns nothing must not make the loop retry the
        // same line forever. Termination may be a normal completion (empty results
        // are applied in single-line mode) or the loop's zero-progress exception -
        // either is fine; only spinning forever is a bug.
        var translator = new FakeTranslator { Translation = _ => string.Empty };
        var doAutoTranslate = new DoAutoTranslate();

        var task = doAutoTranslate.DoTranslate(
            MakeSubtitle(5),
            new TranslationPair("English", "en"),
            new TranslationPair("Danish", "da"),
            translator,
            CancellationToken.None);

        var completed = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken));
        Assert.Same(task, completed);

        if (task.IsCompletedSuccessfully)
        {
            var rows = await task;
            Assert.Equal(5, rows.Count);
        }
    }
}
