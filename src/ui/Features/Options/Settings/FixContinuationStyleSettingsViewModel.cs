using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class FixContinuationStyleSettingsViewModel : ObservableObject
{
    [ObservableProperty] private bool _fixContinuationStyleUncheckInsertsAllCaps;
    [ObservableProperty] private bool _fixContinuationStyleUncheckInsertsItalic;
    [ObservableProperty] private bool _fixContinuationStyleUncheckInsertsLowercase;
    [ObservableProperty] private bool _fixContinuationStyleHideContinuationCandidatesWithoutName;
    [ObservableProperty] private bool _fixContinuationStyleIgnoreLyrics;
    [ObservableProperty] private int _continuationPause;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string CustomContinuationStyleSuffix { get; private set; } = string.Empty;
    public bool CustomContinuationStyleSuffixApplyIfComma { get; private set; }
    public bool CustomContinuationStyleSuffixAddSpace { get; private set; }
    public bool CustomContinuationStyleSuffixReplaceComma { get; private set; }
    public string CustomContinuationStylePrefix { get; private set; } = string.Empty;
    public bool CustomContinuationStylePrefixAddSpace { get; private set; }
    public bool CustomContinuationStyleUseDifferentStyleGap { get; private set; }
    public string CustomContinuationStyleGapSuffix { get; private set; } = string.Empty;
    public bool CustomContinuationStyleGapSuffixApplyIfComma { get; private set; }
    public bool CustomContinuationStyleGapSuffixAddSpace { get; private set; }
    public bool CustomContinuationStyleGapSuffixReplaceComma { get; private set; }
    public string CustomContinuationStyleGapPrefix { get; private set; } = string.Empty;
    public bool CustomContinuationStyleGapPrefixAddSpace { get; private set; }

    private readonly IWindowService _windowService;

    public FixContinuationStyleSettingsViewModel(IWindowService windowService)
    {
        _windowService = windowService;
    }

    public void Initialize(
        bool fixContinuationStyleUncheckInsertsAllCaps,
        bool fixContinuationStyleUncheckInsertsItalic,
        bool fixContinuationStyleUncheckInsertsLowercase,
        bool fixContinuationStyleHideContinuationCandidatesWithoutName,
        bool fixContinuationStyleIgnoreLyrics,
        int continuationPause,
        string customContinuationStyleSuffix,
        bool customContinuationStyleSuffixApplyIfComma,
        bool customContinuationStyleSuffixAddSpace,
        bool customContinuationStyleSuffixReplaceComma,
        string customContinuationStylePrefix,
        bool customContinuationStylePrefixAddSpace,
        bool customContinuationStyleUseDifferentStyleGap,
        string customContinuationStyleGapSuffix,
        bool customContinuationStyleGapSuffixApplyIfComma,
        bool customContinuationStyleGapSuffixAddSpace,
        bool customContinuationStyleGapSuffixReplaceComma,
        string customContinuationStyleGapPrefix,
        bool customContinuationStyleGapPrefixAddSpace)
    {
        FixContinuationStyleUncheckInsertsAllCaps = fixContinuationStyleUncheckInsertsAllCaps;
        FixContinuationStyleUncheckInsertsItalic = fixContinuationStyleUncheckInsertsItalic;
        FixContinuationStyleUncheckInsertsLowercase = fixContinuationStyleUncheckInsertsLowercase;
        FixContinuationStyleHideContinuationCandidatesWithoutName = fixContinuationStyleHideContinuationCandidatesWithoutName;
        FixContinuationStyleIgnoreLyrics = fixContinuationStyleIgnoreLyrics;
        ContinuationPause = continuationPause;

        CustomContinuationStyleSuffix = customContinuationStyleSuffix;
        CustomContinuationStyleSuffixApplyIfComma = customContinuationStyleSuffixApplyIfComma;
        CustomContinuationStyleSuffixAddSpace = customContinuationStyleSuffixAddSpace;
        CustomContinuationStyleSuffixReplaceComma = customContinuationStyleSuffixReplaceComma;
        CustomContinuationStylePrefix = customContinuationStylePrefix;
        CustomContinuationStylePrefixAddSpace = customContinuationStylePrefixAddSpace;
        CustomContinuationStyleUseDifferentStyleGap = customContinuationStyleUseDifferentStyleGap;
        CustomContinuationStyleGapSuffix = customContinuationStyleGapSuffix;
        CustomContinuationStyleGapSuffixApplyIfComma = customContinuationStyleGapSuffixApplyIfComma;
        CustomContinuationStyleGapSuffixAddSpace = customContinuationStyleGapSuffixAddSpace;
        CustomContinuationStyleGapSuffixReplaceComma = customContinuationStyleGapSuffixReplaceComma;
        CustomContinuationStyleGapPrefix = customContinuationStyleGapPrefix;
        CustomContinuationStyleGapPrefixAddSpace = customContinuationStyleGapPrefixAddSpace;
    }

    [RelayCommand]
    private async Task ShowEditCustomContinuationStyle()
    {
        if (Window == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<CustomContinuationStyleWindow, CustomContinuationStyleViewModel>(Window, vm =>
        {
            vm.Initialize(
                ContinuationPause,
                CustomContinuationStyleSuffix,
                CustomContinuationStyleSuffixApplyIfComma,
                CustomContinuationStyleSuffixAddSpace,
                CustomContinuationStyleSuffixReplaceComma,
                CustomContinuationStylePrefix,
                CustomContinuationStylePrefixAddSpace,
                CustomContinuationStyleUseDifferentStyleGap,
                CustomContinuationStyleGapSuffix,
                CustomContinuationStyleGapSuffixApplyIfComma,
                CustomContinuationStyleGapSuffixAddSpace,
                CustomContinuationStyleGapSuffixReplaceComma,
                CustomContinuationStyleGapPrefix,
                CustomContinuationStyleGapPrefixAddSpace);
        });

        if (!result.OkPressed)
        {
            return;
        }

        ApplyCustomContinuationStyle(result);
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    private void ApplyCustomContinuationStyle(CustomContinuationStyleViewModel result)
    {
        CustomContinuationStyleSuffix = result.SelectedSuffix;
        CustomContinuationStyleSuffixApplyIfComma = result.SelectedSuffixesProcessIfEndWithComma;
        CustomContinuationStyleSuffixAddSpace = result.SelectedSuffixesAddSpace;
        CustomContinuationStyleSuffixReplaceComma = result.SelectedSuffixesRemoveComma;
        CustomContinuationStylePrefix = result.SelectedPrefix;
        CustomContinuationStylePrefixAddSpace = result.SelectedPrefixAddSpace;
        CustomContinuationStyleUseDifferentStyleGap = result.UseSpecialStyleAfterLongGaps;
        ContinuationPause = result.LongGapMs;
        CustomContinuationStyleGapSuffix = result.SelectedLongGapSuffix;
        CustomContinuationStyleGapSuffixApplyIfComma = result.SelectedLongGapSuffixesProcessIfEndWithComma;
        CustomContinuationStyleGapSuffixAddSpace = result.SelectedLongGapSuffixesAddSpace;
        CustomContinuationStyleGapSuffixReplaceComma = result.SelectedLongGapSuffixesRemoveComma;
        CustomContinuationStyleGapPrefix = result.SelectedLongGapPrefix;
        CustomContinuationStyleGapPrefixAddSpace = result.SelectedLongGapPrefixAddSpace;
    }
}
