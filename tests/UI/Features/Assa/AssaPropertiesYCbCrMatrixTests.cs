using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Assa;

/// <summary>
/// The YCbCr Matrix combo in the ASSA properties dialog (issue #13391) must round-trip the
/// header without surprises: no silent insertion for files that never had the header, no
/// silent removal of values the dialog does not offer (libass also parses SMPTE240M/FCC),
/// and an explicit NotSet selection must still be able to remove a managed value.
/// </summary>
public class AssaPropertiesYCbCrMatrixTests
{
    private static Subtitle MakeSubtitle(string? ycbCrMatrixValue = null)
    {
        var style = new SsaStyle
        {
            Name = "Test-ASS",
            FontName = "Courier New",
            FontSize = 40,
        };

        var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            new[] { style }.ToList());

        if (ycbCrMatrixValue != null)
        {
            header = header.Replace("[V4+ Styles]", $"YCbCr Matrix: {ycbCrMatrixValue}{Environment.NewLine}{Environment.NewLine}[V4+ Styles]");
        }

        return new Subtitle { Header = header };
    }

    private static AssaPropertiesViewModel MakeViewModel(Subtitle subtitle)
    {
        var vm = new AssaPropertiesViewModel(new StubFileHelper(), new StubWindowService());
        vm.Initialize(subtitle, new AdvancedSubStationAlpha(), "test.ass", string.Empty);
        return vm;
    }

    private static bool HeaderContainsYCbCrMatrix(string header, string value)
    {
        return header.SplitToLines()!
            .Any(line => line.Trim().StartsWith("YCbCr Matrix:", StringComparison.OrdinalIgnoreCase) &&
                         line.Trim().Substring("YCbCr Matrix:".Length).Trim().Equals(value, StringComparison.OrdinalIgnoreCase));
    }

    [AvaloniaFact]
    public void Ok_WithoutYCbCrMatrixHeader_DoesNotAddOne()
    {
        var vm = MakeViewModel(MakeSubtitle());

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        Assert.True(vm.YCbCrMatrixItems.First(p => p.Style == YCbCrMatrixType.NotSet) == vm.SelectedYCbCrMatrix);
        Assert.DoesNotContain("YCbCr Matrix", vm.Header, StringComparison.OrdinalIgnoreCase);
    }

    [AvaloniaFact]
    public void Ok_WithRecognizedValue_RoundTripsUnchanged()
    {
        var vm = MakeViewModel(MakeSubtitle("TV.709"));

        Assert.Equal(YCbCrMatrixType.Tv709, vm.SelectedYCbCrMatrix.Style);

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(HeaderContainsYCbCrMatrix(vm.Header, "TV.709"));
    }

    [AvaloniaFact]
    public void Ok_WithUnrecognizedValue_PreservesIt()
    {
        var vm = MakeViewModel(MakeSubtitle("TV.FCC"));

        Assert.Equal(YCbCrMatrixType.NotSet, vm.SelectedYCbCrMatrix.Style);

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(HeaderContainsYCbCrMatrix(vm.Header, "TV.FCC"));
    }

    [AvaloniaFact]
    public void Ok_ExplicitlySelectedValue_IsWrittenToScriptInfo()
    {
        var vm = MakeViewModel(MakeSubtitle());

        vm.SelectedYCbCrMatrix = vm.YCbCrMatrixItems.First(p => p.Style == YCbCrMatrixType.Tv601);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(HeaderContainsYCbCrMatrix(vm.Header, "TV.601"));
    }

    [AvaloniaFact]
    public void Ok_ExplicitNotSet_RemovesExistingManagedValue()
    {
        var vm = MakeViewModel(MakeSubtitle("TV.709"));

        vm.SelectedYCbCrMatrix = vm.YCbCrMatrixItems.First(p => p.Style == YCbCrMatrixType.NotSet);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.DoesNotContain("YCbCr Matrix", vm.Header, StringComparison.OrdinalIgnoreCase);
    }

    [AvaloniaFact]
    public void YCbCrMatrixRow_RendersBelowTheOtherOptionCombos()
    {
        // The Options grid must declare its rows: an out-of-range Grid.Row gets clamped to 0
        // by Avalonia, which would render the new combo on top of the existing ones.
        var vm = MakeViewModel(MakeSubtitle());
        var window = new AssaPropertiesWindow(vm);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var wrap = FindCombo(window, Se.Language.Assa.WrapStyle);
            var ycbCr = FindCombo(window, Se.Language.Assa.YCbCrMatrix);

            Assert.NotNull(wrap);
            Assert.NotNull(ycbCr);

            // Bounds are parent-relative, so translate both into window coordinates first.
            var wrapTop = wrap.TranslatePoint(new Point(0, 0), window)!.Value;
            var ycbCrTop = ycbCr.TranslatePoint(new Point(0, 0), window)!.Value;

            Assert.True(ycbCrTop.Y >= wrapTop.Y + wrap.Bounds.Height - 0.5,
                $"YCbCr combo starts at window y={ycbCrTop.Y}, overlapping the wrap style combo " +
                $"spanning y={wrapTop.Y}..{wrapTop.Y + wrap.Bounds.Height}");
        }
        finally
        {
            window.Close();
        }
    }

    private static ComboBox? FindCombo(Window window, string automationName)
    {
        return window.GetVisualDescendants().OfType<ComboBox>()
            .FirstOrDefault(c => AutomationProperties.GetName(c) == automationName);
    }
}
