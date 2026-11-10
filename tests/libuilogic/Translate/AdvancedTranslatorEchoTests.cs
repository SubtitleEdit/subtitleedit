using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

/// <summary>
/// A batch reply where the model handed a line back in the source language (TranslateGemma 12B,
/// en -> de) must not count as usable - it is retried, then bisected down to that line, which is
/// then kept (it may legitimately read the same in both languages).
/// </summary>
public class AdvancedTranslatorEchoTests
{
    private static readonly List<LlamaCppAdvancedProtocol.BatchLine> Lines = new()
    {
        new(1, "Where were you last night?"),
        new(2, "Erik."),
        new(3, "I was at the office until late."),
    };

    [Fact]
    public void FullyTranslatedBatch_HasNoEcho()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Erik.", // a name stays the same - not an echo
            [3] = "Ich war bis spät im Büro.",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindUntranslatedEcho(map, Lines, "English", "German"));
    }

    [Fact]
    public void LineReturnedInSourceLanguage_IsReported()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Erik.",
            [3] = "I was at the office until late.",
        };

        Assert.Equal(3, AdvancedTranslatorBase.FindUntranslatedEcho(map, Lines, "English", "German"));
    }

    [Fact]
    public void SameSourceAndTargetLanguage_IsNeverEcho()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Where were you last night?",
            [2] = "Erik.",
            [3] = "I was at the office until late.",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindUntranslatedEcho(map, Lines, "English", "English"));
    }

    [Fact]
    public void VariantOfSameLanguage_IsNeverEcho()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Where were you last night?",
            [2] = "Erik.",
            [3] = "I was at the office until late.",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindUntranslatedEcho(map, Lines, "Spanish", "Spanish (Latin America)"));
    }

    // Issue #16555: rows shifted up by one after the model merged two of them, and the last row
    // padded with a copy of an earlier translation - in the batch or in the rolling history.
    [Fact]
    public void ShiftedBatchPaddedWithEarlierLine_IsReported()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht? Ich war bis spät im Büro.",
            [2] = "Erik.",
            [3] = "Wo warst du letzte Nacht? Ich war bis spät im Büro.",
        };

        Assert.Equal(3, AdvancedTranslatorBase.FindDuplicatedTranslation(map, Lines, new List<LlamaCppAdvancedProtocol.HistoryPair>()));
    }

    [Fact]
    public void BatchLineRepeatingHistoryTranslation_IsReported()
    {
        var history = new List<LlamaCppAdvancedProtocol.HistoryPair>
        {
            new("You know that I would do everything for you.", "Du weißt, dass ich alles für dich tun würde."),
        };
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Erik.",
            [3] = "Du weißt, dass ich alles für dich tun würde.",
        };

        Assert.Equal(3, AdvancedTranslatorBase.FindDuplicatedTranslation(map, Lines, history));
    }

    [Fact]
    public void AlignedBatch_HasNoDuplicate()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Erik.",
            [3] = "Ich war bis spät im Büro.",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindDuplicatedTranslation(map, Lines, new List<LlamaCppAdvancedProtocol.HistoryPair>()));
    }

    // Issue #16585: the same shift, but the freed row padded with "..." instead of a copy.
    [Fact]
    public void ShiftedBatchPaddedWithPlaceholder_IsReported()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht? Erik.",
            [2] = "Ich war bis spät im Büro.",
            [3] = "...",
        };

        Assert.Equal(3, AdvancedTranslatorBase.FindPlaceholderTranslation(map, Lines));
    }

    [Fact]
    public void AlignedBatch_HasNoPlaceholder()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Erik.",
            [3] = "Ich war bis spät im Büro.",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindPlaceholderTranslation(map, Lines));
    }
}
