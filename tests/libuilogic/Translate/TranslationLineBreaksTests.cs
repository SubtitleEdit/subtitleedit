using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace LibUiLogicTests.Translate;

public class TranslationLineBreaksTests
{
    [Theory]
    [InlineData("Can you hold the door?\nMy hands are full.", "Kan du holde døren? Mine hænder er fulde.", "Kan du holde døren?\nMine hænder er fulde.")]
    [InlineData("- Is Nora driving?\n- No.", "- Kører Nora? - Nej.", "- Kører Nora?\n- Nej.")]
    [InlineData("– Is Nora driving?\n– No.", "— Kører Nora? — Nej.", "— Kører Nora?\n— Nej.")]
    [InlineData("- Wait.\n- Why?\n- Go.", "- Vent. - Hvorfor? - Gå.", "- Vent.\n- Hvorfor?\n- Gå.")]
    [InlineData("I have to go.", "Jeg skal\r\ngå.", "Jeg skal gå.")]
    public void RepairsLayoutWithoutRewritingText(string source, string translation, string expected)
    {
        Assert.Equal(expected.ReplaceLineEndings(), TranslationLineBreaks.Repair(source, translation, 43));
    }

    [Theory]
    [InlineData("- You all right?\n- Never better.", "- Er du okay? Aldrig bedre.", 43)]
    [InlineData("- Wait.\n- Go.", "- Vent. - Nej. - Gå.", 43)]
    [InlineData("– Wait.\n– Go.", "– Vent – bare lidt. – Gå.", 43)]
    [InlineData("– Wait\nhere.\n– Go.", "– Vent her. – Gå.", 43)]
    [InlineData("I have\nto go.", "Nej.", 43)]
    [InlineData("I have to go.", "Jeg skal\ngå.", 8)]
    [InlineData("Look at it\nnow.", "Se på <i>det</i> nu.", 43)]
    [InlineData("Go\nright away.", "Gå\nstraks.", 43)]
    [InlineData("A\nB\nC", "A B C", 43)]
    public void RetainsOriginalWhenRepairIsUnnecessaryOrAmbiguous(string source, string translation, int maximumLength)
    {
        Assert.Equal(translation, TranslationLineBreaks.Repair(source, translation, maximumLength));
    }

    [Theory]
    [InlineData("<i>Is Nora driving?\nI hope so.</i>", "Kører Nora? Det håber jeg.", "<i>Kører Nora?\nDet håber jeg.</i>")]
    [InlineData("<b><i>Is Nora driving?\nI hope so.</i></b>", "<i>Kører Nora? Det håber jeg.</i>", "<b><i>Kører Nora?\nDet håber jeg.</i></b>")]
    [InlineData("<i>Is Nora</i> driving?\nI hope so.", "<i>Kører Nora?</i> Det håber jeg.", "<i>Kører Nora?</i> Det håber jeg.")]
    [InlineData("♪ Is Nora driving?\nI hope so. ♪", "Kører Nora? Det håber jeg.", "♪ Kører Nora? Det håber jeg. ♪")]
    [InlineData("<i>Is Nora driving?\nI hope so.</i>", "Kører <b>Nora</b>? Det håber jeg.", "<i>Kører <b>Nora</b>? Det håber jeg.</i>")]
    public void RepairsWholeCueStylingWithTheOriginalTags(string source, string translation, string expected)
    {
        var stripped = StrippedLine.StripForTranslation(source);
        Assert.Equal(expected.ReplaceLineEndings(), TranslationLineBreaks.Restore(stripped, translation, 43));
    }
}
