using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic;
using System.Runtime.CompilerServices;

namespace UITests.Features.Options.Settings;

public class FixContinuationStyleSettingsTests
{
    [Fact]
    public void FixContinuationStyleSettingsViewModel_Initialize_LoadsHeuristicsAndPause()
    {
        var vm = new FixContinuationStyleSettingsViewModel(new FakeWindowService());

        vm.Initialize(
            false,
            true,
            false,
            true,
            false,
            1450,
            "...",
            true,
            true,
            false,
            "--",
            true,
            true,
            "??",
            false,
            true,
            true,
            "__",
            true);

        Assert.False(vm.FixContinuationStyleUncheckInsertsAllCaps);
        Assert.True(vm.FixContinuationStyleUncheckInsertsItalic);
        Assert.False(vm.FixContinuationStyleUncheckInsertsLowercase);
        Assert.True(vm.FixContinuationStyleHideContinuationCandidatesWithoutName);
        Assert.False(vm.FixContinuationStyleIgnoreLyrics);
        Assert.Equal(1450, vm.ContinuationPause);
        Assert.Equal("...", vm.CustomContinuationStyleSuffix);
        Assert.True(vm.CustomContinuationStyleSuffixApplyIfComma);
        Assert.True(vm.CustomContinuationStyleSuffixAddSpace);
        Assert.False(vm.CustomContinuationStyleSuffixReplaceComma);
        Assert.Equal("--", vm.CustomContinuationStylePrefix);
        Assert.True(vm.CustomContinuationStylePrefixAddSpace);
        Assert.True(vm.CustomContinuationStyleUseDifferentStyleGap);
        Assert.Equal("??", vm.CustomContinuationStyleGapSuffix);
        Assert.False(vm.CustomContinuationStyleGapSuffixApplyIfComma);
        Assert.True(vm.CustomContinuationStyleGapSuffixAddSpace);
        Assert.True(vm.CustomContinuationStyleGapSuffixReplaceComma);
        Assert.Equal("__", vm.CustomContinuationStyleGapPrefix);
        Assert.True(vm.CustomContinuationStyleGapPrefixAddSpace);
    }

    [Fact]
    public async Task FixContinuationStyleSettingsViewModel_ShowEditCustomContinuationStyle_UpdatesPauseAndCustomStyle()
    {
        var windowService = new FakeWindowService
        {
            DialogFactory = type =>
            {
                if (type != typeof(CustomContinuationStyleViewModel))
                {
                    throw new InvalidOperationException($"Unexpected dialog type: {type.Name}");
                }

                return new CustomContinuationStyleViewModel();
            },
            AfterConfigureDialog = viewModel =>
            {
                if (viewModel is not CustomContinuationStyleViewModel customVm)
                {
                    return;
                }

                customVm.SelectedSuffix = "!!";
                customVm.SelectedSuffixesProcessIfEndWithComma = true;
                customVm.SelectedSuffixesAddSpace = true;
                customVm.SelectedSuffixesRemoveComma = true;
                customVm.SelectedPrefix = "--";
                customVm.SelectedPrefixAddSpace = true;
                customVm.UseSpecialStyleAfterLongGaps = true;
                customVm.LongGapMs = 1450;
                customVm.SelectedLongGapSuffix = "??";
                customVm.SelectedLongGapSuffixesProcessIfEndWithComma = true;
                customVm.SelectedLongGapSuffixesAddSpace = true;
                customVm.SelectedLongGapSuffixesRemoveComma = true;
                customVm.SelectedLongGapPrefix = "__";
                customVm.SelectedLongGapPrefixAddSpace = true;
                customVm.OkCommand.Execute(null);
            }
        };

        var vm = new FixContinuationStyleSettingsViewModel(windowService)
        {
            Window = MakeWindowOwner(),
        };
        vm.Initialize(
            true,
            true,
            true,
            true,
            true,
            300,
            "...",
            false,
            false,
            false,
            "-",
            false,
            false,
            string.Empty,
            false,
            false,
            false,
            string.Empty,
            false);

        await vm.ShowEditCustomContinuationStyleCommand.ExecuteAsync(null);

        Assert.Equal(1450, vm.ContinuationPause);
        Assert.Equal("!!", vm.CustomContinuationStyleSuffix);
        Assert.True(vm.CustomContinuationStyleSuffixApplyIfComma);
        Assert.True(vm.CustomContinuationStyleSuffixAddSpace);
        Assert.True(vm.CustomContinuationStyleSuffixReplaceComma);
        Assert.Equal("--", vm.CustomContinuationStylePrefix);
        Assert.True(vm.CustomContinuationStylePrefixAddSpace);
        Assert.True(vm.CustomContinuationStyleUseDifferentStyleGap);
        Assert.Equal("??", vm.CustomContinuationStyleGapSuffix);
        Assert.True(vm.CustomContinuationStyleGapSuffixApplyIfComma);
        Assert.True(vm.CustomContinuationStyleGapSuffixAddSpace);
        Assert.True(vm.CustomContinuationStyleGapSuffixReplaceComma);
        Assert.Equal("__", vm.CustomContinuationStyleGapPrefix);
        Assert.True(vm.CustomContinuationStyleGapPrefixAddSpace);
    }

    private sealed class FakeWindowService : IWindowService
    {
        public Func<Type, object>? DialogFactory { get; set; }
        public Action<object>? AfterConfigureDialog { get; set; }

        public T ShowWindow<T>(Window owner, Action<T>? configure = null) where T : Window
        {
            throw new NotSupportedException();
        }

        public TViewModel ShowWindow<T, TViewModel>(Window owner, Action<T, TViewModel>? configure = null)
            where T : Window
            where TViewModel : class
        {
            throw new NotSupportedException();
        }

        public TViewModel ShowIndependentWindow<T, TViewModel>(Action<T, TViewModel>? configure = null)
            where T : Window
            where TViewModel : class
        {
            throw new NotSupportedException();
        }

        public Task<T> ShowDialogAsync<T>(Window owner, Action<T>? configure = null) where T : Window
        {
            throw new NotSupportedException();
        }

        public Task<TViewModel> ShowDialogAsync<TWindow, TViewModel>(
            Window owner,
            Action<TViewModel>? configureViewModel = null,
            Action<TWindow>? configureWindow = null)
            where TWindow : Window
            where TViewModel : class
        {
            var viewModel = (TViewModel)(DialogFactory?.Invoke(typeof(TViewModel)) ?? throw new InvalidOperationException($"No dialog factory for {typeof(TViewModel).Name}"));
            configureViewModel?.Invoke(viewModel);
            AfterConfigureDialog?.Invoke(viewModel);
            return Task.FromResult(viewModel);
        }
    }
    private static Window MakeWindowOwner() =>
        (Window)RuntimeHelpers.GetUninitializedObject(typeof(Window));
}
