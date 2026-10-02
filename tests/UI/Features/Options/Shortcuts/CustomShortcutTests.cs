using Nikse.SubtitleEdit.Features.Options.Shortcuts;
using Nikse.SubtitleEdit.Features.Options.Shortcuts.CustomShortcuts;
using Nikse.SubtitleEdit.Logic.Config;
using System.Text.Json;

namespace UITests.Features.Options.Shortcuts;

// User-built shortcuts: a name plus steps (run command, insert text, find and replace).
public class CustomShortcutTests
{
    private static readonly string Nl = Environment.NewLine;

    [Fact]
    public void Insert_AtEnd_AppendsAndKeepsTypedLineBreaksAsNewLine()
    {
        Assert.Equal("Hello\\N", CustomShortcutText.Insert("Hello", "\\N", CustomShortcutInsertPosition.End));
        Assert.Equal("Hello" + Nl + "-", CustomShortcutText.Insert("Hello", "\n-", CustomShortcutInsertPosition.End));
    }

    [Fact]
    public void Insert_AtStart_Prepends()
    {
        Assert.Equal("{\\an8}Hello", CustomShortcutText.Insert("Hello", "{\\an8}", CustomShortcutInsertPosition.Start));
    }

    [Fact]
    public void Replace_PlainText_IgnoresCaseUnlessCaseSensitive()
    {
        var step = new SeCustomShortcutStep { Type = nameof(CustomShortcutStepType.Replace), Find = "hello", ReplaceWith = "Hi" };
        Assert.Equal("Hi there", CustomShortcutText.Replace("Hello there", step));

        step.CaseSensitive = true;
        Assert.Equal("Hello there", CustomShortcutText.Replace("Hello there", step));
    }

    [Fact]
    public void Replace_PlainText_TreatsRegexCharactersLiterally()
    {
        var step = new SeCustomShortcutStep { Find = "(a)", ReplaceWith = "b" };
        Assert.Equal("b a", CustomShortcutText.Replace("(a) a", step));
    }

    [Fact]
    public void Replace_Regex_UsesGroupsAndWorksPerLine()
    {
        var step = new SeCustomShortcutStep { Find = "(?m)^- ", ReplaceWith = "– ", UseRegex = true };
        Assert.Equal("– Hi" + Nl + "– Bye", CustomShortcutText.Replace("- Hi" + Nl + "- Bye", step));

        step = new SeCustomShortcutStep { Find = @"(\w+) (\w+)", ReplaceWith = "$2 $1", UseRegex = true };
        Assert.Equal("world hello", CustomShortcutText.Replace("hello world", step));
    }

    // A typed \n in the pattern must match a line break whether the text uses \r\n (Windows) or \n,
    // like the Multiple replace window; the result keeps platform line breaks.
    [Theory]
    [InlineData("a\r\nb")]
    [InlineData("a\nb")]
    public void Replace_Regex_TypedNewLineMatchesAnyLineBreak(string text)
    {
        var step = new SeCustomShortcutStep { Find = @"a\nb", ReplaceWith = "x", UseRegex = true };
        Assert.Equal("x", CustomShortcutText.Replace(text, step));

        step = new SeCustomShortcutStep { Find = @"a\r\nb", ReplaceWith = @"b\na", UseRegex = true };
        Assert.Equal("b" + Nl + "a", CustomShortcutText.Replace(text, step));
    }

    [Fact]
    public void GetRegexError_InvalidPattern_ReturnsMessage()
    {
        Assert.NotNull(CustomShortcutText.GetRegexError(new SeCustomShortcutStep { Find = "(", UseRegex = true }));
        Assert.Null(CustomShortcutText.GetRegexError(new SeCustomShortcutStep { Find = "(", UseRegex = false }));
        Assert.Null(CustomShortcutText.GetRegexError(new SeCustomShortcutStep { Find = "a+", UseRegex = true }));
    }

    [Fact]
    public void Step_UnknownTypeAndPosition_FallBackToDefaults()
    {
        var step = new SeCustomShortcutStep { Type = "nonsense", Position = "nonsense" };
        Assert.Equal(CustomShortcutStepType.RunCommand, step.GetStepType());
        Assert.Equal(CustomShortcutInsertPosition.Cursor, step.GetPosition());
    }

    [Fact]
    public void Clone_CopiesStepsDeeply()
    {
        var custom = new SeCustomShortcut { Name = "Lift" };
        custom.Steps.Add(new SeCustomShortcutStep { Type = nameof(CustomShortcutStepType.InsertText), Text = "\\N" });

        var clone = custom.Clone();
        clone.Steps[0].Text = "changed";
        clone.Name = "changed";

        Assert.Equal("\\N", custom.Steps[0].Text);
        Assert.Equal("Lift", custom.Name);
    }

    [Fact]
    public void Slots_EmptyByDefaultAndSetPadsTheList()
    {
        var se = new Se();
        Assert.Empty(se.GetCustomShortcut(3).Steps);

        se.SetCustomShortcut(3, new SeCustomShortcut { Name = "Three" });

        Assert.Equal(Se.CustomShortcutSlotCount, se.CustomShortcuts.Count);
        Assert.Equal("Three", se.GetCustomShortcut(3).Name);
        Assert.Equal(string.Empty, se.GetCustomShortcut(1).Name);
        Assert.Empty(se.GetCustomShortcut(0).Steps);
        Assert.Empty(se.GetCustomShortcut(9).Steps);
    }

    [Fact]
    public void Settings_RoundTripCustomShortcuts()
    {
        var se = new Se();
        var custom = new SeCustomShortcut { Name = "Dash to en dash" };
        custom.Steps.Add(new SeCustomShortcutStep { Type = nameof(CustomShortcutStepType.Replace), Find = "^- ", ReplaceWith = "– ", UseRegex = true });
        custom.Steps.Add(new SeCustomShortcutStep { Type = nameof(CustomShortcutStepType.RunCommand), ActionName = "GoToNextLineCommand" });
        se.SetCustomShortcut(2, custom);

        var json = JsonSerializer.Serialize(se, SeJsonContext.Default.Se);
        var loaded = JsonSerializer.Deserialize(json, SeJsonContext.Default.Se)!;

        var result = loaded.GetCustomShortcut(2);
        Assert.Equal("Dash to en dash", result.Name);
        Assert.Equal(2, result.Steps.Count);
        Assert.True(result.Steps[0].UseRegex);
        Assert.Equal(CustomShortcutStepType.Replace, result.Steps[0].GetStepType());
        Assert.Equal("GoToNextLineCommand", result.Steps[1].ActionName);
    }

    [Fact]
    public void ActiveIn_DefaultsToEverywhereAndSurvivesCloneAndUnknownValues()
    {
        var custom = new SeCustomShortcut();
        Assert.Equal(ShortcutCategory.General, custom.GetActiveIn());

        custom.ActiveIn = nameof(ShortcutCategory.TextBox);
        Assert.Equal(ShortcutCategory.TextBox, custom.Clone().GetActiveIn());

        custom.ActiveIn = "nonsense";
        Assert.Equal(ShortcutCategory.General, custom.GetActiveIn());
        custom.ActiveIn = null!;
        Assert.Equal(ShortcutCategory.General, custom.GetActiveIn());
    }

    [Fact]
    public void ActiveIn_RoundTripsThroughSettings()
    {
        var se = new Se();
        se.SetCustomShortcut(5, new SeCustomShortcut { ActiveIn = nameof(ShortcutCategory.Waveform) });

        var json = JsonSerializer.Serialize(se, SeJsonContext.Default.Se);
        var loaded = JsonSerializer.Deserialize(json, SeJsonContext.Default.Se)!;

        Assert.Equal(ShortcutCategory.Waveform, loaded.GetCustomShortcut(5).GetActiveIn());
    }
}
