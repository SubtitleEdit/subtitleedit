using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.SpellCheck;

/// <summary>
/// The Dutch dictionary flags month names with KEEPCASE ("oktober/Kc"), so Hunspell rejects
/// "Oktober" - also as the first word of a sentence, where the capital is required (#15047).
/// </summary>
public class KeepCaseSentenceStartTests
{
    private static void WithDutchChecker(Action<SpellChecker> test)
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-hunspell-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var dic = Path.Combine(folder, "nl_NL.dic");
            File.WriteAllText(dic, "3\noktober/Kc\nin\nhet\n");
            File.WriteAllText(Path.Combine(folder, "nl_abbreviations.xml"), "<Abbreviations><Item>bijv.</Item></Abbreviations>");
            File.WriteAllText(Path.Combine(folder, "nl_NL.aff"), "SET UTF-8\nFLAG long\nKEEPCASE Kc\n");

            var spellChecker = new SpellChecker();
            Assert.True(spellChecker.Initialize(dic, "nl"));
            test(spellChecker);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    private static bool Check(SpellChecker spellChecker, string text)
    {
        var index = text.IndexOf("Oktober", StringComparison.Ordinal);
        return spellChecker.IsWordCorrect(new SpellCheckWord { Index = index, Text = "Oktober" }, text);
    }

    [Theory]
    [InlineData("Oktober '73, in het")]
    [InlineData("In het. Oktober in het")]
    [InlineData("- In het?\n- Oktober in het")]
    [InlineData("<i>Oktober in het</i>")]
    [InlineData("In het... Oktober in het")]
    [InlineData("{\\an8}\"Oktober in het\"")]
    public void CapitalizedKeepCaseWord_AtSentenceStart_IsCorrect(string text)
    {
        WithDutchChecker(spellChecker => Assert.True(Check(spellChecker, text)));
    }

    [Theory]
    [InlineData("In het Oktober")]
    [InlineData("In het,\nOktober in")]
    [InlineData("In <i>Oktober</i>")]
    [InlineData("In het bijv. Oktober")]
    [InlineData("In het o.a. Oktober")]
    public void CapitalizedKeepCaseWord_MidSentence_IsStillWrong(string text)
    {
        WithDutchChecker(spellChecker => Assert.False(Check(spellChecker, text)));
    }

    [Fact]
    public void UnknownWord_AtSentenceStart_IsStillWrong()
    {
        WithDutchChecker(spellChecker =>
            Assert.False(spellChecker.IsWordCorrect(new SpellCheckWord { Index = 0, Text = "Oktobber" }, "Oktobber in het")));
    }
}
