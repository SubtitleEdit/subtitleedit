using Avalonia;
using Nikse.SubtitleEdit.Features.Edit.ModifySelection;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace UITests.Features.Edit.ModifySelection;

public class ModifySelectionViewModelTests
{
    [Fact]
    public void InitializeRules_RestoresSavedNumberForSavedNumericRule()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleLineMaximumLength = 43;
            Se.Settings.Edit.ModifySelectionRule = nameof(RuleType.LengthGreaterThan);
            Se.Settings.Edit.ModifySelectionNumber = 77;

            var vm = new ModifySelectionViewModel();
            vm.InitializeRules(new List<SubtitleLineViewModel>());

            Assert.Equal(RuleType.LengthGreaterThan, vm.SelectedRule!.RuleType);
            Assert.Equal(77, vm.SelectedRule.Number);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [Fact]
    public void SelectingLengthRule_UsesCurrentSingleLineMaxLengthAsDefault()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleLineMaximumLength = 51;

            var vm = new ModifySelectionViewModel();
            vm.InitializeRules(new List<SubtitleLineViewModel>());
            var lengthRule = vm.Rules.Single(r => r.RuleType == RuleType.LengthGreaterThan);

            vm.SelectedRule = lengthRule;

            Assert.Equal(51, lengthRule.Number);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [Fact]
    public void SelectingPixelWidthRule_UsesCurrentPixelWidthLimitAsDefault()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleLineMaximumPixelWidth = 640;

            var vm = new ModifySelectionViewModel();
            vm.InitializeRules(new List<SubtitleLineViewModel>());
            var pixelRule = vm.Rules.Single(r => r.RuleType == RuleType.PixelWidthLengthGreaterThan);

            vm.SelectedRule = pixelRule;

            Assert.Equal(640, pixelRule.Number);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [Fact]
    public void SwitchingNumericRules_KeepsEachRuleSpecificValue()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleLineMaximumLength = 47;
            Se.Settings.General.SubtitleLineMaximumPixelWidth = 580;

            var vm = new ModifySelectionViewModel();
            vm.InitializeRules(new List<SubtitleLineViewModel>());
            var lengthRule = vm.Rules.Single(r => r.RuleType == RuleType.LengthGreaterThan);
            var pixelRule = vm.Rules.Single(r => r.RuleType == RuleType.PixelWidthLengthGreaterThan);

            vm.SelectedRule = lengthRule;
            lengthRule.Number = 99;

            vm.SelectedRule = pixelRule;
            Assert.Equal(580, pixelRule.Number);

            vm.SelectedRule = lengthRule;
            Assert.Equal(99, lengthRule.Number);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [Fact]
    public void Ok_SavesNumericValueThatIsRestoredOnReopen()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.General.SubtitleLineMaximumLength = 43;

            var vm = new ModifySelectionViewModel();
            vm.InitializeRules(new List<SubtitleLineViewModel>());
            var lengthRule = vm.Rules.Single(r => r.RuleType == RuleType.LengthGreaterThan);
            vm.SelectedRule = lengthRule;
            lengthRule.Number = 88;

            vm.OkCommand.Execute(null);

            var reopened = new ModifySelectionViewModel();
            reopened.InitializeRules(new List<SubtitleLineViewModel>());

            Assert.Equal(RuleType.LengthGreaterThan, reopened.SelectedRule!.RuleType);
            Assert.Equal(88, reopened.SelectedRule.Number);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }

    [Fact]
    public void NullableDoubleConverter_NullInput_DoesNotZeroOutNumber()
    {
        var converter = new NullableDoubleConverter();

        var result = converter.ConvertBack(null, typeof(double), null, CultureInfo.InvariantCulture);

        Assert.Equal(AvaloniaProperty.UnsetValue, result);
    }

    [Fact]
    public void Ok_OnNonNumericRule_DoesNotOverwriteSavedNumericThreshold()
    {
        var originalSettings = Se.Settings;
        try
        {
            Se.Settings = new Se();
            Se.Settings.Edit.ModifySelectionNumber = 88;

            var vm = new ModifySelectionViewModel();
            vm.InitializeRules(new List<SubtitleLineViewModel>());
            vm.SelectedRule = vm.Rules.First(r => !r.HasNumber);
            vm.OkCommand.Execute(null);

            Assert.Equal(88, Se.Settings.Edit.ModifySelectionNumber);
        }
        finally
        {
            Se.Settings = originalSettings;
        }
    }
}
