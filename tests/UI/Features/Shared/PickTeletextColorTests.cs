using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Shared.PickTeletextColor;

namespace UITests.Features.Shared;

/// <summary>
/// EBU STL open subtitles can only carry the eight teletext colors, so the main window
/// swaps the RGB color picker for this constrained palette dialog. "No color" matters
/// because a line without an explicit color code has 37 usable characters instead of 36.
/// </summary>
public class PickTeletextColorTests
{
    [Fact]
    public void Initialize_DetectsNamedColor_FromEbuImportTag()
    {
        // EBU STL import writes named font tags.
        var vm = new PickTeletextColorViewModel();
        vm.Initialize("<font color=\"Cyan\">Hallo</font>");

        Assert.Equal(Color.FromRgb(0x00, 0xFF, 0xFF), vm.CurrentColor);
    }

    [Fact]
    public void Initialize_DetectsHexColor_FromColorServiceTag()
    {
        // SE's own color service writes hex font tags.
        var vm = new PickTeletextColorViewModel();
        vm.Initialize("<font color=\"#ff00ff\">Hallo</font>");

        Assert.Equal(Color.FromRgb(0xFF, 0x00, 0xFF), vm.CurrentColor);
    }

    [Fact]
    public void Initialize_NoColorTag_MeansNoCurrentColor()
    {
        var vm = new PickTeletextColorViewModel();
        vm.Initialize("Hallo");

        Assert.Null(vm.CurrentColor);
    }

    [AvaloniaFact]
    public void Window_OffersNoColorAndAllEightTeletextColors()
    {
        var vm = new PickTeletextColorViewModel();
        vm.Initialize(null);
        var window = new PickTeletextColorWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var pickButtons = window.GetLogicalDescendants().OfType<Button>()
                .Where(b => b.Command == vm.PickColorCommand || b.Command == vm.PickNoColorCommand)
                .ToList();

            Assert.Equal(9, pickButtons.Count);
            Assert.Single(pickButtons, b => b.Command == vm.PickNoColorCommand);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PickingAColorTile_ConfirmsThatColor()
    {
        var vm = new PickTeletextColorViewModel();
        vm.Initialize(null);
        var window = new PickTeletextColorWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var yellow = vm.TeletextColors.Single(c => c.EnglishName == "Yellow");
            var yellowButton = window.GetLogicalDescendants().OfType<Button>()
                .Single(b => ReferenceEquals(b.CommandParameter, yellow));
            yellowButton.Command!.Execute(yellowButton.CommandParameter);

            Assert.True(vm.OkPressed);
            Assert.False(vm.NoColorPressed);
            Assert.Equal(Color.FromRgb(0xFF, 0xFF, 0x00), vm.SelectedColor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void PickingNoColor_RequestsColorRemoval()
    {
        var vm = new PickTeletextColorViewModel();
        vm.Initialize("<font color=\"Red\">Hallo</font>");
        var window = new PickTeletextColorWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var noColorButton = window.GetLogicalDescendants().OfType<Button>()
                .Single(b => b.Command == vm.PickNoColorCommand);
            noColorButton.Command!.Execute(null);

            Assert.True(vm.OkPressed);
            Assert.True(vm.NoColorPressed);
        }
        finally
        {
            window.Close();
        }
    }
}
