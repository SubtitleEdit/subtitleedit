using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

/// <summary>
/// Issue #14621: toggling dialog dashes must be idempotent — pressing the shortcut on a
/// paragraph that already has dashes must never stack "- - - ".
/// </summary>
public class ToggleDialogDashesTests
{
    private static DialogSplitMerge Helper() => new() { DialogStyle = DialogType.DashBothLinesWithSpace, SkipLineEndingCheck = true };

    [Fact]
    public void Add_OnPlainDialog_AddsDashes()
    {
        var result = MainViewModel.AddDialogDashes("Hello" + Environment.NewLine + "World", Helper());
        Assert.Equal("- Hello" + Environment.NewLine + "- World", result);
    }

    [Fact]
    public void Add_OnAlreadyDashed_DoesNotStack()
    {
        var input = "- Elle a des ecchymoses puis" + Environment.NewLine + "- une blessure à la tête.";
        var result = MainViewModel.AddDialogDashes(input, Helper());
        Assert.Equal(input, result);
    }

    [Fact]
    public void Add_OnStackedDashes_CollapsesToOne()
    {
        var input = "- - - - - Elle a des ecchymoses puis" + Environment.NewLine + "- - - - - une blessure à la tête.";
        var result = MainViewModel.AddDialogDashes(input, Helper());
        Assert.Equal("- Elle a des ecchymoses puis" + Environment.NewLine + "- une blessure à la tête.", result);
    }

    [Fact]
    public void Add_KeepsStartTags()
    {
        var result = MainViewModel.AddDialogDashes("<i>Hello</i>" + Environment.NewLine + "World", Helper());
        Assert.Equal("<i>- Hello</i>" + Environment.NewLine + "- World", result);
    }

    [Fact]
    public void Remove_StripsStackedDashes()
    {
        var input = "- - - Hello" + Environment.NewLine + "-World";
        var result = MainViewModel.RemoveDialogDashes(input);
        Assert.Equal("Hello" + Environment.NewLine + "World", result);
    }

    [Fact]
    public void Add_SingleLine_Unchanged()
    {
        Assert.Equal("Hello", MainViewModel.AddDialogDashes("Hello", Helper()));
    }
}
