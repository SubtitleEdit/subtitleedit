using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class FixCommonErrorsRunnerTest
{
    [Fact]
    public void RunAll_OnEmptySubtitle_NoThrow()
    {
        var sub = new Subtitle();
        FixCommonErrorsRunner.RunAll(sub);
        Assert.Empty(sub.Paragraphs);
    }

    [Fact]
    public void RunAll_FixesMissingSpaceAfterComma()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("hello,world.", 0, 1000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        // FixMissingSpaces adds a space after the comma; FixStartWithUppercaseLetter
        // capitalises the leading 'h'. End result: "Hello, world."
        Assert.Equal("Hello, world.", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void RunAll_FixesUnneededSpaceBeforePeriod()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Hello world .", 0, 2000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        Assert.DoesNotContain(" .", sub.Paragraphs[0].Text);
        Assert.EndsWith(".", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void RunAll_RemovesEmptyLines()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Hello world.", 0, 2000));
        sub.Paragraphs.Add(new Paragraph(string.Empty, 3000, 5000));
        sub.Paragraphs.Add(new Paragraph("Goodbye.", 6000, 8000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        // FixEmptyLines drops the empty paragraph
        Assert.Equal(2, sub.Paragraphs.Count);
        Assert.Equal("Hello world.", sub.Paragraphs[0].Text);
        Assert.Equal("Goodbye.", sub.Paragraphs[1].Text);
    }

    [Fact]
    public void RunAll_FixesAloneLowercaseI_InEnglish()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Yes, i went to the store.", 0, 2000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        // FixAloneLowercaseIToUppercaseI fixes lowercase 'i' as a standalone word
        Assert.Contains(" I ", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_WithExplicitRule_OnlyAppliesThatRule()
    {
        var sub = new Subtitle();
        // FixMissingSpaces would add a space after the comma; FixStartWithUppercaseLetter*
        // would capitalise the leading 'h'. With only FixMissingSpaces selected, the 'h'
        // must stay lowercase.
        sub.Paragraphs.Add(new Paragraph("hello,world.", 0, 2000));
        sub.Renumber();

        FixCommonErrorsRunner.Run(sub, ["FixMissingSpaces"]);

        Assert.Equal("hello, world.", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_WithEmptyList_RunsAllRules()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("hello,world.", 0, 2000));
        sub.Renumber();

        FixCommonErrorsRunner.Run(sub, Array.Empty<string>());

        // Same outcome as RunAll: capitalised + space inserted
        Assert.Equal("Hello, world.", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void ResolveRuleIds_NullOrWhitespace_ReturnsAll()
    {
        var all = FixCommonErrorsRunner.AvailableRuleIds;

        Assert.Equal(all, FixCommonErrorsRunner.ResolveRuleIds(null));
        Assert.Equal(all, FixCommonErrorsRunner.ResolveRuleIds(""));
        Assert.Equal(all, FixCommonErrorsRunner.ResolveRuleIds("   "));
        Assert.Equal(all, FixCommonErrorsRunner.ResolveRuleIds("all"));
    }

    [Fact]
    public void ResolveRuleIds_ExplicitList_ReturnsSubsetInCanonicalOrder()
    {
        // Spec is in user-supplied order; result should be in canonical (alphabetical-ish)
        // order so behavior is deterministic.
        var resolved = FixCommonErrorsRunner.ResolveRuleIds("FixMissingSpaces,FixCommas");

        Assert.Equal(new[] { "FixCommas", "FixMissingSpaces" }, resolved);
    }

    [Fact]
    public void ResolveRuleIds_AllMinusOne_DropsThatRule()
    {
        var resolved = FixCommonErrorsRunner.ResolveRuleIds("all,-FixDanishLetterI");

        Assert.Equal(FixCommonErrorsRunner.AvailableRuleIds.Count - 1, resolved.Count);
        Assert.DoesNotContain("FixDanishLetterI", resolved);
    }

    [Fact]
    public void ResolveRuleIds_NegationsOnly_ImpliesAll()
    {
        var resolved = FixCommonErrorsRunner.ResolveRuleIds("-FixDanishLetterI,-FixCommas");

        Assert.Equal(FixCommonErrorsRunner.AvailableRuleIds.Count - 2, resolved.Count);
        Assert.DoesNotContain("FixDanishLetterI", resolved);
        Assert.DoesNotContain("FixCommas", resolved);
    }

    [Fact]
    public void ResolveRuleIds_CaseInsensitive()
    {
        var resolved = FixCommonErrorsRunner.ResolveRuleIds("fixcommas,FIXMISSINGSPACES");

        Assert.Equal(new[] { "FixCommas", "FixMissingSpaces" }, resolved);
    }

    [Fact]
    public void ResolveRuleIds_UnknownRule_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => FixCommonErrorsRunner.ResolveRuleIds("FixCommas,NotARealRule"));

        Assert.Contains("NotARealRule", ex.Message);
    }

    // Issue #11037 review: users prototype in the GUI, then script. Every rule the CLI
    // can run must expose a GUI-equivalent label so `list-fce-rules` / the docs can map
    // CLI IDs back to the desktop window's check-boxes. This guards against a new rule
    // being added to the runner without a matching label.
    [Fact]
    public void GuiLabels_CoversEveryAvailableRule_WithNonEmptyText()
    {
        foreach (var id in FixCommonErrorsRunner.AvailableRuleIds)
        {
            Assert.True(
                FixCommonErrorsRunner.GuiLabels.TryGetValue(id, out var label),
                $"Rule '{id}' has no GUI-equivalent label in FixCommonErrorsRunner.GuiLabels.");
            Assert.False(string.IsNullOrWhiteSpace(label), $"GUI label for '{id}' is empty.");
        }
    }

    [Fact]
    public void GuiLabels_HasNoLabelForUnknownRule()
    {
        // Keep the map tight to the rule set — a stray key would silently drift from
        // AvailableRuleIds and never surface in list-fce-rules.
        var available = new HashSet<string>(FixCommonErrorsRunner.AvailableRuleIds, System.StringComparer.OrdinalIgnoreCase);
        foreach (var id in FixCommonErrorsRunner.GuiLabels.Keys)
        {
            Assert.Contains(id, available);
        }
    }

    // -------------------------------------------------------------------------
    // Issue #11037 item 3: language-conditional rules mirror the GUI's Fix Common
    // Errors window — Spanish-only rules run only when the language matches
    // (auto-detected, or forced via --fce-language). Naming a gated rule in
    // --FixCommonErrorsRules selects it but does not bypass its language gate.
    // -------------------------------------------------------------------------

    [Fact]
    public void RunAll_OnEnglish_SkipsSpanishInvertedMarks()
    {
        // English line that ends with '?' should NOT get a leading '¿'.
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Are you coming home tonight?", 0, 3000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        Assert.DoesNotContain('¿', sub.Paragraphs[0].Text);
        Assert.DoesNotContain('¡', sub.Paragraphs[0].Text);
    }

    [Fact]
    public void RunAll_OnSpanish_AppliesSpanishInvertedMarks()
    {
        // Spanish-language paragraphs should get the inverted-mark fix as the
        // GUI does for "es" content. Use enough text for LanguageAutoDetect.
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Cuántos años tienes?", 0, 3000));
        sub.Paragraphs.Add(new Paragraph("Dónde está la biblioteca?", 4000, 7000));
        sub.Paragraphs.Add(new Paragraph("Qué hora es?", 8000, 11000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);

        Assert.Contains('¿', sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_NamingSpanishRule_DoesNotBypassGateOnNonSpanish()
    {
        // Selecting the Spanish rule by name no longer forces it past the language gate:
        // on English content it stays gated off, so no leading '¿' is inserted (#11037).
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Are you coming home tonight?", 0, 3000));
        sub.Renumber();

        FixCommonErrorsRunner.Run(sub, new[] { "FixSpanishInvertedQuestionAndExclamationMarks" });

        Assert.DoesNotContain('¿', sub.Paragraphs[0].Text);
        Assert.DoesNotContain('¡', sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_CliImplicitPath_GatesSpanishOnNonSpanish()
    {
        // ConvertCommand pre-resolves a bare --FixCommonErrors to the full rule list via
        // ResolveRuleIds(null), then passes that into Run. The Spanish rule must stay gated
        // off on English content even though it is in the resolved "wanted" set.
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Are you coming home tonight?", 0, 3000));
        sub.Renumber();

        var allRules = FixCommonErrorsRunner.AvailableRuleIds;
        FixCommonErrorsRunner.Run(sub, allRules);

        Assert.DoesNotContain('¿', sub.Paragraphs[0].Text);
        Assert.DoesNotContain('¡', sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_NamingSpanishRule_WithFceLanguage_AppliesOnNonSpanish()
    {
        // The escape hatch: naming the rule and forcing --fce-language:es opens the gate,
        // so the rule fires even on content that auto-detects as English.
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Are you coming home tonight?", 0, 3000));
        sub.Renumber();

        var rules = new[] { "FixSpanishInvertedQuestionAndExclamationMarks" };
        FixCommonErrorsRunner.Run(sub, rules, languageOverride: "es");

        Assert.Contains('¿', sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_WithFceLanguageOverride_ForcesGateOnNonSpanish()
    {
        // --fce-language:es forces the Spanish gate open even for content that would
        // auto-detect as something else — the escape hatch for short/mis-detected files.
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Are you coming home tonight?", 0, 3000));
        sub.Renumber();

        var allRules = FixCommonErrorsRunner.AvailableRuleIds;
        FixCommonErrorsRunner.Run(sub, allRules, languageOverride: "es");

        Assert.Contains('¿', sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_WithFceLanguageOverride_AcceptsEnglishName()
    {
        // The override normalizes an English name ("Spanish") to its two-letter code.
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Are you coming home tonight?", 0, 3000));
        sub.Renumber();

        var allRules = FixCommonErrorsRunner.AvailableRuleIds;
        FixCommonErrorsRunner.Run(sub, allRules, languageOverride: "Spanish");

        Assert.Contains('¿', sub.Paragraphs[0].Text);
    }

    [Theory]
    [InlineData("es", "es")]
    [InlineData("ES", "es")]
    [InlineData("spa", "es")]
    [InlineData("Spanish", "es")]
    [InlineData("English", "en")]
    public void NormalizeLanguageOverride_ResolvesCodesAndNames(string input, string expected)
    {
        Assert.Equal(expected, FixCommonErrorsRunner.NormalizeLanguageOverride(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("spanihs")]
    public void NormalizeLanguageOverride_BlankOrUnknown_ReturnsNull(string? input)
    {
        Assert.Null(FixCommonErrorsRunner.NormalizeLanguageOverride(input));
    }

    [Fact]
    public void LanguageGates_MapExpectedRulesToCodes()
    {
        var gates = FixCommonErrorsRunner.LanguageGates;
        Assert.Equal("en", gates["FixAloneLowercaseIToUppercaseI"]);
        Assert.Equal("da", gates["FixDanishLetterI"]);
        Assert.Equal("es", gates["FixSpanishInvertedQuestionAndExclamationMarks"]);
        Assert.Equal("tr", gates["FixTurkishAnsiToUnicode"]);
    }

    [Fact]
    public void RunAll_RunsToConvergence_SecondRunMakesNoChange()
    {
        // Regression for #11873: FixCommonErrors must converge within a single call
        // (SE4 ran the suite 3x). After one RunAll the result must be a fixed point, so
        // a second RunAll changes nothing.
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("hello ,world  .", 0, 2000));
        sub.Paragraphs.Add(new Paragraph(string.Empty, 2100, 2200));
        sub.Paragraphs.Add(new Paragraph("this  is   a    test...", 3000, 6000));
        sub.Renumber();

        FixCommonErrorsRunner.RunAll(sub);
        var afterFirst = Snapshot(sub);

        FixCommonErrorsRunner.RunAll(sub);
        var afterSecond = Snapshot(sub);

        Assert.Equal(afterFirst, afterSecond);
    }

    private static string Snapshot(Subtitle subtitle)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var p in subtitle.Paragraphs)
        {
            sb.Append(p.StartTime.TotalMilliseconds).Append('|')
              .Append(p.EndTime.TotalMilliseconds).Append('|')
              .Append(p.Text).Append('\n');
        }

        return sb.ToString();
    }
}
