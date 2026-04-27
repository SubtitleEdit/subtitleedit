using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings;

public partial class CustomContinuationStyleViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _preAndSuffixes;

    [ObservableProperty] private string _selectedPrefix;
    [ObservableProperty] private bool _selectedPrefixAddSpace;
    [ObservableProperty] private string _selectedSuffix;
    [ObservableProperty] private bool _selectedSuffixesProcessIfEndWithComma;
    [ObservableProperty] private bool _selectedSuffixesAddSpace;
    [ObservableProperty] private bool _selectedSuffixesRemoveComma;

    [ObservableProperty] private bool _useSpecialStyleAfterLongGaps;
    [ObservableProperty] private int _longGapMs;
    [ObservableProperty] private string _selectedLongGapPrefix;
    [ObservableProperty] private bool _selectedLongGapPrefixAddSpace;
    [ObservableProperty] private string _selectedLongGapSuffix;
    [ObservableProperty] private bool _selectedLongGapSuffixesProcessIfEndWithComma;
    [ObservableProperty] private bool _selectedLongGapSuffixesAddSpace;
    [ObservableProperty] private bool _selectedLongGapSuffixesRemoveComma;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public StackPanel PanelPreview { get; internal set; }

    private bool _isDirty;
    private readonly System.Timers.Timer _previewTimer;

    public CustomContinuationStyleViewModel()
    {
        PreAndSuffixes = new ObservableCollection<string>(new[]
        {
            string.Empty,
            "...",
            "…",
            "..",
            "-",
        });

        SelectedPrefix = PreAndSuffixes.First();
        SelectedSuffix = PreAndSuffixes.First();
        SelectedLongGapPrefix = PreAndSuffixes.First();
        SelectedLongGapSuffix = PreAndSuffixes.First();
        LongGapMs = 300;

        PanelPreview = new StackPanel()
        {
            Margin = new Avalonia.Thickness(10),
            Orientation = Avalonia.Layout.Orientation.Vertical
        };

        _previewTimer = new System.Timers.Timer(500);
        _previewTimer.Elapsed += (sender, args) =>
        {
            _previewTimer.Stop();

            if (_isDirty)
            {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(UpdatePreview);
                _isDirty = false;
            }

            _previewTimer.Start();
        };

    }

    public void Initialize(
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
        bool customContinuationStyleGapPrefixAddSpace
        )
    {
        SelectedPrefix = customContinuationStylePrefix;
        SelectedPrefixAddSpace = customContinuationStylePrefixAddSpace;
        SelectedSuffix = customContinuationStyleSuffix;
        SelectedSuffixesProcessIfEndWithComma = customContinuationStyleSuffixApplyIfComma;
        SelectedSuffixesAddSpace = customContinuationStyleSuffixAddSpace;
        SelectedSuffixesRemoveComma = customContinuationStyleSuffixReplaceComma;
        UseSpecialStyleAfterLongGaps = customContinuationStyleUseDifferentStyleGap;
        LongGapMs = continuationPause;
        SelectedLongGapPrefix = customContinuationStyleGapPrefix;
        SelectedLongGapPrefixAddSpace = customContinuationStyleGapPrefixAddSpace;
        SelectedLongGapSuffix = customContinuationStyleGapSuffix;
        SelectedLongGapSuffixesProcessIfEndWithComma = customContinuationStyleGapSuffixApplyIfComma;
        SelectedLongGapSuffixesAddSpace = customContinuationStyleGapSuffixAddSpace;
        SelectedLongGapSuffixesRemoveComma = customContinuationStyleGapSuffixReplaceComma;

        _previewTimer.Start();
    }

    [RelayCommand]
    private void LoadContinuationStyleNone()
    {
        LoadSettings(ContinuationStyle.None);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleNoneTrailingDots()
    {
        LoadSettings(ContinuationStyle.NoneTrailingDots);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleNoneLeadingTrailingDots()
    {
        LoadSettings(ContinuationStyle.NoneLeadingTrailingDots);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleNoneTrailingEllipsis()
    {
        LoadSettings(ContinuationStyle.NoneTrailingEllipsis);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleNoneLeadingTrailingEllipsis()
    {
        LoadSettings(ContinuationStyle.NoneLeadingTrailingEllipsis);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleOnlyTrailingDots()
    {
        LoadSettings(ContinuationStyle.OnlyTrailingDots);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleLeadingTrailingDots()
    {
        LoadSettings(ContinuationStyle.LeadingTrailingDots);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleOnlyTrailingEllipsis()
    {
        LoadSettings(ContinuationStyle.OnlyTrailingEllipsis);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleLeadingTrailingEllipsis()
    {
        LoadSettings(ContinuationStyle.LeadingTrailingEllipsis);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleLeadingTrailingDash()
    {
        LoadSettings(ContinuationStyle.LeadingTrailingDash);
        _isDirty = true;
    }

    [RelayCommand]
    private void LoadContinuationStyleLeadingTrailingDashDots()
    {
        LoadSettings(ContinuationStyle.LeadingTrailingDashDots);
        _isDirty = true;
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

    private void LoadSettings(ContinuationStyle continuationStyle)
    {
        var settings = ContinuationUtilities.GetContinuationProfile(continuationStyle);
        SelectedPrefix = settings.Prefix;
        SelectedPrefixAddSpace = settings.PrefixAddSpace;
        SelectedSuffix = settings.Suffix;
        SelectedSuffixesProcessIfEndWithComma = settings.SuffixApplyIfComma;
        SelectedSuffixesAddSpace = settings.SuffixAddSpace;
        SelectedSuffixesRemoveComma = settings.SuffixReplaceComma;
        UseSpecialStyleAfterLongGaps = settings.UseDifferentStyleGap;
        SelectedLongGapPrefix = settings.GapPrefix;
        SelectedLongGapPrefixAddSpace = settings.GapPrefixAddSpace;
        SelectedLongGapSuffix = settings.GapSuffix;
        SelectedLongGapSuffixesProcessIfEndWithComma = settings.GapSuffixApplyIfComma;
        SelectedLongGapSuffixesAddSpace = settings.GapSuffixAddSpace;
        SelectedLongGapSuffixesRemoveComma = settings.GapSuffixReplaceComma;
    }

    private void UpdatePreview()
    {
        var profile = CreateContinuationProfile();
        var preview = ContinuationUtilities.GetContinuationStylePreview(profile);
        var previewSplit = preview.SplitToLines();

        PanelPreview.Children.Clear();

        foreach (var line in previewSplit)
        {
            if (line == "(...)")
            { 
                if (!UseSpecialStyleAfterLongGaps)
                {
                    break;
                }

                PanelPreview.Children.Add(new TextBlock
                {
                    Text = Se.Language.Options.Settings.AfterLongGap,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(20, 15, 5, 5),
                    Foreground = UiUtil.GetBorderBrush(),
                });

                continue;
            }

            PanelPreview.Children.Add(new TextBlock
            {
                Text = line,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(20, 5, 5, 5),
            });
        }
    }

    private ContinuationUtilities.ContinuationProfile CreateContinuationProfile()
    {
        return new ContinuationUtilities.ContinuationProfile
        {
            Suffix = SelectedSuffix ?? string.Empty,
            SuffixApplyIfComma = SelectedSuffixesProcessIfEndWithComma,
            SuffixAddSpace = SelectedSuffixesAddSpace,
            SuffixReplaceComma = SelectedSuffixesRemoveComma,
            Prefix = SelectedPrefix ?? string.Empty,
            PrefixAddSpace = SelectedPrefixAddSpace,
            GapSuffix = SelectedLongGapSuffix ?? string.Empty,
            GapSuffixApplyIfComma = SelectedLongGapSuffixesProcessIfEndWithComma,
            GapSuffixAddSpace = SelectedLongGapSuffixesAddSpace,
            GapSuffixReplaceComma = SelectedLongGapSuffixesRemoveComma,
            GapPrefix = SelectedLongGapPrefix ?? string.Empty,
            GapPrefixAddSpace = SelectedLongGapPrefixAddSpace,

            UseDifferentStyleGap = UseSpecialStyleAfterLongGaps,
        };
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void StyleChanged()
    {
        _isDirty = true;
    }
}