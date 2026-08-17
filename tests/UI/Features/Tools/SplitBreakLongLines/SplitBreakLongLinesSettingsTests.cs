using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Tools.SplitBreakLongLines;

public class SplitBreakLongLinesSettingsTests
{
    [Fact]
    public void Constructor_NoSavedToolValues_FallsBackToGeneralSettings()
    {
        var savedSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleLineMaximumLength = 47;
            Se.Settings.General.MaxNumberOfLines = 3;
            Se.Settings.General.UnbreakLinesShorterThan = 29;

            var vm = new SplitBreakLongLinesViewModel();
            vm.OnClosingCleanup();

            Assert.Equal(47, vm.SingleLineMaxLength);
            Assert.Equal(3, vm.MaxNumberOfLines);
            Assert.Equal(29, vm.UnbreakLinesShorterThan);
        }
        finally
        {
            Se.Settings = savedSettings;
        }
    }

    [Fact]
    public void Constructor_SavedToolValues_WinOverGeneralSettings()
    {
        var savedSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleLineMaximumLength = 47;
            Se.Settings.General.MaxNumberOfLines = 3;
            Se.Settings.General.UnbreakLinesShorterThan = 29;
            Se.Settings.Tools.SplitRebalanceLongLinesSingleLineMaxLength = 38;
            Se.Settings.Tools.SplitRebalanceLongLinesMaxNumberOfLines = 2;
            Se.Settings.Tools.SplitRebalanceLongLinesUnbreakShorterThan = 21;

            var vm = new SplitBreakLongLinesViewModel();
            vm.OnClosingCleanup();

            Assert.Equal(38, vm.SingleLineMaxLength);
            Assert.Equal(2, vm.MaxNumberOfLines);
            Assert.Equal(21, vm.UnbreakLinesShorterThan);
        }
        finally
        {
            Se.Settings = savedSettings;
        }
    }

    [AvaloniaFact]
    public void Ok_PersistsAllConfiguredValues()
    {
        // Regression for #13514: OK only saved the two checkboxes, so the numeric
        // values reset to the general settings every time the dialog was opened.
        var savedSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();

            var vm = new SplitBreakLongLinesViewModel
            {
                Window = new Window(),
                SplitLongLines = false,
                RebalanceLongLines = true,
                SingleLineMaxLength = 38,
                MaxNumberOfLines = 2,
                UnbreakLinesShorterThan = 21,
            };

            vm.OkCommand.Execute(null);
            vm.OnClosingCleanup();

            Assert.True(vm.OkPressed);
            Assert.False(Se.Settings.Tools.SplitRebalanceLongLinesSplit);
            Assert.True(Se.Settings.Tools.SplitRebalanceLongLinesRebalance);
            Assert.Equal(38, Se.Settings.Tools.SplitRebalanceLongLinesSingleLineMaxLength);
            Assert.Equal(2, Se.Settings.Tools.SplitRebalanceLongLinesMaxNumberOfLines);
            Assert.Equal(21, Se.Settings.Tools.SplitRebalanceLongLinesUnbreakShorterThan);

            // A fresh dialog must come back with the values the user saved.
            var vm2 = new SplitBreakLongLinesViewModel();
            vm2.OnClosingCleanup();
            Assert.Equal(38, vm2.SingleLineMaxLength);
            Assert.Equal(2, vm2.MaxNumberOfLines);
            Assert.Equal(21, vm2.UnbreakLinesShorterThan);
            Assert.False(vm2.SplitLongLines);
            Assert.True(vm2.RebalanceLongLines);
        }
        finally
        {
            Se.Settings = savedSettings;
        }
    }
}
