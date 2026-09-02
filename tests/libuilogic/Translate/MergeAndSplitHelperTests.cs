using System.Collections.ObjectModel;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace LibUiLogicTests.Translate;

public class MergeAndSplitHelperTests
{
    private sealed class FixedResultTranslator : IAutoTranslator
    {
        public string Result { get; set; } = string.Empty;

        public string Name => "FixedResultTranslator";
        public string Url => "https://example.com";
        public string Error { get; set; } = string.Empty;
        public int MaxCharacters => 1500;

        public void Initialize()
        {
        }

        public List<TranslationPair> GetSupportedSourceLanguages() => new() { new TranslationPair("Chinese", "zh"), new TranslationPair("English", "en") };

        public List<TranslationPair> GetSupportedTargetLanguages() => new() { new TranslationPair("English", "en"), new TranslationPair("Danish", "da") };

        public Task<string> Translate(string text, string sourceLanguageCode, string targetLanguageCode, CancellationToken cancellationToken) => Task.FromResult(Result);
    }

    private static ObservableCollection<TranslateRow> MakeRows(params string[] texts)
    {
        var rows = new ObservableCollection<TranslateRow>();
        for (var i = 0; i < texts.Length; i++)
        {
            rows.Add(new TranslateRow
            {
                Number = i + 1,
                Show = TimeSpan.FromMilliseconds(i * 2000),
                Hide = TimeSpan.FromMilliseconds(i * 2000 + 1500),
                Text = texts[i],
            });
        }

        return rows;
    }

    [Theory]
    [InlineData("en", "da", "One two.", "Three four.", "Five six.")]
    [InlineData("zh", "en", "你好朋友", "我们走吧", "谢谢大家")]
    public async Task MergeAndTranslateIfPossible_RowsMatchReportedCount(string sourceCode, string targetCode, params string[] texts)
    {
        var rows = MakeRows(texts);
        // Garbage response: a single line without sentence endings defeats every
        // split strategy for a multi-paragraph merge.
        var translator = new FixedResultTranslator { Result = "garbage garbage garbage" };

        var count = await MergeAndSplitHelper.MergeAndTranslateIfPossible(
            rows,
            new TranslationPair("Source", sourceCode),
            new TranslationPair("Target", targetCode),
            0,
            translator,
            forceSingleLineMode: false,
            CancellationToken.None);

        // Whatever a strategy attempted, the rows must exactly reflect the reported
        // count: a failed split (count 0) must not leave partial garbage in the rows.
        var rowsWithText = rows.Count(r => !string.IsNullOrEmpty(r.TranslatedText));
        Assert.Equal(count, rowsWithText);
    }

    // Issue #14230: two lines are merged and translated as one sentence, and the reply contains
    // a clock time written with a period ("04.00 uur") where the English source had a colon
    // ("4:00am"). The split back over the two rows must not mistake that period for the end of
    // a sentence and cut the number in half.
    [Fact]
    public async Task MergeAndTranslateIfPossible_DoesNotSplitInsideANumber()
    {
        var rows = MakeRows("and Carl Wilsher back to Chislehurst", "together at around 4:00am.");
        var translator = new FixedResultTranslator
        {
            Result = "en Carl Wilsher rond 04.00 uur samen terug naar Chislehurst heeft gereden.",
        };

        var count = await MergeAndSplitHelper.MergeAndTranslateIfPossible(
            rows,
            new TranslationPair("English", "en"),
            new TranslationPair("Dutch", "nl"),
            0,
            translator,
            forceSingleLineMode: false,
            CancellationToken.None);

        Assert.Equal(2, count);
        Assert.DoesNotContain("04." + Environment.NewLine, string.Join(Environment.NewLine, rows.Select(r => r.TranslatedText)));
        Assert.Contains("04.00 uur", rows[0].TranslatedText + " " + rows[1].TranslatedText);
    }
}
