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

    // Issue #14484: "Frau Meier." comes back as "Mrs. Meier." - one period more than the source.
    // The split must not cut that row off at "Mrs." and shift every later row by a sentence.
    private sealed class FixedAbbreviations : IDisposable
    {
        private readonly Func<string, HashSet<string>> _previous = MergeAndSplitHelper.AbbreviationsForLanguage;

        public FixedAbbreviations(Dictionary<string, string[]> perLanguage)
        {
            MergeAndSplitHelper.AbbreviationsForLanguage = code =>
                perLanguage.TryGetValue(code, out var list)
                    ? new HashSet<string>(list, StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void Dispose() => MergeAndSplitHelper.AbbreviationsForLanguage = _previous;
    }

    private static readonly Dictionary<string, string[]> NoAbbreviations = new();

    private static readonly Dictionary<string, string[]> GermanEnglishAbbreviations = new()
    {
        ["de"] = ["Dr.", "usw."],
        ["en"] = ["Mr.", "Mrs.", "Dr.", "etc."],
    };

    private static async Task<int> TranslateGermanToEnglish(ObservableCollection<TranslateRow> rows, string reply)
    {
        return await MergeAndSplitHelper.MergeAndTranslateIfPossible(
            rows,
            new TranslationPair("German", "de"),
            new TranslationPair("English", "en"),
            0,
            new FixedResultTranslator { Result = reply },
            forceSingleLineMode: false,
            CancellationToken.None);
    }

    [Fact]
    public async Task MergeAndTranslateIfPossible_AbbreviationPeriodInReplyDoesNotShiftRows()
    {
        using var _ = new FixedAbbreviations(GermanEnglishAbbreviations);
        var rows = MakeRows("Wer?", "Frau Meier.", "Er wollte eine Frau.", "Was ist los?");

        var count = await TranslateGermanToEnglish(rows, "Who? Mrs. Meier. He wanted a woman. What's wrong?");

        Assert.Equal(4, count);
        Assert.Equal(["Who?", "Mrs. Meier.", "He wanted a woman.", "What's wrong?"], rows.Select(r => r.TranslatedText));
    }

    [Fact]
    public async Task MergeAndTranslateIfPossible_AbbreviationPeriodInSourceDoesNotShiftRows()
    {
        using var _ = new FixedAbbreviations(GermanEnglishAbbreviations);
        var rows = MakeRows("Herr Dr. Meier ist hier.", "Er wollte eine Frau.", "Was ist los?");

        var count = await TranslateGermanToEnglish(rows, "Mr. Dr. Meier is here. He wanted a woman. What's wrong?");

        Assert.Equal(3, count);
        Assert.Equal(["Mr. Dr. Meier is here.", "He wanted a woman.", "What's wrong?"], rows.Select(r => r.TranslatedText));
    }

    [Fact]
    public async Task MergeAndTranslateIfPossible_AbbreviationAtEndOfRowStillEndsTheRow()
    {
        using var _ = new FixedAbbreviations(GermanEnglishAbbreviations);
        var rows = MakeRows("Äpfel, Birnen usw.", "Was ist los?");

        var count = await TranslateGermanToEnglish(rows, "Apples, pears, etc." + Environment.NewLine + "What's wrong?");

        Assert.Equal(2, count);
        Assert.Equal(["Apples, pears, etc.", "What's wrong?"], rows.Select(r => r.TranslatedText));
    }

    // An abbreviation the lists do not know still shifts the split. The relaxed strategy used
    // to accept the shifted result whenever the block ended in a rarer character like '?',
    // because the final row swallowed everything left over. It must fail instead, so the
    // caller falls back to translating the rows one by one.
    [Fact]
    public async Task MergeAndTranslateIfPossible_UnknownAbbreviationDoesNotProduceShiftedRows()
    {
        using var _ = new FixedAbbreviations(NoAbbreviations);
        var rows = MakeRows("Wer?", "Frau Meier.", "Er wollte eine Frau.", "Was ist los?");

        var count = await TranslateGermanToEnglish(rows, "Who? Mrs. Meier. He wanted a woman. What's wrong?");

        Assert.Equal(0, count);
        Assert.All(rows, r => Assert.Equal(string.Empty, r.TranslatedText));
    }

    // The relaxed strategy must keep accepting a split whose period counts differ for a
    // harmless reason: here the extra period sits inside a row that ends in '?'.
    [Fact]
    public async Task MergeAndTranslateIfPossible_ExtraPeriodInsideQuestionRowIsAccepted()
    {
        using var _ = new FixedAbbreviations(NoAbbreviations);
        var rows = MakeRows("Wer ist diese Frau Meier?", "Er wollte eine Frau.");

        var count = await TranslateGermanToEnglish(rows, "Who is this Mrs. Meier? He wanted a woman.");

        Assert.Equal(2, count);
        Assert.Equal(["Who is this Mrs. Meier?", "He wanted a woman."], rows.Select(r => r.TranslatedText));
    }

    // Ellipsis periods keep counting on both sides, so a row ending in "..." still splits.
    [Fact]
    public async Task MergeAndTranslateIfPossible_EllipsisRowStillSplits()
    {
        using var _ = new FixedAbbreviations(GermanEnglishAbbreviations);
        var rows = MakeRows("Warte...", "Was ist los?");

        var count = await TranslateGermanToEnglish(rows, "Wait... What's wrong?");

        Assert.Equal(2, count);
        Assert.Equal(["Wait...", "What's wrong?"], rows.Select(r => r.TranslatedText));
    }

    [Theory]
    [InlineData("Who? Mrs. Meier.", 1)]
    [InlineData("Mr. Dr. Meier is here.", 1)]
    [InlineData("Apples, pears, etc.", 1)]
    [InlineData("Apples, pears, etc.\nWhat's wrong?", 1)]
    [InlineData("At 5 p.m. we leave.", 1)]
    [InlineData("Dr.Meier is here.", 1)]
    [InlineData("Wait... What?", 3)]
    [InlineData("Warte...", 3)]
    [InlineData("Wait. .. What?", 3)]
    [InlineData("One. Two. Three.", 3)]
    [InlineData("No period here", 0)]
    public void CountSentencePeriods_SkipsAbbreviationPeriods(string text, int expected)
    {
        var abbreviations = new HashSet<string>(["Mr.", "Mrs.", "Dr.", "etc."], StringComparer.OrdinalIgnoreCase);

        Assert.Equal(expected, MergeAndSplitHelper.CountSentencePeriods(text, abbreviations));
    }

    // Issue #14484: a row holding "I told you so. Kira Dorn." was re-broken after the period,
    // but "Who? Kira Dorn." stayed on one line - every sentence ending should count.
    [Fact]
    public async Task MergeAndTranslateIfPossible_LineCountSplitBreaksAfterQuestionMarkToo()
    {
        using var _ = new FixedAbbreviations(NoAbbreviations);
        var rows = MakeRows("Ein Stadtflitzer? Wirklich, ich meine es ernst.", "Ja, Herr Schmidt.");

        // Same line count as the request, but a period more ("Mr."), so the line-count
        // strategy is the one that applies.
        var count = await TranslateGermanToEnglish(rows, "A city runabout? Really, I mean it." + Environment.NewLine + "Yes, Mr. Smith.");

        Assert.Equal(2, count);
        Assert.Equal("A city runabout?" + Environment.NewLine + "Really, I mean it.", rows[0].TranslatedText);
        Assert.Equal("Yes, Mr. Smith.", rows[1].TranslatedText);
    }
}
