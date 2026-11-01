using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Shared.GoToLineNumber;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// Go to line number shows how many lines there are and, where the caller gives a preview, the
/// line the field points at.
/// </summary>
public class GoToLineNumberPreviewTests
{
    private static (string Time, string Text)? Preview(int number) => number <= 3 ? ($"0{number}", $"Line {number}") : null;

    [AvaloniaFact]
    public void Preview_FollowsTheField_AndHidesPastTheLastLine()
    {
        var vm = new GoToLineNumberViewModel();
        vm.Initialize(2, 3, Preview);
        Assert.Equal("Line 2", vm.PreviewText);
        Assert.True(vm.IsPreviewVisible);

        vm.LineNumber = 3;
        Assert.Equal("03", vm.PreviewTime);
        Assert.Equal("Line 3", vm.PreviewText);

        vm.LineNumber = 9;
        Assert.False(vm.IsPreviewVisible);
    }

    [AvaloniaFact]
    public void WithoutAPreview_OnlyTheTotalIsShown()
    {
        var vm = new GoToLineNumberViewModel();
        vm.Initialize(2, 3);

        Assert.False(vm.IsPreviewVisible);
        Assert.Equal(string.Format(Se.Language.General.OfX, 3), vm.TotalText);
    }

    [AvaloniaFact]
    public void AnOutOfRangeStart_FallsBackToLineOne()
    {
        var vm = new GoToLineNumberViewModel();
        vm.Initialize(50, 3, Preview);

        Assert.Equal(1, vm.LineNumber);
        Assert.Equal("Line 1", vm.PreviewText);
    }
}
