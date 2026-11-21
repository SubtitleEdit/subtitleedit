using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

/// <summary>
/// The rolling history of the advanced engines: a typeset sign repeated over many frames must not
/// fill it, or the model has nothing but that sign to go by.
/// </summary>
public class AdvancedTranslatorHistoryTests
{
    private static TranslateRow Row(string text, string translation) => new() { Text = text, TranslatedText = translation };

    [Fact]
    public void RepeatedSignFrames_FillOnePair_AndTheDialogueBeforeThemIsKept()
    {
        var rows = new List<TranslateRow>
        {
            Row("Are you going anywhere for winter break?", "Skal du nogen steder i juleferien?"),
            Row("Yeah. I'm visiting my mom's family.", "Ja. Jeg besøger min mors familie."),
        };
        for (var i = 0; i < 16; i++)
        {
            // Each frame has its own position tag; the text is the same.
            rows.Add(Row("{\\fnNinja\\pos(976," + i + ")}Usagiyama Market", "{\\fnNinja\\pos(976," + i + ")}Usagiyama Market"));
        }

        rows.Add(Row("Ow!", "Av!"));
        rows.Add(Row("The sky was so blue.", string.Empty)); // the batch starts here

        var history = AdvancedTranslatorBase.CollectHistory(rows, rows.Count - 1, historyPairs: 12);

        Assert.Equal(
            new[] { "Are you going anywhere for winter break?", "Yeah. I'm visiting my mom's family.", "Usagiyama Market", "Ow!" },
            history.Select(p => p.Source));
        Assert.Equal("Av!", history[^1].Target);
    }

    [Fact]
    public void DistinctLines_AreTheLastN_InOrder()
    {
        var rows = Enumerable.Range(1, 20).Select(i => Row("Line " + i, "Linje " + i)).ToList();

        var history = AdvancedTranslatorBase.CollectHistory(rows, 20, historyPairs: 3);

        Assert.Equal(new[] { "Line 18", "Line 19", "Line 20" }, history.Select(p => p.Source));
    }

    [Fact]
    public void TheSearchStopsAfterTenRowsPerPair()
    {
        // Two pairs may look 20 rows back: the distinct line 21 rows back is out of reach.
        var rows = new List<TranslateRow> { Row("Far back", "Langt tilbage") };
        for (var i = 0; i < 20; i++)
        {
            rows.Add(Row("Okay.", "Okay."));
        }

        var history = AdvancedTranslatorBase.CollectHistory(rows, rows.Count, historyPairs: 2);

        Assert.Equal(new[] { "Okay." }, history.Select(p => p.Source));
        Assert.Equal(2 * AdvancedTranslatorBase.HistoryScanFactor, rows.Count - 1);
    }
}
