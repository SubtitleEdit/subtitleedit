using Nikse.SubtitleEdit.Core.Common;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

public class RemoveFormattingRunnerTest
{
    [Fact]
    public void ResolveRuleIds_NullOrWhitespaceOrAll_ReturnsAllRules()
    {
        var all = RemoveFormattingRunner.AvailableRuleIds;

        Assert.Equal(all, RemoveFormattingRunner.ResolveRuleIds(null));
        Assert.Equal(all, RemoveFormattingRunner.ResolveRuleIds(""));
        Assert.Equal(all, RemoveFormattingRunner.ResolveRuleIds("   "));
        Assert.Equal(all, RemoveFormattingRunner.ResolveRuleIds("all"));
    }

    [Fact]
    public void ResolveRuleIds_ExplicitList_ReturnsSubsetInCanonicalOrder()
    {
        // Canonical order is the GUI checkbox order, not the order the user typed.
        var resolved = RemoveFormattingRunner.ResolveRuleIds("RemoveColor,RemoveItalic");

        Assert.Equal(new[] { "RemoveItalic", "RemoveColor" }, resolved);
    }

    [Fact]
    public void ResolveRuleIds_AllMinusOne_DropsThatRule()
    {
        var resolved = RemoveFormattingRunner.ResolveRuleIds("all,-RemoveItalic");

        Assert.DoesNotContain("RemoveItalic", resolved);
        Assert.Equal(RemoveFormattingRunner.AvailableRuleIds.Count - 1, resolved.Count);
    }

    [Fact]
    public void ResolveRuleIds_NegationsOnly_ImpliesAll()
    {
        var resolved = RemoveFormattingRunner.ResolveRuleIds("-RemoveColor");

        Assert.DoesNotContain("RemoveColor", resolved);
        Assert.Equal(RemoveFormattingRunner.AvailableRuleIds.Count - 1, resolved.Count);
    }

    [Fact]
    public void ResolveRuleIds_CaseInsensitive()
    {
        var resolved = RemoveFormattingRunner.ResolveRuleIds("removeitalic,REMOVEBOLD");

        Assert.Equal(new[] { "RemoveItalic", "RemoveBold" }, resolved);
    }

    [Fact]
    public void ResolveRuleIds_UnknownRule_ThrowsWithListCommandHint()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => RemoveFormattingRunner.ResolveRuleIds("RemoveItalic,NotARealRule"));

        Assert.Contains("NotARealRule", ex.Message);
        Assert.Contains("list-rf-rules", ex.Message);
    }

    [Fact]
    public void ToTypes_Null_MeansWholesaleAll()
    {
        // Bare --remove-formatting keeps the pre-#13518 behaviour: strip everything.
        Assert.Equal(RemoveFormattingType.All, RemoveFormattingRunner.ToTypes(null));
    }

    [Fact]
    public void ToTypes_EmptyList_MeansNone()
    {
        // The user subtracted every rule; a silent fallback to wholesale would be worse.
        Assert.Equal(RemoveFormattingType.None, RemoveFormattingRunner.ToTypes([]));
    }

    [Fact]
    public void ToTypes_MapsEachRuleToItsFlag()
    {
        Assert.Equal(RemoveFormattingType.Italic, RemoveFormattingRunner.ToTypes(["RemoveItalic"]));
        Assert.Equal(RemoveFormattingType.Bold, RemoveFormattingRunner.ToTypes(["RemoveBold"]));
        Assert.Equal(RemoveFormattingType.Underline, RemoveFormattingRunner.ToTypes(["RemoveUnderline"]));
        Assert.Equal(RemoveFormattingType.FontName, RemoveFormattingRunner.ToTypes(["RemoveFontName"]));
        Assert.Equal(RemoveFormattingType.Alignment, RemoveFormattingRunner.ToTypes(["RemoveAlignment"]));
        Assert.Equal(RemoveFormattingType.Color, RemoveFormattingRunner.ToTypes(["RemoveColor"]));
        Assert.Equal(
            RemoveFormattingType.Italic | RemoveFormattingType.Color,
            RemoveFormattingRunner.ToTypes(["RemoveItalic", "RemoveColor"]));
    }

    [Fact]
    public void Run_NullRules_StripsEverythingIncludingPositionTags()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("{\\pos(10,20)}<i>Hi</i>", 0, 1000));

        RemoveFormattingRunner.Run(sub);

        Assert.Equal("Hi", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_ItalicOnly_KeepsOtherFormatting()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("<i>Hi</i> <b>there</b>", 0, 1000));

        RemoveFormattingRunner.Run(sub, ["RemoveItalic"]);

        Assert.Equal("Hi <b>there</b>", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void Run_AllNamedRules_LeavesPositionTagsAlone()
    {
        // 'all' in a rule spec is the union of the named rules - narrower than the
        // wholesale pass a bare --remove-formatting performs.
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("{\\pos(10,20)}<i>Hi</i>", 0, 1000));

        RemoveFormattingRunner.Run(sub, RemoveFormattingRunner.ResolveRuleIds("all"));

        Assert.Equal("{\\pos(10,20)}Hi", sub.Paragraphs[0].Text);
    }

    [Fact]
    public void GuiLabels_CoversEveryAvailableRule_WithNonEmptyText()
    {
        foreach (var id in RemoveFormattingRunner.AvailableRuleIds)
        {
            Assert.True(
                RemoveFormattingRunner.GuiLabels.TryGetValue(id, out var label),
                $"Rule '{id}' has no GUI-equivalent label in RemoveFormattingRunner.GuiLabels.");
            Assert.False(string.IsNullOrWhiteSpace(label));
        }
    }

    [Fact]
    public void GuiLabels_HasNoLabelForUnknownRule()
    {
        var available = new HashSet<string>(RemoveFormattingRunner.AvailableRuleIds, StringComparer.OrdinalIgnoreCase);
        foreach (var id in RemoveFormattingRunner.GuiLabels.Keys)
        {
            Assert.Contains(id, available);
        }
    }
}
