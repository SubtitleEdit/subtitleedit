using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

/// <summary>
/// How a failed advanced-engine batch is shortened, and how the run remembers the size that
/// worked - fewer wasted requests than always halving and starting every batch at full size.
/// </summary>
public class AdvancedTranslatorBatchCutTests
{
    private static readonly List<LlamaCppAdvancedProtocol.BatchLine> Lines = new()
    {
        new(1, "Where were you last night?"),
        new(2, "I was at the office until late."),
        new(3, "Who else was there?"),
        new(4, "Nobody. Just me."),
    };

    private static readonly List<LlamaCppAdvancedProtocol.HistoryPair> NoHistory = new();

    [Fact]
    public void EchoNearEndOfBatch_CutsRightBeforeIt()
    {
        var failure = new AdvancedTranslatorBase.BatchFailure(46, IsShift: false);

        Assert.Equal(45, AdvancedTranslatorBase.NextChunkCount(50, failure));
    }

    [Fact]
    public void EchoEarlyInBatch_CutsSmallerThanHalf()
    {
        var failure = new AdvancedTranslatorBase.BatchFailure(8, IsShift: false);

        Assert.Equal(7, AdvancedTranslatorBase.NextChunkCount(50, failure));
    }

    [Fact]
    public void ShiftNearEndOfBatch_IsStillAtLeastHalved()
    {
        // The merge that caused the padding happened somewhere before line 46.
        var failure = new AdvancedTranslatorBase.BatchFailure(46, IsShift: true);

        Assert.Equal(25, AdvancedTranslatorBase.NextChunkCount(50, failure));
    }

    [Fact]
    public void ShiftEarlyInBatch_CutsBeforeIt()
    {
        var failure = new AdvancedTranslatorBase.BatchFailure(6, IsShift: true);

        Assert.Equal(5, AdvancedTranslatorBase.NextChunkCount(50, failure));
    }

    [Fact]
    public void BadFirstLine_IsRetriedAlone()
    {
        Assert.Equal(1, AdvancedTranslatorBase.NextChunkCount(50, new AdvancedTranslatorBase.BatchFailure(1, IsShift: false)));
        Assert.Equal(1, AdvancedTranslatorBase.NextChunkCount(50, new AdvancedTranslatorBase.BatchFailure(1, IsShift: true)));
    }

    [Fact]
    public void UnusableReplyWithoutPosition_IsHalved()
    {
        Assert.Equal(25, AdvancedTranslatorBase.NextChunkCount(50, new AdvancedTranslatorBase.BatchFailure(0, IsShift: false)));
    }

    [Theory]
    [InlineData(2, 2)]
    [InlineData(2, 0)]
    [InlineData(3, 3)]
    public void NextChunk_IsAlwaysShorterAndNeverEmpty(int count, int line)
    {
        var next = AdvancedTranslatorBase.NextChunkCount(count, new AdvancedTranslatorBase.BatchFailure(line, IsShift: false));

        Assert.InRange(next, 1, count - 1);
    }

    [Fact]
    public void FirstFailure_PicksTheEarlierOfEchoAndShift()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht? Ich war bis spät im Büro.",
            [2] = "Wer war sonst noch da?",
            [3] = "Wo warst du letzte Nacht? Ich war bis spät im Büro.", // padding after the merge
            [4] = "Nobody. Just me.", // echo
        };

        var failure = AdvancedTranslatorBase.FindFirstFailure(map, Lines, NoHistory, "English", "German");

        Assert.Equal(3, failure.Line);
        Assert.True(failure.IsShift);
    }

    [Fact]
    public void FirstFailure_ReportsEchoWhenThereIsNoShift()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "I was at the office until late.",
            [3] = "Who else was there?",
            [4] = "Niemand. Nur ich.",
        };

        var failure = AdvancedTranslatorBase.FindFirstFailure(map, Lines, NoHistory, "English", "German");

        Assert.Equal(2, failure.Line);
        Assert.False(failure.IsShift);
    }

    // "Mission Day, Foxtrot." kept in English by Index-Translate 9B failed whole 50-line batches.
    [Fact]
    public void FirstFailure_SingleEchoInBatch_DoesNotFailIt()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "I was at the office until late.",
            [3] = "Wer war sonst noch da?",
            [4] = "Niemand. Nur ich.",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindFirstFailure(map, Lines, NoHistory, "English", "German").Line);
        Assert.Equal(new List<int> { 2 }, AdvancedTranslatorBase.FindUntranslatedEchoes(map, Lines, "English", "German"));
    }

    [Theory]
    [InlineData(1, 50, true)]
    [InlineData(10, 50, true)]
    [InlineData(11, 50, false)]
    [InlineData(1, 4, true)]
    [InlineData(2, 4, false)]
    [InlineData(1, 1, false)]
    public void IsFewEchoes_AtMostOneInFive(int echoes, int lines, bool expected)
    {
        Assert.Equal(expected, AdvancedTranslatorBase.IsFewEchoes(echoes, lines));
    }

    [Fact]
    public void FirstFailure_NoneForAlignedReply()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Ich war bis spät im Büro.",
            [3] = "Wer war sonst noch da?",
            [4] = "Niemand. Nur ich.",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindFirstFailure(map, Lines, NoHistory, "English", "German").Line);
    }

    // Index-Nailong 9B, batch 50: every value was the prompt's example, numbered per line.
    [Fact]
    public void ExamplePlaceholderCopiedFromPrompt_IsReported()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "<translation of line 1>",
            [2] = "<translation of line 2>",
            [3] = "<translation of line 3>",
            [4] = "<translation of line 4>",
        };

        Assert.Equal(1, AdvancedTranslatorBase.FindPlaceholderTranslation(map, Lines, NoHistory));
        Assert.Equal(1, AdvancedTranslatorBase.FindFirstFailure(map, Lines, NoHistory, "English", "Danish").Line);
    }

    [Fact]
    public void ExamplePlaceholderLaterInBatch_IsReportedAtItsLine()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Hvor var du i går aftes?",
            [2] = "Jeg var på kontoret til sent.",
            [3] = "< Translation of line 20 >",
            [4] = "Ingen. Kun mig.",
        };

        Assert.Equal(3, AdvancedTranslatorBase.FindPlaceholderTranslation(map, Lines, NoHistory));
    }

    [Theory]
    [InlineData("<translation of line 1>", true)]
    [InlineData("<translation of line>", true)]
    [InlineData("I want a translation of line one", false)]
    [InlineData("<i>Hvor var du?</i>", false)]
    public void IsExamplePlaceholder(string value, bool expected)
    {
        Assert.Equal(expected, LlamaCppAdvancedProtocol.IsExamplePlaceholder(value));
    }

    // #16856: the value for "Come on up." also held lines 2-5 as escaped JSON plus the model's notes.
    [Fact]
    public void ValueWithLeakedJsonAndNotes_IsBroken()
    {
        var value = "\u202Bتفضل بالصعود.\n\u202B2\":\"ما الأخبار، مايرون؟\n\u202B3\":\"يا.\n\u202B5\":\"وليس لدي أي مشكلة مع ذلك.\"}]}   (Note: line 5 maintains neutrality.) Let me correct that. {";

        Assert.True(LlamaCppAdvancedProtocol.IsStructureLeak("Come on up.", value, expansion: null));
        Assert.True(LlamaCppAdvancedProtocol.IsBrokenValue("Come on up.", value, expansion: null));
    }

    [Theory]
    [InlineData("Come on up.", "2\":\"What's up?")]
    [InlineData("Come on up.", "Komm hoch.\"}")]
    [InlineData("Come on up.", "Komm hoch.\"}]}")]
    // With the JSON grammar on, a model that closes the string early can only write a typographic
    // quote; Hy-MT2 7B did, with its notes behind it (#16885).
    [InlineData("Come on up.", "Komm hoch.”} No, line 8 is in history as well. Let me fix the JSON properly. {")]
    [InlineData("Come on up.", "Komm hoch.”} Wait, line 8 appears twice in the history. Let me correct that. {")]
    [InlineData("Come on up.", "2”:“What's up?")]
    public void LeakedKeyOrObjectEnd_IsStructureLeak(string source, string value)
    {
        Assert.True(LlamaCppAdvancedProtocol.IsStructureLeak(source, value, expansion: null));
    }

    // The third value is the expansion the rest of the batch measures: about 1 for a Latin pair,
    // 3-5 for Chinese to English (#16885).
    [Theory]
    [InlineData("Come on up.", "Komm hoch.", 1.1)]
    [InlineData("It's 5:30.", "Es ist 5:30.", 1.1)]
    [InlineData("He was 6\" tall.", "Er war 6\" groß.", 1.1)]
    [InlineData("No.", "Nein.", 1.1)]
    [InlineData("Where were you last night? I was at the office until late.", "Wo warst du letzte Nacht? Ich war bis spät im Büro.", 1.1)]
    [InlineData("\"No,\" she said.", "“Nein”, sagte sie.", 1.1)]
    [InlineData("He said “no” and left at 5:30.", "Er sagte „nein“ und ging um 5:30.", 1.1)]
    [InlineData("把他带回来 这次我要亲自问他昨天晚上的每一分钟", "Bring him back. This time, I'll personally question him about every single minute of last night.", 3.3)]
    [InlineData("明天早上八点 所有人都到会议室来 一个都不能少", "Everyone, meet in the conference room at eight o'clock tomorrow morning. Not a single person is allowed to be absent.", 3.3)]
    [InlineData("她说她整晚都在家 可是邻居听见她半夜出门了", "She said she was at home the whole night, but the neighbours heard her leaving in the middle of the night.", 3.3)]
    public void NormalTranslation_IsNotStructureLeak(string source, string value, double expansion)
    {
        Assert.False(LlamaCppAdvancedProtocol.IsStructureLeak(source, value, expansion));
        Assert.False(LlamaCppAdvancedProtocol.IsBrokenValue(source, value, expansion));
    }

    [Fact]
    public void RunawayFarLongerThanSource_IsStructureLeak()
    {
        var value = "Komm hoch. Und dann reden wir darüber, was gestern passiert ist, und warum niemand mir etwas gesagt hat, obwohl alle es wussten.";

        Assert.True(LlamaCppAdvancedProtocol.IsStructureLeak("Come on up.", value, 1.1));
        // Nothing to measure against (a lone line): no length test.
        Assert.False(LlamaCppAdvancedProtocol.IsStructureLeak("Come on up.", value, expansion: null));
    }

    // #16885: the same Chinese line that is a normal translation at 96 characters is a runaway
    // once the model's notes hang off it - the budget is 4 x 3.6 x 23 = 331 characters.
    [Fact]
    public void NotesAfterCjkTranslation_IsStructureLeak()
    {
        var value = "Bring him back. This time, I'll personally question him about every single minute of last night. " +
                    "(Note: the speaker is a police officer, so I kept the tone formal and direct. " +
                    "The phrase about every minute is an idiom for a thorough interrogation; a literal rendering would read oddly in English. " +
                    "Let me know if you would prefer a shorter version for the subtitle.)";

        Assert.True(LlamaCppAdvancedProtocol.IsStructureLeak("把他带回来 这次我要亲自问他昨天晚上的每一分钟", value, 3.6));
    }

    [Fact]
    public void MeasureExpansion_IsWeightedMedianOfTranslationCharactersPerSourceCharacter()
    {
        var pairs = new List<(string Source, string Translation)>
        {
            ("Where were you last night?", "Wo warst du letzte Nacht?"),
            ("I was at the office until late.", "Ich war bis spät im Büro."),
            ("Who else was there?", "Wer war sonst noch da?"),
            ("Nobody. Just me.", "Niemand. Nur ich."),
        };

        // Ratios 25/31, 25/26, 17/16, 22/19 over 92 source characters: the median falls in 25/26.
        Assert.Equal(25.0 / 26, LlamaCppAdvancedProtocol.MeasureExpansion(pairs)!.Value, 3);
    }

    [Fact]
    public void MeasureExpansion_NeedsTwentySourceCharacters()
    {
        var pairs = new List<(string Source, string Translation)> { ("嗯", "Hmm."), ("好的", "Okay."), ("什么？", "What?") };

        Assert.Null(LlamaCppAdvancedProtocol.MeasureExpansion(pairs));
    }

    [Fact]
    public void BatchExpansion_LeavesTheLineOutAndTakesTheHistoryIn()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Ich war bis spät im Büro.",
            [3] = "Wer war sonst noch da?",
            [4] = "Niemand. Nur ich.",
        };
        var history = new List<LlamaCppAdvancedProtocol.HistoryPair> { new("Erik.", "Erik.") };

        // Line 1 left out: ratios 25/31, 17/16, 22/19 - the median is 17/16; the history pair
        // (5/5) moves it to 1.
        Assert.Equal(17.0 / 16, AdvancedTranslatorBase.MeasureExpansion(map, Lines, NoHistory, 1)!.Value, 3);
        Assert.Equal(1.0, AdvancedTranslatorBase.MeasureExpansion(map, Lines, history, 1)!.Value, 3);
    }

    [Fact]
    public void RunawayInBatch_IsReportedAtItsLine()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Ich war bis spät im Büro.",
            [3] = "Wer war sonst noch da? Und dann reden wir darüber, was gestern passiert ist, und warum niemand mir etwas gesagt hat, obwohl alle es wussten.",
            [4] = "Niemand. Nur ich.",
        };

        Assert.Equal(3, AdvancedTranslatorBase.FindPlaceholderTranslation(map, Lines, NoHistory));
    }

    // Two runaways in a short batch must not lift each other's budget: with the overall ratio,
    // each saw the other's 130 characters and measured 4.2, a budget of 184.
    [Fact]
    public void TwoRunawaysInShortBatch_AreReported()
    {
        const string runaway = "Komm hoch. Und dann reden wir darüber, was gestern passiert ist, und warum niemand mir etwas gesagt hat, obwohl alle es wussten.";
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine>
        {
            new(1, "Where were you last night?"),
            new(2, "Come on up."),
            new(3, "Come on up."),
        };
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = runaway,
            [3] = runaway,
        };

        Assert.Equal(2, AdvancedTranslatorBase.FindPlaceholderTranslation(map, lines, NoHistory));
    }

    // #16885: Hy-MT2 7B's reply to the issue's rows 16-20. Row 17 is 4.8 times its source in
    // characters, 101 against a fixed budget of 84; against the 3.2 the other lines measure it
    // has 272.
    [Fact]
    public void LongCorrectChineseLineInBatch_IsNotReported()
    {
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine>
        {
            new(1, "把他带回来 这次我要亲自问他昨天晚上的每一分钟"),
            new(2, "她说她整晚都在家 可是邻居听见她半夜出门了"),
            new(3, "放大一点 我想看清楚坐在副驾驶位置上的那个人"),
            new(4, "那你倒是解释一下 你的指纹是怎么跑到车里去的"),
        };
        var map = new Dictionary<int, string>
        {
            [1] = "Bring him back. This time, I'll personally question him about every minute last night.",
            [2] = "She said she was at home the whole night, but neighbors heard her leaving in the middle of the night.",
            [3] = "Zoom in a bit. I want to see clearly who's sitting in the passenger seat.",
            [4] = "Then explain, how did your fingerprints end up in the car?",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindPlaceholderTranslation(map, lines, NoHistory));
        Assert.Equal(-1, AdvancedTranslatorBase.FindFirstFailure(map, lines, NoHistory, "Chinese", "English").Line);
    }

    // A first batch of short lines has too little to measure, so a long value is simply taken.
    [Fact]
    public void BatchTooShortToMeasure_HasNoLengthTest()
    {
        var lines = new List<LlamaCppAdvancedProtocol.BatchLine> { new(1, "嗯"), new(2, "好的"), new(3, "不") };
        var map = new Dictionary<int, string>
        {
            [1] = "Hmm.",
            [2] = "Okay.",
            [3] = "Absolutely not, I refuse to do anything of the sort, and that is my final answer on the matter.",
        };

        Assert.Equal(-1, AdvancedTranslatorBase.FindPlaceholderTranslation(map, lines, NoHistory));
    }

    [Fact]
    public void LeakInBatch_IsReportedAtItsLine()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Ich war bis spät im Büro.\n3\":\"Wer war sonst noch da?",
            [3] = "Wer war sonst noch da?",
            [4] = "Niemand. Nur ich.",
        };

        Assert.Equal(2, AdvancedTranslatorBase.FindPlaceholderTranslation(map, Lines, NoHistory));
    }

    [Fact]
    public void FirstMissingLine_FindsTruncatedTail()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "Ich war bis spät im Büro.",
        };

        Assert.Equal(3, AdvancedTranslatorBase.FindFirstMissingLine(map, Lines));
    }

    [Fact]
    public void FirstMissingLine_EmptyTranslationCounts()
    {
        var map = new Dictionary<int, string>
        {
            [1] = "Wo warst du letzte Nacht?",
            [2] = "",
            [3] = "Wer war sonst noch da?",
            [4] = "Niemand. Nur ich.",
        };

        Assert.Equal(2, AdvancedTranslatorBase.FindFirstMissingLine(map, Lines));
    }

    [Fact]
    public void FirstMissingLine_ZeroForEmptyReply()
    {
        Assert.Equal(0, AdvancedTranslatorBase.FindFirstMissingLine(new Dictionary<int, string>(), Lines));
    }

    [Fact]
    public void Governor_UsesConfiguredSizeUntilACut()
    {
        var governor = new BatchSizeGovernor();
        governor.OnSuccess();

        Assert.Equal(50, governor.Limit(50));
    }

    [Fact]
    public void Governor_KeepsTheCutSizeThenGrowsBack()
    {
        var governor = new BatchSizeGovernor();
        governor.OnCut(20);
        Assert.Equal(20, governor.Limit(50));

        governor.OnSuccess();
        Assert.Equal(20, governor.Limit(50));

        governor.OnSuccess(); // two in a row -> grow by half
        Assert.Equal(30, governor.Limit(50));

        for (var i = 0; i < 2 * BatchSizeGovernor.SuccessesBeforeGrowing; i++)
        {
            governor.OnSuccess();
        }

        Assert.Equal(50, governor.Limit(50)); // never above the setting
    }

    [Fact]
    public void Governor_CutResetsTheStreakAndOnlyShrinks()
    {
        var governor = new BatchSizeGovernor();
        governor.OnCut(20);
        governor.OnSuccess();
        governor.OnCut(30); // a later, larger cut does not grow the size
        governor.OnSuccess();

        Assert.Equal(20, governor.Limit(50));
    }

    [Fact]
    public void Governor_ResetForgetsTheCut()
    {
        var governor = new BatchSizeGovernor();
        governor.OnCut(5);
        governor.Reset();

        Assert.Equal(50, governor.Limit(50));
    }
}
