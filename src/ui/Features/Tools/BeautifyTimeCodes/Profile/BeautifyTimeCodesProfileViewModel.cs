using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Settings;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Tools.BeautifyTimeCodes.Profile;

public partial class BeautifyTimeCodesProfileViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public ObservableCollection<string> ShotChangeBehaviors { get; }

    [ObservableProperty] private double _frameRate = 25.0;

    // General
    [ObservableProperty] private int _gap;

    // In cues
    [ObservableProperty] private int _inCuesGap;
    [ObservableProperty] private int _inCuesLeftGreenZone;
    [ObservableProperty] private int _inCuesLeftRedZone;
    [ObservableProperty] private int _inCuesRightRedZone;
    [ObservableProperty] private int _inCuesRightGreenZone;

    // Out cues
    [ObservableProperty] private int _outCuesGap;
    [ObservableProperty] private int _outCuesLeftGreenZone;
    [ObservableProperty] private int _outCuesLeftRedZone;
    [ObservableProperty] private int _outCuesRightRedZone;
    [ObservableProperty] private int _outCuesRightGreenZone;

    // Connected subtitles
    [ObservableProperty] private int _connectedInCueClosestLeftGap;
    [ObservableProperty] private int _connectedInCueClosestRightGap;
    [ObservableProperty] private int _connectedOutCueClosestLeftGap;
    [ObservableProperty] private int _connectedOutCueClosestRightGap;
    [ObservableProperty] private int _connectedLeftGreenZone;
    [ObservableProperty] private int _connectedLeftRedZone;
    [ObservableProperty] private int _connectedRightRedZone;
    [ObservableProperty] private int _connectedRightGreenZone;
    [ObservableProperty] private int _connectedTreatConnectedMs;

    // Chaining - General
    [ObservableProperty] private bool _chainingGeneralUseZones;
    [ObservableProperty] private bool _chainingGeneralUseMaxGap;
    [ObservableProperty] private int _chainingGeneralMaxGapMs;
    [ObservableProperty] private int _chainingGeneralLeftGreenZone;
    [ObservableProperty] private int _chainingGeneralLeftRedZone;
    [ObservableProperty] private int _selectedChainingShotChangeBehaviorIndex;

    // Chaining - In on shot
    [ObservableProperty] private bool _chainingInOnShotUseZones;
    [ObservableProperty] private bool _chainingInOnShotUseMaxGap;
    [ObservableProperty] private int _chainingInOnShotMaxGapMs;
    [ObservableProperty] private int _chainingInOnShotLeftGreenZone;
    [ObservableProperty] private int _chainingInOnShotLeftRedZone;
    [ObservableProperty] private bool _chainingInOnShotCheckGeneral;

    // Chaining - Out on shot
    [ObservableProperty] private bool _chainingOutOnShotUseZones;
    [ObservableProperty] private bool _chainingOutOnShotUseMaxGap;
    [ObservableProperty] private int _chainingOutOnShotMaxGapMs;
    [ObservableProperty] private int _chainingOutOnShotRightRedZone;
    [ObservableProperty] private int _chainingOutOnShotRightGreenZone;
    [ObservableProperty] private bool _chainingOutOnShotCheckGeneral;

    // Tab selection (drives which preview shows)
    [ObservableProperty] private int _selectedConnectedTabIndex;
    [ObservableProperty] private int _selectedChainingTabIndex;

    public BeautifyTimeCodesProfileViewModel()
    {
        var lang = Se.Language.Tools.BeautifyTimeCodesProfile;
        ShotChangeBehaviors = new ObservableCollection<string>
        {
            lang.DontChain,
            lang.ExtendCrossingShotChange,
            lang.ExtendUntilShotChange,
        };
    }

    public void Initialize()
    {
        var fr = Configuration.Settings.General.CurrentFrameRate;
        if (fr <= 0)
        {
            fr = 25.0;
        }
        FrameRate = fr;

        LoadFrom(Configuration.Settings.BeautifyTimeCodes.Profile);
    }

    private void LoadFrom(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile p)
    {
        Gap = p.Gap;

        InCuesGap = p.InCuesGap;
        InCuesLeftGreenZone = p.InCuesLeftGreenZone;
        InCuesLeftRedZone = p.InCuesLeftRedZone;
        InCuesRightRedZone = p.InCuesRightRedZone;
        InCuesRightGreenZone = p.InCuesRightGreenZone;

        OutCuesGap = p.OutCuesGap;
        OutCuesLeftGreenZone = p.OutCuesLeftGreenZone;
        OutCuesLeftRedZone = p.OutCuesLeftRedZone;
        OutCuesRightRedZone = p.OutCuesRightRedZone;
        OutCuesRightGreenZone = p.OutCuesRightGreenZone;

        ConnectedInCueClosestLeftGap = p.ConnectedSubtitlesInCueClosestLeftGap;
        ConnectedInCueClosestRightGap = p.ConnectedSubtitlesInCueClosestRightGap;
        ConnectedOutCueClosestLeftGap = p.ConnectedSubtitlesOutCueClosestLeftGap;
        ConnectedOutCueClosestRightGap = p.ConnectedSubtitlesOutCueClosestRightGap;
        ConnectedLeftGreenZone = p.ConnectedSubtitlesLeftGreenZone;
        ConnectedLeftRedZone = p.ConnectedSubtitlesLeftRedZone;
        ConnectedRightRedZone = p.ConnectedSubtitlesRightRedZone;
        ConnectedRightGreenZone = p.ConnectedSubtitlesRightGreenZone;
        ConnectedTreatConnectedMs = p.ConnectedSubtitlesTreatConnected;

        ChainingGeneralUseZones = p.ChainingGeneralUseZones;
        ChainingGeneralUseMaxGap = !p.ChainingGeneralUseZones;
        ChainingGeneralMaxGapMs = p.ChainingGeneralMaxGap;
        ChainingGeneralLeftGreenZone = p.ChainingGeneralLeftGreenZone;
        ChainingGeneralLeftRedZone = p.ChainingGeneralLeftRedZone;
        SelectedChainingShotChangeBehaviorIndex = (int)p.ChainingGeneralShotChangeBehavior;

        ChainingInOnShotUseZones = p.ChainingInCueOnShotUseZones;
        ChainingInOnShotUseMaxGap = !p.ChainingInCueOnShotUseZones;
        ChainingInOnShotMaxGapMs = p.ChainingInCueOnShotMaxGap;
        ChainingInOnShotLeftGreenZone = p.ChainingInCueOnShotLeftGreenZone;
        ChainingInOnShotLeftRedZone = p.ChainingInCueOnShotLeftRedZone;
        ChainingInOnShotCheckGeneral = p.ChainingInCueOnShotCheckGeneral;

        ChainingOutOnShotUseZones = p.ChainingOutCueOnShotUseZones;
        ChainingOutOnShotUseMaxGap = !p.ChainingOutCueOnShotUseZones;
        ChainingOutOnShotMaxGapMs = p.ChainingOutCueOnShotMaxGap;
        ChainingOutOnShotRightRedZone = p.ChainingOutCueOnShotRightRedZone;
        ChainingOutOnShotRightGreenZone = p.ChainingOutCueOnShotRightGreenZone;
        ChainingOutOnShotCheckGeneral = p.ChainingOutCueOnShotCheckGeneral;
    }

    private void SaveTo(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile p)
    {
        p.Gap = Gap;

        p.InCuesGap = InCuesGap;
        p.InCuesLeftGreenZone = InCuesLeftGreenZone;
        p.InCuesLeftRedZone = InCuesLeftRedZone;
        p.InCuesRightRedZone = InCuesRightRedZone;
        p.InCuesRightGreenZone = InCuesRightGreenZone;

        p.OutCuesGap = OutCuesGap;
        p.OutCuesLeftGreenZone = OutCuesLeftGreenZone;
        p.OutCuesLeftRedZone = OutCuesLeftRedZone;
        p.OutCuesRightRedZone = OutCuesRightRedZone;
        p.OutCuesRightGreenZone = OutCuesRightGreenZone;

        p.ConnectedSubtitlesInCueClosestLeftGap = ConnectedInCueClosestLeftGap;
        p.ConnectedSubtitlesInCueClosestRightGap = ConnectedInCueClosestRightGap;
        p.ConnectedSubtitlesOutCueClosestLeftGap = ConnectedOutCueClosestLeftGap;
        p.ConnectedSubtitlesOutCueClosestRightGap = ConnectedOutCueClosestRightGap;
        p.ConnectedSubtitlesLeftGreenZone = ConnectedLeftGreenZone;
        p.ConnectedSubtitlesLeftRedZone = ConnectedLeftRedZone;
        p.ConnectedSubtitlesRightRedZone = ConnectedRightRedZone;
        p.ConnectedSubtitlesRightGreenZone = ConnectedRightGreenZone;
        p.ConnectedSubtitlesTreatConnected = ConnectedTreatConnectedMs;

        var previousBehavior = p.ChainingGeneralShotChangeBehavior;

        p.ChainingGeneralUseZones = ChainingGeneralUseZones;
        p.ChainingGeneralMaxGap = ChainingGeneralMaxGapMs;
        p.ChainingGeneralLeftGreenZone = ChainingGeneralLeftGreenZone;
        p.ChainingGeneralLeftRedZone = ChainingGeneralLeftRedZone;
        p.ChainingGeneralShotChangeBehavior = (BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.ChainingShotChangeBehaviorEnum)SelectedChainingShotChangeBehaviorIndex;

        p.ChainingInCueOnShotUseZones = ChainingInOnShotUseZones;
        p.ChainingInCueOnShotMaxGap = ChainingInOnShotMaxGapMs;
        p.ChainingInCueOnShotLeftGreenZone = ChainingInOnShotLeftGreenZone;
        p.ChainingInCueOnShotLeftRedZone = ChainingInOnShotLeftRedZone;
        p.ChainingInCueOnShotCheckGeneral = ChainingInOnShotCheckGeneral;

        p.ChainingOutCueOnShotUseZones = ChainingOutOnShotUseZones;
        p.ChainingOutCueOnShotMaxGap = ChainingOutOnShotMaxGapMs;
        p.ChainingOutCueOnShotRightRedZone = ChainingOutOnShotRightRedZone;
        p.ChainingOutCueOnShotRightGreenZone = ChainingOutOnShotRightGreenZone;
        p.ChainingOutCueOnShotCheckGeneral = ChainingOutOnShotCheckGeneral;

        if (previousBehavior != p.ChainingGeneralShotChangeBehavior)
        {
            p.ChainingInCueOnShotShotChangeBehavior = p.ChainingGeneralShotChangeBehavior;
            p.ChainingOutCueOnShotShotChangeBehavior = p.ChainingGeneralShotChangeBehavior;
        }
    }

    [RelayCommand]
    private void LoadPresetDefault() => LoadFrom(new BeautifyTimeCodesSettings.BeautifyTimeCodesProfile(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.Default));

    [RelayCommand]
    private void LoadPresetNetflix() => LoadFrom(new BeautifyTimeCodesSettings.BeautifyTimeCodesProfile(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.Netflix));

    [RelayCommand]
    private void LoadPresetSdi() => LoadFrom(new BeautifyTimeCodesSettings.BeautifyTimeCodesProfile(BeautifyTimeCodesSettings.BeautifyTimeCodesProfile.Preset.SDI));

    [RelayCommand]
    private void Ok()
    {
        SaveTo(Configuration.Settings.BeautifyTimeCodes.Profile);

        // Persist to SE5's Settings.json so the profile survives restart.
        // libse remains the source of truth for the engine; we mirror it here.
        Se.Settings.BeautifyTimeCodes.CopyFrom(Configuration.Settings.BeautifyTimeCodes);
        Se.SaveSettings();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel() => Window?.Close();

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
    }

    // GapMs property used by NumericUpDown bindings — derived from frame count
    public double FramesToMs(int frames) => frames * (1000.0 / FrameRate);

    // When the general "Gap" changes, propagate to gap NUDs that already have a non-zero
    // value. Honors the tooltip's "will overwrite custom settings" promise across all
    // explicit gap fields, but leaves intentional zeros alone.
    partial void OnGapChanged(int value)
    {
        if (InCuesGap > 0)
        {
            InCuesGap = value;
        }

        if (OutCuesGap > 0)
        {
            OutCuesGap = value;
        }

        if (ConnectedInCueClosestLeftGap > 0 && ConnectedInCueClosestRightGap == 0)
        {
            ConnectedInCueClosestLeftGap = value;
        }

        if (ConnectedInCueClosestLeftGap == 0 && ConnectedInCueClosestRightGap > 0)
        {
            ConnectedInCueClosestRightGap = value;
        }

        if (ConnectedOutCueClosestLeftGap > 0 && ConnectedOutCueClosestRightGap == 0)
        {
            ConnectedOutCueClosestLeftGap = value;
        }

        if (ConnectedOutCueClosestLeftGap == 0 && ConnectedOutCueClosestRightGap > 0)
        {
            ConnectedOutCueClosestRightGap = value;
        }
    }

    // Enforce greenZone >= redZone (matches SE4 ValidateZones())
    partial void OnInCuesLeftRedZoneChanged(int value)
    {
        if (InCuesLeftGreenZone < value) InCuesLeftGreenZone = value;
    }
    partial void OnInCuesRightRedZoneChanged(int value)
    {
        if (InCuesRightGreenZone < value) InCuesRightGreenZone = value;
    }
    partial void OnOutCuesLeftRedZoneChanged(int value)
    {
        if (OutCuesLeftGreenZone < value) OutCuesLeftGreenZone = value;
    }
    partial void OnOutCuesRightRedZoneChanged(int value)
    {
        if (OutCuesRightGreenZone < value) OutCuesRightGreenZone = value;
    }
    partial void OnConnectedLeftRedZoneChanged(int value)
    {
        if (ConnectedLeftGreenZone < value) ConnectedLeftGreenZone = value;
    }
    partial void OnConnectedRightRedZoneChanged(int value)
    {
        if (ConnectedRightGreenZone < value) ConnectedRightGreenZone = value;
    }
    partial void OnChainingGeneralLeftRedZoneChanged(int value)
    {
        if (ChainingGeneralLeftGreenZone < value) ChainingGeneralLeftGreenZone = value;
    }
    partial void OnChainingInOnShotLeftRedZoneChanged(int value)
    {
        if (ChainingInOnShotLeftGreenZone < value) ChainingInOnShotLeftGreenZone = value;
    }
    partial void OnChainingOutOnShotRightRedZoneChanged(int value)
    {
        if (ChainingOutOnShotRightGreenZone < value) ChainingOutOnShotRightGreenZone = value;
    }
}
