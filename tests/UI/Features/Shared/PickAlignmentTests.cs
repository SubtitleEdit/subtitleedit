using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.PickAlignment;

namespace UITests.Features.Shared;

/// <summary>
/// The alignment picker is a video frame that marks where the selected line sits now.
/// </summary>
public class PickAlignmentTests
{
    [Theory]
    [InlineData("{\\an8}Hello", "an8")]
    [InlineData("{\\an7}Hello", "an7")]
    [InlineData("Hello", "an2")]
    public void CurrentAlignment_ComesFromTheSelectedLine(string text, string expected)
    {
        var vm = new PickAlignmentViewModel();

        vm.Initialize(new SubtitleLineViewModel { Text = text }, 1);

        Assert.Equal(expected, vm.CurrentAlignment);
    }

    [Fact]
    public void CurrentAlignment_IsNoneWithoutALine()
    {
        var vm = new PickAlignmentViewModel();

        vm.Initialize(null, 10);

        Assert.Empty(vm.CurrentAlignment);
    }

    [AvaloniaFact]
    public void Positions_KeepTheirPlaceOnTheFrame()
    {
        var vm = new PickAlignmentViewModel();
        var window = new PickAlignmentWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var buttons = window.GetLogicalDescendants().OfType<Button>().ToList();

        Assert.Equal(["an7", "an8", "an9", "an4", "an5", "an6", "an1", "an2", "an3"], buttons.Select(b => (string)b.CommandParameter!));
        Assert.Equal(0, Grid.GetRow(buttons[0]));
        Assert.Equal(2, Grid.GetRow(buttons[8]));
        Assert.Equal(2, Grid.GetColumn(buttons[8]));
        window.Close();
    }
}
