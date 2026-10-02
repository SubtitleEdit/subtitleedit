using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Shared.TextBoxUtils;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

// "Surround with" slots can toggle, always add, or only remove their pair (#15531).
public class SurroundWithBehaviorTests
{
    [Fact]
    public void Apply_Toggle_RemovesExistingPair()
    {
        var result = TextBoxSurroundToggler.Apply(SurroundWithBehavior.Toggle, "[", "[Hello]", "]", out var added);

        Assert.False(added);
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Apply_Add_AddsPairAgainWhenAlreadyPresent()
    {
        var once = TextBoxSurroundToggler.Apply(SurroundWithBehavior.Add, "", "Hello", "\\N", out var added);
        var twice = TextBoxSurroundToggler.Apply(SurroundWithBehavior.Add, "", once, "\\N", out _);

        Assert.True(added);
        Assert.Equal("Hello\\N", once);
        Assert.Equal("Hello\\N\\N", twice);
    }

    [Fact]
    public void Apply_Add_KeepsItalicTagsOutside()
    {
        var result = TextBoxSurroundToggler.Apply(SurroundWithBehavior.Add, "[", "<i>[Hello]</i>", "]", out _);

        Assert.Equal("<i>[[Hello]]</i>", result);
    }

    [Fact]
    public void Apply_Remove_LeavesTextWithoutPairAlone()
    {
        var result = TextBoxSurroundToggler.Apply(SurroundWithBehavior.Remove, "[", "Hello", "]", out var added);

        Assert.False(added);
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Apply_Remove_RemovesPair()
    {
        var result = TextBoxSurroundToggler.Apply(SurroundWithBehavior.Remove, "[", "[Hello]", "]", out _);

        Assert.Equal("Hello", result);
    }

    [Fact]
    public void Apply_RemoveOnce_RemovesOnePairPerPress()
    {
        var once = TextBoxSurroundToggler.Apply(SurroundWithBehavior.RemoveOnce, "", "Hello\\N\\N", "\\N", out var added);
        var twice = TextBoxSurroundToggler.Apply(SurroundWithBehavior.RemoveOnce, "", once, "\\N", out _);
        var thrice = TextBoxSurroundToggler.Apply(SurroundWithBehavior.RemoveOnce, "", twice, "\\N", out _);

        Assert.False(added);
        Assert.Equal("Hello\\N", once);
        Assert.Equal("Hello", twice);
        Assert.Equal("Hello", thrice);
    }

    [Fact]
    public void Apply_RemoveOnce_RemovesOuterPairKeepingItalicTagsOutside()
    {
        var result = TextBoxSurroundToggler.Apply(SurroundWithBehavior.RemoveOnce, "[", "<i>[[Hello]]</i>", "]", out _);

        Assert.Equal("<i>[Hello]</i>", result);
    }

    [Fact]
    public void Apply_RemoveOnce_SameBeforeAndAfter()
    {
        var once = TextBoxSurroundToggler.Apply(SurroundWithBehavior.RemoveOnce, "*", "**Hello**", "*", out _);
        var single = TextBoxSurroundToggler.Apply(SurroundWithBehavior.RemoveOnce, "*", "*Hello", "*", out _);

        Assert.Equal("*Hello*", once);
        Assert.Equal("Hello", single);
    }

    [Fact]
    public void Apply_RemoveOnce_UndoesAddedMusicSymbols()
    {
        var symbol = Nikse.SubtitleEdit.Core.Common.Configuration.Settings.Tools.MusicSymbol;
        var added = TextBoxSurroundToggler.Apply(SurroundWithBehavior.Add, symbol, "Hello" + Nl + "Bye", symbol, out _);
        var addedTwice = TextBoxSurroundToggler.Apply(SurroundWithBehavior.Add, symbol, added, symbol, out _);

        var removed = TextBoxSurroundToggler.Apply(SurroundWithBehavior.RemoveOnce, symbol, addedTwice, symbol, out _);

        Assert.Equal(added, removed);
        Assert.Equal("Hello" + Nl + "Bye", TextBoxSurroundToggler.Apply(SurroundWithBehavior.RemoveOnce, symbol, removed, symbol, out _));
    }

    [Fact]
    public void ApplyToTexts_EachLineRemoveOnce_RemovesOnePairPerLine()
    {
        var result = TextBoxSurroundToggler.ApplyToTexts(SurroundWithBehavior.RemoveOnce, SurroundWithScope.EachLine,
            "[", ["[[Hello]]" + Nl + "[Bye]", "Again"], "]");

        Assert.Equal(["[Hello]" + Nl + "Bye", "Again"], result);
    }

    [AvaloniaFact]
    public void ToggleSelection_Add_AddsPairAgainToSelection()
    {
        var textBox = new TextBox { Text = "[Hello] world", SelectionStart = 0, SelectionEnd = 7 };

        var result = TextBoxSurroundToggler.ToggleSelection(new TextBoxWrapper(textBox), "[", "]", SurroundWithBehavior.Add);
        Dispatcher.UIThread.RunJobs();

        Assert.True(result);
        Assert.Equal("[[Hello]] world", textBox.Text);
    }

    [Fact]
    public void GetSurroundBehavior_RoundTripsAndFallsBackToToggle()
    {
        var se = new Se();
        Assert.Equal(SurroundWithBehavior.Toggle, se.GetSurroundBehavior(4));

        se.SetSurroundBehavior(4, SurroundWithBehavior.Add);
        Assert.Equal(SurroundWithBehavior.Add, se.GetSurroundBehavior(4));
        Assert.Equal(nameof(SurroundWithBehavior.Add), se.Surround4Behavior);

        se.Surround4Behavior = "nonsense";
        Assert.Equal(SurroundWithBehavior.Toggle, se.GetSurroundBehavior(4));
        se.Surround4Behavior = null!;
        Assert.Equal(SurroundWithBehavior.Toggle, se.GetSurroundBehavior(4));
    }

    private static readonly string Nl = Environment.NewLine;

    [Fact]
    public void ApplyToTexts_EachLine_SurroundsEveryLine()
    {
        var result = TextBoxSurroundToggler.ApplyToTexts(SurroundWithBehavior.Toggle, SurroundWithScope.EachLine,
            "[", ["Hello" + Nl + "Bye"], "]");

        Assert.Equal("[Hello]" + Nl + "[Bye]", Assert.Single(result));
    }

    [Fact]
    public void ApplyToTexts_SelectionOrText_SurroundsWholeText()
    {
        var result = TextBoxSurroundToggler.ApplyToTexts(SurroundWithBehavior.Toggle, SurroundWithScope.SelectionOrText,
            "[", ["Hello" + Nl + "Bye"], "]");

        Assert.Equal("[Hello" + Nl + "Bye]", Assert.Single(result));
    }

    [Fact]
    public void ApplyToTexts_EachLineToggle_FirstLineDecidesForAll()
    {
        // First line has the pair, so it is removed everywhere - the second line is not flipped to added.
        var result = TextBoxSurroundToggler.ApplyToTexts(SurroundWithBehavior.Toggle, SurroundWithScope.EachLine,
            "[", ["[Hello]" + Nl + "Bye", "[Again]"], "]");

        Assert.Equal(["Hello" + Nl + "Bye", "Again"], result);
    }

    [Fact]
    public void ApplyToTexts_EachLineAdd_KeepsItalicTagsPerLine()
    {
        var result = TextBoxSurroundToggler.ApplyToTexts(SurroundWithBehavior.Add, SurroundWithScope.EachLine,
            "[", ["<i>Hello" + Nl + "Bye</i>"], "]");

        Assert.Equal("<i>[Hello]" + Nl + "[Bye]</i>", Assert.Single(result));
    }

    [Fact]
    public void ApplyToTexts_MultipleSubtitlesToggle_FirstSubtitleDecides()
    {
        var result = TextBoxSurroundToggler.ApplyToTexts(SurroundWithBehavior.Toggle, SurroundWithScope.SelectionOrText,
            "[", ["Hello", "[Bye]"], "]");

        Assert.Equal(["[Hello]", "[Bye]"], result);
    }

    [Fact]
    public void GetSurroundScope_RoundTripsAndFallsBackToSelectionOrText()
    {
        var se = new Se();
        Assert.Equal(SurroundWithScope.SelectionOrText, se.GetSurroundScope(2));

        se.SetSurroundScope(2, SurroundWithScope.EachLine);
        Assert.Equal(SurroundWithScope.EachLine, se.GetSurroundScope(2));

        se.Surround2Scope = "nonsense";
        Assert.Equal(SurroundWithScope.SelectionOrText, se.GetSurroundScope(2));
    }
}
