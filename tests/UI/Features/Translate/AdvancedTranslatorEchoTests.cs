using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Xunit;

namespace UITests.Features.Translate;

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
}
