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
/// The YCbCr Matrix combo in the ASSA properties dialog (issue #13391): files without the
/// header default to "None" (the libass-recommended value), known values round-trip, values
/// libass does not recognize are kept as-is, and an explicit N/A removes the header.
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
    public void Ok_WithoutYCbCrMatrixHeader_DefaultsToNone()
    {
        var vm = MakeViewModel(MakeSubtitle());

        Assert.Equal(YCbCrMatrixType.None, vm.SelectedYCbCrMatrix.Style);

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.OkPressed);
        Assert.True(HeaderContainsYCbCrMatrix(vm.Header, "None"));
    }

    [AvaloniaFact]
    public void Ok_WithNotSet_DoesNotAddHeader()
    {
        var vm = MakeViewModel(MakeSubtitle());

        vm.SelectedYCbCrMatrix = vm.YCbCrMatrixItems.First(p => p.Style == YCbCrMatrixType.NotSet);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.DoesNotContain("YCbCr Matrix", vm.Header, StringComparison.OrdinalIgnoreCase);
    }

    [AvaloniaTheory]
    [InlineData("TV.FCC", YCbCrMatrixType.TvFcc)]
    [InlineData("pc.fcc", YCbCrMatrixType.PcFcc)]
    [InlineData("TV.240M", YCbCrMatrixType.Tv240M)]
    [InlineData("PC.240M", YCbCrMatrixType.Pc240M)]
    public void Load_AllLibassValues_AreRecognized(string value, YCbCrMatrixType expected)
    {
        var vm = new AssaPropertiesViewModel(new StubFileHelper(), new StubWindowService());
        vm.Initialize(MakeSubtitle(value), new AdvancedSubStationAlpha(), "test.ass", string.Empty);

        Assert.Equal(expected, vm.SelectedYCbCrMatrix.Style);
    }

    [AvaloniaFact]
    public void Ok_LowercaseKey_IsWrittenCanonically()
    {
        var subtitle = MakeSubtitle();
        subtitle.Header = subtitle.Header.Replace("[V4+ Styles]", $"ycbcr matrix: tv.709{Environment.NewLine}{Environment.NewLine}[V4+ Styles]");
        var vm = MakeViewModel(subtitle);

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Contains("YCbCr Matrix: TV.709", vm.Header, StringComparison.Ordinal);
        Assert.DoesNotContain("ycbcr matrix", vm.Header, StringComparison.Ordinal);
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
        var vm = MakeViewModel(MakeSubtitle("TV.2020"));

        Assert.Equal(YCbCrMatrixType.Custom, vm.SelectedYCbCrMatrix.Style);
        Assert.Equal("TV.2020", vm.SelectedYCbCrMatrix.Name);

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(HeaderContainsYCbCrMatrix(vm.Header, "TV.2020"));
    }

    [AvaloniaFact]
    public void Ok_UnrecognizedValueThenNotSet_RemovesIt()
    {
        var vm = MakeViewModel(MakeSubtitle("TV.2020"));

        vm.SelectedYCbCrMatrix = vm.YCbCrMatrixItems.First(p => p.Style == YCbCrMatrixType.NotSet);
        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.DoesNotContain("YCbCr Matrix", vm.Header, StringComparison.OrdinalIgnoreCase);
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
