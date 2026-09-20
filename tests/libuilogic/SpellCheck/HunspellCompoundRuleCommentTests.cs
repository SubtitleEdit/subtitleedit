using System.Text;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;

namespace LibUiLogicTests.SpellCheck;

/// <summary>
/// WeCantSpell.Hunspell reads a trailing "# comment" on a COMPOUNDRULE line as part of the rule, so
/// the rule never matches (#14788, aarondandy/WeCantSpell.Hunspell#118). SE strips those comments
/// before loading; native Hunspell ignores them.
/// </summary>
public class HunspellCompoundRuleCommentTests
{
    private const string Dic = "2\nachten/N1\nzestig/n2\n";

    [Fact]
    public void CompoundRuleWithTrailingComment_MatchesLikeNativeHunspell()
    {
        var folder = Path.Combine(Path.GetTempPath(), "se-hunspell-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var dic = Path.Combine(folder, "nl_NL.dic");
            File.WriteAllText(dic, Dic);
            File.WriteAllText(Path.Combine(folder, "nl_NL.aff"),
                "SET UTF-8\nFLAG long\nCOMPOUNDRULE 1\nCOMPOUNDRULE (N1)(n2)\t  \t# eenen+zestig[ste]\n");

            var spellChecker = new SpellChecker();
            Assert.True(spellChecker.Initialize(dic, "nl"));
            Assert.True(spellChecker.DoSpell("achtenzestig"));
            Assert.False(spellChecker.DoSpell("zestigachten"));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public void StripCompoundRuleComments_OnlyTouchesCommentedCompoundRules()
    {
        var input = "SET UTF-8\r\nCOMPOUNDRULE 3 # count\r\nCOMPOUNDRULE (N1)(n2)\t# c\r\nCOMPOUNDRULE (N4)(NH)\r\nCOMPOUNDRULE #x\r\nREP a b # keep\r\n# full comment\r\nSFX Yb 0 je [^m]\t\t# keep";
        var expected = "SET UTF-8\r\nCOMPOUNDRULE 3\r\nCOMPOUNDRULE (N1)(n2)\r\nCOMPOUNDRULE (N4)(NH)\r\nCOMPOUNDRULE\r\nREP a b # keep\r\n# full comment\r\nSFX Yb 0 je [^m]\t\t# keep";

        var output = SpellChecker.StripCompoundRuleComments(Encoding.UTF8.GetBytes(input));

        Assert.Equal(expected, Encoding.UTF8.GetString(output));
    }
}
